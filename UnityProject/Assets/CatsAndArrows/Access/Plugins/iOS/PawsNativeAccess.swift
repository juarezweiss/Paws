import Foundation
import UIKit
import DeclaredAgeRange
import PermissionKit

// #103. Only coarse transient evidence crosses this bridge; no Apple response,
// declaration source, age, account or region is written to disk or logged.
private final class PawsAccessSessionCounter {
    static let shared = PawsAccessSessionCounter()
    private let lock = NSLock()
    private var value: Int64 = 0
    func next() -> Int64 {
        lock.lock(); defer { lock.unlock() }; value += 1; return value
    }
}

private final class PawsAccessReportQueue {
    static let shared = PawsAccessReportQueue()
    private let lock = NSLock()
    private var value: (Int64, String)?
    func put(_ report: [String: Any]) {
        guard let data = try? JSONSerialization.data(withJSONObject: report),
              let text = String(data: data, encoding: .utf8) else { return }
        guard let session = report["sessionId"] as? Int64 else { return }
        lock.lock(); value = (session, text); lock.unlock()
    }
    func take(session: Int64) -> String? {
        lock.lock(); defer { lock.unlock() }
        guard let result = value, result.0 == session else { return nil }
        value = nil; return result.1
    }
    func clear(session: Int64) {
        lock.lock(); defer { lock.unlock() }
        if value?.0 == session { value = nil }
    }
}

@MainActor
private enum PawsNativeAccess {
    static let checkpointKey = "paws.access.workflow.v1"
    static let pendingQuestionKey = "paws.access.question.v1"
    static let pendingChangeKey = "paws.access.change.v1"
    static let previousVersionKey = "paws.access.contentVersion.v1"
    // Free 1.0 has no significant update to present. Add only an actual reviewed,
    // localized change, never a test scenario or a fabricated initial permission.
    static let changes: [PawsSignificantChange] = []
    static let store = UserDefaults.standard
    static var permissionWorkflow = PawsPermissionWorkflow()
    static var accountEpoch: Int { permissionWorkflow.epoch }
    static var lifetime = PawsAccessLifetime()
    static var activeId: Int64 { lifetime.request }
    static var running = false
    static var queued: (Int64, Bool)?
    static var responseTask: Task<Void, Never>?
    static var disposed: Bool { lifetime.disposed }
    static var pendingChange: PawsSignificantChange?
    static var lifecycleObservers: [NSObjectProtocol] = []

    static func refresh(_ session: Int64, id: Int64, userInitiated: Bool) {
        let previousSession = lifetime.session
        guard lifetime.begin(session: session, request: id) else { return }
        if previousSession != session {
            running = false; queued = nil
            responseTask?.cancel(); responseTask = nil
            pendingChange = nil; permissionWorkflow = PawsPermissionWorkflow()
            PawsAccessReportQueue.shared.clear(session: previousSession)
        }
        observeLifecycle()
        if running { queued = (id, userInitiated); return }
        running = true
        Task { @MainActor in
            await check(session, id: id, userInitiated: userInitiated)
            guard lifetime.accepts(session: session) else { return }
            running = false
            if let next = queued {
                queued = nil
                refresh(session, id: next.0, userInitiated: next.1)
            }
        }
    }

    static func report(_ id: Int64, result: Int = 0, lower: Int = -1, upper: Int = -1,
                       reliable: Bool = false, required: Bool = false,
                       checkpoint: Bool = false, permission: Int = 0, updateRequired: Bool = false, adultAck: Bool = false) {
        guard !disposed, id == activeId else { return }
        PawsAccessReportQueue.shared.put([
            "sessionId": lifetime.session, "requestId": id, "result": result, "lower": lower, "upper": upper,
            "reliable": reliable, "required": required, "checkpoint": checkpoint,
            "permission": permission, "updateRequired": updateRequired, "adultAcknowledgementRequired": adultAck
        ])
    }

    static func check(_ session: Int64, id: Int64, userInitiated: Bool) async {
        guard lifetime.accepts(session: session, request: id) else { return }
        let epoch = accountEpoch
        let phase = PawsProtectionPhase(rawValue: store.integer(forKey: checkpointKey)) ?? .awaitingResolution
        let checkpoint = phase.protectsEarlierWorkflow
        guard #available(iOS 26.0, *) else {
            report(id, result: 3, checkpoint: checkpoint)
            return
        }
        let pending = currentChange()
        pendingChange = pending
        permissionWorkflow.restore(questionId: store.string(forKey: pendingQuestionKey),
                                   changeId: store.string(forKey: pendingChangeKey))
        var regulated = false
        var required = false
        var parentalUpdate = false
        var adultUpdate = false
        do {
            if #available(iOS 26.4, *) {
                let features = try await AgeRangeService.shared.requiredRegulatoryFeatures
                required = features.contains(.declaredAgeRangeRequired)
                parentalUpdate = pending != nil && features.contains(.significantAppChangeRequiresParentalConsent)
                adultUpdate = pending != nil && features.contains(.significantAppChangeRequiresAdultNotification)
                regulated = !features.isEmpty
            } else if #available(iOS 26.2, *) {
                regulated = try await AgeRangeService.shared.isEligibleForAgeFeatures
                // The old Boolean is not evidence that optional sharing is legally mandatory.
                required = false
            }
        } catch {
            guard epoch == accountEpoch, lifetime.accepts(session: session, request: id) else { return }
            report(id, result: 4, checkpoint: checkpoint)
            return
        }
        guard epoch == accountEpoch, lifetime.accepts(session: session, request: id) else { return }
        let needsRange = PawsAccessWorkflow.needsRange(required: required, checkpoint: checkpoint,
            userInitiated: userInitiated, regulated: regulated,
            pendingSignificantChange: parentalUpdate || adultUpdate)
        guard needsRange else {
            report(id, checkpoint: checkpoint)
            return
        }
        // Explicit native obligations already require recovery if the process ends
        // before managed polling. Optional applicability alone is not an obligation.
        if required || parentalUpdate || adultUpdate { establishProtection() }
        // This pre-request phase records only application workflow. Never overwrite
        // an already established protective phase with a new InFlight marker.
        let currentPhase = PawsProtectionPhase(rawValue: store.integer(forKey: checkpointKey)) ?? .awaitingResolution
        store.set(currentPhase.startingRequest.rawValue, forKey: checkpointKey)
        guard let view = activeViewController() else {
            report(id, result: 3, required: required, checkpoint: checkpoint,
                   updateRequired: parentalUpdate, adultAck: adultUpdate)
            return
        }
        do {
            let response = try await AgeRangeService.shared.requestAgeRange(ageGates: 13, 16, 18, in: view)
            guard epoch == accountEpoch, lifetime.accepts(session: session, request: id) else { return }
            switch response {
            case .declinedSharing:
                report(id, result: 2, required: required, checkpoint: checkpoint,
                       updateRequired: parentalUpdate, adultAck: adultUpdate)
            case .sharing(let range):
                let lower = range.lowerBound ?? -1
                let upper = range.upperBound ?? -1
                let reliable = range.ageRangeDeclaration != nil
                // On 26.2–26.3 combine per-account eligibility, declared age and
                // the legacy control for an actual configured significant change.
                if #available(iOS 26.2, *), !isAtLeast264(), pending != nil, regulated {
                    parentalUpdate = upper >= 0 && upper < 18 &&
                        range.activeParentalControls.contains(.significantAppChangeApprovalRequired)
                }
                // A known below-minimum player never reaches an update approval flow.
                if reliable && upper >= 0 && upper < 13 {
                    establishProtection()
                    report(id, result: 1, lower: lower, upper: upper, reliable: reliable,
                           required: required, checkpoint: checkpoint)
                    return
                }
                if reliable && upper >= 0 && upper < 18 { adultUpdate = false }
                if reliable && lower >= 18 { parentalUpdate = false }
                if parentalUpdate, reliable, upper >= 0, upper < 18, let change = pending {
                    if #available(iOS 26.2, *) {
                        await requestPermission(session, id: id, change: change, view: view,
                            lower: lower, upper: upper, reliable: reliable, required: required, checkpoint: checkpoint, retry: userInitiated)
                    }
                    return
                }
                if parentalUpdate && !(reliable && lower >= 18) {
                    report(id, result: 1, lower: lower, upper: upper, reliable: reliable,
                           required: required, checkpoint: checkpoint, permission: 2, updateRequired: true)
                    return
                }
                if adultUpdate, let change = pending {
                    guard #available(iOS 26.4, *), let scene = view.view.window?.windowScene,
                          lower >= 18, isConfirmed(range) else {
                        report(id, result: 1, lower: lower, upper: upper, reliable: reliable,
                               required: required, checkpoint: checkpoint, adultAck: true)
                        return
                    }
                    do {
                        try await AgeRangeService.shared.showSignificantUpdateAcknowledgment(
                            in: scene, updateDescription: change.description)
                        guard epoch == accountEpoch, lifetime.accepts(session: session, request: id) else { return }
                        markHandled(change)
                        PawsAccessReportQueue.shared.put(["sessionId": lifetime.session, "requestId": activeId, "refresh": true])
                        return
                    } catch {
                        guard epoch == accountEpoch, lifetime.accepts(session: session, request: id) else { return }
                        report(id, result: 1, lower: lower, upper: upper, reliable: reliable,
                               required: required, checkpoint: checkpoint, adultAck: true)
                        return
                    }
                }
                report(id, result: 1, lower: lower, upper: upper, reliable: reliable,
                       required: required, checkpoint: checkpoint)
            @unknown default:
                report(id, result: 4, required: required, checkpoint: checkpoint,
                       updateRequired: parentalUpdate, adultAck: adultUpdate)
            }
        } catch {
            guard epoch == accountEpoch, lifetime.accepts(session: session, request: id) else { return }
            report(id, result: 4, required: required, checkpoint: checkpoint,
                   updateRequired: parentalUpdate, adultAck: adultUpdate)
        }
    }

    @available(iOS 26.2, *)
    static func listenForResponses() {
        guard responseTask == nil else { return }
        let epoch = accountEpoch
        let session = lifetime.session
        responseTask = Task { @MainActor in
            for await response in AskCenter.shared.responses(for: SignificantAppUpdateTopic.self) {
                guard !Task.isCancelled, lifetime.accepts(session: session), let change = pendingChange else { continue }
                let outcome = permissionWorkflow.receive(questionId: response.question.id.uuidString,
                    changeId: change.id, approved: response.choice.answer == .approval, responseEpoch: epoch)
                guard outcome != .ignored else { continue }
                if outcome == .approved {
                    markHandled(change)
                    store.removeObject(forKey: pendingQuestionKey)
                    store.removeObject(forKey: pendingChangeKey)
                    PawsAccessReportQueue.shared.put(["sessionId": lifetime.session, "requestId": activeId, "refresh": true])
                } else {
                    establishProtection()
                    report(activeId, result: 0, checkpoint: true, permission: 4, updateRequired: true)
                }
            }
        }
    }

    @available(iOS 26.2, *)
    static func requestPermission(_ session: Int64, id: Int64, change: PawsSignificantChange, view: UIViewController,
                                  lower: Int, upper: Int, reliable: Bool, required: Bool,
                                  checkpoint: Bool, retry: Bool) async {
        let epoch = accountEpoch
        establishProtection()
        listenForResponses()
        // A deliberate recovery can replace a denied/unanswerable request.
        if retry { store.removeObject(forKey: pendingQuestionKey) }
        // Re-register after restart; never send duplicate requests on every resume.
        if store.string(forKey: pendingChangeKey) == change.id,
           store.string(forKey: pendingQuestionKey) != nil {
            report(id, result: 1, lower: lower, upper: upper, reliable: reliable, required: required,
                   checkpoint: checkpoint, permission: 2, updateRequired: true)
            return
        }
        let question = PermissionQuestion(significantAppUpdateTopic: SignificantAppUpdateTopic(description: change.description))
        permissionWorkflow.begin(questionId: question.id.uuidString, changeId: change.id)
        store.set(question.id.uuidString, forKey: pendingQuestionKey)
        store.set(change.id, forKey: pendingChangeKey)
        report(id, result: 1, lower: lower, upper: upper, reliable: reliable, required: required,
               checkpoint: checkpoint, permission: 2, updateRequired: true)
        do {
            try await AskCenter.shared.ask(question, in: view)
            // Completion only means the system request finished, not parent approval.
        } catch {
            guard epoch == accountEpoch, lifetime.accepts(session: session, request: id) else { return }
            // Keep the obligation pending; a deliberate recovery retry may present it again.
            store.removeObject(forKey: pendingQuestionKey)
            report(id, result: 1, lower: lower, upper: upper, reliable: reliable, required: required,
                   checkpoint: checkpoint, permission: 2, updateRequired: true)
        }
    }

    static func currentChange() -> PawsSignificantChange? {
        let version = Bundle.main.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String ?? "0"
        let previous = store.string(forKey: previousVersionKey)
        let handled = permissionWorkflow.handled
        let pending = PawsAccessWorkflow.pendingChanges(changes, previousVersion: previous,
            currentVersion: version, handled: handled)
        if previous == nil {
            store.set(version, forKey: previousVersionKey)
        }
        return pending.first
    }
    static func markHandled(_ change: PawsSignificantChange) {
        permissionWorkflow.handle(change.id)
    }
    static func establishProtection() {
        let phase = PawsProtectionPhase(rawValue: store.integer(forKey: checkpointKey)) ?? .awaitingResolution
        store.set(phase.observingProtection(required: true).rawValue, forKey: checkpointKey)
        // Flush this tiny application-owned phase before publishing known recovery
        // requirements; no age/category/source/answer is stored.
        store.synchronize()
    }
    static func resolve(_ session: Int64, id: Int64, allowed: Bool) {
        guard lifetime.accepts(session: session, request: id) else { return }
        let phase = PawsProtectionPhase(rawValue: store.integer(forKey: checkpointKey)) ?? .awaitingResolution
        let resolved = phase.evaluated(allowed: allowed)
        if resolved == .none { store.removeObject(forKey: checkpointKey) }
        else { store.set(resolved.rawValue, forKey: checkpointKey) }
        store.synchronize()
    }
    static func dispose(_ session: Int64) {
        guard lifetime.dispose(session: session) else { return }
        running = false; queued = nil; responseTask?.cancel(); responseTask = nil
        pendingChange = nil; permissionWorkflow = PawsPermissionWorkflow()
        PawsAccessReportQueue.shared.clear(session: session)
    }
    @available(iOS 26.0, *)
    static func isConfirmed(_ range: AgeRangeService.AgeRange) -> Bool {
        guard let declaration = range.ageRangeDeclaration else { return false }
        if #available(iOS 26.5, *), declaration == .confirmed { return true }
        if #available(iOS 26.2, *) {
            switch declaration {
            case .checkedByOtherMethod, .guardianCheckedByOtherMethod, .governmentIDChecked,
                 .guardianGovernmentIDChecked, .paymentChecked, .guardianPaymentChecked: return true
            default: break
            }
        }
        return false
    }
    static func observeLifecycle() {
        guard lifecycleObservers.isEmpty else { return }
        lifecycleObservers.append(NotificationCenter.default.addObserver(
            forName: UIApplication.didEnterBackgroundNotification, object: nil, queue: .main) { _ in
                Task { @MainActor in permissionWorkflow.suspend() }
            })
        lifecycleObservers.append(NotificationCenter.default.addObserver(
            forName: NSNotification.Name.NSUbiquityIdentityDidChange, object: nil, queue: .main) { _ in
                Task { @MainActor in
                    permissionWorkflow.identityChanged()
                    store.removeObject(forKey: pendingQuestionKey)
                    store.removeObject(forKey: pendingChangeKey)
                    pendingChange = nil
                    responseTask?.cancel(); responseTask = nil
                    queued = nil
                    PawsAccessReportQueue.shared.put(["sessionId": lifetime.session, "requestId": activeId, "refresh": true])
                }
            })
    }
    static func isAtLeast264() -> Bool {
        if #available(iOS 26.4, *) { return true }; return false
    }
    static func activeViewController() -> UIViewController? {
        guard let window = UIApplication.shared.delegate?.window ?? nil,
              window.isKeyWindow, !window.isHidden,
              let scene = window.windowScene, scene.activationState == .foregroundActive else { return nil }
        var view = window.rootViewController
        while let presented = view?.presentedViewController { view = presented }
        return view
    }
}

@_cdecl("PawsAccessCreateSession")
public func pawsAccessCreateSession() -> Int64 { PawsAccessSessionCounter.shared.next() }
@_cdecl("PawsAccessRefresh")
public func pawsAccessRefresh(_ session: Int64, _ id: Int64, _ userInitiated: Int32) {
    Task { @MainActor in PawsNativeAccess.refresh(session, id: id, userInitiated: userInitiated != 0) }
}
@_cdecl("PawsAccessPoll")
public func pawsAccessPoll(_ session: Int64) -> UnsafeMutablePointer<CChar>? {
    guard let value = PawsAccessReportQueue.shared.take(session: session) else { return nil }
    return strdup(value)
}
@_cdecl("PawsAccessFree")
public func pawsAccessFree(_ value: UnsafeMutablePointer<CChar>?) { free(value) }
@_cdecl("PawsAccessResolve")
public func pawsAccessResolve(_ session: Int64, _ id: Int64, _ allowed: Int32) {
    // Commit application workflow phase before managed code exposes the decision.
    if Thread.isMainThread {
        MainActor.assumeIsolated { PawsNativeAccess.resolve(session, id: id, allowed: allowed != 0) }
    } else {
        DispatchQueue.main.sync {
            MainActor.assumeIsolated { PawsNativeAccess.resolve(session, id: id, allowed: allowed != 0) }
        }
    }
}
@_cdecl("PawsAccessOpenSettings")
public func pawsAccessOpenSettings(_ session: Int64) {
    Task { @MainActor in
        guard PawsNativeAccess.lifetime.accepts(session: session), let url = URL(string: UIApplication.openSettingsURLString) else { return }
        UIApplication.shared.open(url)
    }
}
@_cdecl("PawsAccessDispose")
public func pawsAccessDispose(_ session: Int64) { Task { @MainActor in PawsNativeAccess.dispose(session) } }

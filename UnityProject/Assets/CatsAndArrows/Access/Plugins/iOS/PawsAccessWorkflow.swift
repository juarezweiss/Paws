import Foundation

// #103: pure application workflow; contains no Apple age response or account data.
struct PawsSignificantChange: Equatable {
    let id: String
    let introducedVersion: String
    let description: String
}

enum PawsAccessWorkflow {
    static func needsRange(required: Bool, checkpoint: Bool, userInitiated: Bool,
                           regulated: Bool, pendingSignificantChange: Bool) -> Bool {
        required || checkpoint || userInitiated || regulated || pendingSignificantChange
    }

    static func pendingChanges(_ changes: [PawsSignificantChange], previousVersion: String?,
                               currentVersion: String, handled: Set<String>) -> [PawsSignificantChange] {
        guard let previousVersion = previousVersion else { return [] } // Initial content is not an update.
        return changes.filter {
            !handled.contains($0.id) &&
            $0.introducedVersion.compare(previousVersion, options: .numeric) == .orderedDescending &&
            $0.introducedVersion.compare(currentVersion, options: .numeric) != .orderedDescending
        }
    }

    static func matchesResponse(questionId: String?, receivedQuestionId: String,
                                changeId: String?, expectedChangeId: String?,
                                responseEpoch: Int = 0, currentEpoch: Int = 0) -> Bool {
        responseEpoch == currentEpoch && questionId != nil && questionId == receivedQuestionId &&
            changeId != nil && changeId == expectedChangeId
    }
}

// Only app-created workflow identity/epoch is durable. Approval is session-only.
struct PawsPermissionWorkflow {
    enum Response: Equatable { case ignored, approved, denied }
    private(set) var epoch = 0
    private(set) var pendingQuestionId: String?
    private(set) var pendingChangeId: String?
    private(set) var handled = Set<String>()

    mutating func restore(questionId: String?, changeId: String?) {
        pendingQuestionId = questionId; pendingChangeId = changeId
    }
    mutating func begin(questionId: String, changeId: String) {
        pendingQuestionId = questionId; pendingChangeId = changeId
    }
    mutating func receive(questionId: String, changeId: String, approved: Bool, responseEpoch: Int) -> Response {
        guard PawsAccessWorkflow.matchesResponse(questionId: pendingQuestionId, receivedQuestionId: questionId,
            changeId: pendingChangeId, expectedChangeId: changeId, responseEpoch: responseEpoch, currentEpoch: epoch) else { return .ignored }
        if approved {
            handled.insert(changeId)
            pendingQuestionId = nil; pendingChangeId = nil
            return .approved
        }
        return .denied
    }
    mutating func handle(_ changeId: String) { handled.insert(changeId) }
    mutating func suspend() {
        // App content has not changed. An ordinary background must not repeat an
        // acknowledged update; account/lifetime changes invalidate this session.
    }
    mutating func identityChanged() {
        epoch += 1; handled.removeAll(); pendingQuestionId = nil; pendingChangeId = nil
    }
}

// A session is native-generated and independent of a managed service's request IDs.
struct PawsAccessLifetime {
    private(set) var session: Int64 = 0
    private(set) var request: Int64 = 0
    private(set) var disposed = false
    mutating func begin(session nextSession: Int64, request nextRequest: Int64) -> Bool {
        guard nextSession > session || (nextSession == session && !disposed && nextRequest >= request) else { return false }
        session = nextSession; request = nextRequest; disposed = false
        return true
    }
    func accepts(session expectedSession: Int64, request expectedRequest: Int64? = nil) -> Bool {
        !disposed && session == expectedSession && (expectedRequest == nil || request == expectedRequest)
    }
    mutating func dispose(session expectedSession: Int64) -> Bool {
        guard session == expectedSession else { return false }
        disposed = true; return true
    }
}

enum PawsProtectionPhase: Int {
    case none = 0
    case inFlight = 1
    case awaitingResolution = 2

    var protectsEarlierWorkflow: Bool { self == .awaitingResolution }
    var startingRequest: Self { protectsEarlierWorkflow ? .awaitingResolution : .inFlight }
    // Native can already know a genuine restriction/obligation before managed
    // polling. Preserve that knowledge even if the report is never consumed.
    func observingProtection(required: Bool) -> Self { required ? .awaitingResolution : self }
    func evaluated(allowed: Bool) -> Self { allowed ? .none : .awaitingResolution }
}

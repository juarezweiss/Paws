import Foundation
import UIKit

// Isolated test-only UIApplication. The two production bridge sources are copied
// byte-for-byte from the committed source; no framework or API is mocked here.
@main
final class NativeAccessCompatibilityProbe: UIResponder, UIApplicationDelegate {
    var window: UIWindow?
    let phaseKey = "paws.access.workflow.v1"
    let bundleId = "com.jweiss.paws.native-access-compatibility-probe"

    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        let window = UIWindow(frame: UIScreen.main.bounds)
        let view = UIViewController(); view.view.backgroundColor = .systemBackground
        window.rootViewController = view; window.makeKeyAndVisible(); self.window = window
        Task { @MainActor in
            try? await Task.sleep(nanoseconds: 500_000_000)
            await runProbe()
        }
        return true
    }

    @MainActor
    func runProbe() async {
        let arguments = ProcessInfo.processInfo.arguments
        let mode = arguments.dropFirst().first ?? "ordinary"
        let store = UserDefaults.standard
        if mode == "ordinary" { store.removeObject(forKey: phaseKey) }
        if mode == "inflight" { store.set(1, forKey: phaseKey) }
        if mode == "awaiting" { store.set(2, forKey: phaseKey) }
        store.synchronize()
        let expectedCheckpoint = mode == "awaiting" || mode == "relaunch-awaiting"
        let before = store.integer(forKey: phaseKey)
        let session = pawsAccessCreateSession()
        pawsAccessRefresh(session, 1, 0)
        var payload: [String:Any]?
        for _ in 0..<200 {
            if let value = pawsAccessPoll(session) {
                let data = Data(bytes: value, count: strlen(value))
                pawsAccessFree(value)
                payload = (try? JSONSerialization.jsonObject(with: data)) as? [String:Any]
                break
            }
            try? await Task.sleep(nanoseconds: 100_000_000)
        }
        var assertions: [String:Bool] = [:]
        var observation: [String:Any] = [:]
        assertions["productionNativeReportReceived"] = payload != nil
        if let payload = payload {
            let result = (payload["result"] as? NSNumber)?.intValue ?? -1
            let required = payload["required"] as? Bool ?? false
            let checkpoint = payload["checkpoint"] as? Bool ?? false
            let updateRequired = payload["updateRequired"] as? Bool ?? false
            let adultAck = payload["adultAcknowledgementRequired"] as? Bool ?? false
            let permission = (payload["permission"] as? NSNumber)?.intValue ?? -1
            let category = ["NotRequired", "Shared", "Declined", "Unavailable", "NativeAPIErrorUnknown"]
            observation = ["nativeCategory": result >= 0 && result < category.count ? category[result] : "UnexpectedCategory",
                "requiredRangeFlag": required, "protectiveWorkflowFlag": checkpoint,
                "significantUpdateRequiredFlag": updateRequired, "adultAcknowledgementRequiredFlag": adultAck,
                "permissionSystemManaged": permission == 0]
            assertions["currentRequestMatched"] = (payload["requestId"] as? NSNumber)?.int64Value == 1
            assertions["currentNativeSessionMatched"] = (payload["sessionId"] as? NSNumber)?.int64Value == session
            if #available(iOS 26.0, *) {
                // This is a fresh Simulator environment with no test-account setup.
                // An API error cannot establish its cause or regional compliance.
                assertions["ordinaryNoAccountSmokeWithoutReturnedAge"] = result != 1
                assertions["noClaimedMandatoryPermissionAbsentSignal"] = permission == 0 && !updateRequired && !adultAck
            } else {
                assertions["pre26UnavailableFallback"] = result == 3
                assertions["pre26NoMandatoryRangeInvented"] = !required
                assertions["pre26CheckpointMatchesAppOwnedFixture"] = checkpoint == expectedCheckpoint
                assertions["pre26InitialPermissionRemainsSystemManaged"] = permission == 0
                assertions["pre26NoSignificantUpdateInvented"] = !updateRequired && !adultAck
            }
            if !expectedCheckpoint && !required && !updateRequired && !adultAck {
                // Client resolution represents the tested optional-Unknown policy;
                // this probe is native C ABI evidence, not Unity shell execution.
                pawsAccessResolve(session, 1, 1)
                assertions["optionalUnknownResolutionClearsAppWorkflow"] = store.integer(forKey: phaseKey) == 0
            } else if expectedCheckpoint {
                pawsAccessResolve(session, 1, 0)
                assertions["unresolvedAppRecoveryRemainsProtective"] = store.integer(forKey: phaseKey) == 2
            }
        }
        assertions["probeRootHasNoPresentedController"] = window?.rootViewController?.presentedViewController == nil
        let receipt: [String:Any] = ["mode":mode, "systemVersion":UIDevice.current.systemVersion,
            "sourceKind":"actual-committed-native-bridge", "probeKind":"isolated-UIApplication-not-Unity-gameplay",
            "appOwnedPhaseBefore":before, "appOwnedPhaseAfter":store.integer(forKey:phaseKey),
            "observation":observation, "assertions":assertions, "passed":assertions.values.allSatisfy{$0},
            "sandboxRegionalCoverage":false, "physicalCoverage":false, "ageResponsePersisted":false]
        if let data = try? JSONSerialization.data(withJSONObject: receipt, options:[.prettyPrinted,.sortedKeys]) {
            let directory = FileManager.default.urls(for:.documentDirectory,in:.userDomainMask)[0]
            try? data.write(to:directory.appendingPathComponent("receipt-"+mode+".json"),options:.atomic)
            print("PAWS_NATIVE_COMPATIBILITY_RESULT " + String(decoding:data,as:UTF8.self).replacingOccurrences(of:"\n",with:" "))
        }
        pawsAccessDispose(session)
        fflush(stdout)
        exit(assertions.values.allSatisfy{$0} ? 0 : 1)
    }
}

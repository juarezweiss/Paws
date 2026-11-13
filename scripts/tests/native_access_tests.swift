import Foundation

@main
private enum NativeAccessTests {
    static func main() {
        var count = 0
        func check(_ value: Bool, _ message: String) {
            precondition(value, message); count += 1
        }
        for required in [false, true] {
            for checkpoint in [false, true] {
                for userInitiated in [false, true] {
                    for regulated in [false, true] {
                        for significant in [false, true] {
                            let result = PawsAccessWorkflow.needsRange(required: required, checkpoint: checkpoint,
                                userInitiated: userInitiated, regulated: regulated, pendingSignificantChange: significant)
                            if !required && !checkpoint && !userInitiated && !regulated && !significant {
                                check(!result, "Ordinary optional startup must not ask age")
                            } else {
                                check(result, "A genuine required/recovery workflow must remain reachable")
                            }
                        }
                    }
                }
            }
        }
        // #103 / PR104 B1: modern feature membership is not a range obligation.
        // Exercise all range/update flags, actual-change and recovery combinations
        // using the same modern routing entry point as the production bridge.
        for required in [false, true] {
            for parentalUpdateFlag in [false, true] {
                for adultUpdateFlag in [false, true] {
                    for pendingChange in [false, true] {
                        for checkpoint in [false, true] {
                            for userInitiated in [false, true] {
                                let applicableUpdate = pendingChange && (parentalUpdateFlag || adultUpdateFlag)
                                let result = PawsAccessWorkflow.needsRangeForRegulatoryFeatures(required: required,
                                    checkpoint: checkpoint, userInitiated: userInitiated,
                                    pendingSignificantChange: applicableUpdate)
                                if required {
                                    check(result, "26.4+ mandatory range checks must remain reachable")
                                } else if checkpoint || userInitiated {
                                    check(result, "26.4+ prior protection and explicit recovery must remain reachable")
                                } else if applicableUpdate {
                                    check(result, "26.4+ an actual required significant change must remain reachable")
                                } else {
                                    check(!result, "26.4+ update-only flags without an applicable change must not request age")
                                }
                            }
                        }
                    }
                }
            }
        }
        let first = PawsSignificantChange(id: "actual-change-a", introducedVersion: "1.1", description: "A reviewed change.")
        let second = PawsSignificantChange(id: "actual-change-b", introducedVersion: "1.2", description: "Another reviewed change.")
        check(PawsAccessWorkflow.pendingChanges([first], previousVersion: nil, currentVersion: "1.1", handled: []).isEmpty,
              "Fresh install must not fabricate significant-update permission")
        check(PawsAccessWorkflow.pendingChanges([first], previousVersion: "1.0", currentVersion: "1.1", handled: []) == [first],
              "Actual update must retain obligation")
        check(PawsAccessWorkflow.pendingChanges([first, second], previousVersion: "1.0", currentVersion: "1.2", handled: [first.id]) == [second],
              "Handling one change cannot approve another")
        check(PawsAccessWorkflow.pendingChanges([first], previousVersion: "1.0", currentVersion: "1.0", handled: []).isEmpty,
              "Future update cannot gate current content")
        check(PawsAccessWorkflow.matchesResponse(questionId: "q1", receivedQuestionId: "q1", changeId: "c1", expectedChangeId: "c1"),
              "Current matching response")
        for other in ["q2", "", "obsolete"] {
            check(!PawsAccessWorkflow.matchesResponse(questionId: "q1", receivedQuestionId: other, changeId: "c1", expectedChangeId: "c1"),
                  "Stale question cannot approve")
        }
        check(!PawsAccessWorkflow.matchesResponse(questionId: "q1", receivedQuestionId: "q1", changeId: "c1", expectedChangeId: "c2"),
              "Old change cannot approve new update")
        check(!PawsAccessWorkflow.matchesResponse(questionId: nil, receivedQuestionId: "q1", changeId: "c1", expectedChangeId: "c1"),
              "Missing checkpoint is not approval")
        check(!PawsAccessWorkflow.matchesResponse(questionId: "q1", receivedQuestionId: "q1", changeId: "c1", expectedChangeId: "c1",
                  responseEpoch: 1, currentEpoch: 2), "Response from old account epoch cannot approve new account")
        check(PawsAccessWorkflow.matchesResponse(questionId: "q1", receivedQuestionId: "q1", changeId: "c1", expectedChangeId: "c1",
                  responseEpoch: 2, currentEpoch: 2), "Matching response can resolve current account workflow")
        // A relaunch has no transient approval; the app-created pending identity still matches a current OS response.
        check(PawsAccessWorkflow.pendingChanges([first], previousVersion: "1.0", currentVersion: "1.1", handled: []).count == 1,
              "Restart cannot reuse a persisted approval")
        check(PawsAccessWorkflow.pendingChanges([first], previousVersion: "1.0", currentVersion: "1.1", handled: [first.id]).isEmpty,
              "Current session handles an approved change without repeated request")
        var workflow = PawsPermissionWorkflow()
        workflow.begin(questionId: "parent-q", changeId: first.id)
        check(workflow.handled.isEmpty, "Sending a question does not approve")
        check(workflow.receive(questionId: "other-q", changeId: first.id, approved: true, responseEpoch: 0) == .ignored,
              "Unrelated approval is ignored")
        check(workflow.receive(questionId: "parent-q", changeId: first.id, approved: false, responseEpoch: 0) == .denied,
              "Matching denial protects access")
        var relaunched = PawsPermissionWorkflow()
        relaunched.restore(questionId: workflow.pendingQuestionId, changeId: workflow.pendingChangeId)
        check(relaunched.handled.isEmpty, "Restart does not restore approval")
        check(relaunched.receive(questionId: "parent-q", changeId: first.id, approved: true, responseEpoch: 0) == .approved,
              "Current matching native response resolves restored pending question")
        check(relaunched.handled == [first.id], "Only the approved change is handled")
        relaunched.suspend()
        check(relaunched.handled == [first.id], "Ordinary background preserves handled change without repeated prompt")
        check(PawsAccessWorkflow.pendingChanges([first], previousVersion: "1.0", currentVersion: "1.1", handled: relaunched.handled).isEmpty,
              "Approved background-resume does not present the update again")
        workflow.identityChanged()
        check(workflow.pendingQuestionId == nil && workflow.pendingChangeId == nil, "Account change invalidates old pending identity")
        check(workflow.receive(questionId: "parent-q", changeId: first.id, approved: true, responseEpoch: 0) == .ignored,
              "Old-account approval cannot open the new account")
        workflow.begin(questionId: "new-q", changeId: second.id)
        check(workflow.receive(questionId: "new-q", changeId: second.id, approved: true, responseEpoch: workflow.epoch) == .approved,
              "Fresh new-account workflow can recover")
        var lifetime = PawsAccessLifetime()
        check(lifetime.begin(session: 1, request: 1), "First managed service begins native generation")
        check(lifetime.accepts(session: 1, request: 1), "Live callback accepted")
        check(lifetime.dispose(session: 1), "Dispose invalidates native generation")
        check(!lifetime.accepts(session: 1, request: 1), "Disposed async work cannot complete")
        check(lifetime.begin(session: 2, request: 1), "New service may restart request numbering")
        check(!lifetime.accepts(session: 1, request: 1), "Old service request1 cannot open new service request1")
        check(!lifetime.dispose(session: 1), "Late old dispose cannot cancel new service")
        check(lifetime.accepts(session: 2, request: 1), "New service remains active")
        check(!lifetime.begin(session: 1, request: 99), "Old queued native refresh cannot reset generation")
        check(lifetime.begin(session: 2, request: 2), "Same service can supersede request")
        check(!lifetime.accepts(session: 2, request: 1), "Old request cannot resolve new checkpoint")
        check(lifetime.accepts(session: 2, request: 2), "Current request can resolve")
        check(!lifetime.begin(session: 2, request: 1), "Late same-session task cannot rewind request2 to request1")
        check(lifetime.accepts(session: 2, request: 2), "Rejected rewind does not mutate active correlation")
        check(lifetime.begin(session: 2, request: 2), "Equal current queued continuation remains valid")
        let firstOptional = PawsProtectionPhase.none.startingRequest
        check(firstOptional == .inFlight, "Request writes neutral InFlight before native data arrives")
        check(!firstOptional.protectsEarlierWorkflow, "Interrupted optional first request is not known ineligibility")
        check(firstOptional.evaluated(allowed: true) == .none, "Optional Unknown clears new workflow marker")
        let protective = firstOptional.evaluated(allowed: false)
        check(protective == .awaitingResolution, "Completed unresolved protective workflow survives restart")
        check(protective.startingRequest.protectsEarlierWorkflow, "Refresh retains prior protection")
        check(protective.startingRequest.evaluated(allowed: false) == .awaitingResolution, "Unknown cannot erase prior protection")
        check(protective.evaluated(allowed: true) == .none, "Fresh reliable correction clears protection")
        // Production writes the neutral phase immediately when native receives a
        // reliable ineligible result, before C# Poll/Tick/Resolve can run.
        let receivedKnownRestriction = firstOptional.observingProtection(required: true)
        let restartedBeforeManagedResolve = PawsProtectionPhase(rawValue: receivedKnownRestriction.rawValue)!
        check(restartedBeforeManagedResolve.protectsEarlierWorkflow,
              "Native known restriction survives suspend/restart before managed result consumption")
        check(restartedBeforeManagedResolve.startingRequest.protectsEarlierWorkflow,
              "A new check cannot overwrite received protection with InFlight")
        check(firstOptional.observingProtection(required: false) == .inFlight,
              "Fresh optional error or decline does not become durable protective knowledge")
        let actualMandatoryObligation = PawsProtectionPhase.none.observingProtection(required: true)
        check(actualMandatoryObligation.startingRequest == .awaitingResolution,
              "Known mandatory obligation survives termination before any response reaches managed polling")
        print("Native access workflow checks: \(count) passed")
    }
}

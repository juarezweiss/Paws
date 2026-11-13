using System.Collections.Generic;
using NUnit.Framework;

namespace CatsAndArrows.Access.Tests
{
    public sealed class AccessPolicyTests
    {
        private static AccessEvidence Shared(int lower, int upper, bool eu = false) => new AccessEvidence
        {
            AgeResult = AgeCheckResult.Shared, LowerBound = lower, UpperBound = upper,
            ReliableRange = true, ReliableEuJurisdiction = eu, Permission = AccessPermission.SystemManaged
        };

        [TestCase(-1, 12, false, AccessState.AgeRestricted)]
        [TestCase(-1, 12, true, AccessState.AgeRestricted)]
        [TestCase(13, 15, false, AccessState.Allowed)]
        [TestCase(13, 15, true, AccessState.AgeRestricted)]
        [TestCase(16, 17, true, AccessState.Allowed)]
        [TestCase(16, 17, false, AccessState.Allowed)]
        [TestCase(18, -1, true, AccessState.Allowed)]
        [TestCase(12, 17, false, AccessState.Allowed)]
        [TestCase(13, 18, true, AccessState.Allowed)]
        [TestCase(-1, -1, false, AccessState.Allowed)]
        public void CoarseBoundsDoNotInventAgeOrJurisdiction(int lower, int upper, bool eu, AccessState state)
        {
            Assert.That(AccessPolicy.Evaluate(Shared(lower, upper, eu)), Is.EqualTo(state));
        }

        [TestCase(AgeCheckResult.NotRequired)]
        [TestCase(AgeCheckResult.Declined)]
        [TestCase(AgeCheckResult.Unavailable)]
        [TestCase(AgeCheckResult.Failed)]
        public void UnknownOptionalAgeAllowsWithoutBecomingAdult(AgeCheckResult result)
        {
            var evidence = new AccessEvidence { AgeResult = result, LowerBound = -1, UpperBound = -1 };
            Assert.That(evidence.HasValidRange, Is.False);
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.Allowed));
            evidence.RequiresRange = true;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.RecheckRequired));
            evidence.RequiresRange = false; evidence.HadProtectionCheckpoint = true;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.RecheckRequired));
        }

        [TestCase(AccessPermission.Pending, AccessState.PermissionPending)]
        [TestCase(AccessPermission.Denied, AccessState.PermissionDenied)]
        [TestCase(AccessPermission.Revoked, AccessState.PermissionDenied)]
        [TestCase(AccessPermission.Approved, AccessState.Allowed)]
        [TestCase(AccessPermission.SystemManaged, AccessState.Allowed)]
        public void OnlyActualRequiredPermissionOutcomesRestrict(AccessPermission permission, AccessState state)
        {
            var evidence = Shared(16, 17); evidence.Permission = permission;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(state));
        }

        [TestCase(AccessPermission.SystemManaged)]
        [TestCase(AccessPermission.NotRequired)]
        [TestCase(AccessPermission.Pending)]
        public void InitialConsentIsNotApprovalForRequiredUpdate(AccessPermission permission)
        {
            var evidence = Shared(16, 17); evidence.RequiresUpdatePermission = true; evidence.Permission = permission;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.PermissionPending));
        }

        [Test]
        public void AdultAcknowledgementDoesNotBecomeParentPermission()
        {
            var report = new NativeAccessReport { result = 1, lower = 18, reliable = true, adultAcknowledgementRequired = true };
            Assert.That(AccessPolicy.Evaluate(report.Evidence), Is.EqualTo(AccessState.UpdateAcknowledgementRequired));
            report.adultAcknowledgementRequired = false;
            Assert.That(AccessPolicy.Evaluate(report.Evidence), Is.EqualTo(AccessState.Allowed));
        }
        [Test]
        public void ParentCannotOverrideMinimum()
        {
            var evidence = Shared(-1, 12); evidence.Permission = AccessPermission.Approved;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.AgeRestricted));
        }
        [Test]
        public void ReliableSupersedingRangeCorrectsRestriction()
        {
            var evidence = Shared(16, 17, true); evidence.HadProtectionCheckpoint = true;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.Allowed));
            evidence.LowerBound = 13;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.RecheckRequired));
            evidence.ReliableRange = false;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.RecheckRequired));
        }
        [TestCase(17, 13)]
        [TestCase(-5, 12)]
        [TestCase(-1, -1)]
        public void InvalidRangeCannotResolveMandatoryCheck(int lower, int upper)
        {
            var evidence = Shared(lower, upper); evidence.RequiresRange = true;
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.RecheckRequired));
        }
        [Test]
        public void AppleReportDoesNotInferEuResidenceOrInitialApproval()
        {
            var evidence = new NativeAccessReport { result = 1, lower = 13, upper = 15, reliable = true }.Evidence;
            Assert.That(evidence.ReliableEuJurisdiction, Is.False);
            Assert.That(evidence.Permission, Is.EqualTo(AccessPermission.SystemManaged));
            Assert.That(AccessPolicy.Evaluate(evidence), Is.EqualTo(AccessState.Allowed));
        }
        [Test]
        public void UnrecognizedNativePermissionDoesNotGrantApproval()
        {
            Assert.That(AccessPolicy.Evaluate(new NativeAccessReport { permission = 999 }.Evidence),
                Is.EqualTo(AccessState.PermissionPending));
        }
    }

    public sealed class NativeAccessServiceTests
    {
        private sealed class Bridge : INativeAccessBridge
        {
            public long Id;
            public int Refreshes, Settings, Disposals, Resolutions;
            public bool LastUserInitiated, LastAllowed;
            public Queue<NativeAccessReport> Reports = new Queue<NativeAccessReport>();
            public void Refresh(long requestId, bool userInitiated) { Id = requestId; Refreshes++; LastUserInitiated = userInitiated; }
            public NativeAccessReport Poll() => Reports.Count == 0 ? null : Reports.Dequeue();
            public void Resolve(long id, bool allowed) { Resolutions++; LastAllowed = allowed; }
            public void OpenSystemSettings() { Settings++; }
            public void Dispose() { Disposals++; }
            public void Result(int result = 0, int lower = -1, int upper = -1, bool reliable = false)
                => Reports.Enqueue(new NativeAccessReport { requestId = Id, result = result, lower = lower, upper = upper, reliable = reliable });
        }
        [Test]
        public void CheckingHoldsEntryThenOptionalUnknownAllows()
        {
            var bridge = new Bridge(); using (var service = new NativeAccessService(bridge))
            {
                service.Refresh(); Assert.That(service.IsChecking, Is.True); Assert.That(service.CanPlay, Is.False);
                bridge.Result(2); service.Tick(); Assert.That(service.CanPlay, Is.True); Assert.That(bridge.LastAllowed, Is.True);
            }
        }
        [Test]
        public void SuspendAndSupersededCallbacksCannotReopenBoard()
        {
            var bridge = new Bridge(); using (var service = new NativeAccessService(bridge))
            {
                service.Refresh(); bridge.Result(); service.Suspend(); service.Tick();
                Assert.That(service.CanPlay, Is.False);
                service.Refresh(); service.Tick(); Assert.That(service.CanPlay, Is.False);
                bridge.Result(1, -1, 12, true); service.Tick();
                Assert.That(service.State, Is.EqualTo(AccessState.AgeRestricted));
                service.Refresh(true); bridge.Result(1, 18, -1, true); service.Tick();
                Assert.That(service.CanPlay, Is.True); Assert.That(bridge.LastUserInitiated, Is.True);
            }
        }
        [Test]
        public void NativePermissionResponseRequiresFreshAgeCheck()
        {
            var bridge = new Bridge(); using (var service = new NativeAccessService(bridge))
            {
                service.Refresh(); bridge.Reports.Enqueue(new NativeAccessReport { requestId = bridge.Id, refresh = true });
                service.Tick(); Assert.That(bridge.Refreshes, Is.EqualTo(2)); Assert.That(service.CanPlay, Is.False);
                bridge.Result(1, 16, 17, true); service.Tick(); Assert.That(service.CanPlay, Is.True);
            }
        }
        [Test]
        public void DisposeStopsCallsAndRecoveryDoesNotGrantAccess()
        {
            var bridge = new Bridge(); var service = new NativeAccessService(bridge);
            service.OpenSystemSettings(); Assert.That(service.CanPlay, Is.False); Assert.That(bridge.Settings, Is.EqualTo(1));
            service.Dispose(); service.Dispose(); service.Refresh(true); service.OpenSystemSettings(); service.Tick();
            Assert.That(bridge.Refreshes, Is.Zero); Assert.That(bridge.Settings, Is.EqualTo(1)); Assert.That(bridge.Disposals, Is.EqualTo(1));
        }
        [Test]
        public void UnsupportedFactoryDoesNotRequestAge()
        {
            using (var service = AccessServiceFactory.Create())
            {
                service.Refresh(); service.Tick(); Assert.That(service.CanPlay, Is.True); Assert.That(service.State, Is.EqualTo(AccessState.Allowed));
            }
        }
    }
}

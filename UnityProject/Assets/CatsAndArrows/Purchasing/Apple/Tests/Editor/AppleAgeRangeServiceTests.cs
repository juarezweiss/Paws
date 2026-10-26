using System;
using CatsAndArrows.Monetization;
using NUnit.Framework;

namespace CatsAndArrows.Purchasing.Apple.Tests
{
    public sealed class AppleAgeRangeServiceTests
    {
        private sealed class Client : IAppleAgeRangeClient
        {
            public Action<AppleAgeRequirement> Check;
            public Action<AppleAgeRangeResult> Range;
            public int Checks, Requests;
            public bool ThrowCheck, ThrowRequest, Disposed;
            public void CheckRequirements(Action<AppleAgeRequirement> complete)
            { Checks++; Check = complete; if (ThrowCheck) throw new InvalidOperationException(); }
            public void RequestAgeRange(Action<AppleAgeRangeResult> complete)
            { Requests++; Range = complete; if (ThrowRequest) throw new InvalidOperationException(); }
            public void Dispose() { Disposed = true; }
        }
        [TestCase(AppleAgeRequirement.Unavailable)]
        [TestCase(AppleAgeRequirement.Optional)]
        public void OrdinaryLaunchNeverAsksAgeWithoutRegionalRequirement(AppleAgeRequirement requirement)
        {
            var client = new Client(); using var age = new AppleAgeRangeService(client);
            age.SetPresentationAllowed(true); age.Start(); age.Start(); client.Check(requirement); age.Tick();
            Assert.That(client.Checks, Is.EqualTo(1)); Assert.That(client.Requests, Is.Zero);
            Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
            Assert.That(age.IsNativeFormPresenting, Is.False);
            Assert.That(age.CanRequestAgeRange, Is.EqualTo(requirement == AppleAgeRequirement.Optional));
        }
        [Test]
        public void RequiredRegionalRequestDefersUntilSafeMenuAndKeepsPreflightNonblocking()
        {
            var client = new Client(); using var age = new AppleAgeRangeService(client);
            age.Start(); Assert.That(age.IsNativeFormPresenting, Is.False);
            client.Check(AppleAgeRequirement.Required); age.Tick(); Assert.That(client.Requests, Is.Zero);
            age.SetPresentationAllowed(true); age.Tick();
            Assert.That(client.Requests, Is.EqualTo(1)); Assert.That(age.IsNativeFormPresenting, Is.True);
            Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
        }
        [Test]
        public void OptionalNativeSharingRequiresExplicitSafeSettingsIntent()
        {
            var client = new Client(); using var age = new AppleAgeRangeService(client);
            age.Start(); client.Check(AppleAgeRequirement.Optional); age.RequestAgeRange();
            Assert.That(client.Requests, Is.Zero);
            age.SetPresentationAllowed(true); age.RequestAgeRange(); age.RequestAgeRange();
            Assert.That(client.Requests, Is.EqualTo(1));
            client.Range(AppleAgeRangeResult.ConfirmedAdult);
            Assert.That(age.Current, Is.EqualTo(AudienceBand.Adult));
            age.Tick(); Assert.That(client.Requests, Is.EqualTo(1));
        }
        [TestCase(AppleAgeRangeResult.Unknown, AudienceBand.Unknown)]
        [TestCase(AppleAgeRangeResult.Under13, AudienceBand.Under13)]
        [TestCase(AppleAgeRangeResult.Teen, AudienceBand.Teen)]
        [TestCase(AppleAgeRangeResult.ConfirmedAdult, AudienceBand.Adult)]
        [TestCase((AppleAgeRangeResult)99, AudienceBand.Unknown)]
        public void OnlyConfirmedAdultSignalEnablesAdultPolicy(AppleAgeRangeResult result, AudienceBand expected)
        {
            var client = new Client(); using var age = new AppleAgeRangeService(client);
            age.SetPresentationAllowed(true); age.Start(); client.Check(AppleAgeRequirement.Required);
            var callback = client.Range; callback(result); callback(AppleAgeRangeResult.ConfirmedAdult);
            Assert.That(age.Current, Is.EqualTo(expected)); Assert.That(age.IsNativeFormPresenting, Is.False);
        }
        [Test]
        public void PreflightTimeoutAndLateResponseCannotShowSheetOrGrantAdult()
        {
            double time = 0; var client = new Client(); using var age = new AppleAgeRangeService(client, () => time);
            age.SetPresentationAllowed(true); age.Start(); var late = client.Check;
            time = 31; age.Tick(); late(AppleAgeRequirement.Required);
            Assert.That(client.Requests, Is.Zero); Assert.That(age.CanRequestAgeRange, Is.False);
            Assert.That(age.IsNativeFormPresenting, Is.False); Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
        }
        [Test]
        public void ActiveNativeSheetDoesNotTimeoutUnlockOrDoubleRequest()
        {
            double time = 0; var client = new Client(); using var age = new AppleAgeRangeService(client, () => time);
            age.SetPresentationAllowed(true); age.Start(); client.Check(AppleAgeRequirement.Required);
            time = 300; age.Tick(); age.RequestAgeRange();
            Assert.That(age.IsNativeFormPresenting, Is.True); Assert.That(client.Requests, Is.EqualTo(1));
            client.Range(AppleAgeRangeResult.Unknown); Assert.That(age.IsNativeFormPresenting, Is.False);
        }
        [Test]
        public void RealBackgroundInvalidatesAdultWithoutAutomaticallyNaggingOptionalRegion()
        {
            var client = new Client(); using var age = new AppleAgeRangeService(client);
            age.SetPresentationAllowed(true); age.Start(); client.Check(AppleAgeRequirement.Optional);
            age.RequestAgeRange(); client.Range(AppleAgeRangeResult.ConfirmedAdult);
            age.SetApplicationPaused(true); Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
            Assert.That(age.CanRequestAgeRange, Is.False);
            age.SetApplicationPaused(false); client.Check(AppleAgeRequirement.Optional); age.Tick();
            Assert.That(client.Requests, Is.EqualTo(1)); Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
        }
        [TestCase(true)]
        [TestCase(false)]
        public void BackgroundDuringNativeSheetDiscardsStaleAdultButRetainsSurfaceGuard(bool completeInBackground)
        {
            var client = new Client(); using var age = new AppleAgeRangeService(client);
            age.SetPresentationAllowed(true); age.Start(); client.Check(AppleAgeRequirement.Required);
            var stale = client.Range; age.SetApplicationPaused(true);
            Assert.That(age.IsNativeFormPresenting, Is.True);
            if (!completeInBackground) age.SetApplicationPaused(false);
            stale(AppleAgeRangeResult.ConfirmedAdult);
            Assert.That(age.IsNativeFormPresenting, Is.False); Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
            if (completeInBackground) age.SetApplicationPaused(false);
            client.Check(AppleAgeRequirement.Optional); age.Tick();
            Assert.That(client.Requests, Is.EqualTo(1)); Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
        }
        [Test]
        public void FocusOnlyNativeSurfaceDoesNotInvalidateReturnedAdult()
        {
            var client = new Client(); using var age = new AppleAgeRangeService(client);
            age.SetPresentationAllowed(true); age.Start(); client.Check(AppleAgeRequirement.Optional); age.RequestAgeRange();
            age.SetPresentationAllowed(false); client.Range(AppleAgeRangeResult.ConfirmedAdult);
            age.SetPresentationAllowed(true); age.Tick();
            Assert.That(age.Current, Is.EqualTo(AudienceBand.Adult)); Assert.That(client.Checks, Is.EqualTo(1));
        }
        [Test]
        public void ErrorsFailClosedAndDisposalIgnoresLateCallbacks()
        {
            var client = new Client { ThrowCheck = true }; using var age = new AppleAgeRangeService(client);
            age.SetPresentationAllowed(true); age.Start();
            Assert.That(age.CanRequestAgeRange, Is.False); Assert.That(age.IsNativeFormPresenting, Is.False);
            client.ThrowCheck = false; age.Refresh(); client.Check(AppleAgeRequirement.Required);
            var late = client.Range; age.Dispose(); late(AppleAgeRangeResult.ConfirmedAdult);
            Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown)); Assert.That(client.Disposed, Is.True);
        }
        [Test]
        public void AmbiguousNativeInvocationFailureCannotUnlockPotentialSheet()
        {
            var client = new Client { ThrowRequest = true }; using var age = new AppleAgeRangeService(client);
            age.SetPresentationAllowed(true); age.Start(); client.Check(AppleAgeRequirement.Required);
            age.Tick(); Assert.That(age.IsNativeFormPresenting, Is.True);
            Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
            client.Range(AppleAgeRangeResult.Unknown); Assert.That(age.IsNativeFormPresenting, Is.False);
        }
        [Test]
        public void NewSessionNeverUsesPriorAppLocalAdultSelection()
        {
            var key = "paws.ad-audience.v1"; var had = UnityEngine.PlayerPrefs.HasKey(key);
            var old = UnityEngine.PlayerPrefs.GetInt(key);
            try
            {
                UnityEngine.PlayerPrefs.SetInt(key, (int)AudienceBand.Adult);
                using var age = new AppleAgeRangeService(new Client());
                Assert.That(age.Current, Is.EqualTo(AudienceBand.Unknown));
            }
            finally
            {
                if (had) UnityEngine.PlayerPrefs.SetInt(key, old); else UnityEngine.PlayerPrefs.DeleteKey(key);
                UnityEngine.PlayerPrefs.Save();
            }
        }
    }
}

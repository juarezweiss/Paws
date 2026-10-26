using System;
using CatsAndArrows.Monetization;

namespace CatsAndArrows.Purchasing.Apple
{
    public enum AppleAgeRequirement { Unavailable = 0, Optional = 1, Required = 2 }
    public enum AppleAgeRangeResult { Unknown = 0, Under13 = 1, Teen = 2, ConfirmedAdult = 3 }
    public interface IAppleAgeRangeClient : IDisposable
    {
        void CheckRequirements(Action<AppleAgeRequirement> complete);
        void RequestAgeRange(Action<AppleAgeRangeResult> complete);
    }
    // #67: application-thread orchestration. No DOB, legacy preference or age cache.
    public sealed class AppleAgeRangeService : IAgeAssuranceService, IDisposable
    {
        private readonly IAppleAgeRangeClient client;
        private readonly Func<double> clock;
        private bool started, disposed, checking, available, pending, presentationAllowed, paused, refreshAfterClose;
        private long generation;
        private double deadline;
        public const double PreflightTimeoutSeconds = 30;
        public AudienceBand Current { get; private set; } = AudienceBand.Unknown;
        public bool IsNativeFormPresenting { get; private set; }
        public bool CanRequestAgeRange => !disposed && available && !checking && !IsNativeFormPresenting;
        public event Action Changed;
        public AppleAgeRangeService(IAppleAgeRangeClient client, Func<double> clock = null)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.clock = clock ?? (() => System.Diagnostics.Stopwatch.GetTimestamp() /
                (double)System.Diagnostics.Stopwatch.Frequency);
        }
        public void Start() { if (started || disposed) return; started = true; Refresh(); }
        public void Refresh()
        {
            if (disposed || !started) return;
            Current = AudienceBand.Unknown; available = pending = false;
            if (IsNativeFormPresenting)
            { refreshAfterClose = true; Changed?.Invoke(); return; }
            if (paused) { checking = false; ++generation; Changed?.Invoke(); return; }
            checking = true; deadline = clock() + PreflightTimeoutSeconds;
            var request = ++generation;
            Changed?.Invoke();
            try { client.CheckRequirements(result => RequirementsChecked(request, result)); }
            catch { RequirementsChecked(request, AppleAgeRequirement.Unavailable); }
        }
        private void RequirementsChecked(long request, AppleAgeRequirement result)
        {
            if (disposed || !checking || request != generation) return;
            if (clock() >= deadline) result = AppleAgeRequirement.Unavailable;
            checking = false;
            available = result == AppleAgeRequirement.Optional || result == AppleAgeRequirement.Required;
            pending = result == AppleAgeRequirement.Required;
            Changed?.Invoke();
            TryRequest();
        }
        public void RequestAgeRange()
        {
            if (!CanRequestAgeRange || !presentationAllowed || paused) return;
            pending = true; TryRequest();
        }
        public void SetPresentationAllowed(bool allowed) { presentationAllowed = allowed; }
        public void SetApplicationPaused(bool value)
        {
            if (disposed || paused == value) return;
            paused = value;
            // A real background transition may change accounts/sharing. Focus-only
            // native sheets do not enter this path. Keep an active surface guard.
            Refresh();
        }
        private void TryRequest()
        {
            if (disposed || paused || !pending || !available || checking || IsNativeFormPresenting || !presentationAllowed) return;
            pending = false; Current = AudienceBand.Unknown;
            IsNativeFormPresenting = true;
            var request = ++generation;
            Changed?.Invoke();
            try { client.RequestAgeRange(result => RangeReceived(request, result)); }
            catch
            {
                // Once native invocation is attempted, an exception is not dismissal evidence.
                // The actual completion remains responsible for releasing the native guard.
            }
        }
        private void RangeReceived(long request, AppleAgeRangeResult result)
        {
            if (disposed || !IsNativeFormPresenting || request != generation) return;
            IsNativeFormPresenting = false;
            if (refreshAfterClose || paused)
            {
                refreshAfterClose = false; Current = AudienceBand.Unknown;
                Refresh(); return;
            }
            Current = result == AppleAgeRangeResult.ConfirmedAdult ? AudienceBand.Adult :
                result == AppleAgeRangeResult.Under13 ? AudienceBand.Under13 :
                result == AppleAgeRangeResult.Teen ? AudienceBand.Teen : AudienceBand.Unknown;
            Changed?.Invoke();
        }
        public void Tick()
        {
            if (disposed) return;
            if (checking && clock() >= deadline)
            {
                checking = available = pending = false; Current = AudienceBand.Unknown;
                ++generation; Changed?.Invoke();
            }
            TryRequest();
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; ++generation; client.Dispose(); Changed = null;
        }
    }
}

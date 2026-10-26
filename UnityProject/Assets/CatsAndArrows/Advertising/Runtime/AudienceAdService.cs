using System;
using CatsAndArrows.Monetization;

namespace CatsAndArrows.Advertising
{
    // #67: only a confirmed platform adult may construct the Google binding.
    // Existing native surfaces retain their guard until actual closure on downgrade.
    public sealed class AudienceAdService : IAdService, IAdPrivacyService, IDisposable
    {
        private readonly IAudienceSource audience;
        private readonly Func<IGoogleAdsBridge> createBridge;
        private readonly Func<bool> isOwned;
        private readonly Action<Action> dispatch;
        private readonly Func<double> clock;
        private AdMobAdService inner;
        private IGoogleAdsBridge lastBridge;
        private bool privacyOnlyPresenting;
        private long privacyOperation;
        private bool started, disposed, paused, retiring, attempted;
        public AudienceAdService(IAudienceSource audience, Func<IGoogleAdsBridge> createBridge,
            Func<bool> isOwned, Action<Action> dispatch, Func<double> clock = null)
        {
            this.audience = audience ?? throw new ArgumentNullException(nameof(audience));
            this.createBridge = createBridge ?? throw new ArgumentNullException(nameof(createBridge));
            this.isOwned = isOwned ?? throw new ArgumentNullException(nameof(isOwned));
            this.dispatch = dispatch ?? throw new ArgumentNullException(nameof(dispatch));
            this.clock = clock;
            audience.Changed += AudienceChanged;
        }
        private bool Eligible => !disposed && !privacyOnlyPresenting && audience.Current == AudienceBand.Adult &&
            (audience as IAgeAssuranceService)?.IsNativeFormPresenting != true;
        public event Action Changed;
        public bool IsPresenting => inner?.IsPresenting == true;
        public bool IsPrivacyFormPresenting => privacyOnlyPresenting || inner?.IsPrivacyFormPresenting == true ||
            (audience as IAgeAssuranceService)?.IsNativeFormPresenting == true;
        public bool PrivacyOptionsRequired => !disposed &&
            (inner?.PrivacyOptionsRequired == true || RetainedPrivacyRequired());
        private bool RetainedPrivacyRequired()
        { try { return lastBridge?.PrivacyOptionsRequired == true; } catch { return false; } }
        public void Start() { if (disposed) return; started = true; Reconcile(); }
        private void AudienceChanged() { Reconcile(); Changed?.Invoke(); }
        private void Reconcile()
        {
            if (disposed || !started || privacyOnlyPresenting) return;
            if (!Eligible && inner == null) attempted = false;
            if (!Eligible && inner != null) retiring = true;
            if (retiring && inner != null)
            {
                inner.SetPaused(true);
                if (inner.IsPresenting || inner.IsPrivacyFormPresenting) return;
                inner.Changed -= InnerChanged; inner.Dispose(); inner = null; retiring = attempted = false;
            }
            if (Eligible && inner == null && !attempted)
            {
                attempted = true;
                try
                {
                    var bridge = createBridge();
                    inner = new AdMobAdService(true, bridge, isOwned, dispatch, clock);
                    lastBridge = bridge;
                }
                catch { return; } // Infrastructure unavailable leaves direct eligible hints usable.
                inner.Changed += InnerChanged;
                inner.SetPaused(paused); inner.Start();
            }
        }
        private void InnerChanged() { Reconcile(); Changed?.Invoke(); }
        public AdAvailability GetAvailability(AdPlacement placement) => Eligible && !retiring ?
            inner?.GetAvailability(placement) ?? AdAvailability.Unavailable : AdAvailability.Unavailable;
        public void Show(long requestId, AdPlacement placement, Action<AdSignal> receive)
        {
            if (receive == null) throw new ArgumentNullException(nameof(receive));
            if (!Eligible || retiring || inner == null) receive(new AdSignal(AdSignalKind.InfrastructureUnavailable));
            else inner.Show(requestId, placement, receive);
        }
        public void Cancel(long requestId) => inner?.Cancel(requestId);
        public void ShowPrivacyOptions(Action<bool> complete)
        {
            if (complete == null) throw new ArgumentNullException(nameof(complete));
            if (disposed || IsPrivacyFormPresenting || IsPresenting || paused)
            { complete(false); return; }
            if (Eligible && !retiring && inner != null) { inner.ShowPrivacyOptions(complete); return; }
            if (!RetainedPrivacyRequired()) { complete(false); return; }
            // A previously required withdrawal entry survives age invalidation. This
            // uses only the existing UMP form: no update, initial consent or ad SDK work.
            privacyOnlyPresenting = true;
            var operation = ++privacyOperation;
            Changed?.Invoke();
            if (disposed || paused)
            {
                ++privacyOperation; privacyOnlyPresenting = false;
                complete(false); Changed?.Invoke(); return;
            }
            try
            {
                lastBridge.ShowPrivacyOptions(success => dispatch(() =>
                {
                    if (disposed || !privacyOnlyPresenting || operation != privacyOperation) return;
                    ++privacyOperation; privacyOnlyPresenting = false;
                    Reconcile(); complete(success); Changed?.Invoke();
                }));
            }
            catch { /* Invocation may have presented a native surface; wait for closure evidence. */ }
        }
        public void SetPaused(bool value)
        { paused = value; inner?.SetPaused(value || retiring || !Eligible); }
        public void Tick() { inner?.Tick(); Reconcile(); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; audience.Changed -= AudienceChanged;
            if (inner != null) { inner.Changed -= InnerChanged; inner.Dispose(); }
            Changed = null;
        }
    }
}

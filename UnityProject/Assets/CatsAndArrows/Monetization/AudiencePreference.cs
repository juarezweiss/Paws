using System;

namespace CatsAndArrows.Monetization
{
    public enum AudienceBand { Unknown = 0, Under13 = 1, Teen = 2, Adult = 3, PreferNotToAnswer = 4 }
    public interface IAudienceSource
    {
        AudienceBand Current { get; }
        event Action Changed;
    }
    // Native Apple controls are optional; ordinary play never depends on an age response.
    public interface IAgeAssuranceService : IAudienceSource
    {
        bool CanRequestAgeRange { get; }
        bool IsNativeFormPresenting { get; }
        void Start();
        void Refresh();
        void RequestAgeRange();
        void SetPresentationAllowed(bool allowed);
        void SetApplicationPaused(bool paused);
        void Tick();
    }
    // Retained only for deterministic test/review helpers. Production uses Apple signals.
    public interface IAudiencePreference : IAudienceSource
    {
        void Select(AudienceBand band);
    }
}

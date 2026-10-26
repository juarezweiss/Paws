using System;
using CatsAndArrows.Monetization;

namespace CatsAndArrows.UI
{
    // #67: the ordinary shell depends only on application ports.
    public interface IPawsMonetizationRuntime : IDisposable
    {
        MonetizationCoordinator Initialize();
        string PrivacyPolicyUrl { get; }
        string SupportUrl { get; }
        IAudienceSource Audience { get; }
        IAdPrivacyService Privacy { get; }
        void SetAgePresentationAllowed(bool allowed);
        void StartProviders();
        void Tick();
        void SetPaused(bool paused);
        void SetApplicationPaused(bool paused);
    }
}

using System;
using CatsAndArrows.Monetization;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatsAndArrows.UI
{
    public sealed partial class GameShellView
    {
        private readonly IAudienceSource audience;
        private readonly string privacyPolicyUrl, supportUrl;
        private readonly Action<string> openExternalUrl;
        public void OpenPrivacyPolicy() => OpenReleaseLink(privacyPolicyUrl);
        public void OpenSupport() => OpenReleaseLink(supportUrl);
        private void OpenReleaseLink(string url)
        {
            if (disposed || loading || CommercialBusy || Modal != null || Screen != "settings") return;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed) || parsed.Scheme != Uri.UriSchemeHttps) return;
            CancelInteraction(); ResetNotifications(); openExternalUrl(url);
        }
        // Native sharing is optional outside Apple's required regional flow.
        // No custom age question, birth date or persistent self-declared adult flag.
        public bool CanPresentAgeRange => !disposed && !loading && !CommercialBusy && Modal == null &&
            (Screen == "menu" || Screen == "settings") && applicationFocused && !applicationPaused;
        public void RequestAppleAgeRange()
        {
            if (!CanPresentAgeRange || Screen != "settings" ||
                !(audience is IAgeAssuranceService age) || !age.CanRequestAgeRange) return;
            CancelInteraction(); ResetNotifications(); age.RequestAgeRange();
        }
        private void OnAudienceChanged()
        {
            if (disposed || loading) return;
            if (Screen == "play") BuildControls();
            if (Screen == "settings" && !CommercialBusy && Modal == null) ShowSettings();
        }
    }
}

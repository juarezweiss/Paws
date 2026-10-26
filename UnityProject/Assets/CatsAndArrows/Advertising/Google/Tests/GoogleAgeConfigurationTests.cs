using System;
using CatsAndArrows.Advertising.Google;
using CatsAndArrows.Monetization;
using GoogleMobileAds.Api;
using NUnit.Framework;

namespace CatsAndArrows.Tests
{
    public sealed class GoogleAgeConfigurationTests
    {
        [TestCase(AudienceBand.Under13, AgeRestrictedTreatment.Child)]
        [TestCase(AudienceBand.Teen, AgeRestrictedTreatment.Teen)]
        [TestCase(AudienceBand.Adult, AgeRestrictedTreatment.Unspecified)]
        public void PinnedSdkSupportsReliableAgeTreatmentsAndRetainsApprovedPrivacy(AudienceBand band, AgeRestrictedTreatment treatment)
        {
            var configuration = GoogleAdsBridge.CreateRequestConfiguration(band);
            Assert.That(configuration.AgeRestrictedTreatment, Is.EqualTo(treatment));
            Assert.That(configuration.PublisherFirstPartyIdEnabled, Is.False);
            Assert.That(configuration.PublisherPrivacyPersonalizationState, Is.EqualTo(PublisherPrivacyPersonalizationState.Disabled));
            Assert.That(configuration.MaxAdContentRating.Value, Is.EqualTo(MaxAdContentRating.G.Value));
        }
        [TestCase(AudienceBand.Unknown)]
        [TestCase(AudienceBand.PreferNotToAnswer)]
        public void UnknownAgeCannotBeConfiguredAsAnAdultRequest(AudienceBand band)
        { Assert.Throws<ArgumentException>(() => GoogleAdsBridge.CreateRequestConfiguration(band)); }
    }
}

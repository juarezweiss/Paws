using System;
using System.Collections;
using CatsAndArrows.Advertising;
using CatsAndArrows.Gameplay;
using CatsAndArrows.Monetization;
using CatsAndArrows.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace CatsAndArrows.Tests
{
    public sealed class AudienceShellTests
    {
        private sealed class Preference : IAudiencePreference
        {
            public AudienceBand Current { get; private set; }
            public event Action Changed;
            public void Select(AudienceBand band) { Current = band; Changed?.Invoke(); }
        }
        private sealed class NativeAge : IAgeAssuranceService
        {
            public AudienceBand Current => AudienceBand.Unknown;
            public event Action Changed;
            public bool CanRequestAgeRange { get; set; }
            public bool IsNativeFormPresenting { get; private set; }
            public int Requests;
            public void Start() { }
            public void Refresh() { }
            public void Tick() { }
            public void SetPresentationAllowed(bool value) { }
            public void SetApplicationPaused(bool value) { }
            public void RequestAgeRange() { Requests++; IsNativeFormPresenting = true; Changed?.Invoke(); }
            public void Close() { IsNativeFormPresenting = false; Changed?.Invoke(); }
        }
        [UnityTest]
        public IEnumerator FirstLaunchKeepsMenuAndPlayWithoutCustomAgeQuestionInEveryPortrait()
        {
            foreach (var locale in new[] { "en", "pt-BR", "es" })
            foreach (var size in new[] { new Vector2(320, 568), new Vector2(390, 844), new Vector2(430, 932) })
            {
                var host = new GameObject("native age menu", typeof(UIDocument));
                var panel = ScriptableObject.CreateInstance<PanelSettings>();
                var texture = new RenderTexture((int)size.x, (int)size.y, 0);
                panel.scaleMode = PanelScaleMode.ConstantPixelSize; panel.targetTexture = texture;
                GameShellView shell = null;
                try
                {
                    var doc = host.GetComponent<UIDocument>(); doc.panelSettings = panel;
                    doc.rootVisualElement.style.width = size.x; doc.rootVisualElement.style.height = size.y;
                    var age = new NativeAge { CanRequestAgeRange = true };
                    shell = new GameShellView(14, locale, PlayTestFactory.Loop(), enableTutorial: false,
                        tutorialSeen: () => true, difficultyHistory: new MemoryDifficultyHistoryStore(), audience: age,
                        monetization: new MonetizationCoordinator(true, new MemoryMonetizationStateStore(), new UnavailableAdService(), new UnavailablePurchaseService(), audience: age));
                    doc.rootVisualElement.Add(shell);
                    yield return null; yield return null;
                    shell.ApplySafeArea(new Rect(0, 0, size.x, size.y), size);
                    Assert.That(shell.Screen, Is.EqualTo("menu")); Assert.That(shell.Modal, Is.Null);
                    Assert.That(age.Requests, Is.Zero);
                    Assert.That(shell.Q("audience-selector"), Is.Null);
                    Assert.That(shell.Q<Label>("menu-content-disclosure").text, Does.Contain("A12"));
                    var play = shell.Q<Button>("menu-start");
                    Assert.That(play.enabledInHierarchy, Is.True);
                    Assert.That(play.resolvedStyle.height, Is.GreaterThanOrEqualTo(48));
                    Assert.That(shell.StartNewGame(), Is.True);
                    Assert.That(shell.Screen, Is.EqualTo("play")); Assert.That(age.Requests, Is.Zero);
                    shell.ShowSettings();
                    Assert.That(shell.Q<Button>("settings-age-range"), Is.Null);
                    var native = shell.Q<Button>("settings-apple-age-range");
                    Assert.That(native, Is.Not.Null);
                    yield return null;
                    Assert.That(native.resolvedStyle.minHeight.value, Is.GreaterThanOrEqualTo(48));
                    shell.RequestAppleAgeRange();
                    Assert.That(age.Requests, Is.EqualTo(1)); Assert.That(shell.Modal, Is.Null);
                    Assert.That(shell.CanPresentAgeRange, Is.False);
                    Assert.That(shell.StartNewGame(), Is.False);
                    shell.OpenPrivacyPolicy(); shell.RequestAppleAgeRange();
                    Assert.That(age.Requests, Is.EqualTo(1));
                    age.Close(); shell.ShowMenu();
                    Assert.That(shell.CanPresentAgeRange, Is.True);
                    Assert.That(shell.Q<Button>("menu-start").enabledInHierarchy, Is.True);
                }
                finally
                {
                    shell?.DisposeMonetization(); UnityEngine.Object.DestroyImmediate(host);
                    UnityEngine.Object.DestroyImmediate(panel); UnityEngine.Object.DestroyImmediate(texture);
                }
            }
        }
        [Test]
        public void UnsupportedAppleAgeSharingAddsNoDeadSettingsActionOrGameBlock()
        {
            var age = new NativeAge();
            var shell = new GameShellView(14, "en", PlayTestFactory.Loop(), enableTutorial: false,
                tutorialSeen: () => true, difficultyHistory: new MemoryDifficultyHistoryStore(), audience: age);
            try
            {
                shell.ShowSettings();
                Assert.That(shell.Q<Button>("settings-apple-age-range"), Is.Null);
                shell.RequestAppleAgeRange(); Assert.That(age.Requests, Is.Zero);
                Assert.That(shell.StartNewGame(), Is.True);
            }
            finally { shell.DisposeMonetization(); }
        }
        [Test]
        public void AdultWithUnavailableAdsKeepsFreeHintsWithoutInventingOwnership()
        {
            var preference = new Preference(); preference.Select(AudienceBand.Adult);
            using var coordinator = new MonetizationCoordinator(true, new MemoryMonetizationStateStore(),
                new UnavailableAdService(), new UnavailablePurchaseService(), audience: preference);
            var shell = new GameShellView(14, "en", PlayTestFactory.Loop(), enableTutorial: false, tutorialSeen: () => true,
                difficultyHistory: new MemoryDifficultyHistoryStore(), monetization: coordinator, audience: preference);
            try
            {
                Assert.That(shell.StartNewGame(), Is.True);
                shell.RequestHint();
                Assert.That(shell.ActiveSession.MarkRevision, Is.GreaterThan(0));
                Assert.That(coordinator.IsOwned, Is.False);
                Assert.That(coordinator.Snapshot.Access, Is.EqualTo(AccessState.Unknown));
            }
            finally { shell.DisposeMonetization(); }
        }
        [TestCase(AudienceBand.Unknown)]
        [TestCase(AudienceBand.Under13)]
        [TestCase(AudienceBand.Teen)]
        [TestCase(AudienceBand.PreferNotToAnswer)]
        public void RestrictedAudienceKeepsFreeEligibleHintsAndPublicLinksRequireExplicitTap(AudienceBand band)
        {
            var preference = new Preference(); preference.Select(band);
            var constructions = 0;
            using var ads = new AudienceAdService(preference, () => { constructions++; throw new InvalidOperationException(); }, () => false, action => action());
            using var coordinator = new MonetizationCoordinator(true, new MemoryMonetizationStateStore(), ads, new UnavailablePurchaseService(), audience: preference);
            ads.Start();
            string opened = null;
            var shell = new GameShellView(14, "en", PlayTestFactory.Loop(), enableTutorial: false, tutorialSeen: () => true,
                difficultyHistory: new MemoryDifficultyHistoryStore(), monetization: coordinator, adPrivacy: ads, audience: preference,
                privacyPolicyUrl: "https://example.com/privacy", supportUrl: "https://example.com/support", openExternalUrl: url => opened = url);
            try
            {
                Assert.That(shell.StartNewGame(), Is.True);
                Assert.That(shell.Q<Button>("action-hint").text, Is.EqualTo("Hint"));
                var revision = shell.ActiveSession.MarkRevision;
                shell.RequestHint(); Assert.That(shell.ActiveSession.MarkRevision, Is.GreaterThan(revision));
                Assert.That(constructions, Is.Zero);
                Assert.That(coordinator.Snapshot.Access, Is.EqualTo(AccessState.Unknown));
                Assert.That(coordinator.IsOwned, Is.False);
                shell.ShowSettings();
                Assert.That(shell.Q<Button>("settings-remove-ads"), Is.Not.Null);
                Assert.That(shell.Q<Button>("settings-restore"), Is.Not.Null);
                Assert.That(shell.Q<Button>("settings-privacy-policy"), Is.Not.Null);
                Assert.That(shell.Q<Button>("settings-support"), Is.Not.Null);
                Assert.That(opened, Is.Null);
                shell.OpenPrivacyPolicy(); Assert.That(opened, Is.EqualTo("https://example.com/privacy"));
                shell.OpenSupport();
                Assert.That(opened, Is.EqualTo("https://example.com/support"));
            }
            finally { shell.DisposeMonetization(); }
        }
    }
}

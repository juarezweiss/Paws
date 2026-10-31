using System;
using System.Collections;
using System.Linq;
using CatsAndArrows.Access;
using CatsAndArrows.Core.Localization;
using CatsAndArrows.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace CatsAndArrows.Tests
{
    // #103: enforce the access boundary through actual shell commands, not just hidden buttons.
    public sealed class AccessShellTests
    {
        private static GameShellView Shell(FakeAccess access, string locale = "en") =>
            GameShellView.WithAccess(loop: PlayTestFactory.Loop(), locale: locale,
                tutorialSeen: () => true, markTutorialSeen: () => { }, access: access);

        [TestCase(AccessState.Checking)]
        [TestCase(AccessState.AgeRestricted)]
        [TestCase(AccessState.PermissionPending)]
        [TestCase(AccessState.PermissionDenied)]
        [TestCase(AccessState.RecheckRequired)]
        [TestCase(AccessState.UpdateAcknowledgementRequired)]
        public void WithheldAccessHasNoGameplayEntryButKeepsInformationAndRecovery(AccessState state)
        {
            var access = new FakeAccess(state);
            var shell = Shell(access);
            try
            {
                Assert.That(shell.Screen, Is.EqualTo("access"));
                Assert.That(shell.StartNewGame(), Is.False);
                shell.StartPlaying(); shell.StartTutorial(); shell.ContinueTutorial(); shell.SkipTutorial();
                shell.BackToGame(); shell.RequestHint(); shell.ClearBoard();
                Assert.That(shell.ActiveSession, Is.Null);
                Assert.That(shell.BoardView.InteractionEnabled, Is.False);
                Assert.That(shell.Q<Button>("access-information"), Is.Not.Null);
                Assert.That(shell.Q<Button>("access-system-settings"), Is.Not.Null);
                shell.ShowSettings();
                Assert.That(shell.Q<Button>("settings-tutorial").enabledSelf, Is.False);
                shell.ShowAccessInformation();
                Assert.That(shell.Screen, Is.EqualTo("access-information"));
                Assert.That(shell.Modal, Is.EqualTo("access-information"));
                shell.ShowMenu();
                Assert.That(shell.Screen, Is.EqualTo("access"));
            }
            finally { shell.DisposeMonetization(); }
        }

        [Test]
        public void RestrictionCancelsLivePlayAndCommandsWithoutDiscardingProgressAndRecoveryResumes()
        {
            var access = new FakeAccess(); var shell = Shell(access);
            try
            {
                Assert.That(shell.StartNewGame(), Is.True);
                shell.RequestHint();
                var session = shell.ActiveSession;
                var marks = session.Marks.ToArray();
                var starts = shell.NewGameRequestId;
                access.Set(AccessState.PermissionPending);
                Assert.That(shell.Screen, Is.EqualTo("access"));
                Assert.That(shell.BoardView.InteractionEnabled, Is.False);
                shell.StartPlaying(); shell.StartNewGame(); shell.StartTutorial(); shell.ContinueTutorial();
                shell.SkipTutorial(); shell.ClearBoard(); shell.RequestHint(); shell.BackToGame();
                shell.SetApplicationFocus(false); shell.SetApplicationPaused(true);
                shell.SetApplicationPaused(false); shell.SetApplicationFocus(true);
                Assert.That(shell.ActiveSession, Is.SameAs(session));
                Assert.That(session.Marks, Is.EquivalentTo(marks));
                Assert.That(shell.NewGameRequestId, Is.EqualTo(starts));
                Assert.That(shell.BoardView.InteractionEnabled, Is.False);
                access.Set(AccessState.Allowed);
                Assert.That(shell.Screen, Is.EqualTo("play"));
                Assert.That(shell.ActiveSession, Is.SameAs(session));
                Assert.That(shell.BoardView.InteractionEnabled, Is.EqualTo(!session.IsCompleted));
            }
            finally { shell.DisposeMonetization(); }
        }

        [Test]
        public void EligiblePlayRetainsFreeImmediateHintsAndCompletedBoardLock()
        {
            var access = new FakeAccess(); var shell = Shell(access);
            try
            {
                Assert.That(shell.Screen, Is.EqualTo("menu"));
                Assert.That(shell.Q<Button>("access-recheck"), Is.Null);
                Assert.That(shell.StartNewGame(), Is.True);
                var session = shell.ActiveSession;
                for (var count = 0; !session.IsCompleted && count < 100; count++) shell.RequestHint();
                Assert.That(session.IsCompleted, Is.True);
                var marks = session.Marks.ToArray();
                access.Set(AccessState.RecheckRequired); access.Set(AccessState.Allowed);
                shell.ClearBoard(); shell.RequestHint();
                Assert.That(session.Marks, Is.EquivalentTo(marks));
                Assert.That(shell.BoardView.InteractionEnabled, Is.False);
                Assert.That(access.Checks, Is.Zero, "Ordinary shell commands must not request age information.");
            }
            finally { shell.DisposeMonetization(); }
        }

        [Test]
        public void ExplicitSettingsAndMenuNavigationDiscardAutomaticResumeIntent()
        {
            var access = new FakeAccess(); var shell = Shell(access);
            try
            {
                Assert.That(shell.StartNewGame(), Is.True);
                var session = shell.ActiveSession;
                access.Set(AccessState.Checking);
                shell.ShowSettings();
                access.Set(AccessState.Allowed);
                shell.ShowMenu();
                access.Set(AccessState.Checking); access.Set(AccessState.Allowed);
                Assert.That(shell.Screen, Is.EqualTo("menu"));
                Assert.That(shell.ActiveSession, Is.SameAs(session));
                Assert.That(shell.BoardView.InteractionEnabled, Is.False);
            }
            finally { shell.DisposeMonetization(); }
        }

        [Test]
        public void DisposedShellIgnoresLaterAccessCallbacks()
        {
            var access = new FakeAccess(); var shell = Shell(access);
            shell.DisposeMonetization();
            access.Set(AccessState.PermissionDenied);
            Assert.That(shell.Screen, Is.EqualTo("menu"));
            Assert.That(access.Subscribers, Is.Zero);
        }

        [TestCase("en")]
        [TestCase("pt-BR")]
        [TestCase("es")]
        public void EveryAccessOutcomeAndInformationIsLocalized(string locale)
        {
            var catalog = GameLocalization.CreateCatalog();
            var text = new LocalizationService(catalog); text.SelectLanguage(locale);
            foreach (AccessState state in Enum.GetValues(typeof(AccessState)))
                Assert.That(text.GetText("access.status." + state), Does.Not.Contain("access.status."));
            var help = text.GetText("access.information-body");
            Assert.That(help, Does.Contain("16").And.Contain("13").And.Contain("18"));
            Assert.That(help, Does.Not.Contain("#91"));
        }

        [UnityTest]
        public IEnumerator RecoveryActionsRemainReachableAcrossLocalizedPortraits()
        {
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ConstantPixelSize; settings.scale = 1;
            var host = new GameObject("issue103-access-layout", typeof(UIDocument));
            var document = host.GetComponent<UIDocument>(); document.panelSettings = settings;
            var root = document.rootVisualElement;
            RenderTexture texture = null;
            try
            {
                foreach (var size in new[] { new Vector2(320, 568), new Vector2(393, 852), new Vector2(430, 932) })
                foreach (var locale in new[] { "en", "pt-BR", "es" })
                {
                    if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                    texture = new RenderTexture((int)size.x, (int)size.y, 0);
                    settings.targetTexture = texture;
                    root.style.width = size.x; root.style.height = size.y;
                    var shell = Shell(new FakeAccess(AccessState.AgeRestricted), locale);
                    root.Add(shell);
                    yield return null; yield return null;
                    shell.ApplySafeArea(new Rect(0, 0, size.x, size.y), size);
                    yield return null;
                    var scroll = shell.Q<ScrollView>("access-scroll");
                    var content = shell.Q<VisualElement>("access-content");
                    Assert.That(float.IsNaN(content.layout.width), Is.False);
                    Assert.That(content.layout.width, Is.LessThanOrEqualTo(size.x + 1));
                    foreach (var name in new[] { "access-recheck", "access-system-settings", "access-information", "access-settings" })
                    {
                        var button = shell.Q<Button>(name);
                        Assert.That(button.layout.height, Is.GreaterThanOrEqualTo(48), locale + ": " + name);
                        Assert.That(button.layout.width, Is.GreaterThan(0));
                        scroll.ScrollTo(button);
                        yield return null;
                        Assert.That(button.worldBound.Overlaps(scroll.contentViewport.worldBound), Is.True, locale + ": " + name);
                    }
                    shell.DisposeMonetization(); shell.RemoveFromHierarchy();
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(settings);
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private sealed class FakeAccess : IAccessService
        {
            private Action changed;
            public AccessState State { get; private set; }
            public bool CanPlay => State == AccessState.Allowed;
            public bool IsChecking => State == AccessState.Checking;
            public int Checks { get; private set; }
            public int Subscribers => changed?.GetInvocationList().Length ?? 0;
            public event Action Changed { add => changed += value; remove => changed -= value; }
            public FakeAccess(AccessState state = AccessState.Allowed) { State = state; }
            public void Set(AccessState state) { State = state; changed?.Invoke(); }
            public void Refresh(bool userInitiated = false) { Checks++; }
            public void Suspend() { }
            public void Tick() { }
            public void OpenSystemSettings() { }
            public void Dispose() { }
        }
    }
}

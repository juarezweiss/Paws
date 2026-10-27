using System.Collections;
using System.Linq;
using CatsAndArrows.Gameplay;
using CatsAndArrows.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace CatsAndArrows.Tests
{
    // #95: shared Hint attention mechanism, deterministic step timing and real mounted controls.
    public sealed class TutorialAttentionShellTests
    {
        private GameObject host;
        private PanelSettings settings;
        private RenderTexture texture;
        private VisualElement root;
        private GameShellView view;
        private double now;
        private bool hadLanguage;
        private string previousLanguage;
        private Button Advance => view.Q<Button>("action-continue");
        private Label Guidance => view.Q<Label>("tutorial-continue-guidance");

        [SetUp]
        public void PreserveLanguage()
        {
            hadLanguage = PlayerPrefs.HasKey(LanguagePreference.Key);
            previousLanguage = PlayerPrefs.GetString(LanguagePreference.Key);
        }
        [TearDown]
        public void Cleanup()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (settings != null) Object.DestroyImmediate(settings);
            if (texture != null) Object.DestroyImmediate(texture);
            if (hadLanguage) PlayerPrefs.SetString(LanguagePreference.Key, previousLanguage);
            else PlayerPrefs.DeleteKey(LanguagePreference.Key);
            PlayerPrefs.Save();
        }
        private IEnumerator Mount(string locale = "en")
        {
            now = 0;
            settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ConstantPixelSize; settings.scale = 1;
            texture = new RenderTexture(390, 844, 0); settings.targetTexture = texture;
            host = new GameObject("issue95-tutorial-attention", typeof(UIDocument));
            var document = host.GetComponent<UIDocument>(); document.panelSettings = settings;
            root = document.rootVisualElement;
            root.style.width = 390; root.style.height = 844;
            view = new GameShellView(14, locale, PlayTestFactory.Loop(), tutorialSeen: () => true,
                markTutorialSeen: () => { }, idleClock: () => now);
            root.Add(view); view.StartTutorial();
            yield return Settle();
            view.TickTutorialAttention();
        }
        private static IEnumerator Settle() { yield return null; yield return null; }
        private void At(double time) { now = time; view.TickTutorialAttention(); }
        private void AssertAttention(bool expected, Button button = null, Label guidance = null)
        {
            button = button ?? Advance; guidance = guidance ?? Guidance;
            Assert.That(button.style.backgroundColor.value, Is.EqualTo(expected ? BoardSymbolElement.Ink : PortraitBoardView.Tile));
            Assert.That(button.style.color.value, Is.EqualTo(expected ? PortraitBoardView.Tile : BoardSymbolElement.Ink));
            Assert.That(button.style.borderLeftWidth.value, Is.EqualTo(1));
            Assert.That(button.style.unityFontStyleAndWeight.value, Is.EqualTo(FontStyle.Normal));
            Assert.That(button.enabledInHierarchy, Is.True);
            Assert.That(button.style.transitionProperty.value.Select(p => p.ToString()), Is.EqualTo(new[] { "background-color", "color" }));
            Assert.That(button.style.transitionDuration.value?.FirstOrDefault().value ?? 0, Is.EqualTo(expected ? .2f : 0));
            Assert.That(guidance.style.opacity.value, Is.EqualTo(expected ? 1 : 0));
            Assert.That(guidance.style.transitionProperty.value.Select(p => p.ToString()), Is.EqualTo(new[] { "opacity" }));
            Assert.That(guidance.style.transitionDuration.value.First().value, Is.EqualTo(expected ? .2f : 0));
            Assert.That(guidance.pickingMode, Is.EqualTo(PickingMode.Ignore));
            Assert.That(guidance.focusable, Is.False);
        }
        private void ClickAdvance()
        {
            var button = Advance; var position = button.worldBound.center;
            Assert.That(button.enabledInHierarchy, Is.True);
            var picked = view.panel.Pick(position);
            Assert.That(picked == button || (picked != null && button.Contains(picked)), Is.True, "Mounted panel picks the always-clickable action.");
            using (var down = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, mousePosition = position }))
            { down.target = button; button.SendEvent(down); }
            using (var up = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 0, mousePosition = position }))
            { up.target = button; button.SendEvent(up); }
        }

        [UnityTest]
        public IEnumerator EveryInformationalStepCanBeClickedBeforeItsDelay()
        {
            yield return Mount();
            for (var index = 0; index <= (int)TutorialStep.Arrows; index++)
            {
                Assert.That(view.Tutorial.Step, Is.EqualTo((TutorialStep)index));
                AssertAttention(false);
                var start = now; At(start + 1.999); AssertAttention(false);
                var oldButton = Advance; var oldGuidance = Guidance;
                ClickAdvance(); yield return Settle();
                Assert.That(view.Tutorial.Step, Is.EqualTo((TutorialStep)(index + 1)));
                Assert.That(oldButton.style.backgroundColor.value, Is.EqualTo(PortraitBoardView.Tile));
                Assert.That(oldGuidance.style.opacity.value, Is.Zero);
            }
            Assert.That(view.Tutorial.Step, Is.EqualTo(TutorialStep.Practice));
            Assert.That(Advance, Is.Null); Assert.That(Guidance, Is.Null);
        }

        [UnityTest]
        public IEnumerator ExactTwoSecondBoundaryRestartsOnEveryStepAndSharesHintsFade()
        {
            yield return Mount();
            var marks = view.ActiveSession.Marks.ToArray();
            for (var index = 0; index <= (int)TutorialStep.Arrows; index++)
            {
                var start = now;
                At(start + 1.999); AssertAttention(false);
                At(start + 2); AssertAttention(true);
                var grid = view.BoardView.Q<VisualElement>("board-grid").worldBound;
                yield return Settle();
                Assert.That(Guidance.worldBound.center.y,
                    Is.EqualTo((grid.yMax + Advance.worldBound.yMin) / 2).Within(.75f));
                Assert.That(view.BoardView.Q<VisualElement>("board-grid").worldBound, Is.EqualTo(grid));
                Assert.That(view.ActiveSession.Marks, Is.EquivalentTo(marks));
                At(start + 20); AssertAttention(true);
                ClickAdvance(); yield return Settle();
                if (view.Tutorial.CanContinue) AssertAttention(false);
            }
        }

        [UnityTest]
        public IEnumerator FadeTargetsCancelAcrossLifecycleAndStartUsesItsLocalizedButtonInstruction()
        {
            yield return Mount("pt-BR");
            Assert.That(Guidance.resolvedStyle.opacity, Is.Zero);
            At(2); AssertAttention(true);
            yield return Settle();
            // EditMode's enumerator can advance real time without pumping native
            // panel interpolation (the original .25s assertion observed target1,
            // resolved0). TutorialReviewChecks proves actual intermediate opacity,
            // interruption and the completed fade in the real mounted player.
            // Here retain deterministic timer/style targets and resolved cleanup.
            view.SetApplicationFocus(false); AssertAttention(false);
            yield return Settle();
            Assert.That(Guidance.resolvedStyle.opacity, Is.Zero);
            At(100);
            yield return new WaitForSecondsRealtime(.25f);
            yield return Settle();
            AssertAttention(false);
            Assert.That(Guidance.resolvedStyle.opacity, Is.Zero, "Cancelled native fade cannot reappear after its old window.");
            view.SetApplicationFocus(true);
            At(101.999); AssertAttention(false);
            At(102); AssertAttention(true);
            yield return Settle();
            view.SetApplicationPaused(true); AssertAttention(false);
            yield return Settle();
            Assert.That(Guidance.resolvedStyle.opacity, Is.Zero);
            At(200); AssertAttention(false);
            view.SetApplicationPaused(false);
            At(201.999); AssertAttention(false);
            At(202); AssertAttention(true);
            view.ContinueTutorial(); view.ContinueTutorial(); yield return Settle();
            Assert.That(view.Tutorial.Step, Is.EqualTo(TutorialStep.Begin));
            Assert.That(Advance.text, Is.EqualTo("Vamos começar"));
            Assert.That(Guidance.text, Is.EqualTo("Toque em Vamos começar."));
            AssertAttention(false);
            Assert.That(Guidance.resolvedStyle.opacity, Is.Zero);
        }

        [UnityTest]
        public IEnumerator FocusPauseNavigationLocaleAndReplayCancelAndStartFreshDelays()
        {
            yield return Mount();
            At(2); AssertAttention(true);
            view.SetApplicationFocus(false); AssertAttention(false);
            At(100); AssertAttention(false);
            view.SetApplicationFocus(true); At(101.999); AssertAttention(false); At(102); AssertAttention(true);
            view.SetApplicationPaused(true); AssertAttention(false);
            At(200); AssertAttention(false);
            view.SetApplicationPaused(false); At(201.999); AssertAttention(false); At(202); AssertAttention(true);
            var oldButton = Advance; var oldGuidance = Guidance;
            view.ShowMenu(); AssertAttention(false, oldButton, oldGuidance);
            At(300); AssertAttention(false, oldButton, oldGuidance);
            view.ShowSettings(); view.ShowLanguageSelector(); At(400);
            AssertAttention(false, oldButton, oldGuidance);
            view.SelectLanguage("es"); view.BackToGame(); yield return Settle();
            Assert.That(Advance, Is.Not.SameAs(oldButton)); AssertAttention(false);
            Assert.That(Guidance.text, Is.EqualTo("Toca Continuar."));
            At(401.999); AssertAttention(false); At(402); AssertAttention(true);
            AssertAttention(false, oldButton, oldGuidance);
            oldButton = Advance; oldGuidance = Guidance;
            view.StartTutorial(); yield return Settle();
            Assert.That(view.Tutorial.Step, Is.EqualTo(TutorialStep.Welcome));
            AssertAttention(false); AssertAttention(false, oldButton, oldGuidance);
            At(403.999); AssertAttention(false); At(404); AssertAttention(true);
        }

        [UnityTest]
        public IEnumerator RebindDetachSkipAndDisposeCannotLeaveStaleGuidance()
        {
            yield return Mount();
            At(2); AssertAttention(true);
            view.BoardView.Bind(view.ActiveSession); yield return Settle();
            AssertAttention(false); At(3.999); AssertAttention(false); At(4); AssertAttention(true);
            view.RemoveFromHierarchy();
            Assert.That(Guidance.style.opacity.value, Is.Zero);
            At(100); Assert.That(Guidance.style.opacity.value, Is.Zero);
            root.Add(view); yield return Settle();
            At(101.999); AssertAttention(false); At(102); AssertAttention(true);
            var oldButton = Advance; var oldGuidance = Guidance;
            view.SkipTutorial(); At(200);
            Assert.That(oldButton.style.backgroundColor.value, Is.EqualTo(PortraitBoardView.Tile));
            Assert.That(oldGuidance.style.opacity.value, Is.Zero);
            Assert.That(view.Tutorial, Is.Null);
            view.StartNewGame(); yield return Settle();
            Assert.That(Guidance, Is.Null); Assert.That(Advance, Is.Null);
            view.StartTutorial(); yield return Settle();
            At(202); AssertAttention(true);
            view.DisposeMonetization(); AssertAttention(false);
            At(300); AssertAttention(false);
        }
    }
}

using System;
using CatsAndArrows.Access;
using UnityEngine;
using UnityEngine.UIElements;

namespace CatsAndArrows.UI
{
    // Issue #63: ordinary game composition; no diagnostic arguments or providers.
    [RequireComponent(typeof(UIDocument))]
    public sealed class PawsBootstrap : MonoBehaviour
    {
        private GameShellView shell;
        private bool focused = true;
        private bool paused;
        private bool active;
        private IAccessService access;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.style.position = Position.Absolute;
            root.style.left = root.style.right = root.style.top = root.style.bottom = 0;
            root.style.flexGrow = 1;
            access = AccessServiceFactory.Create();
            shell = GameShellView.WithAccess(access, unchecked((int)DateTime.UtcNow.Ticks ^ Environment.TickCount),
                useInlineHints: true, enableTutorial: true, monetization: null);
            root.Add(shell);
            shell.SetApplicationFocus(focused);
            shell.SetApplicationPaused(paused);
            SyncAccessLifecycle();
        }

        private void Update()
        {
            access?.Tick();
            shell?.UpdateSafeArea();
        }
        private void OnApplicationFocus(bool value)
        {
            focused = value;
            shell?.SetApplicationFocus(value);
            SyncAccessLifecycle();
        }
        private void OnApplicationPause(bool value)
        {
            paused = value;
            shell?.SetApplicationPaused(value);
            SyncAccessLifecycle();
        }
        private void SyncAccessLifecycle()
        {
            if (access == null) return;
            // A system permission sheet can change focus without backgrounding.
            // Do not invalidate/resend its own native request on that focus transition.
            var next = !paused;
            if (next == active) return;
            active = next;
            if (active) access.Refresh(); else access.Suspend();
        }
        private void OnDestroy()
        {
            shell?.DisposeMonetization();
            access?.Dispose();
            access = null;
        }
    }
}

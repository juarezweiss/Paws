using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CatsAndArrows.Access
{
    public interface IAccessService : IDisposable
    {
        bool CanPlay { get; }
        bool IsChecking { get; }
        AccessState State { get; }
        event Action Changed;
        void Refresh(bool userInitiated = false);
        void Suspend();
        void Tick();
        void OpenSystemSettings();
    }

    // The native payload contains transient coarse evidence, not an Apple response object.
    [Serializable]
    public sealed class NativeAccessReport
    {
        public long requestId;
        public int result, lower = -1, upper = -1, permission;
        public bool reliable, required, checkpoint, updateRequired, refresh, adultAcknowledgementRequired;
        public AccessEvidence Evidence => new AccessEvidence
        {
            AgeResult = Enum.IsDefined(typeof(AgeCheckResult), result) ? (AgeCheckResult)result : AgeCheckResult.Failed,
            LowerBound = lower, UpperBound = upper, ReliableRange = reliable, RequiresRange = required,
            HadProtectionCheckpoint = checkpoint, RequiresUpdatePermission = updateRequired,
            RequiresAdultAcknowledgement = adultAcknowledgementRequired,
            Permission = Enum.IsDefined(typeof(AccessPermission), permission) ? (AccessPermission)permission : AccessPermission.Pending,
            // Apple does not expose residence; never infer EU jurisdiction from its regulatory flag.
            ReliableEuJurisdiction = false
        };
    }

    public interface INativeAccessBridge : IDisposable
    {
        void Refresh(long requestId, bool userInitiated);
        NativeAccessReport Poll();
        void Resolve(long requestId, bool allowed);
        void OpenSystemSettings();
    }

    public sealed class NativeAccessService : IAccessService
    {
        private readonly INativeAccessBridge bridge;
        private long requestId;
        private bool suspended, disposed;
        public AccessState State { get; private set; } = AccessState.Checking;
        public bool CanPlay => !disposed && !suspended && State == AccessState.Allowed;
        public bool IsChecking => State == AccessState.Checking;
        public event Action Changed;

        public NativeAccessService(INativeAccessBridge bridge) { this.bridge = bridge ?? throw new ArgumentNullException(nameof(bridge)); }
        public void Refresh(bool userInitiated = false)
        {
            if (disposed) return;
            suspended = false;
            SetState(AccessState.Checking);
            bridge.Refresh(++requestId, userInitiated);
        }
        public void Suspend()
        {
            if (disposed || suspended) return;
            suspended = true;
            ++requestId; // Ignore callbacks from the check that preceded suspension.
            Changed?.Invoke();
        }
        public void Tick()
        {
            if (disposed || suspended) return;
            var report = bridge.Poll();
            if (report == null || report.requestId != requestId) return;
            if (report.refresh) { Refresh(); return; }
            var state = AccessPolicy.Evaluate(report.Evidence);
            bridge.Resolve(requestId, state == AccessState.Allowed);
            SetState(state);
        }
        public void OpenSystemSettings() { if (!disposed) bridge.OpenSystemSettings(); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; ++requestId; bridge.Dispose(); Changed = null;
        }
        private void SetState(AccessState state)
        {
            if (State == state) return;
            State = state; Changed?.Invoke();
        }
    }

    public sealed class UnrestrictedAccessService : IAccessService
    {
        public bool CanPlay => true;
        public bool IsChecking => false;
        public AccessState State => AccessState.Allowed;
        public event Action Changed { add { } remove { } }
        public void Refresh(bool userInitiated = false) { }
        public void Suspend() { }
        public void Tick() { }
        public void OpenSystemSettings() { }
        public void Dispose() { }
    }

    public static class AccessServiceFactory
    {
        public static IAccessService Create()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return new NativeAccessService(new AppleAccessBridge());
#else
            return new UnrestrictedAccessService();
#endif
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    internal sealed class AppleAccessBridge : INativeAccessBridge
    {
        [DllImport("__Internal")] private static extern long PawsAccessCreateSession();
        [DllImport("__Internal")] private static extern void PawsAccessRefresh(long session, long id, int userInitiated);
        [DllImport("__Internal")] private static extern IntPtr PawsAccessPoll(long session);
        [DllImport("__Internal")] private static extern void PawsAccessFree(IntPtr value);
        [DllImport("__Internal")] private static extern void PawsAccessResolve(long session, long id, int allowed);
        [DllImport("__Internal")] private static extern void PawsAccessOpenSettings(long session);
        [DllImport("__Internal")] private static extern void PawsAccessDispose(long session);
        private readonly long session = PawsAccessCreateSession();
        public void Refresh(long requestId, bool userInitiated) => PawsAccessRefresh(session, requestId, userInitiated ? 1 : 0);
        public NativeAccessReport Poll()
        {
            var value = PawsAccessPoll(session);
            if (value == IntPtr.Zero) return null;
            try { return JsonUtility.FromJson<NativeAccessReport>(Marshal.PtrToStringAnsi(value)); }
            finally { PawsAccessFree(value); }
        }
        public void Resolve(long requestId, bool allowed) => PawsAccessResolve(session, requestId, allowed ? 1 : 0);
        public void OpenSystemSettings() => PawsAccessOpenSettings(session);
        public void Dispose() => PawsAccessDispose(session);
    }
#endif
}

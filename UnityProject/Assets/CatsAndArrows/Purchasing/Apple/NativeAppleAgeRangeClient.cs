#if UNITY_EDITOR || PAWS_MONETIZATION
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;

namespace CatsAndArrows.Purchasing.Apple
{
    public sealed class NativeAppleAgeRangeClient : IAppleAgeRangeClient
    {
        private delegate void Receive(long request, int result);
        private static readonly Receive Receiver = OnReceive;
        private static readonly object Gate = new object();
        private static readonly Dictionary<long, Action<int>> Pending = new Dictionary<long, Action<int>>();
        private static long nextRequest;
        private readonly Action<Action> dispatch;
        private long check, range;
        private bool disposed;
        public NativeAppleAgeRangeClient(Action<Action> dispatch)
        { this.dispatch = dispatch ?? throw new ArgumentNullException(nameof(dispatch)); }
#if UNITY_IOS && !UNITY_EDITOR && PAWS_MONETIZATION
        [DllImport("__Internal")] private static extern void PawsAgeRangeCheck(long request, Receive receive);
        [DllImport("__Internal")] private static extern void PawsAgeRangeRequest(long request, Receive receive);
#endif
        public void CheckRequirements(Action<AppleAgeRequirement> complete)
        {
            if (complete == null) throw new ArgumentNullException(nameof(complete));
            if (disposed) { complete(AppleAgeRequirement.Unavailable); return; }
#if UNITY_IOS && !UNITY_EDITOR && PAWS_MONETIZATION
            lock (Gate) Pending.Remove(check);
            check = Register(result => complete(result == 1 ? AppleAgeRequirement.Optional :
                result == 2 ? AppleAgeRequirement.Required : AppleAgeRequirement.Unavailable));
            try { PawsAgeRangeCheck(check, Receiver); }
            catch (DllNotFoundException) { OnReceive(check, 0); }
            catch (EntryPointNotFoundException) { OnReceive(check, 0); }
#else
            complete(AppleAgeRequirement.Unavailable);
#endif
        }
        public void RequestAgeRange(Action<AppleAgeRangeResult> complete)
        {
            if (complete == null) throw new ArgumentNullException(nameof(complete));
            if (disposed) { complete(AppleAgeRangeResult.Unknown); return; }
#if UNITY_IOS && !UNITY_EDITOR && PAWS_MONETIZATION
            range = Register(result => complete(result >= 1 && result <= 3 ? (AppleAgeRangeResult)result : AppleAgeRangeResult.Unknown));
            try { PawsAgeRangeRequest(range, Receiver); }
            catch (DllNotFoundException) { OnReceive(range, 0); }
            catch (EntryPointNotFoundException) { OnReceive(range, 0); }
#else
            complete(AppleAgeRangeResult.Unknown);
#endif
        }
        private long Register(Action<int> complete)
        {
            var request = Interlocked.Increment(ref nextRequest);
            lock (Gate) Pending.Add(request, result => dispatch(() => { if (!disposed) complete(result); }));
            return request;
        }
        [MonoPInvokeCallback(typeof(Receive))]
        private static void OnReceive(long request, int result)
        {
            Action<int> callback;
            lock (Gate)
            {
                if (!Pending.TryGetValue(request, out callback)) return;
                Pending.Remove(request);
            }
            callback(result);
        }
        public void Dispose()
        {
            disposed = true;
            lock (Gate) { Pending.Remove(check); Pending.Remove(range); }
        }
    }
}
#endif

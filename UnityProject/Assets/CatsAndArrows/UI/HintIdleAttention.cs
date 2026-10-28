using System;

namespace CatsAndArrows.UI
{
    // Issue #48: presentation timing only; never requests or applies a Hint.
    public sealed class HintIdleAttention
    {
        public const double IdleSeconds = 6;
        private readonly Func<double> clock;
        private readonly double delaySeconds;
        private bool eligible;
        private bool interacting;
        private double attentionAt;
        public bool IsAttentionVisible { get; private set; }

        public HintIdleAttention(Func<double> clock, double delaySeconds = IdleSeconds)
        {
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            if (!double.IsFinite(delaySeconds) || delaySeconds < 0) throw new ArgumentOutOfRangeException(nameof(delaySeconds));
            this.delaySeconds = delaySeconds;
        }

        public void Update(bool isEligible, bool hasActiveInteraction)
        {
            if (eligible != isEligible || interacting != hasActiveInteraction)
            {
                eligible = isEligible;
                interacting = hasActiveInteraction;
                Reset();
            }
            if (!eligible || interacting) { IsAttentionVisible = false; return; }
            if (clock() >= attentionAt) IsAttentionVisible = true;
        }

        public void Reset()
        {
            attentionAt = clock() + delaySeconds;
            IsAttentionVisible = false;
        }
    }
}

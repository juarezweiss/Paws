namespace CatsAndArrows.Access
{
    // #103: age-data sharing, access permission and adulthood are separate decisions.
    public enum AccessState { Allowed, Checking, AgeRestricted, PermissionPending, PermissionDenied, RecheckRequired, UpdateAcknowledgementRequired }
    public enum AccessPermission { SystemManaged, NotRequired, Pending, Approved, Denied, Revoked }
    public enum AgeCheckResult { NotRequired, Shared, Declined, Unavailable, Failed }

    public struct AccessEvidence
    {
        public AgeCheckResult AgeResult;
        public int LowerBound, UpperBound;
        public bool ReliableRange, RequiresRange, ReliableEuJurisdiction, HadProtectionCheckpoint;
        public AccessPermission Permission;
        public bool RequiresUpdatePermission, RequiresAdultAcknowledgement;

        public bool HasValidRange => AgeResult == AgeCheckResult.Shared && ReliableRange &&
            LowerBound >= -1 && UpperBound >= -1 && (LowerBound >= 0 || UpperBound >= 0) &&
            (LowerBound < 0 || UpperBound < 0 || LowerBound <= UpperBound);
    }

    public static class AccessPolicy
    {
        public static AccessState Evaluate(AccessEvidence evidence)
        {
            if (evidence.Permission == AccessPermission.Denied || evidence.Permission == AccessPermission.Revoked)
                return AccessState.PermissionDenied;
            var minimum = evidence.ReliableEuJurisdiction ? 16 : 13;
            if (evidence.HasValidRange && evidence.UpperBound >= 0 && evidence.UpperBound < minimum)
                return AccessState.AgeRestricted;
            if (evidence.RequiresAdultAcknowledgement)
                return AccessState.UpdateAcknowledgementRequired;
            if (evidence.Permission == AccessPermission.Pending ||
                (evidence.RequiresUpdatePermission && evidence.Permission != AccessPermission.Approved))
                return AccessState.PermissionPending;
            if (evidence.RequiresRange && !evidence.HasValidRange)
                return AccessState.RecheckRequired;
            // A failed/declined refresh must not discard a previous protective workflow.
            if (evidence.HadProtectionCheckpoint && (!evidence.HasValidRange || evidence.LowerBound < minimum))
                return AccessState.RecheckRequired;
            return AccessState.Allowed;
        }
    }
}

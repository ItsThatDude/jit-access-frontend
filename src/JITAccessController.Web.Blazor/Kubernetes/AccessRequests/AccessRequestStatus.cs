namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessRequestStatus
{
    public string State { get; set; } = "";

    public DateTime RequestExpiresAt { get; set; }

    public string ResolvedPolicy { get; set; } = "";

    public int ApprovalsRequired { get; set; }

    public int ApprovalsReceived { get; set; }

    public IEnumerable<AccessRequestApproval> Approvals { get; set; } = Array.Empty<AccessRequestApproval>();
}
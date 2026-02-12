namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessRequestStatus
{
    public string State { get; set; } = "";

    public DateTime RequestExpiresAt { get; set; }

    public string ResolvedPolicy { get; set; } = "";
}
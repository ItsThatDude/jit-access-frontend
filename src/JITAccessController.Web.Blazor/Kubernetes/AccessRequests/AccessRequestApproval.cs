namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessRequestApproval
{
    public string Approver { get; set; } = "";

    public DateTime ApprovedAt { get; set; }
}
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessPolicySpec
{
    public List<Rbacv1Subject> Approvers { get; set; } = new();
}
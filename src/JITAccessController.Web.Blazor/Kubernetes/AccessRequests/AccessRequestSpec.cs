using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessRequestSpec
{
    public string Subject { get; set; } = "";

    public Rbacv1Subject? Role { get; set; }

    public List<V1PolicyRule> Permissions { get; set; } = new();
}
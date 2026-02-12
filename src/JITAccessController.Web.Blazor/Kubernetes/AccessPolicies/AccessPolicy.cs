using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

[KubernetesEntity(
    Kind = "AccessPolicy",
    Group = "access.antware.xyz",
    ApiVersion = "v1alpha1",
    PluralName = "accesspolicies"
)]
public class AccessPolicy : BaseAccessPolicy
{
}
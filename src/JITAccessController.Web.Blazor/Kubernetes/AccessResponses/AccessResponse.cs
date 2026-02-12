using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

[KubernetesEntity(
    Kind = "AccessResponse",
    Group = "access.antware.xyz",
    ApiVersion = "v1alpha1",
    PluralName = "accesspresonses"
)]
public class AccessResponse : BaseAccessResponse
{
}
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

[KubernetesEntity(
    Kind = "AccessRequest",
    Group = "access.antware.xyz",
    ApiVersion = "v1alpha1",
    PluralName = "accessrequests"
)]
public class AccessRequest : BaseAccessRequest
{
}
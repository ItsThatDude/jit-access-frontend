using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

[KubernetesEntity(
    Kind = "ClusterAccessRequest",
    Group = "access.antware.xyz",
    ApiVersion = "v1alpha1",
    PluralName = "clusteraccessrequests"
)]
public class ClusterAccessRequest : BaseAccessRequest
{
}
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

[KubernetesEntity(
    Kind = "ClusterAccessPolicy",
    Group = "access.antware.xyz",
    ApiVersion = "v1alpha1",
    PluralName = "clusteraccesspolicies"
)]
public class ClusterAccessPolicy : BaseAccessPolicy
{
}
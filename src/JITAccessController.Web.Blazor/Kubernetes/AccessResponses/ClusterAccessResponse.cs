using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

[KubernetesEntity(
    Kind = "ClusterAccessResponse",
    Group = "access.antware.xyz",
    ApiVersion = "v1alpha1",
    PluralName = "clusteraccessresponses"
)]
public class ClusterAccessResponse : CustomResource<AccessResponseSpec, AccessResponseStatus>
{
}
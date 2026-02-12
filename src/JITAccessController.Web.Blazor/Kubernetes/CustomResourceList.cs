using k8s;
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class CustomResourceList<T> : KubernetesObject
where T : CustomResource
{
    public V1ListMeta Metadata { get; set; } = new();
    public List<T> Items { get; set; } = new();
}
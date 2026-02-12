using System.Text.Json.Serialization;
using k8s;
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public abstract class CustomResource : KubernetesObject, IMetadata<V1ObjectMeta>
{
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = new();
}

public abstract class CustomResource<TSpec, TStatus> : CustomResource
{
    [JsonPropertyName("spec")]
    public TSpec? Spec { get; set; }

    [JsonPropertyName("status")]
    public TStatus? Status { get; set; }
}
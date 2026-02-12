using System.Text.Json;
using k8s;

namespace JITAccessController.Web.Blazor.Kubernetes;

public static class CustomResourceMapper
{
    public static T? FromObject<T>(object obj) where T : CustomResource
    {
        var json = KubernetesJson.Serialize(obj);
        return KubernetesJson.Deserialize<T>(json);
    }
}

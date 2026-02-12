using k8s;
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class CustomResourceClient<T> : GenericClient where T : IKubernetesObject
{
    public static KubernetesEntityAttribute Metadata = typeof(T).GetKubernetesTypeMetadata();

    public CustomResourceClient(IKubernetes kubernetes, bool disposeClient = true)
     : base(kubernetes, Metadata.Group, Metadata.ApiVersion, Metadata.PluralName, disposeClient)
    {
    }
}
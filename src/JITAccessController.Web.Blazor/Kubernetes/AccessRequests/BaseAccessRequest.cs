using k8s;
using k8s.Autorest;
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public abstract class BaseAccessRequest : CustomResource<AccessRequestSpec, AccessRequestStatus>
{
    private AccessResponse? BuildResponse(string username, List<string> groups, string response) {
        if(string.IsNullOrWhiteSpace(username))
            return null;
        
        string ns = Metadata.NamespaceProperty;

        KubernetesEntityAttribute? metadata;

        if (string.IsNullOrWhiteSpace(ns)) {
            metadata = typeof(ClusterAccessResponse).GetKubernetesTypeMetadata();
        } else {
            metadata = typeof(AccessResponse).GetKubernetesTypeMetadata();
        }

        if(metadata == null) {
            return null;
        }

        string apiVersion = string.Format("{0}/{1}", metadata.Group, metadata.ApiVersion);
        string kind = metadata.Kind;

        var cr = new AccessResponse() {
            Kind = kind,
            ApiVersion = apiVersion,
            Metadata = new V1ObjectMeta {
                GenerateName = "response-",
                NamespaceProperty = ns
            },
            Spec = new AccessResponseSpec {
                RequestRef = this.Name(),
                Response = response,
                Approver = username,
                Groups = groups
            }
        };

        return cr;
    }

    public async Task CreateResponseAsync(IKubernetes client, string username, List<string> groups, string response)
    {
        var requestResponse = BuildResponse(username, groups, response);
        var ns = requestResponse.Namespace();

        KubernetesEntityAttribute? metadata;

        if (string.IsNullOrWhiteSpace(ns)) {
            metadata = typeof(ClusterAccessResponse).GetKubernetesTypeMetadata();
        } else {
            metadata = typeof(AccessResponse).GetKubernetesTypeMetadata();
        }

        if(metadata == null) {
            return;
        }

        if (string.IsNullOrWhiteSpace(requestResponse.Namespace())) {
            var clientResponse = await client.CustomObjects.CreateClusterCustomObjectWithHttpMessagesAsync(
                requestResponse, 
                metadata.Group, 
                metadata.ApiVersion,
                metadata.PluralName);
        }
        else {
            var clientResponse = await client.CustomObjects.CreateNamespacedCustomObjectWithHttpMessagesAsync(
                requestResponse, 
                metadata.Group, 
                metadata.ApiVersion, 
                requestResponse.Namespace(),
                metadata.PluralName);
        }
    }
}
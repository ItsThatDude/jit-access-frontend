using k8s;
using k8s.Autorest;
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public abstract class BaseAccessRequest : CustomResource<AccessRequestSpec, AccessRequestStatus>
{
    public string Scope => string.IsNullOrWhiteSpace(Metadata.NamespaceProperty) ? "Cluster" : Metadata.NamespaceProperty;

    private AccessResponse? BuildResponse(string username, IEnumerable<string> groups, string response) {
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
                Groups = groups.ToList()
            }
        };

        return cr;
    }

    public async Task CreateResponseAsync(IKubernetes client, string username, IEnumerable<string> groups, string response, bool useImpersonation = false)
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

        var headers = new Dictionary<string, IReadOnlyList<string>>();

        if (useImpersonation) {
            headers.Add("Impersonate-User", new List<string>([username]));
            headers.Add("Impersonate-Group", groups.ToList());
        }

        if (string.IsNullOrWhiteSpace(requestResponse.Namespace())) {
            var clientResponse = await client.CustomObjects.CreateClusterCustomObjectWithHttpMessagesAsync(
                requestResponse, 
                metadata.Group, 
                metadata.ApiVersion,
                metadata.PluralName,
                customHeaders: headers);
        }
        else {
            var clientResponse = await client.CustomObjects.CreateNamespacedCustomObjectWithHttpMessagesAsync(
                requestResponse, 
                metadata.Group, 
                metadata.ApiVersion, 
                requestResponse.Namespace(),
                metadata.PluralName,
                customHeaders: headers);
        }
    }

    public bool CanApprove(string username, IEnumerable<string> groups, IEnumerable<BaseAccessPolicy> policies)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        if (username == Spec?.Subject)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Status?.ResolvedPolicy))
        {
            var policy = policies.FirstOrDefault(p => p.Metadata.Name == Status?.ResolvedPolicy);

            if (policy != null)
            {
                if (policy.Spec != null)
                {
                    if (policy.Spec.Approvers.Any(s => s.Kind == "User" && s.Name == username))
                    {
                        return true;
                    }

                    if (groups != null)
                    {
                        if (policy.Spec.Approvers.Any(s => s.Kind == "Group" && groups.Any(g => "oidc:"+g == s.Name)))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}
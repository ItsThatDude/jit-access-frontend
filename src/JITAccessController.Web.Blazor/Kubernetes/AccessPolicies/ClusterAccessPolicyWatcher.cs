using k8s;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class ClusterAccessPolicyWatcher : CustomResourceWatcher<ClusterAccessPolicy, ClusterAccessPolicyStore>
{
    public ClusterAccessPolicyWatcher(
        IKubernetes client,
        ClusterAccessPolicyStore store,
        ILogger<ClusterAccessPolicyWatcher> logger)
        : base(client, store, logger)
    {
    }
}
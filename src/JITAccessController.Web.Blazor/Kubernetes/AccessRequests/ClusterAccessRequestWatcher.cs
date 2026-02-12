using k8s;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class ClusterAccessRequestWatcher : CustomResourceWatcher<ClusterAccessRequest, ClusterAccessRequestStore>
{
    public ClusterAccessRequestWatcher(
        IKubernetes client,
        ClusterAccessRequestStore store,
        ILogger<ClusterAccessRequestWatcher> logger)
        : base(client, store, logger)
    {
    }
}
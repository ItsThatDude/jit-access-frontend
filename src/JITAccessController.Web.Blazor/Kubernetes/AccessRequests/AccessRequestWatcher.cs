using k8s;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessRequestWatcher : CustomResourceWatcher<AccessRequest, AccessRequestStore>
{
    public AccessRequestWatcher(
        IKubernetes client,
        AccessRequestStore store,
        ILogger<AccessRequestWatcher> logger)
        : base(client, store, logger)
    {
    }
}
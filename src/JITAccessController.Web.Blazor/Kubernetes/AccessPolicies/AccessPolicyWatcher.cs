using k8s;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class AccessPolicyWatcher : CustomResourceWatcher<AccessPolicy, AccessPolicyStore>
{
    public AccessPolicyWatcher(
        IKubernetes client,
        AccessPolicyStore store,
        ILogger<AccessPolicyWatcher> logger)
        : base(client, store, logger)
    {
    }
}
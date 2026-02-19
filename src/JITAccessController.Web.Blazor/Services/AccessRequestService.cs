using System;
using System.Threading.Tasks;
using JITAccessController.Web.Blazor.Kubernetes;
using k8s;

namespace JITAccessController.Web.Blazor.Services
{
    public interface IAccessRequestService
    {
        Task SendResponseAsync(BaseAccessRequest request, string response, IKubernetes k8sClient, string username, IEnumerable<string> groups, bool useImpersonation);
    }

    public class AccessRequestService : IAccessRequestService
    {
        public async Task SendResponseAsync(BaseAccessRequest request, string response, IKubernetes k8sClient, string username, IEnumerable<string> groups, bool useImpersonation)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username is required", nameof(username));

            try
            {
                await request.CreateResponseAsync(k8sClient, username, groups, response, useImpersonation: useImpersonation);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

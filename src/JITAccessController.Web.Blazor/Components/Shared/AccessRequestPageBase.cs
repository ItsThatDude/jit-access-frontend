using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace JITAccessController.Web.Blazor.Components.Shared
{
    public class AccessRequestPageBase : ComponentBase
    {
        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        protected AuthenticationState? authState;
        protected double? _offsetMinutes;

        protected override async Task OnInitializedAsync()
        {
            authState = await AuthStateProvider.GetAuthenticationStateAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    _offsetMinutes = await JS.InvokeAsync<double>("getLocalOffsetMinutes");
                    await InvokeAsync(StateHasChanged);
                }
                catch
                {
                }
            }
        }

        protected bool IsAuthenticated()
        {
            return authState != null && authState.User.Identity != null;
        }

        protected string? GetUsername()
        {
            return authState?.User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
        }

        protected List<string> GetGroups()
        {
            if (authState == null)
                return new List<string>();

            return authState.User.Claims.Where(c => c.Type == "groups")
                .Select(c => "oidc" + c.Value).ToList();
        }
    }
}

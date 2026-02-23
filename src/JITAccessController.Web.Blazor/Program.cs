using System.Security.Claims;
using JITAccessController.Web.Blazor;
using JITAccessController.Web.Blazor.Components;
using JITAccessController.Web.Blazor.Kubernetes;
using JITAccessController.Web.Blazor.Options;
using k8s;
using JITAccessController.Web.Blazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Logging.AddSerilog();

builder.Services.AddOptions<JITAccessOptions>();

builder.Services.AddSingleton<IKubernetes>(sp =>
{
    var config = KubernetesClientConfiguration.InClusterConfig();

    //var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();

    return new Kubernetes(config);
});

builder.Services.AddSingleton<AccessPolicyStore>();
builder.Services.AddHostedService<AccessPolicyWatcher>();
builder.Services.AddSingleton<ClusterAccessPolicyStore>();
builder.Services.AddHostedService<ClusterAccessPolicyWatcher>();

builder.Services.AddSingleton<AccessRequestStore>();
builder.Services.AddHostedService<AccessRequestWatcher>();
builder.Services.AddSingleton<ClusterAccessRequestStore>();
builder.Services.AddHostedService<ClusterAccessRequestWatcher>();

builder.Services.AddScoped<IAccessRequestService, AccessRequestService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;

    options.ForwardLimit = null;

    // Replace with IP of your proxy/load balancer
    options.KnownProxies.Clear();

    // 192.168.1.0/24 allows any from 192.168.1.1-254;
    options.KnownIPNetworks.Clear();
});

var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("JITAccess-Frontend");

var dataProtectionOptions = builder.Configuration.GetSection("DataProtectionOptions")
    .Get<JITAccessController.Web.Blazor.Options.DataProtectionOptions>();

if(dataProtectionOptions != null) {
    Log.Information(
        "Configuring DataProtection -- PersistKeysToFileSystem: '{PersistKeysToFileSystem}'; Path: '{FileSystemPath}'; Certificate Path: '{CertificatePath}'",
        dataProtectionOptions.PersistKeysToFileSystem,
        dataProtectionOptions.FileSystemPath,
        dataProtectionOptions.CertificatePath
    );

    if(dataProtectionOptions.PersistKeysToFileSystem) {
        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionOptions.FileSystemPath));
    }

    if(!string.IsNullOrWhiteSpace(dataProtectionOptions.CertificatePath))
    {
        string? password = null;
        if(!string.IsNullOrWhiteSpace(dataProtectionOptions.CertificatePasswordPath))
        {
            if(File.Exists(dataProtectionOptions.CertificatePasswordPath)) {
                password = File.ReadAllText(dataProtectionOptions.CertificatePasswordPath);
            }
        }

        X509Certificate2 certificate;

        if(dataProtectionOptions.CertificatePath.EndsWith(".pfx"))
        {
            certificate = X509CertificateLoader.LoadPkcs12FromFile(dataProtectionOptions.CertificatePath, password);
        }
        else
        {
            string? keyPath = null;

            if(!string.IsNullOrWhiteSpace(dataProtectionOptions.CertificateKeyPath))
            {
                keyPath = dataProtectionOptions.CertificateKeyPath;
            }

            if(!string.IsNullOrWhiteSpace(password))
            {
                certificate = X509Certificate2.CreateFromEncryptedPemFile(dataProtectionOptions.CertificatePath, password, keyPath);
            }
            else {
                certificate = X509Certificate2.CreateFromPemFile(dataProtectionOptions.CertificatePath, keyPath);
            }
        }

        if(certificate != null) {
            dataProtection.ProtectKeysWithCertificate(certificate);
        }
    }
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration.GetValue<string>("Oidc:Authority");
    options.ClientId = builder.Configuration.GetValue<string>("Oidc:ClientId");
    options.ClientSecret = builder.Configuration.GetValue<string>("Oidc:ClientSecret");

    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.TokenValidationParameters.NameClaimType = builder.Configuration.GetValue("Oidc:NameClaimType", ClaimTypes.Name);

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("groups");
});

builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz")
    .AllowAnonymous()
    .DisableAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/auth").MapLoginAndLogout();

app.Run();

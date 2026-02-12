using k8s;

namespace JITAccessController.Web.Blazor.Kubernetes;

public abstract class CustomResourceWatcher<TCustomResource, TCustomResourceStore> : BackgroundService
    where TCustomResource : CustomResource
    where TCustomResourceStore : CustomResourceStore<TCustomResource>
{
    private readonly IKubernetes _client;
    private readonly TCustomResourceStore _store;
    private readonly ILogger _logger;

    public CustomResourceWatcher(
        IKubernetes client,
        TCustomResourceStore store,
        ILogger<CustomResourceWatcher<TCustomResource,TCustomResourceStore>> logger)
    {
        _client = client;
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunWatchAsync(stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{0} Watcher crashed, retrying in 5s", nameof(TCustomResource));
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task RunWatchAsync(CancellationToken ct)
    {
        var generic = new CustomResourceClient<TCustomResource>(_client, false);

        var list = await generic.ListAsync<CustomResourceList<TCustomResource>>(ct);
        if(list != null && list.Items != null)
        {
            foreach(var item in list.Items)
            {
                _store.Upsert(item);
            }
        }

        await foreach (var (type, request) in generic.WatchAsync<TCustomResource>(cancel: ct))
        {
            if (ct.IsCancellationRequested)
                break;

            if (request == null)
                continue;
            
            switch (type)
            {
                case WatchEventType.Added:
                case WatchEventType.Modified:
                    _store.Upsert(request);
                    break;
                case WatchEventType.Deleted:
                    _store.Remove(request.Metadata.Uid);
                    break;
            }
        }
    }
}
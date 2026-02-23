using k8s;

namespace JITAccessController.Web.Blazor.Kubernetes;

public abstract class CustomResourceWatcher<TCustomResource, TCustomResourceStore> : BackgroundService
    where TCustomResource : CustomResource
    where TCustomResourceStore : CustomResourceStore<TCustomResource>
{
    private readonly IKubernetes _client;
    private readonly TCustomResourceStore _store;
    private readonly ILogger _logger;

    private readonly CustomResourceClient<TCustomResource> _genericClient;

    public CustomResourceWatcher(
        IKubernetes client,
        TCustomResourceStore store,
        ILogger<CustomResourceWatcher<TCustomResource,TCustomResourceStore>> logger)
    {
        _client = client;
        _genericClient = new CustomResourceClient<TCustomResource>(_client, false);
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("{WatcherName} Watcher starting", this.GetType().Name);

            try
            {
                await RunWatchAsync(stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{WatcherName} Watcher crashed, retrying in 5s", this.GetType().Name);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task RunWatchAsync(CancellationToken ct)
    {
        var list = await _genericClient.ListAsync<CustomResourceList<TCustomResource>>(ct);
        if(list != null && list.Items != null)
        {
            foreach(var item in list.Items)
            {
                _store.Upsert(item);
            }
        }

        await foreach (var (type, request) in _genericClient.WatchAsync<TCustomResource>(cancel: ct))
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
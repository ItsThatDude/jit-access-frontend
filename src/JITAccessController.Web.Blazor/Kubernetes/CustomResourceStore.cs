using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using k8s;
using k8s.Models;

namespace JITAccessController.Web.Blazor.Kubernetes;

public class CustomResourceStore<T> where T : CustomResource
{
    private readonly ConcurrentDictionary<string, T> _items = new();

    public event Action? OnChanged;

    public IReadOnlyCollection<T> All => _items.Values.ToList();

    public void Upsert(T resource)
    {
        _items[resource.Uid()] = resource;
        OnChanged?.Invoke();
    }

    public void Remove(string uid)
    {
        _items.TryRemove(uid, out _);
        OnChanged?.Invoke();
    }
}
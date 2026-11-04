using Soenneker.Atomics.ValueBools;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.WebLocks.Abstract;

namespace Soenneker.Blazor.WebLocks;


public sealed class WebLocksInterop(IModuleImportUtil modules) : IWebLocksInterop
{
    private const string ModulePath = "./_content/Soenneker.Blazor.WebLocks/js/weblocksinterop.js";
    private readonly ConcurrentDictionary<string, WebLockLease> _leases = new();
    private readonly CancellationTokenSource _lifetime = new();
    private ValueAtomicBool _disposed;
    private readonly object _disposeSync = new();
    private Task? _disposeTask;
    private async ValueTask<IJSObjectReference> Module(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed.Value, this);
        return await modules.GetContentModuleReference(ModulePath, cancellationToken);
    }
    public async ValueTask Initialize(CancellationToken cancellationToken = default) => _ = await Module(cancellationToken);
    public async ValueTask<bool> IsSupported(CancellationToken cancellationToken = default) =>
        await (await Module(cancellationToken)).InvokeAsync<bool>("isSupported", cancellationToken);
    public async ValueTask<IWebLockLease> Acquire(string name, bool shared = false, CancellationToken cancellationToken = default) =>
        (await Request(name, shared, false, cancellationToken))!;
    public ValueTask<IWebLockLease?> TryAcquire(string name, bool shared = false, CancellationToken cancellationToken = default) =>
        Request(name, shared, true, cancellationToken);
    private async ValueTask<IWebLockLease?> Request(string name, bool shared, bool ifAvailable, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.StartsWith('-'))
            throw new ArgumentException("Browser lock names cannot start with '-'.", nameof(name));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetime.Token);
        IJSObjectReference module = await Module(linked.Token);
        var lease = new WebLockLease(name, shared, module, id => _leases.TryRemove(id, out _));
        _leases.TryAdd(lease.Id, lease);
        try
        {
            try
            {
                linked.Token.ThrowIfCancellationRequested();
                // Register before observing cancellation so cleanup cannot overtake creation in JavaScript.
                await module.InvokeVoidAsync("request", lease.Id, name, shared, ifAvailable, lease.Reference);
            }
            finally { lease.Registered.TrySetResult(); }
            bool acquired = await lease.Completion.Task.WaitAsync(linked.Token);
            linked.Token.ThrowIfCancellationRequested();
            if (acquired)
                return lease;
            await lease.DisposeAsync();
            return null;
        }
        catch
        {
            await lease.DisposeAsync();
            throw;
        }
    }
    public ValueTask DisposeAsync()
    {
        lock (_disposeSync)
            return new ValueTask(_disposeTask ??= DisposeCore());
    }

    private async Task DisposeCore()
    {
        if (!_disposed.TrySetTrue())
            return;
        await _lifetime.CancelAsync();
        // Cached module references belong to DI, not to individual consumers.
        try { await Task.WhenAll(_leases.Values.Select(item => item.DisposeAsync().AsTask())); }
        finally { _lifetime.Dispose(); }
    }
}
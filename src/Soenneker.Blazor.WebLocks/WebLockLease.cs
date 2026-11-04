using Soenneker.Atomics.ValueBools;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.WebLocks.Abstract;

namespace Soenneker.Blazor.WebLocks;

internal sealed class WebLockLease : IWebLockLease
{
    private readonly IJSObjectReference _module;
    private readonly Action<string> _remove;
    private ValueAtomicBool _disposed;
    private readonly object _disposeSync = new();
    private Task? _disposeTask;
    internal string Id { get; } = Guid.NewGuid().ToString();
    internal TaskCompletionSource Registered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal TaskCompletionSource<bool> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal DotNetObjectReference<WebLockLease> Reference { get; }
    public string Name { get; }
    public bool IsShared { get; }
    internal WebLockLease(string name, bool shared, IJSObjectReference module, Action<string> remove)
    {
        Name = name;
        IsShared = shared;
        _module = module;
        _remove = remove;
        Reference = DotNetObjectReference.Create(this);
    }
    [JSInvokable]
    public void OnGranted(bool acquired) => Completion.TrySetResult(acquired);
    [JSInvokable]
    public void OnError(string message) => Completion.TrySetException(new InvalidOperationException(message));
    public ValueTask DisposeAsync()
    {
        lock (_disposeSync)
            return new ValueTask(_disposeTask ??= DisposeCore());
    }

    private async Task DisposeCore()
    {
        if (!_disposed.TrySetTrue())
            return;
        Completion.TrySetCanceled();
        try
        {
            await Registered.Task;
            await _module.InvokeVoidAsync("release", Id);
        }
        catch (JSDisconnectedException) { }
        finally
        {
            Reference.Dispose();
            _remove(Id);
        }
    }
}
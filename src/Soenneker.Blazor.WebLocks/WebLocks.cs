using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.WebLocks.Abstract;

namespace Soenneker.Blazor.WebLocks;

public sealed class WebLocks(IWebLocksInterop interop) : IWebLocks
{
    public ValueTask Initialize(CancellationToken cancellationToken = default) => interop.Initialize(cancellationToken);
    public ValueTask<bool> IsSupported(CancellationToken cancellationToken = default) => interop.IsSupported(cancellationToken);
    public ValueTask<IWebLockLease> Acquire(string name, bool shared = false, CancellationToken cancellationToken = default) =>
        interop.Acquire(name, shared, cancellationToken);
    public ValueTask<IWebLockLease?> TryAcquire(string name, bool shared = false, CancellationToken cancellationToken = default) =>
        interop.TryAcquire(name, shared, cancellationToken);
}
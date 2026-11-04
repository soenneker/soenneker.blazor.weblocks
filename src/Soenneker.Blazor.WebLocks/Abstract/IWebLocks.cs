using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Blazor.WebLocks.Abstract;
/// <summary>Browser-origin lock coordination. Requires an interactive browser and secure context.</summary>
public interface IWebLocks
{
    /// <summary>Loads the module after interactive rendering.</summary>
    ValueTask Initialize(CancellationToken cancellationToken = default);
    /// <summary>Reports support in the current browser context.</summary>
    ValueTask<bool> IsSupported(CancellationToken cancellationToken = default);
    /// <summary>Waits for a lock; dispose the lease to release it. Cancellation applies only to acquisition.
    /// The wait is not limited by the normal JavaScript interop timeout.</summary>
    ValueTask<IWebLockLease> Acquire(string name, bool shared = false, CancellationToken cancellationToken = default);
    /// <summary>Returns a disposable lease if immediately available, otherwise null.</summary>
    ValueTask<IWebLockLease?> TryAcquire(string name, bool shared = false, CancellationToken cancellationToken = default);
}
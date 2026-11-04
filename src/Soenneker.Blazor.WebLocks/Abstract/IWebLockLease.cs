using System;

namespace Soenneker.Blazor.WebLocks.Abstract;
/// <summary>A browser-origin lock held until disposal or browser context destruction.</summary>
public interface IWebLockLease : IAsyncDisposable
{
    /// <summary>The locked resource name.</summary>
    string Name { get; }
    /// <summary>Whether other shared holders are permitted.</summary>
    bool IsShared { get; }
}
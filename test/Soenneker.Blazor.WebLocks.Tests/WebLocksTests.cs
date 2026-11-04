using Soenneker.Blazor.WebLocks.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.WebLocks.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class WebLocksTests : HostedUnitTest
{
    private readonly IWebLocks _blazorlibrary;

    public WebLocksTests(Host host) : base(host)
    {
        _blazorlibrary = Resolve<IWebLocks>(true);
    }

    [Test]
    public void Default()
    {

    }
}

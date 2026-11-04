using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.WebLocks.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Registrars;


namespace Soenneker.Blazor.WebLocks.Registrars;

/// <summary>
/// Registration for the interop and utility services.
/// </summary>
public static class WebLocksRegistrar
{
    /// <summary>
    /// Adds <see cref="IWebLocksInterop"/> and <see cref="IWebLocks"/> as scoped services.
    /// </summary>
    public static IServiceCollection AddWebLocksAsScoped(this IServiceCollection services)
    {
        services.AddModuleImportUtilAsScoped()
                .TryAddScoped<IWebLocksInterop, WebLocksInterop>();

        services.TryAddScoped<IWebLocks, WebLocks>();

        return services;
    }
}

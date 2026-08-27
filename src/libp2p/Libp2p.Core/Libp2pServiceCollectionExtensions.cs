// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Nethermind.Libp2p.Core;

/// <summary>
/// Dependency injection helpers for libp2p protocol packages.
/// </summary>
public static class Libp2pServiceCollectionExtensions
{
    /// <summary>
    /// Registers a feature that will configure the peer factory builder when it is created.
    /// Registering the same feature type more than once is idempotent.
    /// </summary>
    public static IServiceCollection AddLibp2pFeature<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFeature>(
        this IServiceCollection services)
        where TFeature : class, ILibp2pFeature
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILibp2pFeature, TFeature>());
        return services;
    }

    /// <summary>
    /// Registers settings with the standard options pipeline and also makes the resulting settings
    /// instance directly injectable for compatibility with protocol constructors that take a settings POCO.
    /// Multiple configuration delegates are applied in registration order.
    /// </summary>
    public static IServiceCollection AddLibp2pSettings<TSettings>(
        this IServiceCollection services,
        Action<TSettings>? configure = null)
        where TSettings : class
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<TSettings>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<TSettings>>().Value);
        return services;
    }
}

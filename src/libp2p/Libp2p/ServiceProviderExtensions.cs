// SPDX-FileCopyrightText: 2024 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nethermind.Libp2p.Core;
using Nethermind.Libp2p.Core.Discovery;
using Nethermind.Libp2p.Protocols;
using Nethermind.Libp2p.Protocols.Pubsub;
using Nethermind.Libp2p.Protocols.Relay;
using Nethermind.Libp2p.Protocols.Yamux;

namespace Nethermind.Libp2p;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Registers the default libp2p peer factory and its supporting services.
    /// Repeated calls are idempotent; the first registered factory setup is retained.
    /// </summary>
    public static IServiceCollection AddLibp2p(this IServiceCollection services, Func<ILibp2pPeerFactoryBuilder, IPeerFactoryBuilder>? factorySetup = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLibp2p<Libp2pPeerFactoryBuilder>(factorySetup is null ? null : new Func<IPeerFactoryBuilder, IPeerFactoryBuilder>(b => factorySetup((ILibp2pPeerFactoryBuilder)b)));
        services.TryAddSingleton(sp => (ILibp2pPeerFactoryBuilder)sp.GetRequiredService<IPeerFactoryBuilder>());
        return services;
    }

    /// <summary>
    /// Registers a custom peer factory builder and its supporting services.
    /// Features are applied to the builder returned by <paramref name="factorySetup"/>, when provided.
    /// Repeated calls are idempotent; the first registered builder and factory setup are retained.
    /// </summary>
    /// <remarks>
    /// Built-in protocol package features require the resulting builder to implement
    /// <see cref="ILibp2pPeerFactoryBuilder"/>. Custom features may support other
    /// <see cref="IPeerFactoryBuilder"/> implementations.
    /// </remarks>
    public static IServiceCollection AddLibp2p<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPeerFactoryBuilder>(this IServiceCollection services, Func<IPeerFactoryBuilder, IPeerFactoryBuilder>? factorySetup = null)
        where TPeerFactoryBuilder : IPeerFactoryBuilder
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLibp2pSettings<MultiplexerSettings>();
        services.AddLibp2pSettings<IdentifyProtocolSettings>();
        services.AddLibp2pSettings<YamuxWindowSettings>();
        services.AddLibp2pSettings<PubsubSettings>();

        services.TryAddSingleton<IRelayReservationStore, RelayReservationStore>();
        services.TryAddSingleton<RelayStopProtocol>();
        services.TryAddSingleton<IProtocolStackSettings, ProtocolStackSettings>();
        services.TryAddSingleton<IPeerFactoryBuilder>(sp =>
        {
            IPeerFactoryBuilder builder = ActivatorUtilities.CreateInstance<TPeerFactoryBuilder>(sp);
            builder = factorySetup is null ? builder : factorySetup(builder);
            foreach (ILibp2pFeature feature in sp.GetServices<ILibp2pFeature>())
            {
                feature.Configure(builder);
            }

            return builder;
        });
        services.TryAddSingleton(sp => sp.GetRequiredService<IPeerFactoryBuilder>().Build());
        services.TryAddSingleton<PubsubRouter>();
        services.TryAddSingleton<PeerStore>();
        services.TryAddSingleton<MDnsDiscoveryProtocol>();
        services.TryAddSingleton<IdentifyNotifier>();

        return services;
    }
}

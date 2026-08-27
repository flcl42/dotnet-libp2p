// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nethermind.Libp2p.Core;

namespace Nethermind.Libp2p.Protocols.Pubsub;

/// <summary>
/// Registers and activates the pubsub protocol family.
/// </summary>
public static class PubsubServiceCollectionExtensions
{
    public static IServiceCollection AddPubsub(
        this IServiceCollection services,
        Action<PubsubSettings>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLibp2pSettings(configure);
        services.TryAddSingleton<PubsubRouter>();
        services.TryAddSingleton<FloodsubProtocol>();
        services.TryAddSingleton<GossipsubProtocol>();
        services.TryAddSingleton<GossipsubProtocolV11>();
        services.TryAddSingleton<GossipsubProtocolV12>();
        services.AddLibp2pFeature<PubsubFeature>();
        return services;
    }

    private sealed class PubsubFeature : ILibp2pFeature
    {
        public PubsubFeature() { }

        public void Configure(IPeerFactoryBuilder builder)
        {
            if (builder is not ILibp2pPeerFactoryBuilder libp2pBuilder)
            {
                throw new InvalidOperationException($"{nameof(PubsubFeature)} requires an {nameof(ILibp2pPeerFactoryBuilder)}.");
            }

            libp2pBuilder.WithPubsub();
        }
    }
}

// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nethermind.Libp2p.Core;

namespace Nethermind.Libp2p.Protocols.Relay;

/// <summary>
/// Registers and activates circuit relay v2.
/// </summary>
public static class RelayServiceCollectionExtensions
{
    public static IServiceCollection AddRelay(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IRelayReservationStore, RelayReservationStore>();
        services.TryAddSingleton<RelayStopProtocol>();
        services.TryAddSingleton<RelayHopProtocol>();
        services.AddLibp2pFeature<RelayFeature>();
        return services;
    }

    private sealed class RelayFeature : ILibp2pFeature
    {
        public RelayFeature() { }

        public void Configure(IPeerFactoryBuilder builder)
        {
            if (builder is not ILibp2pPeerFactoryBuilder libp2pBuilder)
            {
                throw new InvalidOperationException($"{nameof(RelayFeature)} requires an {nameof(ILibp2pPeerFactoryBuilder)}.");
            }

            libp2pBuilder.WithRelay();
        }
    }
}

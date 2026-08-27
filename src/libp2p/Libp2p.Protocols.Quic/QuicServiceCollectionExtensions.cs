// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nethermind.Libp2p.Core;

namespace Nethermind.Libp2p.Protocols;

/// <summary>
/// Registers and activates the QUIC transport.
/// </summary>
public static class QuicServiceCollectionExtensions
{
    public static IServiceCollection AddQuic(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<QuicProtocol>();
        services.AddLibp2pFeature<QuicFeature>();
        return services;
    }

    private sealed class QuicFeature : ILibp2pFeature
    {
        public QuicFeature() { }

        public void Configure(IPeerFactoryBuilder builder)
        {
            if (builder is not ILibp2pPeerFactoryBuilder libp2pBuilder)
            {
                throw new InvalidOperationException($"{nameof(QuicFeature)} requires an {nameof(ILibp2pPeerFactoryBuilder)}.");
            }

            libp2pBuilder.WithQuic();
        }
    }
}

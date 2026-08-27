// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nethermind.Libp2p.Core;

namespace Nethermind.Libp2p.Protocols;

/// <summary>
/// Registers and activates the WebSocket transport.
/// </summary>
public static class WebSocketServiceCollectionExtensions
{
    public static IServiceCollection AddWebSockets(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<WebSocketProtocol>();
        services.AddLibp2pFeature<WebSocketFeature>();
        return services;
    }

    private sealed class WebSocketFeature : ILibp2pFeature
    {
        public WebSocketFeature() { }

        public void Configure(IPeerFactoryBuilder builder)
        {
            if (builder is not ILibp2pPeerFactoryBuilder libp2pBuilder)
            {
                throw new InvalidOperationException($"{nameof(WebSocketFeature)} requires an {nameof(ILibp2pPeerFactoryBuilder)}.");
            }

            libp2pBuilder.WithWebSockets();
        }
    }
}

// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nethermind.Libp2p.Core;

namespace Nethermind.Libp2p.Protocols.WebRtc.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebRtcDirect(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<WebRtcDirectProtocol>();
        services.AddLibp2pFeature<WebRtcDirectFeature>();
        return services;
    }

    public static ILibp2pPeerFactoryBuilder AddWebRtcDirect(this ILibp2pPeerFactoryBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<WebRtcDirectProtocol>();
        builder.WithWebRtcDirect();
        return builder;
    }

    private sealed class WebRtcDirectFeature : ILibp2pFeature
    {
        public WebRtcDirectFeature() { }

        public void Configure(IPeerFactoryBuilder builder)
        {
            if (builder is not ILibp2pPeerFactoryBuilder libp2pBuilder)
            {
                throw new InvalidOperationException($"{nameof(WebRtcDirectFeature)} requires an {nameof(ILibp2pPeerFactoryBuilder)}.");
            }

            libp2pBuilder.WithWebRtcDirect();
        }
    }
}

// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nethermind.Libp2p.Core;

namespace Nethermind.Libp2p.Protocols;

/// <summary>
/// Registers plaintext as the enforced connection security protocol.
/// </summary>
public static class PlaintextServiceCollectionExtensions
{
    public static IServiceCollection AddPlaintext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<PlainTextProtocol>();
        services.AddLibp2pFeature<PlaintextFeature>();
        return services;
    }

    private sealed class PlaintextFeature : ILibp2pFeature
    {
        public PlaintextFeature() { }

        public void Configure(IPeerFactoryBuilder builder)
        {
            if (builder is not ILibp2pPeerFactoryBuilder libp2pBuilder)
            {
                throw new InvalidOperationException($"{nameof(PlaintextFeature)} requires an {nameof(ILibp2pPeerFactoryBuilder)}.");
            }

            libp2pBuilder.WithPlaintextEnforced();
        }
    }
}

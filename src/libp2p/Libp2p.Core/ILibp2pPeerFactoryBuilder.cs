// SPDX-FileCopyrightText: 2025 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace Nethermind.Libp2p.Core;

public interface ILibp2pPeerFactoryBuilder : IPeerFactoryBuilder
{
    /// <summary>
    /// Provides access to services owned by a standalone builder.
    /// When the builder is created by dependency injection, register protocol packages on the
    /// application's <see cref="IServiceCollection"/> before building the service provider.
    /// </summary>
    IServiceCollection Services { get; }

    public ILibp2pPeerFactoryBuilder WithPlaintextEnforced();
    public ILibp2pPeerFactoryBuilder WithPubsub();
    public ILibp2pPeerFactoryBuilder WithRelay();
    public ILibp2pPeerFactoryBuilder WithQuic();
    public ILibp2pPeerFactoryBuilder WithWebSockets();
    public ILibp2pPeerFactoryBuilder WithWebRtcDirect();
}

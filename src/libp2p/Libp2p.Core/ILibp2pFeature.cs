// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

namespace Nethermind.Libp2p.Core;

/// <summary>
/// Configures a peer factory builder when it is created by the libp2p dependency injection integration.
/// Protocol packages can implement this interface to activate their protocols after registering their
/// settings and services with the application's service collection.
/// </summary>
public interface ILibp2pFeature
{
    /// <summary>
    /// Applies the feature to a peer factory builder.
    /// </summary>
    /// <param name="builder">The builder being created by dependency injection.</param>
    void Configure(IPeerFactoryBuilder builder);
}

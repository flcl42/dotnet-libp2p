// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Libp2p.Protocols.KadDht;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nethermind.Libp2p;
using Nethermind.Libp2p.Core;
using Nethermind.Libp2p.Protocols;
using Nethermind.Libp2p.Protocols.Pubsub;
using Nethermind.Libp2p.Protocols.Relay;
using Nethermind.Libp2p.Protocols.WebRtc;
using Nethermind.Libp2p.Protocols.WebRtc.Extensions;
using Nethermind.Libp2p.Protocols.Yamux;
using NUnit.Framework;

namespace Libp2p.E2eTests;

[TestFixture]
public class DependencyInjectionActivationTests
{
    [Test]
    public void AddLibp2pFeature_ResolvesFeatureAndProtocolFromDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ActivationTracker>();
        services.AddLibp2pSettings<ActivationSettings>(settings => settings.Prefix = "configured");
        services.AddLibp2pFeature<ActivationFeature>();
        services.AddLibp2pFeature<ActivationFeature>();
        services.AddSingleton<ActivationProtocol>();
        services.AddLibp2p();
        services.Configure<ActivationSettings>(settings => settings.Suffix = "-feature");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<IPeerFactory>();

        ActivationSettings settings = serviceProvider.GetRequiredService<ActivationSettings>();
        IOptions<ActivationSettings> options = serviceProvider.GetRequiredService<IOptions<ActivationSettings>>();
        ActivationProtocol registeredProtocol = serviceProvider.GetRequiredService<ActivationProtocol>();
        ActivationProtocol activatedProtocol = GetProtocols(serviceProvider).OfType<ActivationProtocol>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(settings, Is.SameAs(options.Value));
            Assert.That(settings.Prefix, Is.EqualTo("configured"));
            Assert.That(settings.Suffix, Is.EqualTo("-feature"));
            Assert.That(activatedProtocol, Is.SameAs(registeredProtocol));
            Assert.That(activatedProtocol.Id, Is.EqualTo("/test/configured-feature/1.0.0"));
            Assert.That(serviceProvider.GetRequiredService<ActivationTracker>().ActivationCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void AddPubsub_RegistersSettingsServicesAndActivatesProtocols()
    {
        var services = new ServiceCollection();
        services.AddLibp2p();
        services.AddPubsub(settings => settings.HeartbeatInterval = 250);
        services.AddPubsub(settings => settings.ReconnectionPeriod = 500);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<IPeerFactory>();

        PubsubSettings settings = serviceProvider.GetRequiredService<PubsubSettings>();
        IOptions<PubsubSettings> options = serviceProvider.GetRequiredService<IOptions<PubsubSettings>>();
        IProtocol[] protocols = GetProtocols(serviceProvider);

        Assert.Multiple(() =>
        {
            Assert.That(settings, Is.SameAs(options.Value));
            Assert.That(settings.HeartbeatInterval, Is.EqualTo(250));
            Assert.That(settings.ReconnectionPeriod, Is.EqualTo(500));
            Assert.That(protocols.OfType<FloodsubProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<FloodsubProtocol>()));
            Assert.That(protocols.OfType<GossipsubProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<GossipsubProtocol>()));
            Assert.That(protocols.OfType<GossipsubProtocolV11>().Single(), Is.SameAs(serviceProvider.GetRequiredService<GossipsubProtocolV11>()));
            Assert.That(protocols.OfType<GossipsubProtocolV12>().Single(), Is.SameAs(serviceProvider.GetRequiredService<GossipsubProtocolV12>()));
            Assert.That(serviceProvider.GetService<PubsubRouter>(), Is.Not.Null);
        });
    }

    [Test]
    public void AddLibp2p_DefaultProtocolSettingsParticipateInOptionsPipeline()
    {
        var services = new ServiceCollection();
        services.AddLibp2p();
        services.Configure<IdentifyProtocolSettings>(settings => settings.AgentVersion = "configured-agent");
        services.Configure<YamuxWindowSettings>(settings => settings.InitialWindowSize = 512 * 1024);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IdentifyProtocolSettings identifySettings = serviceProvider.GetRequiredService<IdentifyProtocolSettings>();
        YamuxWindowSettings yamuxSettings = serviceProvider.GetRequiredService<YamuxWindowSettings>();

        Assert.Multiple(() =>
        {
            Assert.That(identifySettings, Is.SameAs(serviceProvider.GetRequiredService<IOptions<IdentifyProtocolSettings>>().Value));
            Assert.That(identifySettings.AgentVersion, Is.EqualTo("configured-agent"));
            Assert.That(yamuxSettings, Is.SameAs(serviceProvider.GetRequiredService<IOptions<YamuxWindowSettings>>().Value));
            Assert.That(yamuxSettings.InitialWindowSize, Is.EqualTo(512 * 1024));
        });
    }

    [Test]
    public void BuiltInFeaturePackages_RegisterAndActivateTheirProtocols()
    {
        var services = new ServiceCollection();
        services.AddQuic();
        services.AddRelay();
        services.AddPlaintext();
        services.AddWebSockets();
        services.AddWebRtcDirect();
        services.AddLibp2p();

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<IPeerFactory>();

        IProtocolStackSettings stack = serviceProvider.GetRequiredService<IProtocolStackSettings>();
        IProtocol[] protocols = GetProtocols(serviceProvider);
        IProtocol[] transports = stack.TopProtocols!.Select(protocol => protocol.Protocol).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(transports.OfType<QuicProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<QuicProtocol>()));
            Assert.That(transports.OfType<WebSocketProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<WebSocketProtocol>()));
            Assert.That(transports.OfType<WebRtcDirectProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<WebRtcDirectProtocol>()));
            Assert.That(protocols.OfType<PlainTextProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<PlainTextProtocol>()));
            Assert.That(protocols.OfType<RelayHopProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<RelayHopProtocol>()));
            Assert.That(protocols.OfType<RelayStopProtocol>().Single(), Is.SameAs(serviceProvider.GetRequiredService<RelayStopProtocol>()));
        });
    }

    [Test]
    public void AddLibp2p_LegacyBuilderSetupAndSettingsSingletonRemainSupported()
    {
        var settings = new PubsubSettings { HeartbeatInterval = 125 };
        var services = new ServiceCollection();
        services.AddSingleton(settings);
        services.AddLibp2p(builder => builder.WithPubsub());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<IPeerFactory>();

        Assert.Multiple(() =>
        {
            Assert.That(serviceProvider.GetRequiredService<PubsubSettings>(), Is.SameAs(settings));
            Assert.That(GetProtocols(serviceProvider).OfType<FloodsubProtocol>(), Has.Exactly(1).Items);
        });
    }

    [Test]
    public void AddKadDht_LegacyExplicitActivationDoesNotDuplicateProtocol()
    {
        var services = new ServiceCollection();
        services.AddKadDht();
        services.AddLibp2p(builder => builder.WithKadDht());

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<IPeerFactory>();

        Assert.That(GetProtocols(serviceProvider).Count(protocol => protocol.Id == global::Libp2p.Protocols.KadDht.ServiceCollectionExtensions.ProtocolId),
            Is.EqualTo(1));
    }

    private static IProtocol[] GetProtocols(IServiceProvider serviceProvider)
    {
        IProtocolStackSettings stack = serviceProvider.GetRequiredService<IProtocolStackSettings>();
        return stack.Protocols!.Keys.Select(protocol => protocol.Protocol).ToArray();
    }

    public sealed class ActivationSettings
    {
        public string Prefix { get; set; } = "default";
        public string Suffix { get; set; } = string.Empty;
    }

    public sealed class ActivationTracker
    {
        public int ActivationCount { get; set; }
    }

    public sealed class ActivationProtocol(ActivationSettings settings) : IProtocol
    {
        public string Id { get; } = $"/test/{settings.Prefix}{settings.Suffix}/1.0.0";
    }

    public sealed class ActivationFeature(ActivationProtocol protocol, ActivationTracker tracker) : ILibp2pFeature
    {
        public void Configure(IPeerFactoryBuilder builder)
        {
            tracker.ActivationCount++;
            builder.AddProtocol(protocol);
        }
    }
}

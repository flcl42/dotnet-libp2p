// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Nethermind.Libp2p.Protocols.AutoTls.Tests;

[TestFixture]
public class AutoTlsServiceCollectionExtensionsTests
{
    [Test]
    public void AddAutoTls_RegistersSameConfiguredOptionsForBothInjectionStyles()
    {
        var services = new ServiceCollection();
        services.AddAutoTls(options => options.ForgeDomain = "example.test");

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        AutoTlsOptions settings = serviceProvider.GetRequiredService<AutoTlsOptions>();
        IOptions<AutoTlsOptions> options = serviceProvider.GetRequiredService<IOptions<AutoTlsOptions>>();

        Assert.Multiple(() =>
        {
            Assert.That(settings, Is.SameAs(options.Value));
            Assert.That(settings.ForgeDomain, Is.EqualTo("example.test"));
        });
    }
}

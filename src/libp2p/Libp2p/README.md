# The libp2p stack

The entry point API for libp2p.

## Dependency injection setup

`AddLibp2p()` registers the core peer factory. Protocol packages can then register their
settings and services and activate themselves on the peer stack:

```csharp
ServiceProvider provider = new ServiceCollection()
    .AddLibp2p()
    .AddQuic()
    .AddPubsub(settings =>
    {
        settings.HeartbeatInterval = 500;
    })
    .BuildServiceProvider();
```

Settings registered by a package participate in the standard `Microsoft.Extensions.Options`
pipeline. The same configured instance is also directly injectable as its settings type so
existing protocol constructors and application registrations continue to work.

The builder callback and `With...` methods remain supported for manual stack customization:

```csharp
services.AddLibp2p(builder => builder
    .WithQuic()
    .AddProtocol<MyProtocol>());
```

## Protocol package integration

A protocol package can provide one `IServiceCollection` extension that registers all three
parts of the package: settings, services, and an `ILibp2pFeature` that modifies the peer stack.

```csharp
public static IServiceCollection AddMyProtocol(
    this IServiceCollection services,
    Action<MyProtocolSettings>? configure = null)
{
    services.AddLibp2pSettings(configure);
    services.TryAddSingleton<MyProtocol>();
    services.AddLibp2pFeature<MyProtocolFeature>();
    return services;
}

public sealed class MyProtocolFeature : ILibp2pFeature
{
    public void Configure(IPeerFactoryBuilder builder) =>
        builder.AddProtocol<MyProtocol>();
}
```

Features are resolved through dependency injection and applied in registration order when the
peer factory builder is created. Package registration may appear before or after `AddLibp2p()`.

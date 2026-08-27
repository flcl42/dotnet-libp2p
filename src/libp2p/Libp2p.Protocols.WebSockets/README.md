# WebSockets transport

WebSocket and Secure WebSocket transport support for dotnet-libp2p.

Use `services.AddWebSockets()` to register the transport and enable `/ws` and `/wss`
multiaddrs. The existing `WithWebSockets()` peer-builder method remains supported.
Secure WebSocket listeners require an `ITlsCertificateProvider`, which can be supplied by
`Libp2p.Protocols.AutoTls`.

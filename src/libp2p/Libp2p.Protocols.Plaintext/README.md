# Plaintext protocol

See the [libp2p spec](https://github.com/libp2p/specs/tree/master/plaintext)

Use `services.AddPlaintext()` to register plaintext and enforce it as the connection security
protocol. The existing `WithPlaintextEnforced()` peer-builder method remains supported.

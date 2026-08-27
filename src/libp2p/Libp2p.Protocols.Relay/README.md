# Circuit Relay v2

Implements the [Circuit Relay v2](https://github.com/libp2p/specs/blob/master/relay/circuit-v2.md) protocol:

- **Hop** (`/libp2p/circuit/relay/0.2.0/hop`): reservation of relay slots and connection initiation through the relay.
- **Stop** (`/libp2p/circuit/relay/0.2.0/stop`): connection termination between the relay and the target peer.

Use `services.AddRelay()` to register the relay protocols and reservation store and enable relay
in the stack. The existing `WithRelay()` peer-builder method remains supported, and
`AddLibp2p()` continues to register the default reservation store for compatibility.

**Critical gap:** Stream bridging is required for a working relayed connection. Today the relay completes the Hop and Stop handshakes and returns STATUS OK but does not tie the two streams together. Implementing this will require either a way to obtain the stop stream/channel after `DialAsync` or lower-level support to bridge two streams by ID.

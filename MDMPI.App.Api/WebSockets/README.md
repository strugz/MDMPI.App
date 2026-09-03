# WebSocket Manual Test Notes

Production clients should connect with:

```text
wss://<host>/api/ws?apiKey=<new-key>&clientId=<device-or-user-id>&role=rider|watcher
```

`clientId` and `role` are optional (older clients connect fine without them) and are
used for logging/diagnostics only — identity is claimed, not proven. Values are
sanitized: allowed characters `A-Z a-z 0-9 _ . : @ -`, max 64 chars; anything else
falls back to `anon`/`unspecified`. Allowed roles: `rider`, `watcher`.

Set the production key through configuration, preferably:

```text
WebSocket__ApiKey=<new-key>
```

Manual verification:

1. Connect client A and client B to `/api/ws?apiKey=<new-key>`.
2. Send a direct rider payload from client A:

```json
{
  "Type": "location_update",
  "RequestID": "REQ-001",
  "RiderId": "RIDER-001",
  "Latitude": 14.5995,
  "Longitude": 120.9842,
  "Timestamp": "2026-05-26T10:00:00Z",
  "Status": "en_route",
  "RiderInitial": "JB",
  "ETA": "10 mins",
  "Distance": "2 km",
  "Client": "MDMPI"
}
```

3. Confirm client B receives a wrapped `Message=Location` payload and client A does not receive its own broadcast.
4. Send a wrapped `Message=Notification` payload and confirm client B receives `Title` and `Body`.
5. Try connecting with a missing or invalid `apiKey` and confirm the connection is rejected.
6. Send malformed JSON and confirm the server logs a warning but remains connected for the next valid message.

## Location replay on connect

The server caches the **latest** normalized Location envelope per `RequestID`
(TTL 30 min, max 500 entries, in-memory). When a client connects — including a
phone reconnecting after a signal drop — the server immediately sends it the
cached envelope for every active delivery, so the map is never blank while
waiting for the rider's next movement. Replay frames are ordinary canonical
envelopes; render latest-by-`Timestamp` and duplicates are harmless.

## Server limits & guardrails

- **Max message size: 64 KB.** Larger messages close the connection with status
  `1009 MessageTooBig`.
- **Inbound rate limit: 20 messages/second** per connection (fixed 1 s window;
  malformed messages count too). Excess messages are dropped with a warning;
  3 consecutive over-limit windows close the connection with
  `1008 PolicyViolation`.
- **Max concurrent connections: 500.** Further handshakes are rejected with
  `503 Service Unavailable` until a slot frees (soft cap).
- **Broadcast send timeout: 5 seconds.** A client that stops reading (dead radio
  link, full TCP window) is dropped and its socket aborted, so it cannot stall
  broadcasts to other clients.
- String-valued `Latitude`/`Longitude` are parsed with the invariant culture
  (`14.5995` means 14.5995 regardless of server locale).
- Abrupt client disconnects (no close handshake) are logged at Debug and never
  affect other connections.

## Configuration

Code defaults below; override via environment variables. Do **not** add these to
`appsettings.json`.

| Key (env form) | Default |
|---|---|
| `WebSocket__MaxConnections` | 500 |
| `WebSocket__MaxMessagesPerSecond` | 20 |
| `WebSocket__RateLimitStrikeWindows` | 3 |
| `WebSocket__LocationCacheTtlMinutes` | 30 |
| `WebSocket__LocationCacheMaxEntries` | 500 |

## Mobile client guidance

- **Reconnect with exponential backoff + jitter** (e.g. 1 s, 2 s, 4 s … cap 30 s,
  ±20% random). Cellular drops are constant; immediate tight-loop reconnects burn
  battery and hammer the server.
- **Expect replay frames right after connecting** — same envelope shape as live
  broadcasts. Render latest-by-`Timestamp` per `RequestID`.
- If the connection closes with `1008`, the client is sending too fast — back off;
  with `1009`, the payload is oversized — that is a client bug.
- Send `clientId`/`role` so server logs can tell devices apart.

## Notes

- The connection registry and replay cache are **in-memory in a single process**:
  connections do not survive an app restart, and the endpoint assumes a single
  instance (no backplane).
- `WebSocketOptions.KeepAliveTimeout` (server-side pong tracking) requires
  ASP.NET Core 9. On net8.0 half-open connections are reaped by the 30 s pings
  failing at TCP level plus the 5 s send-budget abort — when upgrading to net9,
  add `KeepAliveTimeout` in `Program.cs` for faster reaping.

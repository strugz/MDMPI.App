# WebSocket Manual Test Notes

Production clients should connect with:

```text
wss://<host>/api/ws?apiKey=<new-key>
```

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

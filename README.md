# JSON RPC X

**_JSON RPC X_** is a JSON-RPC 2.0 implementation for multiple languages and frameworks with bidirectional communication support.

## Features

- JSON-RPC 2.0 support (see ["Specification](#specification)).
- Both server and client libraries for multiple languages and frameworks (see ["Support"](#support)).
- Multiple transports (see ["Transports"](#transports)).
    - **Bidirectional communication over WebSockets!**

## Specification

**_JSON RPC X_** uses and implements
[JSON-RPC 2.0](https://www.jsonrpc.org/specification)
specification.

Example JSON RPC payloads can be seen below.

### Request


_Example:_
```json
{
    "jsonrpc": "2.0",
    "method": "getUser",
    "id": "c890b461-fb55-4c31-a4d4-69ddaa7801d4",
    "params": {
        "id": 123
    }
}
```

### Response

_Example - Success:_
```json
{
    "jsonrpc": "2.0",
    "id": "c890b461-fb55-4c31-a4d4-69ddaa7801d4",
    "result": {
        "id": 123,
        "name": "Example User",
        "address": "Example Street 123",
        "isAdmin": false
    }
}
```

_Example - Error:_
```json
{
    "jsonrpc": "2.0",
    "id": "c890b461-fb55-4c31-a4d4-69ddaa7801d4",
    "error": {
        "code": 1,
        "message": "User not found",
        "data": {}
    }
}

### Notifications

_Example:_
```json
{
    "jsonrpc": "2.0",
    "method": "friendLoggedIn",
    "params": {
        "timestamp": "2024-08-21T08:07:00.000Z",
        "userId": 123
    }
}
```

## Support

**TODO:** Fill these when the libraries are implemented!

| **Language** | **Server** | **Client** |
| ----- | :-----: | :-----: |
| .NET | | |
| Node.js | | |

## Transports

| **Transport** | **Description** |
| ----- | ----- |
| HTTP | _(Not yet implemented)_
| WebSocket | Bidirectional communication over a single WebSocket connection. |

## License

[**MIT**](./LICENSE)

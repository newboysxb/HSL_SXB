# WebSocket guide

HslCommunication provides WebSocket server and client classes for pushing real
time data to web pages or other clients.

Important classes:

| Class | Purpose |
| --- | --- |
| `WebSocketServer` | WebSocket server |
| `WebSocketClient` | WebSocket client |
| `WebSocketSession` | One connected client session |
| `WebSocketMessage` | One received WebSocket message |

Namespace:

```csharp
using HslCommunication.WebSocket;
```

## Step 1: start a WebSocketServer

```csharp
WebSocketServer server = new WebSocketServer();
server.ServerStart(2000);

int id = 0;

while (true)
{
    Thread.Sleep(1000);
    server.PublishAllClientPayload(id.ToString());
    id++;
}
```

## Step 2: connect from a web page

```html
<script>
    var ws = new WebSocket("ws://127.0.0.1:2000");

    ws.onmessage = function (evt) {
        document.getElementById("demo").innerHTML = evt.data;
    };
</script>
```

`PublishAllClientPayload` sends text to every connected WebSocket client.

## Step 3: use URL-based topics

Use different WebSocket URLs as topics:

```csharp
server.OnClientConnected += session =>
{
    string topic = session.Url.StartsWith("/")
        ? session.Url.Substring(1)
        : session.Url;

    if (!string.IsNullOrEmpty(topic))
    {
        session.AddTopic(topic);
    }
};

server.ServerStart(2000);
```

Web page:

```javascript
var wsA = new WebSocket("ws://127.0.0.1:2000/A");
var wsB = new WebSocket("ws://127.0.0.1:2000/B");
```

Server publishes only to matching topics:

```csharp
server.PublishClientPayload("A", "message for A");
server.PublishClientPayload("B", "message for B");
```

## Step 4: receive client messages on the server

```csharp
server.OnClientApplicationMessageReceive += (session, message) =>
{
    string text = Encoding.UTF8.GetString(message.Payload);
    Console.WriteLine($"Session[{session.Remote}] -> {text}");
};
```

`WebSocketMessage` fields:

```csharp
int OpCode
byte[] Payload
bool HasMask
bool IsCompressed
```

Text frames use `OpCode = 1`. Binary frames use `OpCode = 2`.

## Step 5: connect with the C# WebSocketClient

```csharp
WebSocketClient client = new WebSocketClient("127.0.0.1", 2000, "/A");
client.GetCarryHostAndPort = false;
client.ConnectTimeOut = 5000;

client.OnClientApplicationMessageReceive += message =>
{
    string text = Encoding.UTF8.GetString(message.Payload);
    Console.WriteLine(text);
};

OperateResult connect = client.ConnectServer();
if (!connect.IsSuccess)
{
    Console.WriteLine(connect.Message);
}

client.SendServer("Hello WebSocket");
client.ConnectClose();
```

The C# client can also pass topics directly to the HSL server:

```csharp
client.ConnectServer(new string[] { "A", "B" });
```

Constructors:

```csharp
new WebSocketClient(string ipAddress, int port)
new WebSocketClient(string ipAddress, int port, string url)
new WebSocketClient(string url) // ws:// or wss://
```

Use a URL such as:

```text
ws://127.0.0.1:2000/A
wss://www.example.com:443/path
```

## Step 6: publish binary data

Server:

```csharp
server.PublishAllClientPayload(byteArray);
server.PublishClientPayload("A", byteArray);
```

Client:

```csharp
client.OnClientApplicationMessageReceive += message =>
{
    byte[] payload = message.Payload;
    int value = BitConverter.ToInt32(payload, 0);
};
```

## Step 7: publish only to one session

```csharp
server.OnClientConnected += session =>
{
    server.SendClientPayload(session, "welcome");
};
```

## Step 8: enable topic wildcard

```csharp
server.TopicWildcard = true;
```

This allows clients to subscribe using MQTT-style wildcard topics such as
`A/#` and `A/+`.

## Step 9: enable WebSocket compression

Server:

```csharp
server.DataCompress = true;
```

Client:

```csharp
client.SupportDeflate = true;
client.AutoDecompress = true;
```

## Step 10: use wss with SSL/TLS

Server with a `.pfx` certificate:

```csharp
WebSocketServer server = new WebSocketServer();
server.OnClientConnected += Server_OnClientConnected;
server.UseSSL(@"D:\cert\server.pfx", "password");
server.ServerStart(2000);
```

Client with a certificate file:

```csharp
WebSocketClient client = new WebSocketClient("wss://127.0.0.1:2000/A");
client.UseSSL(@"D:\cert\client.cer");
```

Client with a certificate object:

```csharp
client.UseSSL(new X509Certificate2(@"D:\cert\client.cer"));
```

Use `wss://` when the page is served over HTTPS.

## Step 11: send custom WebSocket frames

```csharp
// text frame
client.SendServer("message");

// text frame without mask
client.SendServer(mask: false, message: "message");

// custom opcode and raw payload
client.SendServer(opCode: 0x02, mask: true, payload: bytes);
```

## Notes

- `WebSocketServer` inherits the TCP server and uses `ServerStart(port)`.
- `WebSocketClient` must call `ConnectServer()` after construction.
- The URL path can act as a topic when the server adds it in
  `OnClientConnected`.
- Server event names and client event names are different:
  - Server: `OnClientConnected(WebSocketSession session)`
  - Client: `OnClientConnected()`
- The server also provides async publish methods:
  `PublishAllClientPayloadAsync`, `PublishClientPayloadAsync`.
- `PublishClientBinary` sends a raw binary WebSocket frame.

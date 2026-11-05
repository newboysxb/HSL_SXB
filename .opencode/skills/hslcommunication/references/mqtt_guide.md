# MQTT guide

HslCommunication provides an MQTT broker/server and MQTT clients for real-time
data distribution, request/response RPC calls, and file transfer.

Important classes:

| Class | Purpose |
| --- | --- |
| `MqttServer` | MQTT server/broker |
| `MqttClient` | Publish/subscribe client |
| `MqttSyncClient` | Synchronous request/response and file client |
| `MqttConnectionOptions` | Connection options for clients |
| `MqttApplicationMessage` | Message with topic and payload |

Namespace:

```csharp
using HslCommunication.MQTT;
```

## Step 1: start an MqttServer

```csharp
using HslCommunication.MQTT;

MqttServer mqttServer = new MqttServer();
mqttServer.MaxMqttPacketSize = 1024 * 1024; // optional; -1 means unlimited
mqttServer.ServerStart(1883);

Console.ReadLine();
```

`ServerStart(port)` is inherited from the TCP server base class. The default
MQTT port used in examples is `1883`.

`MaxMqttPacketSize` limits the remaining-length field of incoming MQTT packets.
The default is `-1` (unlimited). Set a positive limit in a service exposed to
untrusted clients to avoid oversized-packet memory pressure.

## Step 2: connect an MqttClient

```csharp
MqttClient mqttClient = new MqttClient(new MqttConnectionOptions
{
    IpAddress = "127.0.0.1",
    Port = 1883,
    ClientId = "client-01"
});

OperateResult connect = mqttClient.ConnectServer();
if (!connect.IsSuccess)
{
    Console.WriteLine("Connect failed: " + connect.Message);
    return;
}

// ...

mqttClient.ConnectClose();
```

`MqttConnectionOptions` defaults:

```csharp
IpAddress = "127.0.0.1"
Port = 1883
ConnectTimeout = 5000
KeepAlivePeriod = 100 seconds
KeepAliveSendInterval = 30 seconds
CleanSession = true
```

`MqttConnectionOptions.WillMessage` is an `MqttApplicationMessage`; its
`Payload` remains `byte[]`. Do not convert the will payload through a string
when binary data must be preserved.

## Step 3: add account verification

On the server, subscribe to `ClientVerification`. Return `0` for success and a
non-zero MQTT error code for failure:

```csharp
mqttServer.ClientVerification += (session, clientId, userName, password) =>
{
    if (userName == "admin" && password == "123456") return 0;
    return 4; // bad user name or password
};
```

On the client, provide credentials:

```csharp
Credentials = new MqttCredential("admin", "123456")
```

The client ID can also be checked in `ClientVerification`.

## Step 4: use RSA encryption between HSL clients

When both sides use HslCommunication, enable `UseRSAProvider` to encrypt the
login credentials:

```csharp
MqttClient mqttClient = new MqttClient(new MqttConnectionOptions
{
    IpAddress = "127.0.0.1",
    Port = 1883,
    Credentials = new MqttCredential("admin", "123456"),
    UseRSAProvider = true
});
```

This option is intended for communication with an HslCommunication `MqttServer`.

## Step 5: publish data from the server

The server publishes to subscribed clients:

```csharp
int tick = 0;

while (true)
{
    Thread.Sleep(1000);
    byte[] payload = Encoding.UTF8.GetBytes(tick.ToString());

    mqttServer.PublishTopicPayload("A", payload);
    tick++;
}
```

Other publish overloads:

```csharp
mqttServer.PublishTopicPayload(topic, payload, retain);
mqttServer.PublishTopicPayload(topic, payload, retain, session => session.UserName == "admin");
mqttServer.PublishTopicPayload(clientId, topic, payload, retain);
mqttServer.PublishAllClientTopicPayload(topic, payload, retain);
```

## Step 6: subscribe from the client

```csharp
mqttClient.OnMqttMessageReceived += (client, message) =>
{
    Console.WriteLine("Topic: " + message.Topic);
    Console.WriteLine("Payload: " + Encoding.UTF8.GetString(message.Payload));
};

OperateResult subscribe = mqttClient.SubscribeMessage("A");
```

Subscribe to multiple topics:

```csharp
mqttClient.SubscribeMessage(new string[] { "A", "B", "C" });
```

Unsubscribe:

```csharp
mqttClient.UnSubscribeMessage("A");
```

`MqttApplicationMessage` fields:

```csharp
string Topic
byte[] Payload
bool Retain
MqttQualityOfServiceLevel QualityOfServiceLevel
```

## Step 7: control subscription on the server

`OnClientSubscribe` is triggered when a client subscribes. Return `0` to allow
the subscription, or return another value to reject it:

```csharp
mqttServer.OnClientSubscribe += (session, topics) =>
{
    if (topics.Contains("B")) return 1;
    return 0;
};
```

This event requires HslCommunication V12.6.4 or later.

## Step 8: resubscribe after reconnect

Use `OnClientConnected` to subscribe immediately after every successful
connection:

```csharp
mqttClient.OnClientConnected += client =>
{
    client.SubscribeMessage("A");
};

OperateResult connect = mqttClient.ConnectServer();
```

This avoids losing subscriptions after network reconnection.

## Step 9: use different payload formats

### Text payload

```csharp
mqttServer.PublishTopicPayload(
    "A",
    Encoding.UTF8.GetBytes(value1 + "," + value2 + "," + value3));
```

Client:

```csharp
string text = Encoding.UTF8.GetString(message.Payload);
string[] values = text.Split(',');
```

### Binary payload

```csharp
byte[] payload = new byte[12];
BitConverter.GetBytes(value1).CopyTo(payload, 0);
BitConverter.GetBytes(value2).CopyTo(payload, 4);
BitConverter.GetBytes(value3).CopyTo(payload, 8);

mqttServer.PublishTopicPayload("A", payload);
```

Client:

```csharp
int value1 = BitConverter.ToInt32(message.Payload, 0);
int value2 = BitConverter.ToInt32(message.Payload, 4);
int value3 = BitConverter.ToInt32(message.Payload, 8);
```

### JSON payload

```csharp
using Newtonsoft.Json.Linq;

JObject json = new JObject();
json["Value1"] = 1;
json["Value2"] = 2;

mqttServer.PublishTopicPayload("A", Encoding.UTF8.GetBytes(json.ToString()));
```

Client:

```csharp
JObject json = JObject.Parse(Encoding.UTF8.GetString(message.Payload));
int value1 = json.Value<int>("Value1");
```

## Step 10: subscribe by topic in separate controls

`MqttClient` also supports receiving a topic through `GetSubscribeTopic`:

```csharp
OperateResult subscribe = mqttClient.SubscribeMessage("A");

SubscribeTopic topic = mqttClient.GetSubscribeTopic("A");
if (topic != null)
{
    topic.OnMqttMessageReceived += (client, message) =>
    {
        Console.WriteLine(Encoding.UTF8.GetString(message.Payload));
    };
}
```

When closing a view, remove the event handler before unsubscribing:

```csharp
SubscribeTopic topic = mqttClient.GetSubscribeTopic("A");
if (topic != null)
{
    topic.OnMqttMessageReceived -= Handler;
    if (topic.RemoveSubscribeTick() <= 0)
    {
        mqttClient.UnSubscribeMessage("A");
    }
}
```

## Step 11: expose RPC APIs on the server

```csharp
using HslCommunication.Reflection;

public class Program
{
    public static void Main()
    {
        MqttServer mqttServer = new MqttServer();
        mqttServer.ServerStart(1883);

        mqttServer.RegisterMqttRpcApi("Api", typeof(Program));
    }

    [HslMqttApi(Description = "Calculate")]
    public static OperateResult<int> Calculate(
        ISessionContext context,
        int a,
        int b)
    {
        if (context.UserName != "admin")
        {
            return new OperateResult<int>("No permission");
        }

        return OperateResult.CreateSuccessResult(a * 10 + b * 2);
    }
}
```

Registering:

```csharp
mqttServer.RegisterMqttRpcApi("Api", typeof(Program));
mqttServer.RegisterMqttRpcApi("Api", this); // instance methods
mqttServer.RegisterMqttRpcApi(typeof(Program)); // default topic name
```

Use `[HslMqttApi]` to mark callable methods.

## Step 12: call RPC APIs with MqttSyncClient

```csharp
MqttSyncClient rpc = new MqttSyncClient(new MqttConnectionOptions
{
    IpAddress = "127.0.0.1",
    Port = 1883,
    Credentials = new MqttCredential("admin", "123456"),
    UseRSAProvider = true
});

OperateResult connect = rpc.ConnectServer();
if (!connect.IsSuccess) return;

OperateResult<int> result =
    rpc.ReadRpc<int>("Api/Calculate", new { a = 1, b = 2 });

rpc.ConnectClose();
```

The server can also return richer errors by returning `OperateResult<T>`.

## Step 13: enable the file server

On the server:

```csharp
using System.IO;

MqttServer mqttServer = new MqttServer();
mqttServer.UseFileServer(
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files"));
mqttServer.ServerStart(1883);
```

File-server paths containing `..` are rejected, and the existing invalid
path-character validation remains active. Generate normalized relative paths
inside the configured file root rather than accepting arbitrary client paths.

The server event receives file operation notifications:

```csharp
mqttServer.OnFileChangedEvent += (session, operateInfo) =>
{
    Console.WriteLine(operateInfo.Operate);
    Console.WriteLine(string.Join(",", operateInfo.FileNames));
};
```

## Step 14: file upload, download, and delete

File operations are implemented by `MqttSyncClient`:

```csharp
OperateResult upload = rpc.UploadFile(
    @"D:\data\report.txt",
    "Files/Report",
    "report",
    null);

OperateResult download = rpc.DownloadFile(
    "Files/Report",
    "report.txt",
    null,
    @"D:\download\report.txt");

OperateResult delete = rpc.DeleteFile(
    "Files/Report",
    "report.txt");
```

List folders and files:

```csharp
OperateResult<string[]> folders = rpc.DownloadPathFolders("Files");
OperateResult<GroupFileItem[]> files = rpc.DownloadPathFileNames("Files/Report");
```

Async file methods also exist, such as `UploadFileAsync`,
`DownloadFileAsync`, and `DeleteFileAsync`.

## Step 15: service redirection

`MqttServer` can redirect clients to another MQTT server:

```csharp
mqttServer.ServiceRedirection += (session, clientId, userName, password) =>
{
    if (session.Protocol == "MQTT") return null;

    return new System.Net.IPEndPoint(
        System.Net.IPAddress.Parse("127.0.0.1"),
        1883);
};
```

Returning `null` keeps the client on the current server. This is useful for
server migration or load balancing. The feature requires HslCommunication
V12.6.3 or later.

## Notes

- `MqttClient` is suitable for publish/subscribe.
- `MqttSyncClient` is suitable for request/response RPC and file operations.
- File operations open separate connections internally and do not require a
  persistent `ConnectServer()` session.
- `UseRSAProvider` is only for HSL MQTT clients talking to HSL MqttServer.
- `UseSSL`, `CertificateFile`, and `SSLSecure` are for SSL/TLS connections.
- `MqttApplicationMessage.Payload` and will payloads are byte arrays; binary
  payloads are not converted through a string before publication.
- `OnClientSubscribe` requires V12.6.4 or later.
- `ServiceRedirection` requires V12.6.3 or later.

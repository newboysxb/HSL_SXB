# SECS/HSMS guide

HslCommunication implements SECS message encoding plus the HSMS transport
used by SEMI equipment. `SecsHsms` is the active TCP client, normally called
the HOST side. `SecsHsmsServer` is the TCP listener that simulates an
equipment side and replies to SECS requests. The SECS message body is modeled
by `SecsValue`, and a received message is represented by `SecsMessage`.

Important classes:

| Class | Purpose |
| --- | --- |
| `SecsHsms` | SECS/HSMS client; connects to equipment and sends S/F requests |
| `SecsHsmsServer` | SECS/HSMS equipment simulator; accepts clients and replies |
| `SecsValue` | Hierarchical SECS data item used as message body |
| `SecsMessage` | Parsed SECS message with stream, function, and system bytes |
| `Gem` | Convenience methods for common GEM S1/S2 operations |
| `OnlineData` | Parsed model/version data from S1F1 or S1F13 replies |
| `VariableName` | Parsed SVID/name/units data from S1F11 replies |

Namespaces used in this guide:

```csharp
using HslCommunication;
using HslCommunication.Secs;
using HslCommunication.Secs.Types;
using HslCommunication.Core.Net;
using HslCommunication.Core.Pipe;
using System;
using System.Text;
using System.Threading.Tasks;
```

The HslCommunicationDemo project contains two matching test pages:
`FormSecsGem` for the `SecsHsms` HOST client and `FormSecsHsmsServer` for the
equipment server. Their message trees show the S/F codes, W-bit settings, and
example data bodies used by the library. Use those pages to verify a real
equipment side before writing a production integration.

## SECS message model

A SECS message is identified as `SxFy`. S1 is equipment status, S2 is
equipment control, S5 is alarms, and S6 is trace/event reports. The stream and
function numbers themselves are not PLC addresses; the body format is defined
by the SEMI standard or by the equipment supplier.

The normal transaction convention is:

- The initiating side sends an odd function such as S1F1.
- The responding side sends the next even function such as S1F2.
- A reply reuses the same system bytes (`MessageID`) as the request.
- The W bit (`wBit`) means the sender expects a reply.

`SecsHsms.ReadSecsMessage` sends a request with the W bit and waits for the
reply that has the same system bytes. `SendByCommand` sends a message and
returns after the bytes are written; it does not wait for a business reply.

## Step 1: create a SecsValue body

Scalar and array convenience constructors:

```csharp
// Scalar values
new SecsValue((sbyte)1);    // I1
new SecsValue((byte)2);     // U1
new SecsValue((short)3);    // I2
new SecsValue((ushort)4);   // U2
new SecsValue(5);           // I4
new SecsValue((uint)6);     // U4
new SecsValue((long)7);     // I8
new SecsValue((ulong)8);    // U8
new SecsValue((float)9);    // F4
new SecsValue((double)10);  // F8
new SecsValue("ABC");       // A
new SecsValue(true);        // BOOLEAN

// Typed arrays become arrays of the same item type
new SecsValue(new uint[] { 1, 2, 3 }); // U4[3]
new SecsValue(new sbyte[] { -1, 2, 3 }); // I1[3]
new SecsValue(new string[] { "A", "B" }); // List of two A items
new SecsValue(new bool[] { true, false });
```

Important constructor details:

- `new SecsValue(byte)` creates a U1 scalar.
- `new SecsValue(byte[])` creates SECS Binary (`B`), not a U1 array.
- To create a U1 array explicitly, pass the item type:
  `new SecsValue(SecsItemType.Byte, new byte[] { 1, 2 })`.
- `new SecsValue(string[])` creates a `List` whose members are ASCII items.
- `new SecsValue(object[])` creates a `List`. Nested `object[]` values create
  nested lists, which is how complex SML documents are represented.
- `new SecsValue()` creates an empty `None` value with no data.
- `SecsValue.EmptySecsValue()` is the same empty value.
- `SecsValue.EmptyListValue()` creates an empty `List` item.

Nested list example used for an S1F19-style request:

```csharp
SecsValue secsValue = new SecsValue(
    new object[]
    {
        "A Carrier",
        new object[] { "Job0001", "Job0002" },
        new object[] { "SourceURL", "SourceURL2" }
    });
```

`SecsValue` properties:

```csharp
SecsItemType ItemType
int Length
object Value
```

For a list item, `Value` is a `SecsValue[]`. For a typed scalar/array item,
`Value` is the corresponding scalar or array. `Length` reports the item length
used by the object, such as the member count of a list or the element count of
an array.

Useful conversions:

```csharp
System.Xml.Linq.XElement xml = secsValue.ToXElement();
SecsValue fromXml = new SecsValue(xml);

byte[] source = secsValue.ToSourceBytes();              // SECS-2 item bytes
SecsValue fromSource = SecsValue.ParseFromSource(source, Encoding.ASCII);

string sml = secsValue.ToSMLString();                   // SML display text
string code = secsValue.ToSourceCode(format: true);     // equivalent C# code
```

`ToSourceBytes(Encoding)` and `ParseFromSource(byte[], Encoding)` select the
encoding used by ASCII items.

## Step 2: start SecsHsmsServer

The server waits for HSMS clients and raises `OnSecsMessageReceived` for each
received message. Control messages with stream 0 and function 0 are also
raised after the library writes the automatic reply:

```csharp
private static SecsHsmsServer server;

public static void StartEquipmentServer()
{
    server = new SecsHsmsServer();
    server.DeviceId = 1;
    server.OnSecsMessageReceived += Server_OnSecsMessageReceived;
    server.ServerStart(5000);
}

private static void Server_OnSecsMessageReceived(
    object sender,
    PipeSession session,
    SecsMessage message)
{
    if (message.StreamNo == 1 && message.FunctionNo == 1)
    {
        // S1F1 Are You Online -> S1F2 with MDLN and SOFTREV
        server.SendByCommand(
            session,
            message,
            1,
            2,
            new SecsValue(new string[] { "MDLN", "SOFTREV" }));
    }
    else if (message.StreamNo == 2 && message.FunctionNo == 17)
    {
        // S2F17 Date and Time Request -> S2F18 with a time string
        server.SendByCommand(
            session,
            message,
            2,
            18,
            new SecsValue(DateTime.Now.ToString("yyyyMMddHHmmssff")));
    }
    else if (message.StreamNo == 1 && message.FunctionNo == 13)
    {
        // S1F13 Establish Communications -> S1F14
        server.SendByCommand(
            session,
            message,
            1,
            14,
            new SecsValue(new object[]
            {
                new byte[] { 0x00 },          // COMMACK, 0 = accepted
                new SecsValue(new object[] { "gemsim", "1.0" })
            }));
    }
}
```

`SendByCommand(session, receiveMessage, ...)` copies the message ID from the
received request. Pass the reply function explicitly, normally
`message.FunctionNo + 1`.

Common server properties:

```csharp
server.DeviceId = 1;            // ushort, default 1; used by PublishSecsMessage
server.StringEncoding = Encoding.ASCII;
server.SessionsMax = 100;       // inherited from CommunicationServer
```

Server lifecycle methods inherited from the TCP server base:

```csharp
server.ServerStart(5000);
server.ServerClose();
```

The server handles HSMS `S0F0` control replies such as `select.req`,
`linktest.req`, and `separate.req` internally. Those control messages are still
visible in `OnSecsMessageReceived` for custom handling.

## Step 3: publish from server to all clients

```csharp
// S1F1 broadcast without requiring a reply
OperateResult publish = server.PublishSecsMessage(
    1,
    1,
    SecsValue.EmptySecsValue());

// Broadcast with W bit; returns the generated message ID
OperateResult<uint> publish2 = server.PublishSecsMessage(
    5,
    1,
    new SecsValue(new object[]
    {
        new byte[] { 0x00 },
        (uint)1000,
        "sensor timeout at load elevator"
    }),
    wBit: true);
```

Use the session-aware overload to publish to one connected client only:

```csharp
PipeSession[] sessions = server.GetPipeSessions();
if (sessions != null && sessions.Length > 0)
{
    OperateResult send = server.PublishSecsMessage(
        sessions[0],
        5,
        1,
        new SecsValue(new object[]
        {
            new byte[] { 0x00 },
            (uint)1000,
            "sensor timeout at load elevator"
        }),
        wBit: true);
}
```

To select a client by IP, iterate `GetPipeSessions()` and inspect
`session.Communication` when it is a `PipeTcpNet`:

```csharp
foreach (PipeSession item in server.GetPipeSessions())
{
    if (item.Communication is PipeTcpNet pipe &&
        pipe.IpAddress == "192.168.0.100")
    {
        server.PublishSecsMessage(
            item,
            1,
            1,
            SecsValue.EmptySecsValue(),
            wBit: true);
        break;
    }
}
```

## Step 4: server asks the client for a reply

`SecsHsmsServer.ReadSecsMessage` sends a message to one session and waits for
the reply with the same system bytes:

```csharp
OperateResult<SecsMessage> read = server.ReadSecsMessage(
    session,
    1,
    13,
    new SecsValue(new object[] { (uint)1, (uint)2 }),
    wBit: true,
    timeout: 5000);

if (read.IsSuccess)
{
    SecsValue replyValue = read.Content.GetItemValues();
}
```

Do not call this blocking method synchronously inside
`OnSecsMessageReceived`. The event is raised from the server receive path, and
blocking inside it prevents the reply from being received. Run it on a
background thread when it must be triggered from a received message:

```csharp
private static void HandleS0F0Async(PipeSession session)
{
    Task.Run(() =>
    {
        OperateResult<SecsMessage> read = server.ReadSecsMessage(
            session,
            1,
            13,
            new SecsValue(new object[] { (uint)1, (uint)2 }),
            wBit: true);
    });
}
```

`ReadSecsMessage` uses one internal wait state protected by a lock on the
server object. Concurrent blocking reads on the same `SecsHsmsServer` are
serialized.

## Step 5: connect SecsHsms as the HOST

```csharp
private static SecsHsms secs;

private static void ConnectSecs()
{
    secs = new SecsHsms("127.0.0.1", 5000);
    secs.DeviceID = 1;
    secs.StringEncoding = Encoding.ASCII;
    secs.InitializationS0F0 = true;   // send select.req during ConnectServer

    OperateResult connect = secs.ConnectServer();
    if (!connect.IsSuccess)
    {
        Console.WriteLine("Connect failed: " + connect.Message);
        return;
    }

    // ... send or read SECS messages ...

    secs.ConnectClose();
}
```

Client properties:

```csharp
ushort DeviceID
Gem Gem
bool InitializationS0F0
Encoding StringEncoding
bool AutoBackS1F1
```

Common network properties inherited from the TCP base: `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, and `LogNet`.

`InitializationS0F0` defaults to `false`. When it is `true`, connecting sends
the HSMS `select.req` control message automatically.

## Step 6: read a reply with ReadSecsMessage

Low-level question/answer access:

```csharp
OperateResult<SecsMessage> read = secs.ReadSecsMessage(
    1,
    13,
    new SecsValue(new object[] { (uint)1, (uint)2 }),
    true);

if (!read.IsSuccess)
{
    Console.WriteLine("Read failed: " + read.Message);
    return;
}

SecsValue secsValue = read.Content.GetItemValues();
Console.WriteLine(secsValue.ToString());   // XML form of the parsed body
```

The response object is a `SecsMessage`:

```csharp
ushort DeviceID
bool R
bool W
bool E
byte StreamNo
byte FunctionNo
int BlockNo
uint MessageID
byte[] Data
Encoding StringEncoding
SecsValue GetItemValues()
SecsValue GetItemValues(Encoding encoding)
```

To parse a returned `List`, inspect `ItemType` and cast `Value`:

```csharp
SecsValue root = read.Content.GetItemValues();
if (root.ItemType == SecsItemType.List &&
    root.Value is SecsValue[] items)
{
    foreach (SecsValue item in items)
    {
        Console.WriteLine(item.ToString());
    }
}
```

`SendByCommand` is the fire-and-forget counterpart:

```csharp
OperateResult<uint> send = secs.SendByCommand(
    2,
    31,
    new SecsValue(DateTime.Now.ToString("yyyyMMddHHmmss")),
    true);
```

The returned `OperateResult<uint>` contains the generated message ID
(`Content`) after a successful send.

Async variants are compiled when the target framework supports `Task` and
async/await, for example .NET Framework 4.5 and newer or .NET Standard:

```csharp
Task<OperateResult> sendTask =
    secs.SendByCommandAsync(2, 31, new SecsValue("20260903120000"), true);

Task<OperateResult<SecsMessage>> readTask =
    secs.ReadSecsMessageAsync(2, 17, SecsValue.EmptySecsValue(), true);
```

The async `SendByCommandAsync` overloads return `Task<OperateResult>`, so they
do not expose the generated message ID that the synchronous overload returns
through `OperateResult<uint>`.

## Step 7: use Gem convenience methods

`secs.Gem` wraps common GEM flows with the same client:

```csharp
OperateResult<OnlineData> online = secs.Gem.S1F1_AreYouThere();
if (online.IsSuccess)
{
    Console.WriteLine(online.Content.ModelType);
    Console.WriteLine(online.Content.SoftVersion);
}

OperateResult<VariableName[]> variables =
    secs.Gem.S1F11_StatusVariableNamelist(810, 811);
if (variables.IsSuccess)
{
    foreach (VariableName item in variables.Content)
    {
        Console.WriteLine($"{item.ID} {item.Name} {item.Units}");
    }
}

OperateResult<OnlineData> established = secs.Gem.S1F13_EstablishCommunications();
OperateResult<byte> offline = secs.Gem.S1F15_OfflineRequest();
OperateResult<byte> requestOnline = secs.Gem.S1F17_OnlineRequest();
OperateResult<SecsValue> equipmentConstant = secs.Gem.S2F13_EquipmentConstantRequest();
```

Available `Gem` methods:

```csharp
OperateResult<OnlineData> S1F1_AreYouThere()
OperateResult<VariableName[]> S1F11_StatusVariableNamelist()
OperateResult<VariableName[]> S1F11_StatusVariableNamelist(params int[] statusVaruableId)
OperateResult<OnlineData> S1F13_EstablishCommunications()
OperateResult<byte> S1F15_OfflineRequest()
OperateResult<byte> S1F17_OnlineRequest()
OperateResult<SecsValue> S2F13_EquipmentConstantRequest(object[] list = null)
```

For S1F15 and S1F17 the returned byte means `0: ok`, `1: refused`, and
`2: already online`.

When parsing an S1F11 body manually, `SecsValue.ToVaruableNames()` converts the
returned list directly to `VariableName[]`:

```csharp
OperateResult<SecsMessage> raw = secs.ReadSecsMessage(1, 11, SecsValue.EmptySecsValue(), true);
if (raw.IsSuccess)
{
    VariableName[] variables = raw.Content.GetItemValues().ToVaruableNames();
}
```

## Step 8: receive equipment push messages

Register the client event before connecting:

```csharp
secs.OnSecsMessageReceived += Secs_OnSecsMessageReceived;

OperateResult connect = secs.ConnectServer();
if (!connect.IsSuccess)
{
    return;
}
```

```csharp
private static void Secs_OnSecsMessageReceived(
    object sender,
    SecsMessage secsMessage)
{
    // Example: equipment sends S5F1 Alarm Report Send
    if (secsMessage.StreamNo == 5 && secsMessage.FunctionNo == 1)
    {
        SecsValue root = secsMessage.GetItemValues();
        if (root.ItemType == SecsItemType.List &&
            root.Value is SecsValue[] alarms)
        {
            foreach (SecsValue alarm in alarms)
            {
                SecsValue[] attrs = alarm.Value as SecsValue[];
                if (attrs == null) continue;
                Console.WriteLine(attrs[1].Value);   // ALID or alarm text
                Console.WriteLine(attrs[2].Value);
            }
        }
    }
}
```

The event runs on the receive thread. Do not update WinForms controls directly
in it without `Invoke`.

## Step 9: manual replies and AutoBackS1F1

`AutoBackS1F1` defaults to `true`, which makes `SecsHsms` automatically answer
S1F1, S1F13, and S2F17 when those requests are received. When this automatic
handling is enabled, those messages are answered internally and are not raised
again through `OnSecsMessageReceived`.

Set it to `false` before connecting when every reply must be decided manually:

```csharp
private static SecsHsms secs;

private static void ConfigureManualReplies()
{
    secs = new SecsHsms("127.0.0.1", 5000);
    secs.AutoBackS1F1 = false;
    secs.OnSecsMessageReceived += ManualReply;

    OperateResult connect = secs.ConnectServer();
    if (!connect.IsSuccess)
    {
        Console.WriteLine("Connect failed: " + connect.Message);
    }
}
```

Then reply from the received-message event. The
`SendByCommand(SecsMessage, SecsValue, bool back)` overload reuses the received
message ID and automatically sends `FunctionNo + 1`:

```csharp
private static void ManualReply(object sender, SecsMessage secsMessage)
{
    if (secsMessage.StreamNo == 2 && secsMessage.FunctionNo == 17)
    {
        secs.SendByCommand(
            secsMessage,
            new SecsValue(DateTime.Now.ToString("yyyyMMddHHmmssff")),
            false);
    }
}
```

## API summary for SecsHsms

Constructors:

```csharp
new SecsHsms()
new SecsHsms(string ipAddress, int port)
```

Public members:

```csharp
ushort DeviceID
Gem Gem
bool InitializationS0F0
Encoding StringEncoding
bool AutoBackS1F1

OperateResult ConnectServer()
OperateResult ConnectClose()
Task<OperateResult> ConnectServerAsync()
Task<OperateResult> ConnectCloseAsync()

OperateResult<uint> SendByCommand(byte stream, byte function, byte[] data, bool back)
OperateResult<uint> SendByCommand(byte stream, byte function, SecsValue data, bool back)
OperateResult SendByCommand(SecsMessage secsMessage, SecsValue data, bool back)

OperateResult<SecsMessage> ReadSecsMessage(byte stream, byte function, byte[] data, bool back)
OperateResult<SecsMessage> ReadSecsMessage(byte stream, byte function, SecsValue data, bool back)

OperateResult SendControlMessage(byte pType, byte sType)

Task<OperateResult> SendByCommandAsync(byte stream, byte function, byte[] data, bool back)
Task<OperateResult> SendByCommandAsync(byte stream, byte function, SecsValue data, bool back)
Task<OperateResult<SecsMessage>> ReadSecsMessageAsync(byte stream, byte function, byte[] data, bool back)
Task<OperateResult<SecsMessage>> ReadSecsMessageAsync(byte stream, byte function, SecsValue data, bool back)

event OnSecsMessageReceivedDelegate OnSecsMessageReceived
```

`SendControlMessage` is for HSMS control messages. With `pType = 0`, `sType`
can be `1` for `select.req`, `5` for `linktest.req`, or `9` for
`separate.req`.

## API summary for SecsHsmsServer

Public members:

```csharp
Encoding StringEncoding
ushort DeviceId
uint SessionsMax

void ServerStart(int port)
void ServerStart()
void ServerClose()

PipeSession[] GetPipeSessions()

OperateResult SendByCommand(PipeSession session, SecsMessage receiveMessage,
    byte stream, byte function, byte[] data)
OperateResult SendByCommand(PipeSession session, SecsMessage receiveMessage,
    byte stream, byte function, byte[] data, bool wBit)
OperateResult SendByCommand(PipeSession session, SecsMessage receiveMessage,
    byte stream, byte function, SecsValue data)
OperateResult SendByCommand(PipeSession session, SecsMessage receiveMessage,
    byte stream, byte function, SecsValue data, bool wBit)

OperateResult PublishSecsMessage(byte stream, byte function, SecsValue data)
OperateResult<uint> PublishSecsMessage(byte stream, byte function, SecsValue data, bool wBit)
OperateResult<uint> PublishSecsMessage(byte stream, byte function, SecsValue data,
    uint messageId, bool wBit)
OperateResult PublishSecsMessage(PipeSession session, byte stream, byte function,
    SecsValue data, bool wBit)

OperateResult<SecsMessage> ReadSecsMessage(PipeSession pipe,
    byte stream, byte function, SecsValue data, bool wBit, int timeout = 5000)

event SecsMessageReceivedDelegate OnSecsMessageReceived
event OnClientStatusChangeDelegate OnClientOnline
event OnClientStatusChangeDelegate OnClientOffline
```

Use the session-aware overloads of `SendByCommand` when replying to the client
that sent the current message, and the `PublishSecsMessage` overloads when the
server starts a message on its own.

## Notes

- There are no fixed register addresses in SECS. Generate the `SecsValue` body
  from the SEMI definition or the equipment supplier's S/F documentation.
- Keep `DeviceID`/`DeviceId` consistent with the equipment configuration.
- Set `StringEncoding` identically on both sides before data is exchanged.
  The Demo supports ASCII, default, UTF-8, Unicode, and GB2312.
- `SecsHsms.ReadSecsMessage` and `SecsHsmsServer.ReadSecsMessage` are blocking
  calls; use a background task when the caller must not be blocked.
- For a quick link test, call `secs.Gem.S1F1_AreYouThere()` and check
  `OperateResult.IsSuccess`.
- The Demo pages also show real S1 through S6 examples, including S2F33 Define
  Report, S2F35 Link Event Report, S5F1 Alarm Report Send, and S6F11 Event
  Report Send. Their message bodies can be used as starting templates.

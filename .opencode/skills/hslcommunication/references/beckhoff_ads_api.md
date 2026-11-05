# Beckhoff ADS clients API subset

`BeckhoffAdsNet` implements the Beckhoff ADS protocol (AMS/TCP). How you configure the AMS NetIds
depends on **how you reach the PLC** - the two supported routes are covered below.

Port notes:

- TwinCAT 2 commonly uses AMS port `801`.
- TwinCAT 3 commonly uses AMS port `851` (this is also the default `AmsPort`).

Configuration reference:

[http://www.hsltechnology.cn:7900/Doc/HslCommunication](http://www.hsltechnology.cn:7900/Doc/HslCommunication)

Namespace:

```csharp
using HslCommunication.Profinet.Beckhoff;
```

Constructors:

```csharp
new BeckhoffAdsNet()
new BeckhoffAdsNet(string ipAddress, int port)
```

Prefer the `(ipAddress, port)` constructor. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Choose the connection route first

ADS is a routed protocol: your program is one AMS node, the PLC is another. There are two ways to
reach a PLC, and they need **different** settings.

| How you reach the PLC | `IpAddress` / `Port` | Target AMS NetId | Sender AMS NetId | PLC-side route |
| --- | --- | --- | --- | --- |
| **Through the local TwinCAT router** | `127.0.0.1` / `48898` | the **PLC's** AMS NetId, for example `5.131.230.105.1.1` | **leave empty** - `PortConnect` assigns NetId + port | not required (the router forwards and already knows the PLC) |
| **Directly to the PLC's AMS/TCP port** | PLC IP / `48898` | the **PLC's** AMS NetId | **required**: your PC's own AMS NetId (use a dedicated one, see below) | **required**: add a route for that sender NetId in the PLC's TwinCAT |

In short:

- **Local router** (`127.0.0.1:48898`): the router is the ADS node, your program is a client of it.
  Leave `Sender` empty; on connect the client sends `AMS_TCP_PORT_CONNECT (0x1000)` and adopts the
  NetId **and port the router assigns**. Nothing has to be configured on the PLC.
- **Direct to the PLC**: your PC becomes an ADS node itself, so the PLC must have a **route back to
  your sender NetId**. An empty `Sender` cannot work here. Set the sender explicitly and create the
  route (TwinCAT on the PLC: *System -> Routes -> Add*, with the sender NetId and the PC's IP).

## AMS NetId configuration

### A. Through the local router (easiest, no route to configure)

```csharp
BeckhoffAdsNet ads = new BeckhoffAdsNet("127.0.0.1", 48898)
{
    UseTagCache = true
};

ads.SetTargetAMSNetId("5.131.230.105.1.1");   // the PLC
// Do NOT call SetSenderAMSNetId: connecting performs PortConnect and fills it in.

OperateResult connect = ads.ConnectServer();
if (!connect.IsSuccess) return;

string senderInUse = ads.GetSenderAMSNetId();   // e.g. "192.168.110.13.1.1:32957" - assigned by the router
```

### B. Direct to the PLC (needs a route in the PLC)

```csharp
BeckhoffAdsNet ads = new BeckhoffAdsNet("192.168.1.10", 48898)   // the PLC's IP
{
    UseTagCache = true
};

ads.SetTargetAMSNetId("192.168.1.10.1.1");          // the PLC
ads.SetSenderAMSNetId("192.168.1.100.1.2:32957");   // this PC, a dedicated NetId + a free AMS port

OperateResult connect = ads.ConnectServer();
```

Requirements for variant B:

1. **`Sender` must be set**; an empty sender means "let the peer assign one", which cannot work for a
   direct connection because the PLC has no route to a NetId it invented.
2. The sender NetId must have a **route on the PLC** (TwinCAT on the PLC side), pointing at this
   PC's IP address (`192.168.1.100` in the example).
3. Prefer a **dedicated** sender NetId (for example `192.168.1.100.1.2`, not the router's
   `192.168.1.100.1.1`). The PC's TwinCAT router already owns `.1.1`; reusing it while TwinCAT XE is
   online makes the PLC re-bind that NetId to your TCP link and **TwinCAT XE drops offline**.
4. The sender port may be omitted (`192.168.1.100.1.2`); the client then uses the local TCP port.
   Using an explicit free port (an `0x8000`-range value such as `32957`) is tidier and easier to see
   in a capture.

`SetTargetAMSNetId` / `SetSenderAMSNetId` accept AMS NetId strings such as:

```text
192.168.110.13.1.1            // 6 bytes: NetId only
192.168.110.13.1.1:32957      // 8 bytes: NetId + AMS port
```

### Rules and traps

- **Local-router mode: never pass the router's own NetId as `Sender`.** If the sender equals the
  local router's NetId (TwinCAT shows it as "Local NetId", for example `192.168.110.13.1.1`), the
  router answers `ERR_PORTDISABLED (0x12)`. Leave `Sender` empty there instead.
- **Direct mode: `Sender` is mandatory, and the route must exist.** Without the PLC-side route the
  PLC cannot deliver its answers; typically you see `ERR_TARGETMACHINENOTFOUND (0x07)`, a timeout,
  or `0x12`.
- **A hand-made sender port can be rejected by the router.** Ports outside the range the router
  manages (for example `50000` or `35888`) are not in its port table, so going through the local
  router answers `0x12 ERR_PORTDISABLED`. Only a **direct** connection may use a free port, and even
  there it must be a port nobody else uses.
- If you call `SetSenderAMSNetId` with a NetId **without** `:port`, the client uses the local TCP
  port as the AMS port. That is the convenient form for direct connections.
- `UseAutoAmsNetID = true` (both target and sender empty) asks the PLC for its NetId via
  `GET_LOCAL_NETID (0x1002)` and then does `PortConnect`. It is meant for **direct TC3 PLC**
  connections where the PLC answers that command; against the local router it makes no sense (the
  router's NetId is not the PLC's).

## Properties

```csharp
bool UseTagCache
bool UseAutoAmsNetID
int AmsPort
```

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

## AMS NetId helpers

```csharp
void SetTargetAMSNetId(string amsNetId)
void SetSenderAMSNetId(string amsNetId)
string GetTargetAMSNetId()
string GetSenderAMSNetId()
void ClearTagsCache()
```

## Connection

`BeckhoffAdsNet` is a TCP client and uses `ConnectServer()`.

```csharp
BeckhoffAdsNet ads = new BeckhoffAdsNet("127.0.0.1", 48898);
ads.SetTargetAMSNetId("5.131.230.105.1.1");
// Sender is filled by PortConnect during ConnectServer()

OperateResult connect = ads.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte[]> read = ads.Read("M100", 2);
}

ads.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples below match the HslCommunicationDemo Beckhoff ADS address examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `M100` | Absolute memory address | Bit access: `M100.0` |
| `I100` | Absolute input address | Bit access: `I100.0` |
| `Q100` | Absolute output address | Bit access: `Q100.0` |
| `s=abc` | Symbol/tag address | `abc` is a global variable |
| `s=.A` | Symbol/tag address | TwinCAT 2 global variables use a leading dot |
| `s=MAIN.A` | Symbol/tag address | `A` is inside the MAIN program |
| `i=100000` | Absolute memory offset address | |
| `ig=0xF030;8` | Index group | Q data area with offset 8 |
| `ig=0xF020;8` | Index group | I data area with offset 8 |
| `ig=0x4020;8` | Index group | M data area with offset 8, equivalent to `M8` |

Tag addresses require the `s=` prefix. **Tag names are defined in the PLC project** - they are not
free-form strings. Use `BrowseSymbolInfos` (below) to get the real list instead of guessing.

Array elements have no separate address: read the whole array and pick the element in code.
`s=MAIN.arr[2]` is **not** resolved by the ADS path (`0xF003` answers `0x710 Symbol not found`),
so use `Read("s=MAIN.arr", (ushort)(arrayLength * elementSize))` and slice the bytes.
The virtual server's direct API (`HslCommunication.Profinet.Beckhoff.BeckhoffAdsServer.Read`)
does accept the `[n]` subscript, which is handy for server-side tests only.

## Symbol (tag) browsing

```csharp
OperateResult<List<AdsSymbolInfo>> BrowseSymbolInfos(bool bufferCache = false)
OperateResult<List<AdsSymbolInfo>> BrowseSymbolStructInfos(bool bufferCache = false)
Task<OperateResult<List<AdsSymbolInfo>>> BrowseSymbolInfosAsync(bool bufferCache = false)
Task<OperateResult<List<AdsSymbolInfo>>> BrowseSymbolStructInfosAsync(bool bufferCache = false)
```

- `BrowseSymbolInfos` / `BrowseSymbolInfosAsync` return every symbol (tag) the PLC project exposes.
- `BrowseSymbolStructInfos` / `BrowseSymbolStructInfosAsync` return the **struct definitions**; use
  them to expand struct-typed symbols into their members.
- `bufferCache: true` reuses the cached symbol length, which is faster when browsing repeatedly.

`AdsSymbolInfo` shape:

| Member | Type | Meaning |
| --- | --- | --- |
| `Name` | `string` | Full symbol name, for example `CoEModeSwitch.bBusy` or `MAIN.a` |
| `TypeInfo` | `string` | Data type name, for example `BOOL`, `INT`, or a struct type name |
| `Size` | `int` | Byte size inside the PLC |
| `Buffer` | `byte[]` | Raw definition data from the symbol upload |
| `Children` | `List<AdsSymbolInfo>` | Struct members (only filled for struct definitions) |

Typical flow: browse, pick the tag whose `Name` you need, then read it with `s=` + that name.

```csharp
OperateResult<List<AdsSymbolInfo>> symbols = ads.BrowseSymbolInfos(bufferCache: true);
if (!symbols.IsSuccess) return symbols.ConvertFailed<List<AdsSymbolInfo>>();

AdsSymbolInfo tag = symbols.Content.FirstOrDefault(m => m.Name == "MAIN.a");
if (tag != null)
{
    OperateResult<float> value = ads.ReadFloat("s=" + tag.Name);
}
```

Demo equivalent: the client page has a **浏览变量 / Browse Tag** tab whose "浏览" button performs
exactly this pair of calls and shows the tree. The AI Gateway exposes it as
`client.browseSymbols` (see below).

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address, ushort[] length)
OperateResult<byte[]> Read(string address, int length)          // > 64 KB, TC3
OperateResult<byte[]> Read(string[] address, int[] length)      // batch, > 64 KB per item
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte> ReadByte(string address)

OperateResult<T> Read<T>() where T : class, new()
OperateResult<T> ReadStruct<T>(string address) where T : struct
```

Use the `int` overloads when one symbol/area is larger than 64 KB (for example a big `ARRAY OF
STRUCT` or a large `%M` range); the `ushort` overloads cannot express that length.
`Read(string[], int[])` packs several addresses into one ADS `SUMUP` read.

Inherited typed read methods such as `ReadInt16`, `ReadInt32`, `ReadFloat`,
`ReadDouble`, and `ReadString` are available through `DeviceCommunication`.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool[] value)
OperateResult Write(string address, byte value)
OperateResult WriteStruct<T>(string address, T value) where T : struct
```

Inherited typed write methods such as `Write(string, short)`,
`Write(string, int)`, `Write(string, float)`, and `Write(string, string)` are
available through `DeviceCommunication`.

## ADS special methods

```csharp
OperateResult<uint> ReadValueHandle(string address)
OperateResult<string> TransValueHandle(string address)

OperateResult<AdsDeviceInfo> ReadAdsDeviceInfo()
OperateResult<ushort, ushort> ReadAdsState()
OperateResult WriteAdsState(short state, short deviceState, byte[] data)
OperateResult ReleaseSystemHandle(uint handle)
```

`ReadValueHandle` and `TransValueHandle` require an address beginning with
`s=`. `TransValueHandle` converts a symbolic address to an `i=` memory address.

## Async methods

Async versions exist for the same operations, including `ReadAsync`,
`ReadBoolAsync`, `ReadByteAsync`, `ReadAsync<T>()`, `WriteAsync`,
`WriteStructAsync`, `ReadValueHandleAsync`, `TransValueHandleAsync`,
`ReadAdsDeviceInfoAsync`, `ReadAdsStateAsync`, `WriteAdsStateAsync`,
`ReleaseSystemHandleAsync`, and the two `BrowseSymbol*Async` methods above.

## Notes

- Verify the target AMS NetId against the PLC configuration (TwinCAT: System -> Routes, or the
  "Local NetId" of the PLC).
- `Sender`: empty is correct **only through the local router**. A direct PLC connection must set it
  and register a route on the PLC; see "Choose the connection route first".
- TwinCAT 2 commonly uses AMS port `801`; TwinCAT 3 commonly uses `851`.
- The Demo's virtual `BeckhoffAdsServer` answers `PortConnect` and does not enforce routes, so a
  direct-IP test against that virtual server can pass with `Sender` empty. A **real** PLC will not:
  for it, set `Sender` and create the route.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `BeckhoffAdsServer`
- client page / form type: `FormBeckhoffAdsNet`
- transport: TCP, server port `48898`
- verified address for read/write: `M100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="48898" ConnectTimeOut="5000" ReceiveTimeOut="10000" Target="192.168.163.8.1.1" Sender="" TagCache="true" Interval="1000" SleepTime="0" Type="FormBeckhoffAdsNet" Name="p_FormBeckhoffAdsNet" Guid="e6757c37dd27460bb4664db846ed7b51" />
```

`IP="127.0.0.1"` + `Sender=""` is the **local-router** form: the page fills the sender in during
connect (it displays the value the router assigned), and no PLC-side route is needed. When the page
points at the **PLC's own IP** instead, `Sender` must be filled with a sender NetId that has a route
on the PLC - see the next section.

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. Use `127.0.0.1` for the local router, the PLC IP for a direct connection. |
| `Port` | `48898` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Target` | `192.168.163.8.1.1` | AMS NetId of the target (the PLC). Port defaults to `AmsPort` (`851`). |
| `Sender` | *(empty)* | Empty **only** for the local-router form (`IP=127.0.0.1`). For a direct connection fill it with a routed sender NetId. |
| `TagCache` | `true` | Enable the tag cache. |

Create it with one call, then connect (create never connects by itself). Local-router form:

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormBeckhoffAdsNet",
    "name": "FormBeckhoffAdsNet (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "48898",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Target": "5.131.230.105.1.1",
      "Sender": "",
      "TagCache": "true"
    }
  }
}
```

Direct-to-PLC form (the PLC needs a route for `Sender`):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormBeckhoffAdsNet",
    "name": "FormBeckhoffAdsNet direct (AI)",
    "parameters": {
      "IpAddress": "192.168.1.10",
      "Port": "48898",
      "PipeSetting": "TcpPipe",
      "Target": "192.168.1.10.1.1",
      "Sender": "192.168.1.100.1.2:32957",
      "TagCache": "true"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`.

### Listing the PLC's tags through the gateway

Tags live in the PLC project, so the agent should **browse** them instead of guessing names:

```json
{"operation":"client.browseSymbols","parameters":{"clientId":"<clientId>"}}
```

Response (trimmed):

```jsonc
{
  "success": true, "clientId": "...", "device": "FormBeckhoffAdsNet (AI)",
  "count": 12, "structCount": 17, "elapsedMs": 16,
  "tab": { "title": "浏览变量", "activated": true },
  "symbols": [ { "name": "CoEModeSwitch.bBusy", "type": "BOOL", "size": 1 } ],
  "structs": [ { "name": "ST_Test", "type": "", "size": 8, "children": [ ... ] } ],
  "note": "Same data as clicking 浏览 on the 浏览变量 tab. Read a symbol with address 's=<name>' ..."
}
```

`client.browseSymbols` switches the page to the **浏览变量** tab for the user, performs the same
two uploads as the button (symbol lengths + symbol table), refreshes the tree on screen, and
returns the same data as JSON. Pass `"switchTab": false` to keep the currently visible tab.
Errors: `cli_not_found`, `cli_not_connected`, `cli_browse_unsupported` (page is not a
`BeckhoffAdsNet`, or it has no browse tab), `cli_browse_failed`.

Then read/write through `/api/read` and `/api/write` with the same `clientId`, and finish with
`client.disconnect` then `client.remove`.

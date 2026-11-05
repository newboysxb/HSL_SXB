# Omron Fins clients API subset

Namespace:

```csharp
using HslCommunication.Profinet.Omron;
```

Constructors:

```csharp
new OmronFinsNet()
new OmronFinsNet(string ipAddress, int port)

new OmronFinsUdp()
new OmronFinsUdp(string ipAddress, int port)
```

| Class | Frame format | Transport |
| --- | --- | --- |
| `OmronFinsNet` | FINS TCP | TCP |
| `OmronFinsUdp` | FINS UDP | UDP |

The usual FINS port is `9600`, but the PLC may be configured differently and must
be restarted after changing it. Parameterless network constructors inherit
`127.0.0.1:5000`, so always pass the port explicitly.

## Connection notes

- Many FINS TCP ports support only two clients online at the same time.
- FINS UDP is connectionless; concurrent access from multiple threads can cause
  packet interleaving. Serialize UDP access or keep only one reader.
- FINS UDP, the parameter `SA1` is very important. It represents the last octet of the local IP address. 
  For example, if the local IP address is `192.168.0.100`, SA1 should be set to `100`. 
  If error code `8456` is returned during communication, you need to configure 
  "Dynamic change of remote IP address" to "Convert" on the PLC side.

Properties:

```csharp
byte ICF
byte RSV
byte GCT
byte DNA
byte DA1
byte DA2
byte SNA
byte SA1
byte SA2
byte SID
int ReadSplits
OmronPlcType PlcType
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

Connection:

```csharp
OmronFinsNet finsTcp = new OmronFinsNet("192.168.1.10", 9600);
OperateResult connect = finsTcp.ConnectServer();

OmronFinsUdp finsUdp = new OmronFinsUdp("192.168.1.10", 9600);

// FINS UDP has no ConnectServer()/ConnectClose() methods. Read and write directly.
OperateResult<byte[]> readUdp = finsUdp.Read("D0", (ushort)1);
```

Methods:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address)
OperateResult Write(string address, byte[] value)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult Write(string address, bool[] value)
OperateResult Run()
OperateResult Stop()
OperateResult<OmronCpuUnitData> ReadCpuUnitData()
```

`ReadCpuUnitData()` requires commercial authorization and returns
`InsufficientPrivileges` when the library is not activated.

Typed methods inherited from `DeviceCommunication` are also available.

## Address rules

This list matches the HslCommunicationDemo Omron FINS address examples.

| Address | Type | Notes |
| --- | --- | --- |
| `D0` / `DM0` | DM area | `DM10.11` reads a bit; bit suffix accepts hexadecimal `0`-`F` |
| `C0` / `CIO0` | CIO area | `C10.11` reads a bit; bit suffix accepts hexadecimal `0`-`F` |
| `W0` / `WR0` | Work area | `W10.11` reads a bit; bit suffix accepts hexadecimal `0`-`F` |
| `H0` / `HR0` | Holding bit area | `H10.11` reads a bit; bit suffix accepts hexadecimal `0`-`F` |
| `A0` / `AR0` | Auxiliary bit area | `A10.11` reads a bit; bit suffix accepts hexadecimal `0`-`F` |
| `E0.0` / `EM0.0` | EM area | `EM1.0.0` reads a bit |
| `E100` / `EM100` | EM current bank | `E0..E32767` is current bank |
| `TIM0` | Timer area | Bit is completion flag, word is current value |
| `CNT0` | Counter area | Bit is completion flag, word is current value |
| `IR0` | Index register | Word only |
| `DR0` | Data register | Word only |
| `CF1.2` | Condition flags | Bit only |

EM block numbers are hexadecimal. `E10.100` means EM block `0x10`, word
`100`; it is not EM block 10 in decimal. Dot bit suffixes accept `0`-`F`, so
`DM10.A` means bit 10 and `DM10.F` means bit 15.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `OmronFinsServer`
- client page / form type: `FormOmron`
- transport: TCP, server port `9600`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="9600" ConnectTimeOut="5000" ReceiveTimeOut="10000" UnitNumber="0" DataFormat="2" ReceiveUntilEmpty="false" IsStringReverseByteWord="true" OmronType="0" Interval="1000" SleepTime="0" Type="FormOmron" Name="p_FormOmron" Guid="8d6d9f74109c4da7b73d2ef353577eca" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `9600` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `UnitNumber` | `0` | -  |
| `DataFormat` | `2` | Byte/word order enum index. Some protocol servers only accept a specific value. |
| `ReceiveUntilEmpty` | `false` | -  |
| `IsStringReverseByteWord` | `true` | Reverse byte/word order for strings. |
| `OmronType` | `0` | CPU/PLC type enum index for FINS. |

`DataFormat` (`2`) and `OmronType` (`0`) are the page's own defaults - the values in the table above are
what the page produces on its own. Pass them explicitly only if you want to be independent of the page
default; leaving them out is fine on the current build.

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormOmron",
    "name": "FormOmron (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "9600",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "UnitNumber": "0",
      "DataFormat": "2",
      "ReceiveUntilEmpty": "false",
      "IsStringReverseByteWord": "true",
      "OmronType": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

## Creating this client inside HslCommunicationDemo (AI Gateway) - UDP (FINS/UDP)

Same protocol as above but over **UDP**. Verified end-to-end against a Demo virtual server:
a value written by the client is visible on the server, and a value written on the server is
read back by the client. The XML is what the page really generates.

- Demo virtual-server type: `OmronFinsUdpServer`
- client page / form type: `FormOmronUdp`
- transport: **UDP**
- verified address for read/write: `D100` (also verified: `D0`, `DM100`, `CIO100`)

```xml
<Device PipeSetting="UdpPipe" IP="127.0.0.1" Port="9610" ReceiveTimeOut="10000" NetNumber="1" DA1="" GCT="2" DataFormat="2" StringReverse="true" OmronType="0" SID="0" Interval="1000" SleepTime="0" Type="FormOmronUdp" Name="sp_c" Guid="57b377a5a5d04777ba65784c6a161510" />
```

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `UdpPipe` | Must be `UdpPipe` for this page. |
| `IP` | `127.0.0.1` | Peer IP address. |
| `Port` | `9610` | Peer UDP port. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `NetNumber` | `1` | FINS network number. |
| `DA1` | `` | Destination node; keep empty when the server resolves it. |
| `GCT` | `2` | Gateway count. |
| `DataFormat` | `2` | Byte/word order enum index. `2` is what the working page uses. |
| `StringReverse` | `true` | Reverse the two bytes of a string. |
| `OmronType` | `0` | CPU/PLC type enum index. `0` is what the working page uses. |
| `SID` | `0` | Service id / sequence. |

**One thing must be set explicitly, or the pair silently never works:**

- On the **server** side, `virtualServer.create` must be given `"networkMode":"udp"`.
  The Omron FINS UDP server otherwise starts a **TCP** listener on the same port, so the UDP
  client reports "connected" but every read and write fails.

`DataFormat` (`2`) and `OmronType` (`0`) are the client page's own defaults, so they do not have to be
passed.

Create the pair with:

```json
{"operation":"virtualServer.create","parameters":{"type":"OmronFinsUdpServer","tcpPort":9610,"networkMode":"udp","startTcp":true}}
```

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormOmronUdp",
    "name": "OmronFinsUdp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "9610",
      "PipeSetting": "UdpPipe",
      "ReceiveTimeOut": "10000",
      "NetNumber": "1",
      "GCT": "2",
      "DataFormat": "2",
      "StringReverse": "true",
      "OmronType": "0",
      "SID": "0"
    }
  }
}
```

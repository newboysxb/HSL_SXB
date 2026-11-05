# Allen-Bradley PCCC clients API subset

`AllenBradleyPcccNet` uses PCCC commands inside the CIP transport. It is
mainly used with MicroLogix 1400 style data-file addressing and is not a
tag-based client.

The usual port is `44818`.

Namespace:

```csharp
using HslCommunication.Profinet.AllenBradley;
```

Constructors:

```csharp
new AllenBradleyPcccNet()
new AllenBradleyPcccNet(string ipAddress, int port = 44818)
```

Prefer the `(ipAddress, port)` constructor. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Connection

`AllenBradleyPcccNet` is a TCP client and uses `ConnectServer()`.

```csharp
AllenBradleyPcccNet plc = new AllenBradleyPcccNet("192.168.1.10", 44818);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("N7:0", 2);
}
plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

```csharp
uint SessionHandle
uint OTConnectionId
uint TOConnectionId
```

`AllenBradleyPcccNet` uses `RegularByteTransform`.

## Address rules

PCCC uses data-file addresses rather than tag names.

| Address | Meaning | Notes |
| --- | --- | --- |
| `A9:0` | ASCII file | |
| `B2:0` | Bit file | |
| `N2:0` | Integer file | |
| `L17:0` | Long integer file | |
| `ST2:0` | String file | |
| `F8:5` | Floating-point file | |
| `S:1/15` | Status file bit | Bit access example |
| `C2:0` | Counter file | |
| `T2:0` | Timer file | |
| `I:0/15` | Input file bit | Bit access example |
| `O:0/15` | Output file bit | Bit access example |

These examples match the HslCommunicationDemo PCCC address examples.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte> ReadByte(string address)
OperateResult<string> ReadString(string address)
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
```

For `ST` string files, use `ReadString("ST10:2")`. When length is zero, the
client reads the length from the PLC and then reads the complete string.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, byte value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, string value)
OperateResult Write(string address, string value, Encoding encoding)
```

For `ST` string writes, the client automatically prepends the two-byte length
and expands the data to an even byte length.

## Async methods

Async versions exist for the same operations:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult<byte>> ReadByteAsync(string address)
Task<OperateResult<string>> ReadStringAsync(string address)
Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult> WriteAsync(string address, byte value)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
```

## Notes

- This client is not tag-based; use PCCC data-file addresses.
- Use the Demo PCCC page to verify the address layout for the connected PLC.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `PcccServer`
- client page / form type: `FormAllenBrandlyPCCC`
- transport: TCP, server port `44819`
- verified address for read/write: `N7:0`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="44819" ConnectTimeOut="5000" ReceiveTimeOut="10000" Interval="1000" SleepTime="0" Type="FormAllenBrandlyPCCC" Name="p_FormAllenBrandlyPCCC" Guid="f98897431e31492eb394ea32ece535a2" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `44819` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormAllenBrandlyPCCC",
    "name": "FormAllenBrandlyPCCC (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "44819",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

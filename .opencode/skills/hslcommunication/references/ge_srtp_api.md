# GE SRTP API subset

`GeSRTPNet` implements the General Electric SRTP protocol over TCP.

The default port is `18245`.

Namespace:

```csharp
using HslCommunication.Profinet.GE;
```

## Constructors

```csharp
new GeSRTPNet()
new GeSRTPNet(string ipAddress, int port = 18245)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`.

## Properties

```csharp
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.

Common inherited TCP properties include `IpAddress`, `Port`, `ConnectTimeOut`,
and `ReceiveTimeOut`.

## Connection

`GeSRTPNet` is a TCP client and uses `ConnectServer()`.

```csharp
GeSRTPNet plc = new GeSRTPNet("192.168.1.10", 18245);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("R1");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

SRTP addresses start from `1`, not `0`.

The examples match the HslCommunicationDemo GE SRTP address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `I1` | Discrete inputs | Bit and byte access use separate address domains |
| `Q1` | Discrete outputs | Bit and byte access use separate address domains |
| `M1` | Discrete internals | Bit and byte access use separate address domains |
| `T1` | Discrete temporaries | Bit and byte access use separate address domains |
| `SA1` | SA discrete | Bit and byte access use separate address domains |
| `SB1` | SB discrete | Bit and byte access use separate address domains |
| `SC1` | SC discrete | Bit and byte access use separate address domains |
| `S1` | S discrete | Bit and byte access use separate address domains |
| `G1` | Genius global data | Bit and byte access use separate address domains |
| `AI1` | Analog inputs | Word data; no bit or single-byte access |
| `AQ1` | Analog outputs | Word data; no bit or single-byte access |
| `R1` | Registers | Word data; no bit or single-byte access |

The Demo UI displays `QI1` for analog outputs, but the library parser expects
`AQ1`. Use `AQ1` in generated code.

Bool access supports `I`, `Q`, `M`, `T`, `SA`, `SB`, `SC`, `S`, and `G`.
Bool access is not supported for `AI`, `AQ`, or `R`.

Byte access supports the discrete areas `I`, `Q`, `M`, `T`, `SA`, `SB`, `SC`,
`S`, and `G`. Byte access is not supported for `AI`, `AQ`, or `R`.

Word access supports all SRTP address areas, including `AI`, `AQ`, and `R`.

## Read methods

Core reads:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte> ReadByte(string address)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
```

For raw `Read(address, length)` on `AI`, `AQ`, and `R`, `length` is the byte
length, so one word requires `2`. For typed methods such as `ReadInt16`, use the
normal element count.

Inherited typed reads from `DeviceCommunication` are also available:

```csharp
OperateResult<short> ReadInt16(string address)
OperateResult<short[]> ReadInt16(string address, ushort length)
OperateResult<ushort> ReadUInt16(string address)
OperateResult<ushort[]> ReadUInt16(string address, ushort length)
OperateResult<int> ReadInt32(string address)
OperateResult<int[]> ReadInt32(string address, ushort length)
OperateResult<uint> ReadUInt32(string address)
OperateResult<uint[]> ReadUInt32(string address, ushort length)
OperateResult<float> ReadFloat(string address)
OperateResult<float[]> ReadFloat(string address, ushort length)
OperateResult<double> ReadDouble(string address)
OperateResult<double[]> ReadDouble(string address, ushort length)
OperateResult<long> ReadInt64(string address)
OperateResult<long[]> ReadInt64(string address, ushort length)
OperateResult<ulong> ReadUInt64(string address)
OperateResult<ulong[]> ReadUInt64(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
```

`ReadByte` and bool reads return an error for `AI`, `AQ`, and `R`.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, byte value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, short value)
OperateResult Write(string address, short[] values)
OperateResult Write(string address, ushort value)
OperateResult Write(string address, ushort[] values)
OperateResult Write(string address, int value)
OperateResult Write(string address, int[] values)
OperateResult Write(string address, uint value)
OperateResult Write(string address, uint[] values)
OperateResult Write(string address, float value)
OperateResult Write(string address, float[] values)
OperateResult Write(string address, double value)
OperateResult Write(string address, double[] values)
OperateResult Write(string address, long value)
OperateResult Write(string address, long[] values)
OperateResult Write(string address, ulong value)
OperateResult Write(string address, ulong[] values)
OperateResult Write(string address, string value)
```

For `AI`, `AQ`, and `R`, byte writes are word-oriented; the byte array length
should be even. Single-byte and bool writes are not supported for those areas.

## PLC time and program name

```csharp
OperateResult<DateTime> ReadPLCTime()
OperateResult<string> ReadProgramName()
```

Both methods require commercial authorization and return
`InsufficientPrivileges` when the library is not activated.

The PLC time may be in UTC and may need conversion to local time.

## Async methods

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult<byte>> ReadByteAsync(string address)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult> WriteAsync(string address, byte value)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] values)
Task<OperateResult<DateTime>> ReadPLCTimeAsync()
Task<OperateResult<string>> ReadProgramNameAsync()
```

Inherited typed async methods from `DeviceCommunication` are also available.

## Notes

- SRTP addresses are 1-based. `R0`, `I0`, and similar zero addresses are invalid.
- For raw reads and writes on `AI`, `AQ`, and `R`, use byte lengths rather than
  word counts.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `GeSRTPServer`
- client page / form type: `FormGeSRTP`
- transport: TCP, server port `18245`
- verified address for read/write: `R100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="18245" ConnectTimeOut="5000" ReceiveTimeOut="10000" Interval="1000" SleepTime="0" Type="FormGeSRTP" Name="p_FormGeSRTP" Guid="2a1963357ca24eda97ae7df51d325963" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `18245` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormGeSRTP",
    "name": "FormGeSRTP (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "18245",
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

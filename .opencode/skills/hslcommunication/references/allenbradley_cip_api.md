# Allen-Bradley CIP clients API subset

`AllenBradleyNet` and `AllenBradleyConnectedCipNet` provide EtherNet/IP CIP
tag access for Rockwell/Allen-Bradley PLCs.

- `AllenBradleyNet` uses UCMM unconnected messaging.
- `AllenBradleyConnectedCipNet` uses Class 3 connected messaging.

The usual port for both classes is `44818`.

Namespace:

```csharp
using HslCommunication.Profinet.AllenBradley;
```

If using low-level tag or enumerator types directly, also add:

```csharp
using HslCommunication.Profinet.AllenBradley;
```

Constructors:

```csharp
new AllenBradleyNet()
new AllenBradleyNet(string ipAddress, int port = 44818)

new AllenBradleyConnectedCipNet()
new AllenBradleyConnectedCipNet(string ipAddress, int port = 44818)
```

Prefer the `(ipAddress, port)` constructors. Parameterless TCP forms inherit
`127.0.0.1:5000` from `DeviceTcpNet`.

## Connection

Both classes are TCP clients and use `ConnectServer()`.

```csharp
AllenBradleyConnectedCipNet plc = new AllenBradleyConnectedCipNet("192.168.1.10", 44818);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("A1");
}
plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

`AllenBradleyNet` exposes:

```csharp
byte Slot
byte[] PortSlot
ushort CipCommand
MessageRouter MessageRouter
bool ContextCheck
bool ContextIdAutoIncrement
bool ReadArrayUseSegment
uint SessionHandle
```

`AllenBradleyConnectedCipNet` exposes the connected CIP properties inherited
from `OmronConnectedCipNet`:

```csharp
string ProductName
byte ConnectionTimeoutMultiplier
uint SessionHandle
uint OTConnectionId
uint TOConnectionId
```

Both classes use `RegularByteTransform` and default to `DataFormat.DCBA`.

## Address rules

The examples below match the HslCommunicationDemo Allen-Bradley CIP address
examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `A1` | Global tag | Type must match the PLC tag definition |
| `type=0xDA;A2` | Global tag with explicit type | Use when the tag type is special |
| `x=0x52;A3[0]` | Fragment read | Use for very large data |
| `class=0x6b;0xf68f` | Symbol instance | `class=107;63119` is equivalent |
| `class=0x6b;attr=0x03;0xf68f` | Symbol instance attribute | `class=107;attr=3;63119` is equivalent |
| `Program:MainProgram.A1` | Local tag | Prefix with the program name |
| `slot=2;A1` | Global tag with slot | Overrides the default slot |
| `i=A[0]` | Bool-array style access | Use when the PLC tag is an integer-backed bit array |
| `B[0]` | Array element | Use an index for array tags |
| `C[0,1]` | Multi-dimensional array element | Use comma-separated indexes |

For a tag whose actual PLC type is an integer array but which the application
uses as a bool array, read bool values with:

```csharp
OperateResult<bool[]> bits = plc.ReadBool("i=A[0]", 10);
```

Use `ReadTags` or HslCommunicationDemo to determine the actual tag type before
writing special tags. If the tag is named `A` and requires an explicit type,
write it with an address such as:

```csharp
OperateResult write = plc.WriteTag("type=0xD2;A", typeCode, data);
```

## AllenBradleyNet methods

Core byte and typed reads:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address)
OperateResult<byte[]> Read(string[] address, ushort[] length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<bool[]> ReadBoolArray(string address)
OperateResult<byte> ReadByte(string address)

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
```

CIP helpers:

```csharp
OperateResult<ushort, byte[]> ReadTag(string address, ushort length = 1)
OperateResult<AbTagValue[]> ReadTags(string[] address, ushort[] length)
OperateResult<byte[]> ReadSegment(string address, int startIndex, int length)
OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
OperateResult<byte[]> ReadEipFromServer(params byte[][] eip)
OperateResult<AbTagItem[]> TagEnumerator()
OperateResult<AbTagItem[]> StructTagEnumerator(AbTagItem structTag)
```

Writes:

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, byte value)
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
OperateResult Write(string address, string value, Encoding encoding)
OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
```

CIP date and time:

```csharp
OperateResult<DateTime> ReadDate(string address)
OperateResult WriteDate(string address, DateTime date)
OperateResult WriteTimeAndDate(string address, DateTime date)
OperateResult<TimeSpan> ReadTime(string address)
OperateResult WriteTime(string address, TimeSpan time)
OperateResult WriteTimeOfDate(string address, TimeSpan timeOfDate)
```

Async versions exist for the core read/write methods, CIP helpers, and date/time
methods.

## AllenBradleyConnectedCipNet methods

`AllenBradleyConnectedCipNet` inherits the connected CIP API from
`OmronConnectedCipNet` and adds Allen-Bradley string behavior.

Core reads:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address, ushort[] length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte> ReadByte(string address)
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
OperateResult<ushort, byte[]> ReadTag(string address, ushort length = 1)
OperateResult<string> ReadPlcType()
OperateResult<T> ReadStruct<T>(string address) where T : struct
```

Writes:

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, byte value)
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
OperateResult Write(string address, string value, Encoding encoding)
OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
```

CIP helpers and date/time methods match the connected CIP subset:

```csharp
OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
OperateResult<DateTime> ReadDate(string address)
OperateResult WriteDate(string address, DateTime date)
OperateResult WriteTimeAndDate(string address, DateTime date)
OperateResult<TimeSpan> ReadTime(string address)
OperateResult WriteTime(string address, TimeSpan time)
OperateResult WriteTimeOfDate(string address, TimeSpan timeOfDate)
```

Async versions exist for the same operations.

## Structure tag behavior

When an entire structure tag is read as raw bytes, the returned data starts
with a two-byte structure ID:

```text
[structure-id-low][structure-id-high][actual structure bytes...]
```

`ReadStruct<T>()` skips these two bytes before converting the remainder to a C#
structure. If raw bytes are modified and written back, the two leading structure
ID bytes must be retained.

## Authorization

Multi-address `Read(string[], ushort[])` and `ReadTags(...)` may require
commercial authorization when more than one address is requested.
`ReadStruct<T>()` is documented as a commercial feature in the connected CIP
implementation. Check the failed `OperateResult` message and use the license
activation guidance if `InsufficientPrivileges` is returned.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `CipServer`
- client page / form type: `FormAllenBrandly`
- transport: TCP, server port `44818`
- verified address for read/write: `A1`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="44818" ConnectTimeOut="5000" ReceiveTimeOut="10000" Slot="0" Router="" Context="False" Interval="1000" SleepTime="0" Type="FormAllenBrandly" Name="p_FormAllenBrandly" Guid="cf53383131cc44689f87fe4ef546f0a0" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `44818` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Slot` | `0` | Module slot number. |
| `Router` | `` | -  |
| `Context` | `False` | -  |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormAllenBrandly",
    "name": "FormAllenBrandly (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "44818",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Slot": "0",
      "Router": "",
      "Context": "False"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

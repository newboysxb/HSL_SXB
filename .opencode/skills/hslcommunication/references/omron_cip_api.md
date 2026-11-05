# Omron CIP clients API subset

Omron NJ/NX/NY PLCs expose tag-based data access through EtherNet/IP CIP.
`OmronCipNet` uses UCMM (unconnected) messages. `OmronConnectedCipNet` uses
Class 3 connected messages. Both use tag addresses defined in the PLC project.

Prefer `OmronConnectedCipNet` when the PLC supports connected CIP access.
The usual port for both classes is `44818`.

Namespace:

```csharp
using HslCommunication.Profinet.Omron;
```

Constructors:

```csharp
new OmronCipNet()
new OmronCipNet(string ipAddress, int port = 44818)

new OmronConnectedCipNet()
new OmronConnectedCipNet(string ipAddress, int port = 44818)
```

Prefer the `(ipAddress, port)` constructors. The parameterless forms inherit
`127.0.0.1:5000` from `DeviceTcpNet`.

| Class | CIP access | Transport | Recommendation |
| --- | --- | --- | --- |
| `OmronCipNet` | UCMM unconnected | TCP | Supported |
| `OmronConnectedCipNet` | Class 3 connected | TCP | Preferred |

## Connection

Both classes are TCP clients and use the standard device connection methods.

```csharp
OmronConnectedCipNet plc = new OmronConnectedCipNet("192.168.1.10", 44818);
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

`OmronCipNet` inherits `AllenBradleyNet`, so it also exposes:

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

`OmronConnectedCipNet` exposes:

```csharp
string ProductName
byte ConnectionTimeoutMultiplier
uint SessionHandle
uint OTConnectionId
uint TOConnectionId
```

## OmronCipNet methods

`OmronCipNet` inherits the full `AllenBradleyNet` API and overrides several
methods for Omron tag behavior.

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
OperateResult<string> ReadString(string address)
OperateResult<string> ReadString(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
OperateResult<T> ReadStruct<T>(string address, ushort length) where T : class, new()
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

Async versions exist for the same operations, including `ReadAsync`,
`ReadBoolAsync`, `ReadStringAsync`, `ReadTagAsync`, `ReadCipFromServerAsync`,
`WriteAsync`, `WriteTagAsync`, and the CIP date/time async methods.

## OmronConnectedCipNet methods

`OmronConnectedCipNet` is implemented specifically for connected CIP and does
not inherit all `AllenBradleyNet` helper methods.

Core byte and typed reads:

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
OperateResult<string> ReadString(string address)
OperateResult<string> ReadString(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
OperateResult<ushort, byte[]> ReadTag(string address, ushort length = 1)
OperateResult<string> ReadPlcType()
OperateResult<T> ReadStruct<T>(string address) where T : struct
```

CIP helpers:

```csharp
OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
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

Async versions exist for the same operations, including `ReadAsync`,
`ReadBoolAsync`, `ReadByteAsync`, `ReadStringAsync`, `ReadTagAsync`,
`ReadStructAsync<T>`, `ReadCipFromServerAsync`, `WriteAsync`, `WriteTagAsync`,
and the CIP date/time async methods.

## Address rules

The examples below match the HslCommunicationDemo Omron CIP address examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `A1` | Global tag | Type must match the PLC tag definition |
| `type=0xDA;A2` | Global tag with explicit CIP type | Use when the tag type is special |
| `x=0x52;A3[0]` | Fragment read | Use for very large data |
| `class=0x6b;0xf68f` | Symbol instance | `class=107;63119` is equivalent |
| `class=0x6b;attr=0x03;0xf68f` | Symbol instance attribute | `class=107;attr=3;63119` is equivalent |
| `Program:MainProgram.A1` | Local variable | Prefix with the program name |
| `slot=2;A1` | Global tag with slot | Overrides the default slot |
| `i=A[0]` | Bool array value | Use when the PLC bool array is backed by integer words |
| `B[0]` | Array element | Use an index for array tags |
| `C[0,1]` | Multi-dimensional array element | Use comma-separated indexes |

Array tags should use indexed addresses such as `A[0]` when reading a typed
array with `ReadInt16("A[0]", 10)`.

## Structure tag behavior

When an entire structure tag is read as raw bytes, the returned data starts
with a two-byte structure ID. Example raw layout:

```text
[structure-id-low][structure-id-high][actual structure bytes...]
```

`ReadStruct<T>()` already skips these two bytes before converting the remainder
to the C# structure. If an application reads the raw bytes, modifies them, and
writes the whole tag back with `Write(address, byte[])`, it must keep those two
leading structure ID bytes in the write payload.

## Authorization

`OmronCipNet` multi-address `Read(string[], ushort[])` and `ReadTags(...)` require
commercial authorization when more than one address is requested.
`OmronConnectedCipNet.Read(string[], ushort[])` explicitly requires commercial
authorization and returns `InsufficientPrivileges`. The source documentation
also marks `ReadStruct<T>()` and `ReadStructAsync<T>()` as commercial methods.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `OmronCipServer`
- client page / form type: `FormOmronCip`
- transport: TCP, server port `44820`
- verified address for read/write: `A1`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="44820" ConnectTimeOut="5000" ReceiveTimeOut="10000" Slot="0" Interval="1000" SleepTime="0" Type="FormOmronCip" Name="p_FormOmronCip" Guid="528ecae482bb4022a585fd428ded8373" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `44820` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Slot` | `0` | Module slot number. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormOmronCip",
    "name": "FormOmronCip (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "44820",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Slot": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

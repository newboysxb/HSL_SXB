# Inovance Connected CIP API subset

`InovanceConnectedCipNet` inherits `OmronConnectedCipNet` and uses Class 3
connected CIP messaging for Inovance tag access. Tag addresses are defined by
the actual PLC project.

The usual port is `44818`.

Namespace:

```csharp
using HslCommunication.Profinet.Inovance;
```

## Constructors

```csharp
new InovanceConnectedCipNet()
new InovanceConnectedCipNet(string ipAddress, int port = 44818)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`.

## Properties

```csharp
string ProductName
byte ConnectionTimeoutMultiplier
uint SessionHandle
uint OTConnectionId
uint TOConnectionId
```

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.
`ConnectionTimeoutMultiplier` defaults to `0x02`.

## Connection

```csharp
InovanceConnectedCipNet plc = new InovanceConnectedCipNet("192.168.0.110", 44818);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("A1");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address examples

The examples match the HslCommunicationDemo CIP address declarations.

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

## Read methods

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
OperateResult<T> ReadStruct<T>(string address) where T : struct
OperateResult<string> ReadPlcType()
```

CIP helper:

```csharp
OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
```

## Write methods

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

## CIP date and time methods

```csharp
OperateResult<DateTime> ReadDate(string address)
OperateResult WriteDate(string address, DateTime date)
OperateResult WriteTimeAndDate(string address, DateTime date)
OperateResult<TimeSpan> ReadTime(string address)
OperateResult WriteTime(string address, TimeSpan time)
OperateResult WriteTimeOfDate(string address, TimeSpan timeOfDate)
```

## Async methods

Async versions exist for the core read/write operations, `ReadTagAsync`,
`ReadStructAsync<T>`, `ReadCipFromServerAsync`, `WriteTagAsync`, and the CIP
date/time methods.

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

`Read(string[], ushort[])` and `ReadStruct<T>()` require commercial
authorization and return `InsufficientPrivileges` when the library is not
activated.

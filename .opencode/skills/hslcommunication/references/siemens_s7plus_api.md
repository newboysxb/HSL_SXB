# Siemens S7 Plus clients API subset

`SiemensS7Plus` is an experimental S7 Plus client. It supports some Siemens
PLCs, but support varies by PLC model and firmware. Before using it in a
project, connect with HslCommunicationDemo first and use the Browser tab to
confirm the PLC exposes the expected DB/tag structure.

The default TCP port is `102`.

Namespace:

```csharp
using HslCommunication.Profinet.Siemens;
```

If using the advanced browse/tag types directly, also add:

```csharp
using HslCommunication.Profinet.Siemens.S7PlusHelper;
```

Constructors:

```csharp
new SiemensS7Plus()
new SiemensS7Plus(string ipAddress, int port = 102)
```

The parameterless constructor uses `127.0.0.1:5000` from `DeviceTcpNet`; prefer
the `(ipAddress, port)` constructor.

## Connection

`SiemensS7Plus` is a TCP client and uses `ConnectServer()`.

```csharp
SiemensS7Plus plc = new SiemensS7Plus("192.168.1.10", 102);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("8A0E03E8.12", 0);
}
plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

```csharp
ushort LocalTSAP
byte[] DestTSAP
byte LastPDUType
int PDULength
uint SessionID
string OrderNumber
```

`SiemensS7Plus` uses `ReverseBytesTransform`.

## Address rules

S7 Plus supports two address styles:

1. RelationId memory addresses from the Demo Browser tab.
2. Tag names. Internally the client browses the PLC and resolves the tag name to
   a RelationId.

Examples from HslCommunicationDemo:

| Address | Style | Notes |
| --- | --- | --- |
| `8A0E03E8.12` | RelationId | Physical address from the Browser tab |
| `51.A` | RelationId | Physical address from the Browser tab |
| `"QArea"."急停"` | Tag name | Mapped address from the Browser tab |
| `8A0E0001.A` | RelationId | Example used by the Demo connection form |

For array or nested tags, use the Browser tab to determine the exact name and
RelationId.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
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
```

For raw `Read(address, length)`, the `length` parameter is ignored. The returned
`S7Value` buffer must be parsed according to the tag type.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, byte value)
OperateResult Write(string address, bool value)
OperateResult WriteByte(string address, byte[] value)

OperateResult Write(string address, short value)
OperateResult Write(string address, ushort value)
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
```

Raw `Write(address, byte[])` expects an S7 Plus value buffer with its flags,
type information, and data. To write plain `uint8[]` data, use `WriteByte`.

## Browse and tag resolution

```csharp
OperateResult<List<S7Object>> BrowseDB()
OperateResult<List<S7Object>> BrowseTags()
OperateResult<List<S7Object>> BrowseTags(S7Object s7Object)
OperateResult<List<S7Object>> BrowseTags(uint exploreId)

OperateResult<string> GetLIDFromTagName(string tagName)
```

Use these methods when building a Browser-like experience. `GetLIDFromTagName`
converts a tag address such as `"QArea"."急停"` into a RelationId string.

## Tag-based batch APIs

```csharp
OperateResult<List<S7Value>> ReadTags(S7Tag[] s7Tags)
OperateResult<List<S7Value>> ReadTags(string[] ids)

OperateResult WriteTag(string id, byte[] buffer)
OperateResult WriteTag(string[] ids, List<byte[]> buffers)
OperateResult WriteTag(S7Tag[] ids, List<byte[]> buffers)
```

These methods are the lower-level batch interface behind `Read` and `Write`.

## Async methods

Async versions exist for the same operations, including `ReadAsync`,
`ReadBoolAsync`, `ReadByteAsync`, `ReadStringAsync`, `WriteAsync`,
`WriteByteAsync`, `ReadTagsAsync`, and `WriteTagAsync`.

## Notes

- Verify PLC support with HslCommunicationDemo and its Browser tab first.
- String writes depend on the PLC string type. Test a known tag before using
  them in production.
- `SiemensS7Plus` depends on the C++ OpenSSL encryption library. It is currently
  only available on the .NET Framework platform and cannot be used
  cross-platform yet.
- The default binaries are for 64-bit .NET Framework. To use a 32-bit process,
  the native library must be rebuilt from source for x86.

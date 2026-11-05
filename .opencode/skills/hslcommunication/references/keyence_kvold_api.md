# Keyence KV old upper-link API subset

`KeyenceKvOld` implements the upper-link protocol for older Keyence KV models:
`KV-10xx`, `16xx`, `24xx`, `40xx`, `KV-80`, and `KV-300`, where `xx` means
`AR/AT/DR/DT`.

It is TCP-based and defaults to port `8501`.

Namespace:

```csharp
using HslCommunication.Profinet.Keyence;
```

## Constructors

```csharp
new KeyenceKvOld()
new KeyenceKvOld(string ipAddress, int port = 8501)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`.

## Properties

```csharp
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ByteTransform` is `RegularByteTransform`, defaults to `DataFormat.DCBA`, and
sets `IsStringReverseByteWord = true`.

Common inherited TCP properties include `IpAddress`, `Port`, `ConnectTimeOut`,
and `ReceiveTimeOut`.

## Connection

```csharp
KeyenceKvOld plc = new KeyenceKvOld("192.168.1.10", 8501);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("DM100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo Keyence old-model address
declarations.

### Bit addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `T0` | Timer coil | Read bool contact |
| `C0` | Counter coil | Read bool contact |
| `CTC0` | High-speed counter coil | `CTC0-CTC1` |
| `CTH0` | High-speed counter | `CTH0-CTH1` |
| `R0` | Relay | Range `R00000-R99915` |
| `R100.0` | Relay bit | Dot format is also accepted |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `DM0` | Data register | `DM000-DM999`; bit access `DM100.0` |
| `TM0` | Temporary data register | `TM00-TM99`; bit access `TM100.0` |
| `TC0` | Timer current value | `TC000-TC999` |
| `TS0` | Timer set value | `TS000-TS999` |
| `CC0` | Counter current value | `CC000-CC999` |
| `CS0` | Counter set value | `CS000-CS999` |
| `CTH0` | High-speed counter current value | `CTH0-CTH1` |

This old protocol does not support the newer Nano address set such as `MR`,
`LR`, `CR`, `EM`, `FM`, `ZF`, `W`, `VB`, or `VM`.

The source parser also supports `AT0` through `AT7` for digital trimmers,
although the old Demo address table does not show that row.

## Read methods

Core methods:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
```

Useful inherited 16-bit typed reads:

```csharp
OperateResult<short> ReadInt16(string address)
OperateResult<ushort> ReadUInt16(string address)
```

`Read(address, length)` is a single-word read. The `length` parameter is ignored
by this protocol implementation.

This class is designed for single-point and 16-bit reads. Multi-word typed reads
such as `ReadInt32`, `ReadFloat`, and `ReadDouble` are not suitable for this
old protocol and should be avoided.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, short value)
OperateResult Write(string address, ushort value)
```

Raw `byte[]` writes use only the first two bytes. Pass at least two bytes for a
single-word write.

Bool-array writes and multi-word writes are not supported by this old protocol.

## Async methods

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult<short>> ReadInt16Async(string address)
Task<OperateResult<ushort>> ReadUInt16Async(string address)
Task<OperateResult> WriteAsync(string address, short value)
Task<OperateResult> WriteAsync(string address, ushort value)
```

Only the single-word typed async methods are useful for this protocol.

## Notes

- The protocol is TCP only. There is no serial class for `KeyenceKvOld`.
- Use `ConnectServer()` to connect.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

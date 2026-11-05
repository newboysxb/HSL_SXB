# Keyence MC protocol API subset

`KeyenceMcNet` and `KeyenceMcAsciiNet` use the Mitsubishi QnA-compatible 3E
(MC) frame protocol, adapted to Keyence address conventions.

`KeyenceMcNet` uses binary MC frames. `KeyenceMcAsciiNet` uses ASCII MC frames.
Both are TCP clients and inherit the full Melsec MC read/write API.

The PLC must be configured for the matching MC protocol format, correct IP
address, and port. The Demo default port is `6000`.

Namespace:

```csharp
using HslCommunication.Profinet.Keyence;
```

## Class and transport table

| Class | Frame format | Transport |
| --- | --- | --- |
| `KeyenceMcNet` | Binary QnA 3E | TCP |
| `KeyenceMcAsciiNet` | ASCII QnA 3E | TCP |

## Constructors

```csharp
new KeyenceMcNet()
new KeyenceMcNet(string ipAddress, int port)

new KeyenceMcAsciiNet()
new KeyenceMcAsciiNet(string ipAddress, int port)
```

Prefer `(ipAddress, port)`. The parameterless forms inherit `127.0.0.1:5000`
from `DeviceTcpNet`. There is no constructor default MC port; use `6000` when
the PLC uses the usual MC TCP port.

## Properties

Both classes inherit the Melsec MC properties:

```csharp
byte NetworkNumber
byte PLCNumber
byte NetworkStationNumber
ushort TargetIOStation
bool EnableWriteBitToWordRegister
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.
The Demo also exposes `ByteTransform.IsStringReverseByteWord` for string reads.

TCP common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

```csharp
KeyenceMcNet plc = new KeyenceMcNet("192.168.1.10", 6000)
{
    NetworkNumber = 0,
    PLCNumber = 0xFF,
    NetworkStationNumber = 0,
    TargetIOStation = 0x03FF
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("DM100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

For ASCII mode:

```csharp
KeyenceMcAsciiNet plc = new KeyenceMcAsciiNet("192.168.1.10", 6000);
```

## Address rules

The examples match the HslCommunicationDemo Keyence MC address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `R015` or `R0.15`, `XF`/`X1A0` | Input relay | `X` numeric portion is hexadecimal; `R` compact bit form uses decimal word and hexadecimal bit digit |
| `R015` or `R0.15`, `YF`/`Y1A0` | Output relay | `Y` numeric portion is hexadecimal; `R` compact bit form uses decimal word and hexadecimal bit digit |
| `B0` | Link relay | Hexadecimal; `B0-B7FFF` |
| `MR000` or `MR0.0` | Internal auxiliary relay | Range `MR00000-MR99915` |
| `LR015` or `LR0.15` | Latch relay | Range `LR00000-LR99915` |
| `CR015` or `CR0.15` | Control relay | Range `CR0000-CR7915` |
| `CM0` | Control register | Range `CM0000-CM5999` |
| `DM0` | Data register | Range `DM00000-DM65534` |
| `EM0` | Extended data register | Range `EM00000-EM65534` |
| `FM0` | File register | `FM00000-FM32767` |
| `ZF0` | File register | `ZF000000-ZF524287` |
| `ZR0` | File register | Hexadecimal; `ZF0` is the decimal-numbered alias |
| `W0` | Link register | Hexadecimal; `W0000-W7FFF` |
| `TN0` | Timer current value | Range `TN0000-TN3999` |
| `TS0` | Timer contact | Range `TS0000-TS3999` |
| `TC0` | Timer coil | Range `TC0000-TC3999` |
| `CN0` | Counter current value | Range `CN0000-CN3999` |
| `CS0` | Counter contact | Range `CS0000-CS3999` |
| `CC0` | Counter coil | Range `CC0000-CC3999` |

Keyence MC address bases:

- `X` and `Y` use hexadecimal numeric text. `XF` means bit 15, and `X1A0`
  means word `0x1A0`.
- `B`, `ZR`, and `W` use hexadecimal numeric text.
- `R015` is the word+bit form: the word part is decimal and the final bit
  digit is hexadecimal. Therefore `R015` equals `R0.15` (bit 15).
- `ZF` uses decimal numeric text even though it maps to the file-register area.

Keyence MC also accepts Mitsubishi-style addresses such as `D100` and `W100`,
as shown in the Demo code examples.

## Read methods

Core byte and bool reads:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
```

Inherited typed reads:

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
```

Random word reads:

```csharp
OperateResult<byte[]> ReadRandom(string[] address)
OperateResult<byte[]> ReadRandom(string[] address, ushort[] length)
OperateResult<short[]> ReadRandomInt16(string[] address)
OperateResult<ushort[]> ReadRandomUInt16(string[] address)
```

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
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
OperateResult WriteRandom(string[] address, byte[] values)
```

## Advanced MC methods

The full inherited Melsec MC advanced API is available:

```csharp
OperateResult<byte[]> ReadTags(string tag, ushort length)
OperateResult<byte[]> ReadTags(string[] tags, ushort[] length)
OperateResult<byte[]> ReadExtend(ushort extend, string address, ushort length)
OperateResult<byte[]> ReadMemory(string address, ushort length)
OperateResult<byte[]> ReadSmartModule(ushort module, string address, ushort length)
OperateResult RemoteRun()
OperateResult RemoteStop()
OperateResult RemoteReset()
OperateResult<string> ReadPlcType()
OperateResult ErrorStateReset()
```

Async versions exist for all core and advanced MC methods.

Advanced methods such as `ReadTags`, `ReadExtend`, `ReadMemory`, and
`ReadSmartModule` require commercial authorization. They return
`InsufficientPrivileges` when the library is not activated.

## Notes

- Configure the PLC Ethernet port for MC protocol, not the upper-link protocol.
- Select binary `KeyenceMcNet` or ASCII `KeyenceMcAsciiNet` according to the PLC
  side setting.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

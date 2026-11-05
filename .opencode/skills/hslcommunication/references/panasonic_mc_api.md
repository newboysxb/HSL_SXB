# Panasonic MC protocol API subset

`PanasonicMcNet` uses the Mitsubishi QnA-compatible 3E (MC) binary frame
protocol, adapted to Panasonic address conventions.

It is TCP-based. The PLC must be configured for MC protocol with the correct IP
address and port. The Demo default port is `5000`.

Namespace:

```csharp
using HslCommunication.Profinet.Panasonic;
```

## Constructors

```csharp
new PanasonicMcNet()
new PanasonicMcNet(string ipAddress, int port)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`. There is no constructor default MC port; use the
PLC-configured port, often `5000` in the Demo.

## Properties

`PanasonicMcNet` inherits Melsec MC properties:

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

TCP common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

```csharp
PanasonicMcNet plc = new PanasonicMcNet("192.168.1.10", 5000)
{
    NetworkNumber = 0,
    PLCNumber = 0xFF,
    NetworkStationNumber = 0,
    TargetIOStation = 0x03FF
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo Panasonic MC address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `X0` | External input relay | `X33` equals `X3.3` |
| `Y0` | External output relay | `Y33` equals `Y3.3` |
| `R2.1` | Internal relay | `R21` equals `R2.1` |
| `TN0` | Timer current value | Word read/write |
| `TS0` | Timer contact | Bool read/write |
| `CN0` | Counter current value | Word read/write |
| `CS0` | Counter contact | Bool read/write |
| `L2.1` | Link relay | `L21` equals `L2.1` |
| `D0` or `DT0` | Data register | `D90000` equals `SD0` |
| `LD0` | Link register | |
| `SD0` | Special data register | |

Panasonic MC also accepts Mitsubishi-style `D100` and similar addresses, as
shown by the Demo test code.

The compact bit syntax uses a hexadecimal final digit. `X33` means `X3.3`,
`X3.A` means bit 10, and `X3.F` means bit 15. The word portion before the bit
is decimal.

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
`ReadSmartModule` require commercial authorization and return
`InsufficientPrivileges` when the library is not activated.

## Notes

- Configure the PLC Ethernet port for MC protocol.
- Use the PLC-configured port; the Demo defaults to `5000`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

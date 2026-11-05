# Lsis Fast Enet client API subset

`LSFastEnet` implements the LSIS Fast Enet (FEnet) frame protocol over TCP. It
is intended for LSIS XGK, XGI, XGR, and XGB series PLCs equipped with the Fast
Enet interface.

Namespace:

```csharp
using HslCommunication.Profinet.LSIS;
```

## Constructors

```csharp
new LSFastEnet()
new LSFastEnet(string ipAddress, int port)
new LSFastEnet(string CpuType, string ipAddress, int port, byte slotNo)
```

Unlike most `DeviceTcpNet` clients, the parameterless constructor defaults to
`127.0.0.1:2004`. Prefer the `(ipAddress, port)` constructor for real devices.

## Properties

```csharp
string SetCpuType
LSCpuInfo CpuInfo
string CompanyID
byte BaseNo
byte SlotNo
string CpuType
bool CpuError
LSCpuStatus LSCpuStatus
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

Defaults:

| Property | Default |
| --- | --- |
| `CpuInfo` | `LSCpuInfo.XGK` |
| `CompanyID` | `"LSIS-XGT"` |
| `BaseNo` | `0` |
| `SlotNo` | `3` |

`CpuType`, `CpuError`, and `LSCpuStatus` are read-only diagnostic values that
are updated after a PLC response is received.

The Demo `CompanyID` choices are `LSIS-XGT`, `LGIS-GLOGA`, and `MASTER-K`.

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.
The protocol word length is 2 bytes.

Important: the Demo assigns `SetCpuType = cboxModel.Text`, but the Fast Enet
packet header is generated from `CpuInfo`. Set `CpuInfo` explicitly so it
matches the PLC model instead of relying on `SetCpuType` alone.

## Connection

`LSFastEnet` is TCP only and uses `ConnectServer()`.

```csharp
LSFastEnet plc = new LSFastEnet("192.168.1.10", 2004)
{
    CpuInfo = LSCpuInfo.XGK,
    CompanyID = "LSIS-XGT",
    SlotNo = 3,
    BaseNo = 0
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("MW100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo LSIS address declarations, which
use the same `X/B/W/D/L` suffix rules as LSCnet.

| Suffix | Meaning |
| --- | --- |
| `X` | Bit |
| `B` | Byte, 1 byte |
| `W` | Word, 2 bytes |
| `D` | Double word, 4 bytes |
| `L` | Long word, 8 bytes |

Supported area prefixes:

| Prefix | Examples |
| --- | --- |
| `P` | `PX100`, `PB100`, `PW100`, `PD100`, `PL100` |
| `M` | `MX100`, `MB100`, `MW100`, `MD100`, `ML100` |
| `L` | `LX100`, `LB100`, `LW100`, `LD100`, `LL100` |
| `K` | `KX100`, `KB100`, `KW100`, `KD100`, `KL100` |
| `F` | `FX100`, `FB100`, `FW100`, `FD100`, `FL100` |
| `T` | `TX100`, `TB100`, `TW100`, `TD100`, `TL100` |
| `C` | `CX100`, `CB100`, `CW100`, `CD100`, `CL100` |
| `D` | `DX100`, `DB100`, `DW100`, `DD100`, `DL100` |
| `S` | `SX100`, `SB100`, `SW100`, `SD100`, `SL100` |
| `Q` | `QX100`, `QB100`, `QW100`, `QD100`, `QL100` |
| `I` | `IX100`, `IB100`, `IW100`, `ID100`, `IL100` |
| `N` | `NX100`, `NB100`, `NW100`, `ND100`, `NL100` |
| `U` | `UX100`, `UB100`, `UW100`, `UD100`, `UL100` |
| `Z` | `ZX100`, `ZB100`, `ZW100`, `ZD100`, `ZL100` |
| `R` | `RX100`, `RB100`, `RW100`, `RD100`, `RL100` |

Additional rules:

| Address | Notes |
| --- | --- |
| `M0` | A bare address with no suffix is treated as `MW0` by default |
| `MW100.0` | Bit access in a word register |

Unlike `LSCnet`, `LSFastEnet` does not support the `s=<station>;address` prefix.
The Demo address tab reuses the Cnet address list, so ignore the `s=2;M0` row
when using Fast Enet.

For raw `Read(string address, ushort length)`, `length` is the number of bytes.
For `ReadBool(string address, ushort length)`, `length` is the number of bool
values to read.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte> ReadByte(string address)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<bool> ReadCoil(string address)
OperateResult<bool[]> ReadCoil(string address, ushort length)
```

`ReadCoil` and `ReadBool` are the same operation.

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

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, byte value)
OperateResult Write(string address, bool value)
OperateResult WriteCoil(string address, bool value)
```

Inherited typed writes from `DeviceCommunication` are also available:

```csharp
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

The inherited `Write(string, bool[])` and `WriteAsync(string, bool[])` methods
are not implemented for the Fast Enet protocol and return
`NotSupportedFunction`. Use single bool writes or raw byte writes instead.

## Async methods

Native async methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult<byte>> ReadByteAsync(string address)
Task<OperateResult> WriteAsync(string address, byte value)
Task<OperateResult<bool>> ReadCoilAsync(string address)
Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
Task<OperateResult> WriteCoilAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool value)
```

Inherited typed async read/write methods from `DeviceCommunication` are also
available.

## Notes

- `LSFastEnet` is TCP only. The default port is `2004`.
- `SetCpuType` is the string property used by the Demo; `CpuInfo` controls the
  packet header CPU code.
- Set `CompanyID`, `SlotNo`, and `BaseNo` according to the PLC and Fast Enet
  module configuration.
- `ReadBool(address, length)` reads bool arrays. Writing bool arrays is not
  supported by this protocol implementation.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `LSisServer`
- client page / form type: `FormLsisFEnet`
- transport: TCP, server port `2004`
- verified address for read/write: `MB0`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="2004" ConnectTimeOut="5000" ReceiveTimeOut="10000" CpuType="XGK" CompanyID="LSIS-XGT" Slot="3" BaseNo="0" Interval="1000" SleepTime="0" Type="FormLsisFEnet" Name="p_FormLsisFEnet" Guid="5183137f269741c0a5ad2102abb4216d" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `2004` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `CpuType` | `XGK` | CPU model string. Must match the PLC; an empty value produces an invalid frame header. |
| `CompanyID` | `LSIS-XGT` | Company / frame-header id. **It forms the first bytes of the frame**, an empty value makes the peer reply with an unusable header. |
| `Slot` | `3` | Module slot number. |
| `BaseNo` | `0` | Module base number. |

`CompanyID` (`LSIS-XGT`), `CpuType` (`XGK`), `Slot` (`3`) and `BaseNo` (`0`) are the page's own
defaults - the table above shows what the page produces by itself. `CompanyID` matters because it forms
the first bytes of the frame: if it is empty the peer's reply fails `CheckHeadBytesLegal` with "command
head check failed". Pass these explicitly if you want to be independent of the page default.

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormLsisFEnet",
    "name": "FormLsisFEnet (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "2004",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "CpuType": "XGK",
      "CompanyID": "LSIS-XGT",
      "Slot": "3",
      "BaseNo": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

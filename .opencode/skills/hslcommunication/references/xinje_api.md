# XinJE PLC clients API subset

XinJE clients are Modbus-derived clients with XinJE-specific address
translation. They target XinJE `XC`, `XD`, and `XL` series PLCs.

Namespace:

```csharp
using HslCommunication.Profinet.XINJE;
```

## Class and transport table

| Class | Base | Transport |
| --- | --- | --- |
| `XinJESerial` | `ModbusRtu` | Serial RTU |
| `XinJESerialOverTcp` | `ModbusRtuOverTcp` | RTU frames over TCP |
| `XinJETcpNet` | `ModbusTcpNet` | Modbus TCP |

All three share the same XinJE address logic and typed read/write API. Use
`Series` to select the address range.

## Constructors

```csharp
new XinJESerial()
new XinJESerial(byte station = 0x01)
new XinJESerial(XinJESeries series, byte station = 0x01)

new XinJESerialOverTcp()
new XinJESerialOverTcp(string ipAddress, int port = 502, byte station = 0x01)
new XinJESerialOverTcp(XinJESeries series, string ipAddress, int port = 502, byte station = 0x01)

new XinJETcpNet()
new XinJETcpNet(string ipAddress, int port = 502, byte station = 0x01)
new XinJETcpNet(XinJESeries series, string ipAddress, int port = 502, byte station = 0x01)
```

`Series` defaults to `XinJESeries.XC`. Prefer explicit network constructors;
parameterless TCP forms inherit `127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

```csharp
XinJESeries Series
byte Station
bool AddressStartWithZero
DataFormat DataFormat
bool IsStringReverse
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`DataFormat` defaults to `CDAB`.

Serial clients also expose `Crc16CheckEnable`. TCP clients expose `IpAddress`,
`Port`, `ConnectTimeOut`, and `ReceiveTimeOut`.

## Connection

Serial RTU:

```csharp
XinJESerial plc = new XinJESerial(XinJESeries.XC, station: 1);

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
```

The Demo default serial parameters are `9600`, `8` data bits, no parity, and
`1` stop bit.

TCP clients use `ConnectServer()`.

```csharp
XinJETcpNet plc = new XinJETcpNet(XinJESeries.XD, "192.168.1.10", 502, 1);
OperateResult connect = plc.ConnectServer();

XinJESerialOverTcp rtu = new XinJESerialOverTcp(XinJESeries.XD, "192.168.1.10", 502, 1);
OperateResult rtuConnect = rtu.ConnectServer();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo XinJE address declarations.

### XC series examples

Bit addresses:

| Address | Meaning | Notes |
| --- | --- | --- |
| `M100` | Internal relay | `M0-M7999`, `M8000-M8511` |
| `S100` | Process relay | `S0-S1023` |
| `T100` | Timer | `T0-T618`, bool read/write |
| `C100` | Counter | `C0-C634`, bool read/write |
| `X10.7` | Input coil | Octal; `X0-X1037` or `X0.0-X103.7` |
| `Y10.7` | Output coil | Octal; `Y0-Y1037` or `Y0.0-Y103.7` |

Word addresses:

| Address | Meaning | Notes |
| --- | --- | --- |
| `D100` | Data register | `D0-D8511` |
| `F100` | Flash register | `F0-F5000`, `F8000-F8511` |
| `E100` | Extended internal register | `E0-E36863` |
| `T100` | Timer value | `T0-T618` |
| `C100` | Counter value | `C0-C634` |
| `s=2;D100` | Station-prefixed address | |

### XD/XL series examples

Bit addresses:

| Address | Meaning | Notes |
| --- | --- | --- |
| `M100` | Internal relay | `M0-M7999` |
| `X10.7` | Input coil | Octal; `X0-X77`, `X10000-X11177` |
| `Y10.7` | Output coil | Octal; `Y0-Y77`, `Y10000-Y11177` |
| `S100` | Process relay | `S0-S1023` |
| `SM100` | Special relay | `SM0-SM2047` |
| `T100` | Timer | `T0-T575` |
| `C100` | Counter | `C0-C575` |
| `ET100` | Precision timer | `ET0-ET31` |
| `SEM100` | SFC instruction | `SEM0-SEM31` |
| `HM100` | Internal relay | `HM0-HM959` |
| `HT100` | Timer | `HT0-HT95` |
| `HC100` | Counter | `HC0-HC95` |
| `HSC100` | High-speed counter | `HSC0-HSC31` |

Word addresses:

| Address | Meaning | Notes |
| --- | --- | --- |
| `D100` | Data register | `D0-D7999` |
| `ID0` | Main/extension input module | `ID0-ID99`, `ID10000` etc. |
| `QD0` | Main/extension output module | `QD0-QD99`, `QD10000` etc. |
| `SD0` | Special register | `SD0-SD2047` |
| `TD0` | Timer current value | `TD0-TD575` |
| `CD0` | Counter current value | `CD0-CD575` |
| `ETD0` | Precision timer current value | `ETD0-ETD31` |
| `HD0` | Data register | `HD0-HD999` |
| `HSD0` | Special register | `HSD0-HSD499` |
| `HTD0` | Timer current value | `HTD0-HTD95` |
| `HCD0` | Counter current value | `HCD0-HCD95` |
| `HSCD0` | High-speed counter current value | `HSCD0-HSCD31` |
| `FD0` | FlashROM register | `FD0-FD5119` |
| `SFD0` | Special FlashROM register | `SFD0-SFD1999` |
| `FSD0` | Special secure register | `FSD0-FSD47` |
| `s=2;D100` | Station-prefixed address | |

## Read methods

All three clients expose the Modbus-derived read API:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
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
```

## Async methods

Async versions exist for the core read/write and typed methods from the Modbus
base classes.

## Notes

- Select `XinJESeries.XC`, `XD`, or `XL` according to the PLC model.
- Use `s=<station>;address` for station-specific access.
- `XinJETcpNet` can automatically use XinJE internal TCP commands when an
  address range exceeds the Modbus critical address. Other classes rely on
  Modbus address translation.
- The low-level address translation and command-building methods are
  intentionally omitted from this reference.

# Inovance PLC clients API subset

Inovance clients are built on HslCommunication Modbus clients and translate
Inovance addresses to Modbus addresses.

Supported classes:

| Class | Base | Transport |
| --- | --- | --- |
| `InovanceSerial` | `ModbusRtu` | Serial RTU |
| `InovanceSerialOverTcp` | `ModbusRtuOverTcp` | RTU over TCP |
| `InovanceTcpNet` | `ModbusTcpNet` | Modbus TCP |

Namespace:

```csharp
using HslCommunication.Profinet.Inovance;
```

## Series property

All Inovance clients expose:

```csharp
InovanceSeries Series
```

The default is `InovanceSeries.AM`. The valid address formats depend on the
selected series.

## Constructors

```csharp
new InovanceSerial()
new InovanceSerial(byte station = 0x01)

new InovanceSerialOverTcp()
new InovanceSerialOverTcp(string ipAddress, int port = 502, byte station = 0x01)

new InovanceTcpNet()
new InovanceTcpNet(string ipAddress, int port = 502, byte station = 0x01)
```

Prefer setting `Series` after construction for consistent behavior.

Prefer explicit constructors for network clients. Parameterless TCP forms
inherit `127.0.0.1:5000` from `DeviceTcpNet`.

## Connection

Serial client uses `SerialPortInni(...)`, `Open()`, and `Close()`.

```csharp
InovanceSerial plc = new InovanceSerial(station: 1)
{
    Series = InovanceSeries.H3U
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
```

TCP clients use `ConnectServer()` and `ConnectClose()`.

```csharp
InovanceTcpNet plc = new InovanceTcpNet("192.168.1.10", 502, station: 1)
{
    Series = InovanceSeries.H3U
};

OperateResult connect = plc.ConnectServer();
```

## Address rules

Inovance addresses are converted through `InovanceHelper.PraseInovanceAddress`.
Use the Demo pages to determine the exact address set for the selected series.

### AM400-800/AC/AP examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `Q0.0` | Output | `Q0.0-Q8191.7` or `Q0-Q65535` |
| `IX0.0` | Input | `IX0.0-IX8191.7` or `I0-I65535` |
| `MX0.0` | M register bit | `MX0.0-MX1000.10` |
| `MW0` | M register word | `MW0-MW65535` |
| `MD0` | M register dword | `MD100 = MW200` |
| `MB0` | M register byte | `MB100 = MW50`, must be even address |
| `SM0` | SM register | AM600 also supports `SM0-SM65535` |
| `SD0` | SD register | AM600 also supports `SDW0-SDW65535` |
| `s=2;SD0` | Station-prefixed address | |

### H3U examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `M0` | Middle relay | `M0-M7679`, `M8000-M8511` |
| `SM0` | Middle relay | `SM0-SM1023` |
| `S0` | Middle relay | `S0-S4095` |
| `T0` | Timer | `T0-T511`; bool is coil, word is current value |
| `C0` | Counter | `C0-C255`; bool is coil, word is current value |
| `X0` | Input | Octal address; `X0-X377` or `X0.0-X37.7` |
| `Y0` | Output | Octal address; `Y0-Y377` or `Y0.0-Y37.7` |
| `D0` | Data register | `D0-D8511` |
| `SD0` | Special register | `SD0-SD1023` |
| `R0` | File register | `R0-R32767` |
| `s=2;SD0` | Station-prefixed address | |

### H5U/Easy examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `M0` | Middle relay | `M0-M7999` |
| `B0` | Link relay | `B0-B32767` |
| `S0` | Middle relay | `S0-S4095` |
| `X0` | Input | Octal address; `X0-X1777` or `X0.0-X177.7` |
| `Y0` | Output | Octal address; `Y0-Y1777` or `Y0.0-Y177.7` |
| `D0` | Data register | `D0-D7999` |
| `R0` | File register | `R0-R32767` |
| `s=2;D0` | Station-prefixed address | |

### EVO examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `QX0.0` | Output | `QX0.0-QX8191.7` or `Q0-Q65535` |
| `IX0.0` | Input | `IX0.0-IX8191.7` or `I0-I65535` |
| `MX0.0` | M register bit | `MX0.0-MX1000.10` |
| `MW0.0` | M register bit | `MW0.0-MW65535.0` |
| `MW0` | M register word | `MW0-MW65535` |
| `MD0` | M register dword | `MD100 = MW200` |
| `MB0` | M register byte | Byte reads support odd addresses |
| `s=2;MW0` | Station-prefixed address | |

For H3U, H5U, and Easy series, `InovanceHelper.CalculateH3UStartAddress`
converts X/Y with base 8. Use the PLC notation directly: `X10` means octal 10
(decimal 8), not hexadecimal `0x10`. The same rule applies to `Y10`.
This differs from Mitsubishi MC/A1E/A4C, where the library requires a leading
`0` to select octal parsing.

## Read methods

All Inovance clients inherit the standard Modbus typed read API:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte> ReadByte(string address)

OperateResult<short> ReadInt16(string address)
OperateResult<ushort> ReadUInt16(string address)
OperateResult<int> ReadInt32(string address)
OperateResult<uint> ReadUInt32(string address)
OperateResult<float> ReadFloat(string address)
OperateResult<double> ReadDouble(string address)
OperateResult<long> ReadInt64(string address)
OperateResult<ulong> ReadUInt64(string address)
OperateResult<string> ReadString(string address, ushort length)
```

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, short value)
OperateResult Write(string address, ushort value)
OperateResult Write(string address, int value)
OperateResult Write(string address, uint value)
OperateResult Write(string address, float value)
OperateResult Write(string address, double value)
OperateResult Write(string address, long value)
OperateResult Write(string address, ulong value)
OperateResult Write(string address, string value)
```

## Notes

- Inovance clients default `ByteTransform.DataFormat` to `DataFormat.CDAB`.
- `ReadByte` is overridden by `InovanceHelper.ReadByte`.
- Verify `Series`, `AddressStartWithZero`, `IsStringReverse`, and `DataFormat`
  against the PLC configuration.

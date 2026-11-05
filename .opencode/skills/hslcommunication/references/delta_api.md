# Delta PLC clients API subset

Delta clients are built on the HslCommunication Modbus clients and translate
Delta addresses to Modbus addresses.

Supported classes:

| Class | Base | Transport |
| --- | --- | --- |
| `DeltaSerial` | `ModbusRtu` | Serial RTU |
| `DeltaSerialAscii` | `ModbusAscii` | Serial ASCII |
| `DeltaSerialOverTcp` | `ModbusRtuOverTcp` | RTU over TCP |
| `DeltaSerialAsciiOverTcp` | `ModbusAsciiOverTcp` | ASCII over TCP |
| `DeltaTcpNet` | `ModbusTcpNet` | Modbus TCP |

Namespace:

```csharp
using HslCommunication.Profinet.Delta;
```

## Series property

All Delta clients expose:

```csharp
DeltaSeries Series
```

The default is `DeltaSeries.Dvp`. The correct address format depends on the
selected series. Demo groups examples by:

- `ES/EX/SS`
- `SA/SX/SC`
- `Dvp/EH`

## Constructors

```csharp
new DeltaSerial()
new DeltaSerial(byte station = 0x01)

new DeltaSerialAscii()
new DeltaSerialAscii(byte station = 0x01)

new DeltaSerialOverTcp()
new DeltaSerialOverTcp(string ipAddress, int port = 502, byte station = 0x01)

new DeltaSerialAsciiOverTcp()
new DeltaSerialAsciiOverTcp(string ipAddress, int port = 502, byte station = 0x01)

new DeltaTcpNet()
new DeltaTcpNet(string ipAddress, int port = 502, byte station = 0x01)
```

Prefer explicit constructors for network clients. Parameterless TCP forms
inherit `127.0.0.1:5000` from `DeviceTcpNet`.

## Connection

Serial clients use `SerialPortInni(...)`, `Open()`, and `Close()`.

```csharp
DeltaSerial plc = new DeltaSerial(station: 1)
{
    Series = DeltaSeries.Dvp
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
```

TCP clients use `ConnectServer()` and `ConnectClose()`.

```csharp
DeltaTcpNet plc = new DeltaTcpNet("192.168.1.10", 502, station: 1)
{
    Series = DeltaSeries.Dvp
};

OperateResult connect = plc.ConnectServer();
```

## Address rules

Delta addresses are converted through `DeltaHelper.TranslateToModbusAddress`.
The supported address set depends on `Series`.

### ES/EX/SS examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `S0` | Step relay | `S0-S127` |
| `X0` | Input relay | `X0-X177`, octal address, read only |
| `Y0` | Output relay | `Y0-Y177`, octal address |
| `T0` | Timer | Bit or current value |
| `C0` | Counter | Bit or current value |
| `M0` | Internal relay | `M0-M1279` |
| `D0` | Data register | `D0-D1311` |

### SA/SX/SC examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `S0` | Step relay | `S0-S1023` |
| `X0` | Input relay | `X0-X177`, octal address, read only |
| `Y0` | Output relay | `Y0-Y177`, octal address |
| `T0` | Timer | Bit or current value |
| `C0` | Counter | Bit or current value |
| `M0` | Internal relay | `M0-M4095` |
| `D0` | Data register | `D0-D4999` |

### DVP/EH examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `S0` | Step relay | `S0-S1023` |
| `X0` | Input relay | `X0-X377`, octal address, read only |
| `Y0` | Output relay | `Y0-Y377`, octal address |
| `T0` | Timer | Bit or current value |
| `C0` | Counter | Bit or current value |
| `M0` | Internal relay | `M0-M4095` |
| `D0` | Data register | `D0-D9999` |

For `DeltaSeries.Dvp` (including the legacy ES/EX/SS/SA/SX/SC families), X/Y
numbers are octal. For `DeltaSeries.AS`, the X/Y word number is decimal and a
dotted bit suffix is hexadecimal `0`-`F`; for example, `X10` means word 10,
not octal 8, and `X10.A` means bit 10.

Station prefixes are supported, for example:

```text
s=2;D100
s=3;M100
```

## Read methods

All Delta clients inherit the standard Modbus typed read API and override the
raw byte and bool methods for Delta address translation:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)

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

Array overloads and async versions also exist through the Modbus base classes.

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

- Delta clients default `ByteTransform.DataFormat` to `DataFormat.CDAB`.
- Use the Demo Delta pages to confirm the exact address range for the selected
  `Series`.
- Exclude low-level Modbus command building methods from generated user code.

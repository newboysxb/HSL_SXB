# MegMeet PLC clients API subset

MegMeet clients are Modbus-derived clients with MegMeet-specific address
translation. They target `MC80`, `MC100`, `MC200`, `MC280`, and `MC200E`.

Namespace:

```csharp
using HslCommunication.Profinet.MegMeet;
```

## Class and transport table

| Class | Base | Transport |
| --- | --- | --- |
| `MegMeetSerial` | `ModbusRtu` | Serial RTU |
| `MegMeetSerialOverTcp` | `ModbusRtuOverTcp` | RTU frames over TCP |
| `MegMeetTcpNet` | `ModbusTcpNet` | Modbus TCP |

All three share the same MegMeet address rules and typed read/write API.

## Constructors

```csharp
new MegMeetSerial()
new MegMeetSerial(byte station = 0x01)

new MegMeetSerialOverTcp()
new MegMeetSerialOverTcp(string ipAddress, int port = 502, byte station = 0x01)

new MegMeetTcpNet()
new MegMeetTcpNet(string ipAddress, int port = 502, byte station = 0x01)
```

Prefer explicit network constructors. Parameterless TCP forms inherit
`127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

Common Modbus-derived properties include:

```csharp
byte Station
bool AddressStartWithZero
DataFormat DataFormat
bool IsStringReverse
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`DataFormat` defaults to `CDAB`. The Demo exposes `AddressStartWithZero`,
`IsStringReverse`, `Station`, and `DataFormat`.

Serial clients also expose `Crc16CheckEnable`. TCP clients expose `IpAddress`,
`Port`, `ConnectTimeOut`, and `ReceiveTimeOut`.

## Connection

Serial RTU:

```csharp
MegMeetSerial plc = new MegMeetSerial(station: 1);

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.Close();
```

The Demo default serial parameters are `9600`, `8` data bits, no parity, and
`1` stop bit.

TCP clients use `ConnectServer()`.

```csharp
MegMeetTcpNet plc = new MegMeetTcpNet("192.168.1.10", 502, station: 1);
OperateResult connect = plc.ConnectServer();

MegMeetSerialOverTcp rtu = new MegMeetSerialOverTcp("192.168.1.10", 502, station: 1);
OperateResult rtuConnect = rtu.ConnectServer();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo MegMeet address declarations.

### Bit addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `X0` | Input relay | Octal; `X0-X377` |
| `Y0` | Output relay | Octal; `Y0-Y377` |
| `M0` | Internal relay | `M0-M10239` |
| `SM0` | Special relay | `SM0-SM511` |
| `S0` | Status relay | `S0-S4095` |
| `T0` | Timer status | `T0-T511` |
| `C0` | Counter status | `C0-C511` |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `D0` | Data register | `D0-D7999` |
| `SD0` | Special register | `SD0-SD511` |
| `Z0` | Index register | `Z0-Z15` |
| `T0` | Timer current value | `T0-T511` |
| `C0` | Counter current value | Word `C0-C199`; double-word `C200-C306` |
| `R0` | File register | `R0-R32767` |
| `s=2;D0` | Station-prefixed address | All addresses can carry a station |

For `C200` and above, use `int` or `uint` typed read/write methods.

The `s=<station>;address` prefix overrides the client `Station` for that
individual operation.

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

- Use `Open()` after `SerialPortInni(...)` for `MegMeetSerial`.
- Use `ConnectServer()` for `MegMeetSerialOverTcp` and `MegMeetTcpNet`.
- Confirm `Station`, `AddressStartWithZero`, `DataFormat`, and
  `IsStringReverse` against the PLC configuration.
- The low-level address translation and command-building methods are
  intentionally omitted from this reference.

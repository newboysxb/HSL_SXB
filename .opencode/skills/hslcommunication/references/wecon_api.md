# WeCon PLC over Modbus API subset

WeCon PLCs do not have a dedicated client class. Use a standard
HslCommunication Modbus client and register the WeCon LX5V address mapping
before reading or writing:

```csharp
plc.RegisteredAddressMapping(HslCommunication.ModBus.ModbusMappingAddress.WeCon_Lx5v);
```

After registration, the Modbus client accepts the native WeCon LX5V addresses
listed below instead of raw Modbus addresses.

Supported clients:

| Class | Transport |
| --- | --- |
| `ModbusTcpNet` | Modbus TCP |
| `ModbusUdpNet` | Modbus UDP |
| `ModbusRtu` | Serial RTU |
| `ModbusAscii` | Serial ASCII |
| `ModbusRtuOverTcp` | RTU over TCP |
| `ModbusAsciiOverTcp` | ASCII over TCP |

`ModbusUdpNet` inherits `ModbusTcpNet`, `ModbusAscii` inherits `ModbusRtu`, and
`ModbusAsciiOverTcp` inherits `ModbusRtuOverTcp`, so the mapping call is
available on all of them.

Namespace:

```csharp
using HslCommunication.ModBus;
```

## Registration

Register the mapping after constructing the client and before read/write
operations.

Modbus TCP:

```csharp
ModbusTcpNet plc = new ModbusTcpNet("192.168.1.10", 502, station: 1);
plc.RegisteredAddressMapping(ModbusMappingAddress.WeCon_Lx5v);

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

Serial RTU:

```csharp
ModbusRtu plc = new ModbusRtu(station: 1);
plc.RegisteredAddressMapping(ModbusMappingAddress.WeCon_Lx5v);

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<bool> read = plc.ReadBool("M0");
}

plc.Close();
```

RTU over TCP:

```csharp
ModbusRtuOverTcp plc = new ModbusRtuOverTcp("192.168.1.10", 502, station: 1);
plc.RegisteredAddressMapping(ModbusMappingAddress.WeCon_Lx5v);

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<ushort> read = plc.ReadUInt16("D100");
}

plc.ConnectClose();
```

To restore normal raw Modbus address behavior, clear the mapping:

```csharp
plc.RegisteredAddressMapping(null);
```

## Address rules

The examples match the HslCommunicationDemo WeCon LX5V address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `T0` | Timer | `T0-T511`; bit access controls the contact/coil, typed word access controls the current value |
| `C0` | Counter | `C0-C255`; bit access controls the contact/coil, typed word access controls the current value |
| `LC0` | Long counter | `LC0-LC255`; use `ReadInt32`/`Write(int)` for the current value |
| `HSC0` | High-speed counter | `HSC0-HSC15`; use `ReadInt32`/`Write(int)` for the current value |
| `M0` | Internal relay | `M0-M8000`; bit address |
| `SM0` | Special relay | `SM0-SM4095`; bit address |
| `S0` | Step relay | `S0-S4095`; bit address |
| `X0` | Input relay | `X0-X1777`; bit address, octal |
| `Y0` | Output relay | `Y0-Y1777`; bit address, octal |
| `D0` | Data register | `D0-D7999`; word address |
| `SD0` | Special register | `SD0-SD4095`; word address |
| `R0` | Data register | `R0-R30000`; word address |
| `D0.4` | Register bit | Direct access to one bit of a register |
| `s=2;D0` | Station-prefixed address | Overrides the client `Station` for one operation |

`X` and `Y` are octal addresses. `LC` and `HSC` are 32-bit values, so use the
`int32` read/write methods for their current values.

## Read methods

The WeCon mapping works with the standard Modbus read API:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<bool> ReadCoil(string address)
OperateResult<bool[]> ReadCoil(string address, ushort length)
OperateResult<bool> ReadDiscrete(string address)
OperateResult<bool[]> ReadDiscrete(string address, ushort length)

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
See `references/modbus_tcp_api.md` and `references/modbus_variants_api.md`.

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
OperateResult WriteMask(string address, ushort andMask, ushort orMask)
```

Register-bit writes may use the Modbus mask write function and require
commercial authorization, as described in the Modbus references.
For WeCon register-bit writes such as `D0.4`, use `Write("D0.4", value)`; do
not rely on `WriteMask` for dotted WeCon addresses, because the WeCon mapping
does not translate that function for them.

## Notes

- Call `RegisteredAddressMapping(ModbusMappingAddress.WeCon_Lx5v)` before the
  first read/write operation.
- The Demo uses `DataFormat.CDAB` by default for the TCP client; adjust
  `DataFormat` and `IsStringReverse` if typed values appear wrong.
- `X` and `Y` use octal numbers.
- `s=<station>;address` is supported by the WeCon mapping.
- Low-level Modbus command-building methods are intentionally omitted from this
  reference.

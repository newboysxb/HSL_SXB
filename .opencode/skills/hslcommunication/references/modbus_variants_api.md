# Other Modbus classes

`ModbusRtu`, `ModbusAscii`, `ModbusRtuOverTcp`, `ModbusAsciiOverTcp`, and
`ModbusUdpNet` share the same address formats and the same typed read/write API
as `ModbusTcpNet`. Only the transport and construction differ.

## TCP and UDP

```csharp
ModbusTcpNet modbusTcp = new ModbusTcpNet("192.168.1.10", 502, station: 1);

ModbusUdpNet modbusUdp = new ModbusUdpNet("192.168.1.10", 502, station: 1);

ModbusRtuOverTcp modbusRtuOverTcp = new ModbusRtuOverTcp("192.168.1.10", 502, station: 1);

ModbusAsciiOverTcp modbusAsciiOverTcp = new ModbusAsciiOverTcp("192.168.1.10", 502, station: 1);
```

All four use `ConnectServer()` and `ConnectClose()`.
Prefer the `(ipAddress, port, station)` constructor for TCP and UDP variants.
Their parameterless constructors inherit `127.0.0.1:5000` from `DeviceTcpNet`.

## Serial RTU and ASCII

```csharp
using System.IO.Ports;
using HslCommunication.ModBus;

ModbusRtu modbusRtu = new ModbusRtu(station: 1);
modbusRtu.SerialPortInni("COM3", 9600, 8, StopBits.One, Parity.None);
modbusRtu.Open();

ModbusAscii modbusAscii = new ModbusAscii(station: 1);
modbusAscii.SerialPortInni("COM3-9600-8-N-1");
modbusAscii.Open();
```

Serial classes use `Open()` and `Close()`, not `ConnectServer()`.
After `Open()`, the same `ReadFloat`, `ReadBool`, `Write`, and address strings
are used.

## Constructor summary

```csharp
new ModbusRtu()
new ModbusRtu(byte station = 0x01)

new ModbusAscii()
new ModbusAscii(byte station = 0x01)

new ModbusRtuOverTcp()
new ModbusRtuOverTcp(string ipAddress, int port = 502, byte station = 0x01)

new ModbusAsciiOverTcp()
new ModbusAsciiOverTcp(string ipAddress, int port = 502, byte station = 0x01)

new ModbusUdpNet()
new ModbusUdpNet(string ipAddress, int port = 502, byte station = 0x01)
```

The same constructor warning applies to the parameterless forms above. Always
pass the port explicitly for network clients.

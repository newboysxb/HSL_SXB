# Melsec MC clients API subset

Common behavior such as `OperateResult`, reconnection, logging, raw byte reads,
and attribute-based reads is shared with Modbus clients. See
`references/common_behavior.md`.

Namespace:

```csharp
using HslCommunication.Profinet.Melsec;
```

Constructors:

```csharp
new MelsecMcNet()
new MelsecMcNet(string ipAddress, int port)

new MelsecMcAsciiNet()
new MelsecMcAsciiNet(string ipAddress, int port)

new MelsecMcUdp()
new MelsecMcUdp(string ipAddress, int port)

new MelsecMcAsciiUdp()
new MelsecMcAsciiUdp(string ipAddress, int port)
```

Prefer the `(ipAddress, port)` constructors. `MelsecMcNet()` and
`MelsecMcAsciiNet()` inherit `127.0.0.1:5000` from `DeviceTcpNet`; the UDP
parameterless constructors explicitly use port `6000`.

| Class | Frame format | Transport |
| --- | --- | --- |
| `MelsecMcNet` | Binary QnA 3E | TCP |
| `MelsecMcAsciiNet` | ASCII QnA 3E | TCP |
| `MelsecMcUdp` | Binary QnA 3E | UDP |
| `MelsecMcAsciiUdp` | ASCII QnA 3E | UDP |

The address rules and read/write methods are identical for all four classes.

The common Mitsubishi QnA 3E binary port is `6000`, but it is not open by
default on the PLC. Always pass it explicitly for TCP clients. If
`ConnectServer()` returns a connection-refused error, use Mitsubishi programming
software to configure the Ethernet port and network parameters, then power-cycle
the PLC so the settings take effect.

Properties:

```csharp
byte NetworkNumber
byte PLCNumber
byte NetworkStationNumber
ushort TargetIOStation
bool EnableWriteBitToWordRegister
int ConnectTimeOut
int ReceiveTimeOut
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

## Generated connection code rule

When generating Melsec MC connection code, do not set `NetworkNumber`,
`PLCNumber`, or `NetworkStationNumber` by default. Generate the instance from
the IP address and port only, then call `ConnectServer()` and read/write:

```csharp
MelsecMcNet plc = new MelsecMcNet("192.168.1.10", 6000);

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

If the user reports that the network has been checked and confirmed correct
but the PLC still cannot communicate, handle the network parameters according
to the requested output style.

For an interactive sample or connection test, ask the user for the actual
values configured in the PLC, then add the three properties before connecting:

```csharp
MelsecMcNet plc = new MelsecMcNet("192.168.1.10", 6000);
plc.NetworkNumber = <value from PLC network parameters>;
plc.PLCNumber = <value from PLC network parameters>;
plc.NetworkStationNumber = <value from PLC network parameters>;
```

For configurable or complete-project code, add these fields to the project's
configuration model and assign them when the device object is created. Do not
require the current PLC values while generating source code, and do not add a
runtime prompt that asks the operator to confirm them on every retry.

Do not guess or invent these values. Keep the sample code free of the three
properties unless the user confirms the network and provides the real PLC
values.

Connection:

```csharp
OperateResult ConnectServer()
OperateResult ConnectClose()
Task<OperateResult> ConnectServerAsync()
Task<OperateResult> ConnectCloseAsync()
```

## Regular read/write methods

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

OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, short value)
OperateResult Write(string address, ushort value)
OperateResult Write(string address, int value)
OperateResult Write(string address, uint value)
OperateResult Write(string address, long value)
OperateResult Write(string address, ulong value)
OperateResult Write(string address, float value)
OperateResult Write(string address, double value)
OperateResult Write(string address, string value)
```

Async variants such as `ReadFloatAsync` and `WriteAsync` also exist.

Random word reads:

```csharp
OperateResult<byte[]> ReadRandom(string[] address)
OperateResult<byte[]> ReadRandom(string[] address, ushort[] length)
OperateResult<short[]> ReadRandomInt16(string[] address)
OperateResult<ushort[]> ReadRandomUInt16(string[] address)
OperateResult WriteRandom(string[] address, byte[] values)
```

`ReadRandom(string[])` reads one word per address. The returned bytes are in the
same order as the input addresses.

## Address rules

This list matches the HslCommunicationDemo Melsec MC address examples.

| Address | Type | Notes |
| --- | --- | --- |
| `M0` | Internal relay | Bit/word |
| `X0` | Input relay | PLC X/Y notation is octal; see the X/Y rule below before changing the address |
| `Y0` | Output relay | PLC X/Y notation is octal; see the X/Y rule below before changing the address |
| `SM0` | Special relay | Bit/word |
| `S0` | Step relay | Bit/word |
| `L0` | Latch relay | Bit/word |
| `F0` | Annunciator | Bit/word |
| `V0` | Edge relay | Bit/word |
| `B0` | Link relay | Hexadecimal address |
| `SB0` | Special link relay | Hexadecimal address |
| `DX0` | Direct input | Hexadecimal address |
| `DY0` | Direct output | Hexadecimal address |
| `TS0` | Timer contact | Bit |
| `TC0` | Timer coil | Bit |
| `SS0` | Cumulative timer contact | Bit |
| `SC0` | Cumulative timer coil | Bit |
| `CS0` | Counter contact | Bit |
| `CC0` | Counter coil | Bit |
| `D0` | Data register | Word |
| `SD0` | Special data register | Word |
| `W0` | Link register | Hexadecimal word address |
| `SW0` | Special link register | Hexadecimal word address |
| `R0` | File register | Word |
| `Z0` | Index register | Word |
| `ZR0` | ZR file register | Word |
| `TN0` | Timer current value | Word |
| `SN0` | Cumulative timer current value | Word |
| `CN0` | Counter current value | Word |

Important X/Y rule for MC/A1E/A4C clients: Mitsubishi PLC displays and manuals
normally use octal for X/Y addresses. The HSL MC parser treats a value without
a leading `0` as hexadecimal because `MelsecMcDataType.X.FromBase` and
`MelsecMcDataType.Y.FromBase` are 16. Therefore:

- PLC `X10` means octal 10, decimal 8. Use HSL address `X010`.
- HSL `X10` means hexadecimal `0x10`, decimal 16.
- PLC `Y17` means octal 17, decimal 15. Use HSL address `Y017`.

For PLC-facing code, use the leading-zero form for X/Y addresses unless the
user explicitly intends a hexadecimal address.

For register bit access, use `D100.1`.

Advanced commercial addresses, when supported:

```text
ext=1;W100     extended area 1, address W100
mem=32         buffer memory address 32
module=3;4106  intelligent module 3, offset 4106
s=AAA          label/tag address
```

Do not reuse Modbus `s=1;100` station syntax for Melsec MC. Mitsubishi station
and network parameters are separate properties.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `MelsecMcServer`
- client page / form type: `FormMelsecBinary`
- transport: TCP, server port `6000`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6000" ConnectTimeOut="5000" ReceiveTimeOut="10000" EnableWriteBitToWordRegister="支持位写入字寄存器(读字，修改位，写字)" IsStringReverseByteWord="false" NetworkNumber="0" NetworkStationNumber="0" TargetIOStation="0x03FF" Interval="1000" SleepTime="0" Type="FormMelsecBinary" Name="p_FormMelsecBinary" Guid="6fcdac6589984d038bb54cb79167330a" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6000` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `EnableWriteBitToWordRegister` | `支持位写入字寄存器(读字，修改位，写字)` | Allow bit writes into word registers. |
| `IsStringReverseByteWord` | `false` | Reverse byte/word order for strings. |
| `NetworkNumber` | `0` | Network number. |
| `NetworkStationNumber` | `0` | Network station number. |
| `TargetIOStation` | `0x03FF` | Target IO station, for example `0x03FF`. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormMelsecBinary",
    "name": "FormMelsecBinary (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6000",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "EnableWriteBitToWordRegister": "支持位写入字寄存器(读字，修改位，写字)",
      "IsStringReverseByteWord": "false",
      "NetworkNumber": "0",
      "NetworkStationNumber": "0",
      "TargetIOStation": "0x03FF"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

### Same page over a serial pipe (no TCP)

`MelsecMcNet` derives from `DeviceTcpNet`, but the Demo assigns the pipe at connect time, so the
**same page** (`FormMelsecBinary`) can talk MC over a serial port instead of TCP. Verified
end-to-end against a `MelsecMcServer` whose serial slave ran on `COM5`, with the client on the
paired port `COM6`:

| Action | Result |
| --- | --- |
| client writes `D100`=4321, `D0`=1111, `D200`=2222 | client read and server-side read all identical |
| server-side writes `D300`=5555, `D301`=6666 | client reads `5555`, `6666` |
| bit write `M100` / `M101` / `Y0` = true | all read back true |
| batch read `D0` length 5 | `[1111, 0, 0, 0, 0]` |
| round-trip | about 48 ms per read over the virtual serial pair |

```xml
<Device PipeSetting="SerialPipe" ComPort="COM6" BaudRate="9600" DataBit="8" StopBit="1" Parity="0" EnableWriteBitToWordRegister="支持位写入字寄存器(读字，修改位，写字)" IsStringReverseByteWord="false" NetworkNumber="0" NetworkStationNumber="0" TargetIOStation="0x03FF" Interval="1000" SleepTime="0" Type="FormMelsecBinary" Name="mc_serial" Guid="956d18032d484467a70601446fa295ea" />
```

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `SerialPipe` | Use the serial port instead of TCP. The page default is `TcpPipe`, so this must be passed. |
| `ComPort` | `COM6` | Serial port name. Use `ComPort`; the serial fields are not part of this page's saved template, so pass `PipeSetting=SerialPipe` together with `ComPort`. |
| `BaudRate` | `9600` | Must match the server serial slave. |
| `DataBit` | `8` | **Must match the slave.** The page default is `7`. |
| `StopBit` | `1` | Must match the slave. |
| `Parity` | `0` | **Must match the slave.** The page default is `2` (even). |
| `EnableWriteBitToWordRegister` | (text) | Allow bit writes into word registers. |
| `IsStringReverseByteWord` | `false` | Reverse byte/word order for strings. |
| `NetworkNumber` | `0` | MC network number. |
| `NetworkStationNumber` | `0` | MC station number. |
| `TargetIOStation` | `0x03FF` | Target IO station. |

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormMelsecBinary",
    "name": "MC serial (AI)",
    "parameters": {
      "PipeSetting": "SerialPipe",
      "ComPort": "COM6",
      "BaudRate": "9600",
      "DataBit": "8",
      "StopBit": "1",
      "Parity": "0"
    }
  }
}
```

**Gotcha - the serial parameters are not free.** The page defaults are `9600-7-E-1`, but a
`MelsecMcServer` serial slave started as `COM5-9600-8-N-1` uses `8-N-1`. With the mismatched
defaults the client still reports "connected" and the server does answer, yet every read fails with
`command head check failed` - the mismatched frame size garbles the bytes, so it looks like a
protocol fault rather than a port-setting mismatch. Set `DataBit` and `Parity` to match the slave
exactly (`8` / `0` for an `8-N-1` slave).

# DLT645-2007 API subset

`DLT645` and `DLT645OverTcp` implement the DLT645-2007 multi-function energy
meter protocol.

`DLT645` is the serial version. `DLT645OverTcp` is the TCP pass-through version.
Both share the same data identification address rules.

Namespace:

```csharp
using HslCommunication.Instrument.DLT;
```

## Class and transport table

| Class | Base | Transport |
| --- | --- | --- |
| `DLT645` | `DeviceSerialPort` | Serial |
| `DLT645OverTcp` | `DeviceTcpNet` | TCP pass-through |

## Constructors

```csharp
new DLT645()
new DLT645(string station, string password = "", string opCode = "")

new DLT645OverTcp()
new DLT645OverTcp(string ipAddress, int port = 502, string station = "1", string password = "", string opCode = "")
```

`station` is a 12-character BCD address, for example `149100007290`.

## Properties

```csharp
string Station
string Password
string OpCode
bool EnableCodeFE
bool CheckDataId
DLT645Type DLTType
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Password` and `OpCode` default to `00000000`. They are used for write
operations.

`EnableCodeFE` adds the `FE FE FE FE` command header when enabled.
`CheckDataId` defaults to `true` and checks the returned data identifier.

`ByteTransform` is `RegularByteTransform`.

## Connection

Serial:

```csharp
DLT645 meter = new DLT645("149100007290", "00000000", "00000000");
meter.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.Even);
OperateResult open = meter.Open();
```

The Demo default serial parameters are `9600`, `8` data bits, even parity, and
`1` stop bit.

TCP:

```csharp
DLT645OverTcp meter = new DLT645OverTcp("192.168.1.10", 2000, "149100007290");
OperateResult connect = meter.ConnectServer();
```

The constructor default port is `502`. The Demo UI uses `2000`.

For point-to-point mode, call `ReadAddress()` after opening/connecting to obtain
or activate the meter address.

## Address rules

Addresses are DLT645 data identifiers in `DI3-DI2-DI1-DI0` format.

The examples match the HslCommunicationDemo DLT645-2007 declarations.

| Address | Meaning | Read method |
| --- | --- | --- |
| `00-00-00-00` | Current combined active total energy | `ReadDouble` |
| `00-01-00-00` | Current forward active total energy | `ReadDouble` |
| `00-02-00-00` | Current reverse active total energy | `ReadDouble` |
| `00-03-00-00` | Current combined reactive total energy | `ReadDouble` |
| `00-09-00-00` | Current forward apparent total energy | `ReadDouble` |
| `00-0A-00-00` | Current reverse apparent total energy | `ReadDouble` |
| `02-01-01-00` | Phase A voltage | `ReadDouble` |
| `02-01-02-00` | Phase B voltage | `ReadDouble` |
| `02-01-03-00` | Phase C voltage | `ReadDouble` |
| `02-02-01-00` | Phase A current | `ReadDouble` |
| `02-02-02-00` | Phase B current | `ReadDouble` |
| `02-02-03-00` | Phase C current | `ReadDouble` |
| `02-03-00-00` | Total instantaneous active power | `ReadDouble` |
| `02-04-00-00` | Total instantaneous reactive power | `ReadDouble` |
| `02-05-00-00` | Total instantaneous apparent power | `ReadDouble` |
| `02-06-00-00` | Total power factor | `ReadDouble` |
| `02-07-01-00` | Phase A angle | `ReadDouble` |
| `02-08-01-00` | Phase A voltage distortion | `ReadDouble` |
| `02-80-00-01` | Neutral current | `ReadDouble` |
| `02-80-00-02` | Grid frequency | `ReadDouble` |
| `02-80-00-03` | One-minute active average power | `ReadDouble` |
| `02-80-00-04` | Current active demand | `ReadDouble` |
| `02-80-00-05` | Current reactive demand | `ReadDouble` |
| `02-80-00-06` | Current apparent demand | `ReadDouble` |
| `02-80-00-07` | Meter temperature | `ReadDouble` |
| `02-80-00-08` | Clock battery voltage | `ReadDouble` |
| `02-80-00-09` | Backup battery voltage | `ReadDouble` |
| `02-80-00-0A` | Internal battery working time | `ReadDouble` |
| `04-00-01-01` | Date and weekday | `ReadString` |
| `04-00-01-02` | Time | `ReadString` |
| `04-00-04-03` | Asset management code | `ReadString` |
| `04-00-04-0B` | Meter model | `ReadString` |
| `04-00-04-0C` | Production date | `ReadString` |

One-to-many mode can carry a station prefix:

```text
s=2;00-00-00-00
```

For non-standard devices that require no data byte reversal:

```text
reverse=false;02-01-01-00
```

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<double[]> ReadDouble(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
OperateResult<string[]> ReadStringArray(string address)
```

`ReadDouble` is the main method for numeric meter values.
`ReadString` and `ReadStringArray` are used for text/identification data.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, short[] values)
OperateResult Write(string address, ushort[] values)
OperateResult Write(string address, int[] values)
OperateResult Write(string address, uint[] values)
OperateResult Write(string address, float[] values)
OperateResult Write(string address, double[] values)
OperateResult Write(string address, string value, Encoding encoding)
```

Writes require the correct `Password` and `OpCode` for DLT645-2007.

## Meter control methods

```csharp
OperateResult ActiveDeveice()
OperateResult<string> ReadAddress()
OperateResult WriteAddress(string address)
OperateResult BroadcastTime(DateTime dateTime)
OperateResult FreezeCommand(string dataArea)
OperateResult ChangeBaudRate(string baudRate)
OperateResult Trip(DateTime validTime)
OperateResult Trip(string station, DateTime validTime)
OperateResult SwitchingOn(DateTime validTime)
OperateResult SwitchingOn(string station, DateTime validTime)
```

`ChangeBaudRate` supports `600`, `1200`, `2400`, `4800`, `9600`, and `19200`.

## Async methods

`DLT645OverTcp` provides full async read/write and meter control methods,
including:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
Task<OperateResult<string[]>> ReadStringArrayAsync(string address)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult> WriteAsync(string address, double[] values)
Task<OperateResult<string>> ReadAddressAsync()
Task<OperateResult> WriteAddressAsync(string address)
Task<OperateResult> BroadcastTimeAsync(DateTime dateTime)
Task<OperateResult> FreezeCommandAsync(string dataArea)
Task<OperateResult> ChangeBaudRateAsync(string baudRate)
```

`DLT645` serial provides async versions for `ReadDouble`, `ReadString`,
`ReadStringArray`, and the write overloads. Core `ReadAsync` is implemented by
`DLT645OverTcp`.

## Notes

- DLT645-2007 does not use generic PLC bool/word register methods such as
  `ReadBool`; use `ReadDouble` or `ReadString` with the DLT645 data identifier.
- In point-to-point mode, call `ReadAddress()` after connection.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

Verified end-to-end against a Demo `DLT645Server`, with **both** a TCP page and a serial page
connected to the same server instance, and cross-checked in every direction: client write is
visible on the server, a TCP write is readable from the serial page and the reverse, and a
server-side write is readable from a client.

Use **`double`** for this protocol, and the address form `02-01-01-00` (four hyphen-separated
pairs).

### Address form

| Address | Result |
| --- | --- |
| `02-01-01-00` | valid (the verified one) |
| `02-01-02-00` | valid |
| `00-00-00-00` | valid |
| `02010100` (no hyphens) | valid |
| `00-01-01-00` | rejected |
| `02-01-01-01` | rejected - the last pair is not free |
| `B6-11` | rejected (that form belongs to DLT645-**1997**, not this one) |

### Server side

```json
{"operation":"virtualServer.create","parameters":{"type":"DLT645Server","tcpPort":2000,"startTcp":true}}
{"operation":"virtualServer.startSerial","parameters":{"serverId":"<serverId>","serialPort":"COM5-9600-8-E-1"}}
```

### Client A - TCP (`FormDLT645OverTcp`)

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="2000" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="1" Password="" OpCode="" FeHead="false" Interval="1000" SleepTime="0" Type="FormDLT645OverTcp" Name="DLT645访问Demo" Guid="baec94cbd6fa4ff1880b3583de76bb7f">
  <AddressList>
    <Address Value="02-01-01-00" />
  </AddressList>
</Device>
```

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Must be `TcpPipe` for the TCP page. |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress`. |
| `Port` | `2000` | Peer TCP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `1` | Meter address; `1` is what the working page uses. |
| `Password` | (empty) | Meter password, only needed for write-protected meters. |
| `OpCode` | (empty) | Operator code, only needed for write-protected meters. |
| `FeHead` | `false` | Prepend the `FE FE FE FE` wake-up bytes (some meters require it). |

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormDLT645OverTcp",
    "name": "DLT645OverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "2000",
      "PipeSetting": "TcpPipe",
      "Station": "1"
    }
  }
}
```

### Client B - Serial (`FormDLT645`)

```xml
<Device PipeSetting="SerialPipe" ComPort="COM6" BaudRate="9600" DataBit="8" StopBit="1" Parity="2" Station="1" Password="" OpCode="" FeHead="false" Interval="1000" SleepTime="0" Type="FormDLT645" Name="DLT645访问Demo" Guid="0f06a12c7e3c499bad4a0d626baec17a">
  <AddressList>
    <Address Value="02-01-01-00" />
  </AddressList>
</Device>
```

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `SerialPipe` | Must be `SerialPipe` for the serial page. |
| `ComPort` | `COM6` | Serial port name. `PortName` is accepted as an alias. |
| `BaudRate` | `9600` | Serial baud rate. |
| `DataBit` | `8` | Serial data bits. |
| `StopBit` | `1` | Serial stop bits. |
| `Parity` | `2` | **Even parity** - this page uses `2`, not `0` like most other serial protocols. |
| `Station` | `1` | Meter address. |

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormDLT645",
    "name": "DLT645 serial (AI)",
    "parameters": {
      "PipeSetting": "SerialPipe",
      "ComPort": "COM6",
      "BaudRate": "9600",
      "DataBit": "8",
      "StopBit": "1",
      "Parity": "2",
      "Station": "1"
    }
  }
}
```

### Verified read/write results (`02-01-01-00`, `double`)

| Action | Result |
| --- | --- |
| TCP page writes `0` / `100` / `220.5` / `999.9` | TCP read and server read all identical |
| Serial page writes `200` / `555.5` | serial read and server read identical |
| TCP writes `777.5` | **serial** page reads `777.5` |
| Serial writes `666.5` | **TCP** page reads `666.5` |
| Server-side write `321.5` | TCP page reads `321.5` |

Unlike DLT645-**1997**, this protocol has **no server-side write scaling offset** - the
client write path and the `virtualServer.points.write` path agree exactly.

### Measured value limits

This address holds **one decimal place and at most four digits**, so keep values in
`0.0` .. `999.9`:

| Written by the TCP page | Read back |
| --- | --- |
| `0`, `100`, `220.5`, `777.5`, `999.5`, `999.9` | identical (exact round-trip) |
| `333.25` | `333.2` (second decimal dropped) |
| `1000` | `100.0` (overflows, digits shift) |
| `1234` | `123.4` |
| `9999` | `999.9` |
| `10000` | `100.0` |

For exact round-trips stay within `0.0` .. `999.9` and use at most one decimal digit.

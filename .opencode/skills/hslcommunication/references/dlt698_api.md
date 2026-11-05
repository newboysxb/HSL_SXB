# DLT698 API subset

`DLT698` and `DLT698TcpNet` implement the DL/T 698.45 energy meter protocol.

`DLT698` is the serial version. `DLT698TcpNet` is the TCP version. Both share the
same object-identifier address rules.

Namespace:

```csharp
using HslCommunication.Instrument.DLT;
```

## Class and transport table

| Class | Base | Transport |
| --- | --- | --- |
| `DLT698` | `DeviceSerialPort` | Serial |
| `DLT698OverTcp` | `DeviceTcpNet` | TCP pass-through |
| `DLT698TcpNet` | `DLT698OverTcp` | TCP |
| `DLT698Server` | `DLT645Server` | Virtual server (Demo) |

## Constructors

```csharp
new DLT698()
new DLT698(string station)

new DLT698TcpNet()
new DLT698TcpNet(string station = "AAAAAAAAAAAA")
new DLT698TcpNet(string ipAddress, int port, string station = "AAAAAAAAAAAA")
```

## Key properties

```csharp
string Station                  // meter address, default "AAAAAAAAAAAA"
bool UseSecurityResquest = true // security (encrypted) request
byte CA = 0x00                  // client address
```

## Read / write methods

`ReadDouble` / `Write` come from the shared device base, so the usual
`ReadDouble(address, length)` and `Write(address, double)` work. In addition this
protocol exposes:

```csharp
OperateResult<byte[]> ReadByApdu(byte[] apdu)
OperateResult ActiveDeveice()
OperateResult<string[]> ReadStringArray(string address)
OperateResult<string[]> ReadStringArray(string[] address)
OperateResult<string> ReadAddress()
OperateResult WriteAddress(string address)
OperateResult WriteDateTime(string address, DateTime time)
```

## Address form

`XX-YY-02-00` - four hyphen-separated pairs. The third pair `02` identifies the
DLT698 interface class and is present in every verified address.

The full list of documented object identifiers lives in the Demo's own address
example panel (`DLTHelper.GetDlt698Address()`); the ones verified in this
reference are listed under the Gateway section below.

Many of these are **measurement points, i.e. read-only**. Reading them works;
writing them reports success at the protocol level but the value is not stored,
because the meter (and the Demo virtual server) treats them as measurements. The
verified addresses are marked accordingly below.

## Creating this client inside HslCommunicationDemo (AI Gateway)

Verified end-to-end against a Demo `DLT698Server`, with **both** a TCP page and a
serial page connected to the same server instance. Every reading below was
cross-checked three ways - TCP page, serial page, and server-side read - and all
three agreed.

### Server side

```json
{"operation":"virtualServer.create","parameters":{"type":"DLT698Server","tcpPort":2000,"startTcp":true}}
{"operation":"virtualServer.startSerial","parameters":{"serverId":"<serverId>","serialPort":"COM5-9600-8-E-1"}}
```

### Client A - TCP (`FormDLT698TcpNet`)

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="2000" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="1" UseSecurityResquest="true" Interval="1000" SleepTime="0" Type="FormDLT698TcpNet" Name="DLT698访问Demo" Guid="6054cfc3c0064b7284b72ad424779e14">
  <AddressList>
    <Address Value="20-00-02-00" />
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
| `Station` | `1` | Meter address. |
| `UseSecurityResquest` | `true` | Security (encrypted) request; the verified page uses `true`. |

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormDLT698TcpNet",
    "name": "DLT698TcpNet (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "2000",
      "PipeSetting": "TcpPipe",
      "Station": "1",
      "UseSecurityResquest": "true"
    }
  }
}
```

### Client B - Serial (`FormDLT698`)

```xml
<Device PipeSetting="SerialPipe" ComPort="COM6" BaudRate="9600" DataBit="8" StopBit="1" Parity="2" Station="1" UseSecurityResquest="true" EnableFE="false" CA="0" Interval="1000" SleepTime="0" Type="FormDLT698" Name="DLT698访问Demo" Guid="bd344c9341a9498ea5761a0b5b1a0f14">
  <AddressList>
    <Address Value="20-00-02-00" />
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
| `UseSecurityResquest` | `true` | Security (encrypted) request. |
| `EnableFE` | `false` | Prepend the `FE FE FE FE` wake-up bytes. |
| `CA` | `0` | Client address byte. |

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormDLT698",
    "name": "DLT698 serial (AI)",
    "parameters": {
      "PipeSetting": "SerialPipe",
      "ComPort": "COM6",
      "BaudRate": "9600",
      "DataBit": "8",
      "StopBit": "1",
      "Parity": "2",
      "Station": "1",
      "UseSecurityResquest": "true"
    }
  }
}
```

### Verified addresses and values

Read with `"type":"double"`. TCP page, serial page and the server-side read all
returned the same number.

| Address | Meaning | Value read |
| --- | --- | --- |
| `20-00-02-00` | voltage, `double[3]` = phase A/B/C | `[223.1, 233.1, 243.1]` |
| `20-01-02-00` | current, `double[3]` = phase A/B/C | `11.0` (first element) |
| `20-04-02-00` | active power (W) | `123.4` |
| `20-0F-02-00` | grid frequency (Hz) | `50.12` |
| `20-10-02-00` | meter internal temperature (C) | `23.4` |
| `00-00-02-00` | combined active total energy (kwh) | `11.0` |
| `10-00-02-00` | current combined active total energy (kwh) | `41.0` |

For a multi-element address pass `"length"` to get the whole array:

```json
{"device":"<clientId>","address":"20-00-02-00","type":"double","length":3}
```

returns

```json
{"success":true,"value":[223.1,233.1,243.1]}
```

### Measurement points are read-only

Writing to a measurement point reports protocol-level success but does **not**
store the value. Measured on `20-00-02-00`:

| Action | Result |
| --- | --- |
| write `100` | read back `223.1` (unchanged) |
| write `220.5` | read back `223.1` (unchanged) |
| write `999.9` | read back `223.1` (unchanged) |

So `write.success == true` is not evidence that the value took effect on this
protocol. For read/write tests use an address from the writable set (for example
the communication address or date/time object identifiers, written through
`WriteAddress` / `WriteDateTime`), or verify with a following read instead of
trusting the write response.

Serial (`Parity=2`, even) is used by this protocol - passing `Parity=0` will not
connect to a meter that expects even parity.

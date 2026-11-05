# Fuji SPB protocol API subset

`FujiSPB` and `FujiSPBOverTcp` implement the Fuji PLC SPB protocol.
`FujiSPB` is the serial version. `FujiSPBOverTcp` is the TCP pass-through
version. Both share the same address rules and high-level read/write API.

On the PLC side, configure SPB with no BCC calculation and include the `0D 0A`
end code.

Namespace:

```csharp
using HslCommunication.Profinet.Fuji;
```

## Class and transport table

| Class | Base | Transport | Typical port |
| --- | --- | --- | --- |
| `FujiSPB` | `DeviceSerialPort` | Serial | Serial only |
| `FujiSPBOverTcp` | `DeviceTcpNet` | TCP pass-through | No universal default; Demo uses `9600` |

## Constructors

```csharp
new FujiSPB()

new FujiSPBOverTcp()
new FujiSPBOverTcp(string ipAddress, int port)
```

For TCP, prefer `(ipAddress, port)`. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

Both classes expose:

```csharp
byte Station
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Station` defaults to `1`. `ByteTransform` is `RegularByteTransform` and
defaults to `DataFormat.DCBA`.

TCP client common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

`FujiSPB` is serial only.

```csharp
FujiSPB plc = new FujiSPB
{
    Station = 1
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("D100", 10);
}

plc.Close();
```

The Demo default serial parameters are `9600`, `8` data bits, no parity, and
`1` stop bit. Confirm these against the actual PLC configuration.

`FujiSPBOverTcp` uses `ConnectServer()`.

```csharp
FujiSPBOverTcp plc = new FujiSPBOverTcp("192.168.1.10", 9600)
{
    Station = 1
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo Fuji SPB address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `M0` | Internal relay | When reading as a word, `M2` represents bit `M32` |
| `X0` | Input relay | When reading as a word, `X2` represents bit `X32` |
| `Y0` | Output relay | When reading as a word, `Y2` represents bit `Y32` |
| `L0` | Latch relay | |
| `TC0` | Timer coil | |
| `TN0` | Timer current value | Word-oriented |
| `CC0` | Counter coil | |
| `CN0` | Counter current value | Word-oriented |
| `D0` | Data register | Use `D10.15` to access bit 15 of word 10 |
| `R0` | File register | Use `R10.15` to access bit 15 of word 10 |
| `W0` | Link register | Use `W10.15` to access bit 15 of word 10 |
| `s=2;M0` | Station-prefixed address | All addresses can carry an explicit station |

Bool access supports `X`, `Y`, `M`, `L`, `TC`, `CC`, and register bit addresses
such as `D10.15`, `R10.15`, and `W10.15`. Word/raw reads support `X`, `Y`,
`M`, `L`, `D`, `TN`, `CN`, `TC`, `CC`, `R`, and `W`.

The `s=<station>;address` prefix overrides the client `Station` for that
individual operation.

## Read methods

Both classes expose:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
```

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
OperateResult Write(string address, bool value)
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

Raw `byte[]` writes are word-oriented; the byte length should normally be even.
The current SPB implementation does not expose a working `Write(string, bool[])`
overload, so write bool values one at a time.

## Async methods

`FujiSPBOverTcp` provides native async core methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
```

`FujiSPB` provides:

```csharp
Task<OperateResult> WriteAsync(string address, bool value)
```

Other inherited async read/write methods are available from
`DeviceCommunication`. For `FujiSPB`, those inherited async methods fall back to
wrapping the synchronous implementation.

The inherited bool-array write async overload is not supported because the
underlying `Write(string, bool[])` method is not implemented by either class.

## Notes

- Use `Open()` after `SerialPortInni(...)` for `FujiSPB`.
- Use `ConnectServer()` for `FujiSPBOverTcp`.
- Confirm the TCP port against the serial-to-Ethernet converter. The Demo
  default is `9600`, but the class has no universal default port.
- A single SPB read is limited to 105 words; a single byte write is limited to
  103 words. Split larger operations manually.
- The low-level command-building methods are intentionally omitted from this
  reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

Verified end-to-end against a Demo `FujiSPBServer`: a value written by the client is visible on
the server, and a value written on the server is read back by the client. Both transports were
verified **against the same server instance**, so the TCP and serial pages were also checked to
share one data pool (write through TCP, read through serial, and the reverse).

Verified addresses: `D100`, `D0`, `M100`, `R100` (word devices).

### Server side

```json
{"operation":"virtualServer.create","parameters":{"type":"FujiSPBServer","tcpPort":9600,"startTcp":true}}
{"operation":"virtualServer.startSerial","parameters":{"serverId":"<serverId>","serialPort":"COM5-9600-8-N-1"}}
```

- `virtualServer.create` **reuses an existing window of the same type**, and when that server is
  already running the `tcpPort` you pass does **not** take effect. Read the port back from the
  response (`server.tcpPort`) and connect to that. To really change it, `virtualServer.stop` first.
- The serial slave needs a port that is free **and** paired with the port the client will open.

### Client A - TCP (`FormFujiSPBOverTcp`)

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="9600" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="0" Interval="1000" SleepTime="0" Type="FormFujiSPBOverTcp" Name="富士PLC访问Demo" Guid="86106f2df4854266a5659e55613ed3f7">
  <AddressList>
    <Address Value="M100" />
  </AddressList>
</Device>
```

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Must be `TcpPipe` for the TCP page. |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `9600` | Peer TCP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `0` | Station number. **The working page uses `0`** - with `1` the server accepts the TCP connection but never answers a request. |

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormFujiSPBOverTcp",
    "name": "FujiSPBOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "9600",
      "PipeSetting": "TcpPipe",
      "Station": "0"
    }
  }
}
```

### Client B - Serial (`FormFujiSPB`)

```xml
<Device PipeSetting="SerialPipe" ComPort="COM9" BaudRate="9600" DataBit="8" StopBit="1" Parity="0" Station="0" Interval="1000" SleepTime="0" Type="FormFujiSPB" Name="spb_rtu9" Guid="d657f9936d6e468396099fc85118e9ac" />
```

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `SerialPipe` | Must be `SerialPipe` for the serial page. |
| `ComPort` | `COM9` | Serial port name. `PortName` is accepted as an alias. |
| `BaudRate` | `9600` | Serial baud rate. |
| `DataBit` | `8` | Serial data bits. |
| `StopBit` | `1` | Serial stop bits. |
| `Parity` | `0` | Serial parity (0 = none). |
| `Station` | `0` | Station number; `0` verified. |

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormFujiSPB",
    "name": "FujiSPB serial (AI)",
    "parameters": {
      "PipeSetting": "SerialPipe",
      "ComPort": "COM9",
      "BaudRate": "9600",
      "DataBit": "8",
      "StopBit": "1",
      "Parity": "0",
      "Station": "0"
    }
  }
}
```

**Gotcha:** `Station` must be `0`. With `Station=1` the TCP page still reports "connected",
but every read and write times out, which looks like a server fault rather than a station mismatch.

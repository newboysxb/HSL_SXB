# Fatek Program protocol API subset

`FatekProgram` and `FatekProgramOverTcp` implement the FATEK PLC programming
port protocol. `FatekProgram` is the serial version. `FatekProgramOverTcp` is
the TCP pass-through version. Both share the same address rules and high-level
read/write API.

Namespace:

```csharp
using HslCommunication.Profinet.FATEK;
```

## Class and transport table

| Class | Base | Transport | Typical port |
| --- | --- | --- | --- |
| `FatekProgram` | `DeviceSerialPort` | Serial | Serial only |
| `FatekProgramOverTcp` | `DeviceTcpNet` | TCP pass-through | No universal default; Demo uses `2000` |

## Constructors

```csharp
new FatekProgram()

new FatekProgramOverTcp()
new FatekProgramOverTcp(string ipAddress, int port)
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

`FatekProgram` is serial only.

```csharp
FatekProgram plc = new FatekProgram
{
    Station = 1
};

plc.SerialPortInni("COM3", 9600, 7, System.IO.Ports.StopBits.Two, System.IO.Ports.Parity.Even);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("D100", 10);
}

plc.Close();
```

The Demo default serial parameters are `9600`, `7` data bits, even parity, and
`2` stop bits. Confirm these against the actual PLC configuration.

`FatekProgramOverTcp` uses `ConnectServer()`.

```csharp
FatekProgramOverTcp plc = new FatekProgramOverTcp("192.168.1.10", 2000)
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

The examples match the HslCommunicationDemo FATEK address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `M100` | Internal relay | `M0-M9999` |
| `X10` | Input relay | `X0-X9999` |
| `Y10` | Output relay | `Y0-Y9999` |
| `S100` | Step relay | `S0-S9999` |
| `T100` | Timer contact | `T0-T9999` |
| `RT100` | Timer current value | `RT0-RT9999` |
| `C100` | Counter contact | `C0-C9999` |
| `RC100` | Counter current value | `RC0-RC9999` |
| `D100` | Data register | `D0-D65535` |
| `R100` | File register | `R0-R65535` |
| `D100.0` | Data register bit | Use this format for a bit in a word register |
| `s=2;M100` | Station-prefixed address | All addresses can carry an explicit station |

Bool access is supported for `X`, `Y`, `M`, `S`, `T`, and `C`. Raw/word reads
support the word-oriented areas including `D`, `R`, `RT`, and `RC`; the library
also supports word access to the bit-area prefixes when appropriate.

The `s=<station>;address` prefix overrides the client `Station` for that
individual operation.

## Read methods

Both classes expose the core byte/bool reads:

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

Raw `byte[]` writes are word-oriented; the byte length should normally be even.
Typed string writes should use an even byte length, as shown by the Demo code.

## PLC control methods

```csharp
OperateResult Run()
OperateResult Run(byte station)
OperateResult Stop()
OperateResult Stop(byte station)
OperateResult<bool[]> ReadStatus()
OperateResult<bool[]> ReadStatus(byte station)
```

`ReadStatus()` returns seven status flags:

| Index | Meaning when true | Meaning when false |
| --- | --- | --- |
| `0` | RUN | STOP |
| `1` | Battery low | Normal |
| `2` | Ladder checksum error | Normal |
| `3` | ROM pack used | Not used |
| `4` | WDT timeout | Normal |
| `5` | ID set | ID not set |
| `6` | Emergency stop | Normal |

## Async methods

Both classes inherit async core and typed read/write methods from
`DeviceCommunication`, including `ReadAsync`, `ReadBoolAsync`, and `WriteAsync`.
For `FatekProgram`, the inherited async core methods fall back to wrapping the
synchronous implementation. `FatekProgramOverTcp` provides native async core
implementations.

`FatekProgramOverTcp` additionally exposes:

```csharp
Task<OperateResult> RunAsync()
Task<OperateResult> RunAsync(byte station)
Task<OperateResult> StopAsync()
Task<OperateResult> StopAsync(byte station)
Task<OperateResult<bool[]>> ReadStatusAsync()
Task<OperateResult<bool[]>> ReadStatusAsync(byte station)
```

## Notes

- Use `Open()` after `SerialPortInni(...)` for `FatekProgram`.
- Use `ConnectServer()` for `FatekProgramOverTcp`.
- Confirm the TCP port against the serial-to-Ethernet converter. The Demo
  default is `2000`, but the class has no universal default port.
- The low-level command-building methods are intentionally omitted from this
  reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `FatekProgrameServer`
- client page / form type: `FormFatekProgrameOverTcp`
- transport: TCP, server port `2000`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="2000" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="1" Interval="1000" SleepTime="0" Type="FormFatekProgrameOverTcp" Name="p_FormFatekProgrameOverTcp" Guid="2726c31ac90b459485788c2000004724" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `2000` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `1` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormFatekProgrameOverTcp",
    "name": "FormFatekProgrameOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "2000",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "1"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

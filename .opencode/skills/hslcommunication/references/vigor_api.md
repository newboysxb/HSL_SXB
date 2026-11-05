# Vigor PLC API subset

`VigorSerial` and `VigorSerialOverTcp` implement the Vigor VS-series
programming-port protocol. `VigorSerial` is the serial version.
`VigorSerialOverTcp` is the TCP pass-through version. Both share the same
address rules and high-level read/write API.

Namespace:

```csharp
using HslCommunication.Profinet.Vigor;
```

## Class and transport table

| Class | Base | Transport | Typical port |
| --- | --- | --- | --- |
| `VigorSerial` | `DeviceSerialPort` | Serial | Serial only |
| `VigorSerialOverTcp` | `DeviceTcpNet` | TCP pass-through | No universal default; Demo uses `5000` |

## Constructors

```csharp
new VigorSerial()

new VigorSerialOverTcp()
new VigorSerialOverTcp(string ipAddress, int port)
```

For TCP, prefer `(ipAddress, port)`. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

```csharp
byte Station
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Station` defaults to `0`. `ByteTransform` is `RegularByteTransform` and
defaults to `DataFormat.DCBA`.

TCP client common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

`VigorSerial` is serial only.

```csharp
VigorSerial plc = new VigorSerial
{
    Station = 0
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.Close();
```

The Demo UI default serial parameters are `9600`, `8` data bits, no parity, and
`1` stop bit. The source documentation also mentions `19200-8-N-1` as a common
Vigor setting. Confirm the actual PLC serial configuration.

`VigorSerialOverTcp` uses `ConnectServer()`.

```csharp
VigorSerialOverTcp plc = new VigorSerialOverTcp("192.168.1.10", 5000)
{
    Station = 0
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

The examples match the HslCommunicationDemo Vigor address declarations.

### Bit addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `X0` | Input relay | Octal; `X0-X377` |
| `Y0` | Output relay | Octal; `Y0-Y377` |
| `M0` | Auxiliary relay | `M0-M8191` |
| `S0` | Step relay | `S0-S4095` |
| `SM0` | Special relay | `SM0-SM511` |
| `TS0` | Timer contact | `TS0-TS511` |
| `TC0` | Timer coil | `TC0-TC511` |
| `CS0` | Counter contact | `CS0-CS255` |
| `CC0` | Counter coil | `CC0-CC255` |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `SD0` | Special register | `SD0-SD511` |
| `D0` | Data register | `D0-D8999`; bit access can use `D100.0` |
| `R0` | R register | `R0-R25999` |
| `T0` | Timer current value | `T0-T511` |
| `C0` | Counter current value | 16-bit `C0-C199`; 32-bit `C200-C255` |
| `s=2;D0` | Station-prefixed address | All addresses can carry a station |

For `C200` and above, use `int` or `uint` typed read/write methods because
these are 32-bit counters.

The `s=<station>;address` prefix overrides the client `Station` for that
individual operation.

## Read methods

Core reads:

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

Bool writes target the bit-oriented addresses such as `X/Y/M/SM/S/TS/TC/CS/CC`.
Word-register bit write support is limited; verify against the PLC version.

## Async methods

Both classes provide:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] values)
```

Inherited async core and typed read/write methods from `DeviceCommunication` are
also available. The TCP class uses the overridden device command builders for
async core reads/writes.

## Notes

- Use `Open()` after `SerialPortInni(...)` for `VigorSerial`.
- Use `ConnectServer()` for `VigorSerialOverTcp`.
- Confirm the serial baud rate. The Demo UI uses `9600`; the source
  documentation mentions `19200-8-N-1`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `VigorServer`
- client page / form type: `FormVigorSerialOverTcp`
- transport: TCP, server port `6008`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6008" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="0" Interval="1000" SleepTime="0" Type="FormVigorSerialOverTcp" Name="p_FormVigorSerialOverTcp" Guid="6f115024ecec4756b4d4c098bac7c7d7" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6008` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `0` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormVigorSerialOverTcp",
    "name": "FormVigorSerialOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6008",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

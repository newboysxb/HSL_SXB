# Panasonic Mewtocol API subset

`PanasonicMewtocol` and `PanasonicMewtocolOverTcp` implement the Panasonic
Mewtocol protocol. `PanasonicMewtocol` is the serial version.
`PanasonicMewtocolOverTcp` is the TCP pass-through version. Both share the same
address rules and high-level read/write API.

Namespace:

```csharp
using HslCommunication.Profinet.Panasonic;
```

## Class and transport table

| Class | Base | Transport | Typical port |
| --- | --- | --- | --- |
| `PanasonicMewtocol` | `DeviceSerialPort` | Serial | Serial only |
| `PanasonicMewtocolOverTcp` | `DeviceTcpNet` | TCP pass-through | No universal default; Demo uses `2000` |

## Constructors

```csharp
new PanasonicMewtocol()
new PanasonicMewtocol(byte station = 238)

new PanasonicMewtocolOverTcp()
new PanasonicMewtocolOverTcp(byte station = 238)
new PanasonicMewtocolOverTcp(string ipAddress, int port, byte station = 238)
```

The default station is `238` (`0xEE`). For TCP, prefer `(ipAddress, port,
station)`; the parameterless form inherits `127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

```csharp
byte Station
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.

TCP client common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

`PanasonicMewtocol` is serial only.

```csharp
PanasonicMewtocol plc = new PanasonicMewtocol(station: 238);

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

`PanasonicMewtocolOverTcp` uses `ConnectServer()`.

```csharp
PanasonicMewtocolOverTcp plc = new PanasonicMewtocolOverTcp("192.168.1.10", 2000, 238);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo Panasonic Mewtocol address
declarations.

### Bit addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `X0` | External input relay | `X33` equals `X3.3` |
| `Y0` | External output relay | `Y33` equals `Y3.3` |
| `R2.1` | Internal relay | `R21` equals `R2.1` |
| `SR2.1` | Special internal relay | `SR21` equals `SR2.1` |
| `T0` | Timer | |
| `C0` | Counter | |
| `L2.1` | Link relay | `L21` equals `L2.1` |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `D0` or `DT0` | Data register | `D100` equals `DT100`; bit access `D100.0` |
| `LD0` | Link register | Bit access `LD100.0` |
| `F0` | File register | Bit access `F100.0` |
| `S0` | Target value SV | |
| `K0` | Elapsed value EV | |
| `IX` | Index register IX | |
| `IY` | Index register IY | |
| `s=2;R2.1` | Station-prefixed address | All addresses can carry a station |

For word reads on bit-oriented areas, use the dotted word format such as
`X1.0`, `Y2.0`, `R100.0`.

Panasonic compact bit and dotted-bit addresses use a hexadecimal bit digit
`0`-`F`. For example:

- `X3.A` means `X3.10` and represents bit 10.
- `R21` means `R2.1`, not decimal address 21.
- The word portion before the bit remains decimal.

The `s=<station>;address` prefix overrides the client `Station` for that
individual operation.

## Read methods

Core byte and bool reads:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<bool[]> ReadBool(string[] address)
```

`ReadBool(string[])` reads discrete bool values from multiple addresses and
returns them in the same order.

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
OperateResult Write(string[] address, bool[] values)
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

`Write(string[], bool[])` writes discrete bool values to multiple addresses.

## PLC model method

```csharp
OperateResult<string> ReadPlcType()
```

## Async methods

`PanasonicMewtocolOverTcp` provides native async core methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult<bool[]>> ReadBoolAsync(string[] address)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] values)
Task<OperateResult> WriteAsync(string[] address, bool[] values)
```

`PanasonicMewtocol` provides:

```csharp
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string[] address)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string[] address, bool[] values)
```

Other inherited async read/write methods from `DeviceCommunication` are
available. For `PanasonicMewtocol`, inherited async byte/typed methods fall back
to wrapping the synchronous implementation.

## Notes

- Use `Open()` after `SerialPortInni(...)` for `PanasonicMewtocol`.
- Use `ConnectServer()` for `PanasonicMewtocolOverTcp`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `PanasonicMewtocolServer`
- client page / form type: `FormPanasonicMewOverTcp`
- transport: TCP, server port `6006`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6006" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="238" Interval="1000" SleepTime="0" Type="FormPanasonicMewOverTcp" Name="p_FormPanasonicMewOverTcp" Guid="5b4dca4adb884339b9ef2c85220c67bf" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6006` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `238` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormPanasonicMewOverTcp",
    "name": "FormPanasonicMewOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6006",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "238"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

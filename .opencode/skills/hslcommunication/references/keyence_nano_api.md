# Keyence Nano upper-link protocol API subset

`KeyenceNanoSerial` and `KeyenceNanoSerialOverTcp` implement the Keyence KV
upper-link protocol. `KeyenceNanoSerial` is the serial version.
`KeyenceNanoSerialOverTcp` is the TCP pass-through version. Both share the same
address rules and high-level read/write API.

Namespace:

```csharp
using HslCommunication.Profinet.Keyence;
```

## Class and transport table

| Class | Base | Transport | Typical port |
| --- | --- | --- | --- |
| `KeyenceNanoSerial` | `DeviceSerialPort` | Serial | Serial only |
| `KeyenceNanoSerialOverTcp` | `DeviceTcpNet` | TCP pass-through | `8501` by default |

## Constructors

```csharp
new KeyenceNanoSerial()

new KeyenceNanoSerialOverTcp()
new KeyenceNanoSerialOverTcp(string ipAddress, int port = 8501)
```

For TCP, prefer `(ipAddress, port)`. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

Both classes expose:

```csharp
byte Station
bool UseStation
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Station` and `UseStation` control whether station-prefixed connection frames
are used. For RS232 point-to-point use, set `UseStation = false`. For
RS485/RS422 multi-drop use, set `UseStation = true` and configure the correct
`Station`.

`ByteTransform` is `RegularByteTransform`, defaults to `DataFormat.DCBA`, and
sets `IsStringReverseByteWord = true`.

TCP client common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

`KeyenceNanoSerial` is serial only.

```csharp
KeyenceNanoSerial plc = new KeyenceNanoSerial
{
    Station = 0,
    UseStation = false
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("DM100");
}

plc.Close();
```

The Demo default serial parameters are `9600`, `8` data bits, no parity, and
`1` stop bit.

`KeyenceNanoSerialOverTcp` uses `ConnectServer()`.

```csharp
KeyenceNanoSerialOverTcp plc = new KeyenceNanoSerialOverTcp("192.168.1.10", 8501)
{
    Station = 0,
    UseStation = false
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("DM100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo Keyence Nano address declarations.

### Bit-oriented examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `R015` or `R0.15` | Input relay | Word+bit format; range `R00000-R99915` |
| `R015` or `R0.15` | Output relay | Word+bit format; range `R00000-R99915` |
| `B0` | Link relay | Hexadecimal address; range `B0-B3FFF` |
| `MR000` or `MR0.0` | Internal auxiliary relay | Range `MR00000-MR99915` |
| `LR015` or `LR0.15` | Latch relay | Range `LR00000-LR99915` |
| `CR015` or `CR0.15` | Control relay | Range `CR0000-CR3915` |
| `T0` | Timer coil | Range `T0000-T3999` |
| `C0` | Counter coil | Range `C0000-C3999` |
| `CTC0` | High-speed counter coil | Range `CTC0-CTC3` |
| `CTH0` | High-speed counter | Range `CTH0-CTH1` |
| `VB0` | Word coil | Hexadecimal; range `VB0000-VB3FFF` |

### Word and double-word examples

| Address | Meaning | Notes |
| --- | --- | --- |
| `CM0` | Control register | Range `CM0000-CM5999` |
| `DM0` | Data register | Range `DM00000-DM65534`; bit access `DM100.0` |
| `EM0` | Extended data register | Range `EM00000-EM65534`; bit access `EM100.0` |
| `FM0` | File register | `FM00000-FM32767`; bit access `FM100.0` |
| `ZF0` | File register | `ZF000000-ZF131071`; bit access `ZF100.0` |
| `W0` | Link register | Hexadecimal; range `W0000-W3FFF` |
| `TM0` | Temporary data register | Range `TM000-TM511` |
| `Z0` | Index register | Range `Z1-Z12` |
| `TC0` | Timer current value | Range `TC0000-TC3999` |
| `TS0` | Timer set value | Range `TS0000-TS3999` |
| `CC0` | Counter current value | Range `CC0000-CC3999` |
| `CS0` | Counter set value | Range `CS0000-CS3999` |
| `CTC0` | High-speed counter set value | Range `CTC0-CTC3` |
| `AT0` | Digital trimmer | Range `AT0-AT7` |
| `VM0` | Word memory | Range `VM0-VM59999`; bit access `VM100.0` |
| `unit=1;100` | Expansion buffer memory | Commercial authorization |

The Demo row for `AT0` displays `T0` in the address column, but the parser
expects `AT0` for the digital trimmer. Use `AT0`.

Bit access for word devices such as `DM`, `EM`, `FM`, `ZF`, and `VM` is
performed through the dot format, for example `DM100.0`.

The bit suffix accepts hexadecimal digits `0`-`F`. `DM100.A` means bit 10 and
`DM100.F` means bit 15. The numeric part of `B`, `W`, and `VB` is also
hexadecimal.

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

## Advanced methods

Both classes expose:

```csharp
OperateResult ClearError()
OperateResult<KeyencePLCS> ReadPlcType()
OperateResult<int> ReadPlcMode()
OperateResult SetPlcDateTime(DateTime dateTime)
OperateResult<string> ReadAddressAnnotation(string address)
OperateResult<byte[]> ReadExpansionMemory(byte unit, ushort address, ushort length)
OperateResult WriteExpansionMemory(byte unit, ushort address, byte[] value)
```

All advanced methods require commercial authorization and return
`InsufficientPrivileges` when the library is not activated.

`ReadPlcMode()` returns `0` for PROG mode or no registered ladder, and `1` for
RUN mode.

## Async methods

`KeyenceNanoSerialOverTcp` provides native async core methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] values)
```

`KeyenceNanoSerialOverTcp` also provides async advanced methods:

```csharp
Task<OperateResult> ClearErrorAsync()
Task<OperateResult<KeyencePLCS>> ReadPlcTypeAsync()
Task<OperateResult<int>> ReadPlcModeAsync()
Task<OperateResult> SetPlcDateTimeAsync(DateTime dateTime)
Task<OperateResult<string>> ReadAddressAnnotationAsync(string address)
Task<OperateResult<byte[]>> ReadExpansionMemoryAsync(byte unit, ushort address, ushort length)
Task<OperateResult> WriteExpansionMemoryAsync(byte unit, ushort address, byte[] value)
```

`KeyenceNanoSerial` provides:

```csharp
Task<OperateResult> WriteAsync(string address, bool value)
```

Other inherited async read/write methods from `DeviceCommunication` are
available. For `KeyenceNanoSerial`, those inherited async methods fall back to
wrapping the synchronous implementation.

## Notes

- Use `Open()` after `SerialPortInni(...)` for `KeyenceNanoSerial`.
- Use `ConnectServer()` for `KeyenceNanoSerialOverTcp`.
- Configure `Station` and `UseStation` according to the physical RS232/RS485
  wiring.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `KeyenceNanoServer`
- client page / form type: `FormKeyenceNanoSerialOverTcp`
- transport: TCP, server port `8501`
- verified address for read/write: `DM0`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="8501" ConnectTimeOut="5000" ReceiveTimeOut="10000" UseStation="false" Station="0" Interval="1000" SleepTime="0" Type="FormKeyenceNanoSerialOverTcp" Name="p_FormKeyenceNanoSerialOverTcp" Guid="66b281df939644b5a30f91f8697cbc6c" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `8501` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `UseStation` | `false` | -  |
| `Station` | `0` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormKeyenceNanoSerialOverTcp",
    "name": "FormKeyenceNanoSerialOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "8501",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "UseStation": "false",
      "Station": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

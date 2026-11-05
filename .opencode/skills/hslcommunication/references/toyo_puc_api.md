# ToyoPuc API subset

`ToyoPuc` implements the Toyota Engineering computer-link protocol through the
2PORT-EFR Ethernet module.

It is TCP-based. Configure the PLC Ethernet module with the correct IP address
and port before connecting. The Demo default port is `6000`.

Namespace:

```csharp
using HslCommunication.Profinet.Toyota;
```

## Constructors

```csharp
new ToyoPuc()
new ToyoPuc(string ipAddress, int port)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`. There is no constructor default protocol port; use the
PLC-configured port.

## Properties

```csharp
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.

TCP common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

```csharp
ToyoPuc plc = new ToyoPuc("192.168.1.10", 6000);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo ToyoPuc address declarations.

### Bit addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `K0` | Keep relay | Hexadecimal; `K0-K2FF` |
| `V0` | Special relay | Hexadecimal; `V0-VFF` |
| `T0` | Timer | Hexadecimal; `T0-T1FF` |
| `C0` | Counter | Hexadecimal; `C0-C1FF` |
| `L0` | Link relay | Hexadecimal; `L0-L7FF` |
| `X0` | Input relay | Hexadecimal; `X0-X7FF` |
| `Y0` | Output relay | Hexadecimal; `Y0-Y7FF` |
| `M0` | Internal relay | Hexadecimal; `M0-M7FF` |
| `EK0` | Extended keep relay | Hexadecimal; `EK0-EKFFF` |
| `EV0` | Extended special relay | Hexadecimal; `EV0-EVFFF` |
| `ET0` | Extended timer | Hexadecimal; `ET0-ET7FF` |
| `EC0` | Extended counter | Hexadecimal; `EC0-EC7FF` |
| `EL0` | Extended link relay | Hexadecimal; `EL0-EL1FFF` |
| `EX0` | Extended input relay | Hexadecimal; `EX0-EX7FF` |
| `EY0` | Extended output relay | Hexadecimal; `EY0-EY7FF` |
| `EM0` | Extended internal relay | Hexadecimal; `EM0-EM1FFF` |
| `GX0` | Extended input relay | Hexadecimal; `GX0-GXFFFF` |
| `GY0` | Extended output relay | Hexadecimal; `GY0-GYFFFF` |
| `GM0` | Extended internal relay | Hexadecimal; `GM0-GMFFFF` |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `S0` | Special register | Hexadecimal; `S0-S3FF` |
| `N0` | Timer/counter current value | Hexadecimal; `N0-N1FF` |
| `R0` | Link register | Hexadecimal; `R0-R7FF` |
| `D0` | Data register | Hexadecimal; `D0-D2FFF` |
| `B0` | File register | Hexadecimal; `B0-B1FFF` |
| `ES0` | Extended special register | Hexadecimal; `ES0-ES7FF` |
| `EN0` | Extended current value register | Hexadecimal; `EN0-EN7FF` |
| `H0` | Extended set value register | Hexadecimal; `H0-H7FF` |
| `U0` | Extended data register | Hexadecimal; `U0-U7FFF` |
| `EB0` | Extended file register | `EB0-EB7FFF`, `EB8000-EBFFFF`, `EB10000-EB17FFF` |
| `prg=1;D0` | Program-prefixed data register | Non-extended addresses can carry a `prg` parameter |

Word access is also available for bit-oriented address prefixes by reading the
corresponding word group. For example, `M100-M10F` can be accessed as `M10`.

The Demo table displays `ENO` for extended current-value registers, but the
source parser expects `EN0`. Use `EN0` in generated code.

## Read methods

Core reads:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte[]> ReadRandom(string[] address)
```

`ReadRandom(string[])` reads one word per address and returns raw bytes in the
same order as the input addresses.

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

## Async methods

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] values)
Task<OperateResult<byte[]>> ReadRandomAsync(string[] address)
```

Inherited typed async methods from `DeviceCommunication` are also available.

## Notes

- `ToyoPuc` is TCP only.
- Use the PLC-configured port. The Demo defaults to `6000`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `ToyoPucServer`
- client page / form type: `FormToyoPuc`
- transport: TCP, server port `6007`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6007" ConnectTimeOut="5000" ReceiveTimeOut="10000" Interval="1000" SleepTime="0" Type="FormToyoPuc" Name="p_FormToyoPuc" Guid="0d416a17ea2e44bfbe6bf3c9f94b7ee6" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6007` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormToyoPuc",
    "name": "FormToyoPuc (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6007",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

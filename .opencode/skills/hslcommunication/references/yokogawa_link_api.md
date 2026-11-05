# Yokogawa Link TCP API subset

`YokogawaLinkTcp` implements Yokogawa PLC binary communication over TCP.

The constructor has no default protocol port. The HslCommunicationDemo UI uses
`12289`.

Namespace:

```csharp
using HslCommunication.Profinet.Yokogawa;
```

## Constructors

```csharp
new YokogawaLinkTcp()
new YokogawaLinkTcp(string ipAddress, int port)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`.

## Properties

```csharp
byte CpuNumber
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`CpuNumber` defaults to `1`. `ByteTransform` is `RegularByteTransform` and
defaults to `DataFormat.CDAB`.

TCP common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

```csharp
YokogawaLinkTcp plc = new YokogawaLinkTcp("192.168.1.10", 12289)
{
    CpuNumber = 1
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

The examples match the HslCommunicationDemo Yokogawa Link address declarations.

### Bit addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `X0` | Input relay | Decimal |
| `Y0` | Output relay | |
| `M0` | Special relay | |
| `I0` | Internal relay | |
| `E0` | Shared relay | |
| `T0` | Timer relay | Timer coil |
| `C0` | Counter relay | Counter coil |
| `L0` | Link relay | |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `D0` | Data register | Bit access `D100.0` |
| `B0` | File register | |
| `F0` | Cache register | |
| `R0` | Shared register | |
| `V0` | Index register | |
| `Z0` | Special register | |
| `W0` | Link register | |
| `CN0` | Counter current value | |
| `TN0` | Timer current value | |
| `cpu=2;D100` | CPU-prefixed address | |
| `Special:unit=0;slot=1;100` | Special module data | Commercial authorization |

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

## Random read/write methods

```csharp
OperateResult<bool[]> ReadRandomBool(string[] address)
OperateResult WriteRandomBool(string[] address, bool[] value)
OperateResult<byte[]> ReadRandom(string[] address)
OperateResult<short[]> ReadRandomInt16(string[] address)
OperateResult<ushort[]> ReadRandomUInt16(string[] address)
OperateResult WriteRandom(string[] address, byte[] value)
OperateResult WriteRandom(string[] address, short[] value)
OperateResult WriteRandom(string[] address, ushort[] value)
```

All random read/write methods require commercial authorization and return
`InsufficientPrivileges` when the library is not activated.

## Advanced PLC methods

```csharp
OperateResult Start()
OperateResult Stop()
OperateResult ModuleReset()
OperateResult<int> ReadProgramStatus()
OperateResult<YokogawaSystemInfo> ReadSystemInfo()
OperateResult<DateTime> ReadDateTime()
OperateResult<byte[]> ReadSpecialModule(byte moduleUnit, byte moduleSlot, ushort dataPosition, ushort length)
```

These advanced methods require commercial authorization.

## Async methods

Async versions exist for core, random, and advanced methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool[] values)
Task<OperateResult<bool[]>> ReadRandomBoolAsync(string[] address)
Task<OperateResult> WriteRandomBoolAsync(string[] address, bool[] value)
Task<OperateResult<byte[]>> ReadRandomAsync(string[] address)
Task<OperateResult<short[]>> ReadRandomInt16Async(string[] address)
Task<OperateResult<ushort[]>> ReadRandomUInt16Async(string[] address)
Task<OperateResult> WriteRandomAsync(string[] address, byte[] value)
Task<OperateResult> WriteRandomAsync(string[] address, short[] value)
Task<OperateResult> WriteRandomAsync(string[] address, ushort[] value)
Task<OperateResult> StartAsync()
Task<OperateResult> StopAsync()
Task<OperateResult> ModuleResetAsync()
Task<OperateResult<int>> ReadProgramStatusAsync()
Task<OperateResult<YokogawaSystemInfo>> ReadSystemInfoAsync()
Task<OperateResult<DateTime>> ReadDateTimeAsync()
Task<OperateResult<byte[]>> ReadSpecialModuleAsync(byte moduleUnit, byte moduleSlot, ushort dataPosition, ushort length)
```

Inherited typed async methods from `DeviceCommunication` are also available.

## Notes

- `YokogawaLinkTcp` is TCP only.
- Use the PLC-configured port; the Demo uses `12289`.
- Low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `YokogawaLinkServer`
- client page / form type: `FormYokogawaLinkTcp`
- transport: TCP, server port `12289`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="12289" ConnectTimeOut="5000" ReceiveTimeOut="10000" UnitNumber="1" Interval="1000" SleepTime="0" Type="FormYokogawaLinkTcp" Name="p_FormYokogawaLinkTcp" Guid="9fce89590292444dbf2709d66afee8ee" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `12289` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `UnitNumber` | `1` | -  |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormYokogawaLinkTcp",
    "name": "FormYokogawaLinkTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "12289",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "UnitNumber": "1"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

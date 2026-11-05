# Fuji SPH Net API subset

`FujiSPHNet` implements the Fuji SPH TCP protocol.

The constructor default port is `18245`. The HslCommunicationDemo UI uses
`507` as its editable default; verify the actual PLC module port before
connecting.

Namespace:

```csharp
using HslCommunication.Profinet.Fuji;
```

## Constructors

```csharp
new FujiSPHNet()
new FujiSPHNet(string ipAddress, int port = 18245)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`.

## Properties

```csharp
byte ConnectionID
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ConnectionID` defaults to `0xFE` (`254`), which targets CPU0. CPU0 to CPU7
use `0xFE` through `0xF7`. P/PE link and FL-net destination modules use
`0xF6` through `0xEF`.

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.
The Demo exposes this byte-order setting, so adjust it if the PLC returns
reversed multi-byte values.

Common inherited TCP properties include `IpAddress`, `Port`, `ConnectTimeOut`,
and `ReceiveTimeOut`.

## Connection

`FujiSPHNet` is a TCP client and uses `ConnectServer()`.

```csharp
FujiSPHNet plc = new FujiSPHNet("192.168.1.10", 18245)
{
    ConnectionID = 0xFE
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("I0");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo Fuji SPH address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `M1.0` | M data block 1, word 0, bit 0 | |
| `M3.0` | M data block 3, word 0, bit 0 | |
| `M10.0` | M data block 10, word 0, bit 0 | |
| `I0` | Input relay word | Optional bit format is `I0.0` |
| `Q0` | Output relay word | Optional bit format is `Q0.0` |

SPH addresses are parsed as:

- `M<dataBlock>.<word>[.<bit>]`
- `I<word>[.<bit>]`
- `Q<word>[.<bit>]`

Only data blocks `1`, `3`, and `10` are supported for `M`.

This class does not support the generic `D100`-style register address used by
some Demo test snippets. Use the SPH address forms above.

## Read methods

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

Long word reads are split internally into multiple SPH commands.

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

Bool writes are implemented as read-modify-write word operations. The SPH
protocol does not provide a dedicated bit-write command, so this is not a
safe atomic bit write. Make sure the target address is only written by one
host application.

## Async methods

Async versions exist for the core read/write methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] value)
```

Inherited typed async methods from `DeviceCommunication` are also available.

## CPU control methods

```csharp
OperateResult CpuBatchStart()
OperateResult CpuBatchInitializeAndStart()
OperateResult CpuBatchStop()
OperateResult CpuBatchReset()
OperateResult CpuIndividualStart()
OperateResult CpuIndividualInitializeAndStart()
OperateResult CpuIndividualStop()
OperateResult CpuIndividualReset()
```

All CPU control methods require commercial authorization and return
`InsufficientPrivileges` when the library is not activated.

Async versions exist for the same CPU control methods:

```csharp
Task<OperateResult> CpuBatchStartAsync()
Task<OperateResult> CpuBatchInitializeAndStartAsync()
Task<OperateResult> CpuBatchStopAsync()
Task<OperateResult> CpuBatchResetAsync()
Task<OperateResult> CpuIndividualStartAsync()
Task<OperateResult> CpuIndividualInitializeAndStartAsync()
Task<OperateResult> CpuIndividualStopAsync()
Task<OperateResult> CpuIndividualResetAsync()
```

## Notes

- Confirm `ConnectionID` against the destination CPU or module.
- The constructor default port is `18245`; the Demo UI default is `507`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `FujiSPHServer`
- client page / form type: `FormFujiSPHNet`
- transport: TCP, server port `507`
- verified address for read/write: `M1.0`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="507" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="254" Interval="1000" SleepTime="0" Type="FormFujiSPHNet" Name="p_FormFujiSPHNet" Guid="e584ffaa368c483ca48d9cd7acba8a5e" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `507` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `254` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormFujiSPHNet",
    "name": "FormFujiSPHNet (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "507",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "254"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

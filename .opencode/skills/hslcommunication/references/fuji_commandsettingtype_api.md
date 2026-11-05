# Fuji Command-Setting-Type API subset

`FujiCommandSettingType` implements the Fuji Command-Setting-Type protocol over
TCP.

The class has no universal default port. The HslCommunicationDemo UI uses
`7000`; set the actual PLC or serial-to-Ethernet converter port explicitly.

Namespace:

```csharp
using HslCommunication.Profinet.Fuji;
```

## Constructors

```csharp
new FujiCommandSettingType()
new FujiCommandSettingType(string ipAddress, int port)
```

Prefer `(ipAddress, port)`. The parameterless form inherits `127.0.0.1:5000`
from `DeviceTcpNet`.

## Properties

```csharp
bool DataSwap
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`DataSwap` defaults to `false`. When `false`, the client uses
`ReverseBytesTransform`. When `true`, it switches to `RegularByteTransform`.
Set `DataSwap` according to the actual PLC data order.

Common inherited TCP properties include `IpAddress`, `Port`, `ConnectTimeOut`,
and `ReceiveTimeOut`.

## Connection

Normal TCP construction:

```csharp
FujiCommandSettingType plc = new FujiCommandSettingType("192.168.1.10", 7000)
{
    DataSwap = false
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte> read = plc.ReadByte("S100");
}

plc.ConnectClose();
```

For a serial-to-Ethernet pass-through device, replace `CommunicationPipe` with
`PipeTcpNet`:

```csharp
using HslCommunication.Core.Pipe;

FujiCommandSettingType plc = new FujiCommandSettingType
{
    DataSwap = false,
    CommunicationPipe = new PipeTcpNet("192.168.1.10", 7000)
};

OperateResult connect = plc.ConnectServer();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo Fuji Command-Setting-Type address
declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `B0` | I/O relay | Word length 16 bits |
| `M0` | Auxiliary relay | Word length 16 bits |
| `K0` | Keep relay | Word length 16 bits |
| `F0` | Special relay | Word length 16 bits |
| `A0` | Announce relay | |
| `D0` | Differential relay | |
| `S0` | Step control | Word length 8 bits |
| `W9.0` | Current value of 0.1-sec timer | Word length 16 bits |
| `TS0` | Set value of timer | Word length 32 bits |
| `TR0` | Current value of timer | Word length 32 bits |
| `CS0` | Set value of counter | Word length 32 bits |
| `CR0` | Current value of counter | Word length 32 bits |
| `BD0` | Data memory in BCD | Word length 32 bits |
| `WL0` | P-link station 0 memory | Word length 16 bits |
| `W21.0` | No.2 block | P-link station 0 memory, 16 bits |
| `W22.0` | No.3 block | P-link station 0 memory, 16 bits |
| `W23.0` | No.4 block | P-link station 0 memory, 16 bits |
| `W24.0` | Direct I/O | Word length 16 bits |
| `W25.0` | Analog work area | Word length 32 bits |
| `W26.0` | Last value of differential relay | Word length 16 bits |
| `W30.0 ~ W109.0` | File memory | 16 or 32 bits depending on file |
| `W120.0` | P-link station 1 memory, No.1 block | Word length 16 bits |
| `W121.0` | P-link station 1 memory, No.2 block | Word length 16 bits |
| `W122.0` | P-link station 1 memory, No.3 block | Word length 16 bits |
| `W123.0` | P-link station 1 memory, No.4 block | Word length 16 bits |
| `W125.0` | Calendar | Word length 16 bits |

The supported `W` sub-addresses are `W9`, `W21-W26`, `W30-W109`,
`W120-W123`, and `W125`.

## Read methods

```csharp
OperateResult<byte> ReadByte(string address)
OperateResult<byte[]> Read(string address, ushort length)
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

Bool reads are not supported by this protocol implementation.

## Write methods

```csharp
OperateResult Write(string address, byte value)
OperateResult Write(string address, byte[] value)
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

Bool writes are not supported by this protocol implementation.

## Async methods

```csharp
Task<OperateResult<byte>> ReadByteAsync(string address)
Task<OperateResult> WriteAsync(string address, byte value)
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
```

Inherited typed async methods from `DeviceCommunication` are also available.

## Notes

- `FujiCommandSettingType` is TCP-based. For normal use, prefer the
  `(ipAddress, port)` constructor.
- For serial-to-Ethernet pass-through, assign a `PipeTcpNet` object to
  `CommunicationPipe`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `FujiCSTServer`
- client page / form type: `FormFujiCSTNet`
- transport: TCP, server port `7000`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="7000" ConnectTimeOut="5000" ReceiveTimeOut="10000" Interval="1000" SleepTime="0" Type="FormFujiCSTNet" Name="p_FormFujiCSTNet" Guid="84699a6d54af408fa748fe3db398f087" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `7000` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormFujiCSTNet",
    "name": "FormFujiCSTNet (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "7000",
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

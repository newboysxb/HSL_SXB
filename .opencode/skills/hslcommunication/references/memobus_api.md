# Memobus API subset

`MemobusTcpNet` and `MemobusUdpNet` implement Yaskawa's extended Memobus
protocol. They share the same address rules and high-level read/write API.

Namespace:

```csharp
using HslCommunication.Profinet.YASKAWA;
```

## Class and transport table

| Class | Base | Transport |
| --- | --- | --- |
| `MemobusTcpNet` | `DeviceTcpNet` | TCP |
| `MemobusUdpNet` | `MemobusTcpNet` | UDP |

## Constructors

```csharp
new MemobusTcpNet()
new MemobusTcpNet(string ipAddress, int port = 502)

new MemobusUdpNet()
new MemobusUdpNet(string ipAddress, int port = 502)
```

Prefer `(ipAddress, port)`. Parameterless forms inherit `127.0.0.1:5000` from
`DeviceTcpNet`.

The class constructor default port is `502`. The HslCommunicationDemo UI uses
`9999`.

## Properties

```csharp
byte CpuTo
byte CpuFrom
DataFormat DataFormat
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`CpuTo` defaults to `2`. `CpuFrom` defaults to `1`.

`ByteTransform` is `RegularByteTransform` with `DataFormat.CDAB`.
`DataFormat` is a convenience property over `ByteTransform`.

TCP/UDP common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`. Both classes also expose `LocalBinding`.

## Connection

TCP:

```csharp
MemobusTcpNet plc = new MemobusTcpNet("192.168.1.10", 9999)
{
    CpuFrom = 1,
    CpuTo = 2
};

OperateResult connect = plc.ConnectServer();
```

UDP:

```csharp
MemobusUdpNet plc = new MemobusUdpNet("192.168.1.10", 9999)
{
    CpuFrom = 1,
    CpuTo = 2
};

OperateResult connect = plc.ConnectServer();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

### Local client port

If the PLC requires the client to use a specific local port, set `LocalBinding`.
For example, when the PLC expects the client source port to be `9999`:

```csharp
plc.LocalBinding = new System.Net.IPEndPoint(
    System.Net.IPAddress.Parse("192.168.1.100"),
    9999);
```

This applies to both TCP and UDP clients.

## Address rules

The examples match the HslCommunicationDemo Memobus address declarations.

### Bool addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `100` | Coil | Default bool read uses function code `01` |
| `x=2;100` | Input relay | Use `x=2` for input relay read |
| `mfc=67;x=61;0` | Bit data | Explicit MFC and SFC |
| `M100.0` | Holding-register bit | Also supports `MB1000` style |
| `S100.0` | System-register bit | Also supports `SB1000` style |
| `G100.0` | Data-register bit | Also supports `GB1000` style |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `100` | Holding register | Default word read uses function code `03` |
| `x=4;100` | Input register | |
| `x=9;100` | Extended holding register | Write uses function code `0B` |
| `M100` | Holding register | Uses `43` main function code for large addresses |
| `G100` | Data register | Uses `43` main function code for large addresses |
| `I100` | Input register | Uses `43` main function code for large addresses |
| `O100` | Output register | Uses `43` main function code for large addresses |
| `S100` | System register | Uses `43` main function code for large addresses |

Function codes:

- Default bool read: `x=1`
- Input relay read: `x=2`
- Default word read: `x=3`
- Input register read: `x=4`
- Extended holding register read: `x=9`
- Extended input register read: `x=10`
- Extended holding register write: `x=11`

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte[]> ReadRandom(string[] address)
OperateResult<byte[]> ReadRandom(ushort[] address)
```

`ReadRandom(ushort[])` targets extended holding registers.

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
OperateResult WriteRandom(ushort[] address, byte[] values)
```

`WriteRandom(ushort[], byte[])` writes extended holding registers. The byte
array length must be twice the address array length.

## Async methods

Async versions exist for the core read/write, random read/write, and typed
methods from `DeviceCommunication`.

## Notes

- For UDP, use `MemobusUdpNet`; the protocol is otherwise the same.
- If connection fails after verifying IP and port, check whether the PLC
  requires a specific client-side port. Use `LocalBinding` as shown above.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `MemobusTcpServer`
- client page / form type: `FormYASKAWAMemobusTcpNet`
- transport: TCP, server port `9999`
- verified address for read/write: `100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="9999" ConnectTimeOut="5000" ReceiveTimeOut="10000" DataFormat="2" cpu_from="1" cpu_to="2" Interval="1000" SleepTime="0" Type="FormYASKAWAMemobusTcpNet" Name="p_FormYASKAWAMemobusTcpNet" Guid="c10146d193834f30b793a44338d94a08" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `9999` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `DataFormat` | `2` | Byte/word order enum index. Some protocol servers only accept a specific value. |
| `cpu_from` | `1` | -  |
| `cpu_to` | `2` | -  |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormYASKAWAMemobusTcpNet",
    "name": "FormYASKAWAMemobusTcpNet (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "9999",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "DataFormat": "2",
      "cpu_from": "1",
      "cpu_to": "2"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

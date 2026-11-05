# Verified ModbusTcpNet API subset

Namespaces:

```csharp
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.ModBus;
```

Constructors:

```csharp
new ModbusTcpNet()
new ModbusTcpNet(string ipAddress, int port = 502, byte station = 0x01)
```

Prefer the `(ipAddress, port, station)` constructor. The parameterless constructor
inherits `127.0.0.1:5000` from `DeviceTcpNet`; setting only `IpAddress` later does
not change that port.

Properties:

```csharp
string IpAddress
int Port
int ConnectTimeOut
int ReceiveTimeOut
byte Station
DataFormat DataFormat
bool AddressStartWithZero
bool IsStringReverse
bool StationCheckMatch
int BroadcastStation
int WordReadBatchLength
bool DisableFunctionCode06
bool EnableWriteMaskCode
bool AutoReConnect
SoftIncrementCount MessageId
IByteTransform ByteTransform
ILogNet LogNet
```

`DataFormat` defaults to `DataFormat.CDAB` for `ModbusTcpNet`. Values are
`DataFormat.ABCD`, `DataFormat.BADC`, `DataFormat.CDAB`, and `DataFormat.DCBA`.
If typed values appear reversed, change this property or use `format=` in the
address string.

Connection:

```csharp
OperateResult ConnectServer()
Task<OperateResult> ConnectServerAsync()
OperateResult ConnectClose()
Task<OperateResult> ConnectCloseAsync()
```

Read methods:

```csharp
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<bool> ReadCoil(string address)
OperateResult<bool[]> ReadCoil(string address, ushort length)
OperateResult<bool> ReadDiscrete(string address)
OperateResult<bool[]> ReadDiscrete(string address, ushort length)
OperateResult<byte[]> Read(string address, ushort length)
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

Async read methods exist for the same operations, for example:

```csharp
Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
```

Single-value overloads return `OperateResult<T>`. Array overloads that accept
`length` return `OperateResult<T[]>`.

Write methods:

```csharp
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, short value)
OperateResult Write(string address, ushort value)
OperateResult Write(string address, int value)
OperateResult Write(string address, uint value)
OperateResult Write(string address, long value)
OperateResult Write(string address, ulong value)
OperateResult Write(string address, float value)
OperateResult Write(string address, double value)
OperateResult Write(string address, string value)
OperateResult WriteMask(string address, ushort andMask, ushort orMask)
```

Register-bit writes such as `Write("100.2", true)` use Modbus mask function
`0x16` and require commercial authorization. The free version returns
`InsufficientPrivileges` for that path.

Result object:

```csharp
bool IsSuccess
T Content
string Message
int ErrorCode
```

Namespace notes:

- `Task` and async methods require `using System.Threading.Tasks;`
- `Encoding` requires `using System.Text;`
- `SoftIncrementCount` requires `using HslCommunication.BasicFramework;`
- `ILogNet` requires `using HslCommunication.LogNet;`

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `ModbusTcpServer`
- client page / form type: `FormModbus`
- transport: TCP, server port `502`
- verified address for read/write: `100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="502" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="1" AddressStartWithZero="true" DataFormat="2" StringReverse="false" BroadcastStation="" StationCheckMatch="true" WordReadBatchLength="" DisableFunctionCode06="false" Interval="1000" SleepTime="0" Type="FormModbus" Name="p_FormModbus" Guid="883e1b99a55e43e2b379ea1bab98b273" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `502` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `1` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |
| `AddressStartWithZero` | `true` | Whether addresses are zero-based. |
| `DataFormat` | `2` | Byte/word order enum index. Some protocol servers only accept a specific value. |
| `StringReverse` | `false` | Reverse the two bytes of a string. |
| `BroadcastStation` | `` | -  |
| `StationCheckMatch` | `true` | -  |
| `WordReadBatchLength` | `` | -  |
| `DisableFunctionCode06` | `false` | -  |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormModbus",
    "name": "FormModbus (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "502",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "1",
      "AddressStartWithZero": "true",
      "DataFormat": "2",
      "StringReverse": "false",
      "BroadcastStation": "",
      "StationCheckMatch": "true",
      "WordReadBatchLength": "",
      "DisableFunctionCode06": "false"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

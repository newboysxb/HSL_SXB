# Inovance EasyNet API subset

`InovanceEasyNet` implements the Inovance EasyNet protocol directly on
`DeviceTcpNet`. It does not derive from Modbus or CIP and uses its own address
rules.

There is no universal default port in the class constructor. The Demo TCP port
is `12939`; use the port configured in the PLC.

Namespace:

```csharp
using HslCommunication.Profinet.Inovance;
```

## Constructors

```csharp
new InovanceEasyNet()
new InovanceEasyNet(string ipAddress, int port)
```

The parameterless constructor inherits `127.0.0.1:5000` from `DeviceTcpNet`.
For a real PLC, prefer `(ipAddress, port)`.

## Properties

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.

## Connection

```csharp
InovanceEasyNet plc = new InovanceEasyNet("192.168.1.10", 12939);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address examples

The examples match the HslCommunicationDemo Inovance EasyNet address
declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `M0` | Middle relay | `M0-M7990` |
| `S0` | Middle relay | `S0-S4090` |
| `X0` | Input | `X0-X1770`, octal |
| `Y0` | Output | `Y0-Y377`, octal |
| `B0` | Special relay | `B0-B32760` |
| `D0` | Data register | `D0-D7990`; read a bit with `D100.0` |
| `W0` | Link register | `W0-W32760`; read a bit with `W100.0` |
| `R0` | File register | `R0-R32760`; read a bit with `R100.0` |
| `UW2100010` | U register | U-area addresses use hexadecimal digits |

`InovanceEasyNet` converts X/Y with `Convert.ToInt32(..., 8)`. Therefore
`X10` and `Y10` are PLC octal addresses, not hexadecimal. Only the U-area
addresses use hexadecimal digits.

## Read methods

`InovanceEasyNet` inherits the standard `DeviceCommunication` API.

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
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
OperateResult Write(string address, string value, Encoding encoding)
```

## Async methods

Async versions exist for the core read/write methods and the inherited typed
methods from `DeviceCommunication`.

## Notes

Use the normal inherited read/write API above.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `InovanceEasyNetServer`
- client page / form type: `FormInovanceEasyNet`
- transport: TCP, server port `12939`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="12939" ConnectTimeOut="5000" ReceiveTimeOut="10000" Interval="1000" SleepTime="0" Type="FormInovanceEasyNet" Name="p_FormInovanceEasyNet" Guid="48b5528debb0498c9a2ec0f27763eb9a" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `12939` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormInovanceEasyNet",
    "name": "FormInovanceEasyNet (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "12939",
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

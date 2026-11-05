# Melsec A3C clients API subset

`A3C` stands for the QnA-compatible 3C frame protocol. Mitsubishi Q-series C24
serial communication modules support QnA-compatible 3C, 4C, 2C, and A-compatible
1C frames.

The supported format numbers are:

- QnA-compatible 3C frame: formats `1`, `2`, `3`, `4`.
- QnA-compatible 4C frame: formats `1`, `2`, `3`, `4`, `5`.
- QnA-compatible 2C frame: formats `1`, `2`, `3`, `4`.
- A-compatible 1C frame: formats `1`, `2`, `3`, `4`.

This class specifically uses the A3C frame and exposes `Format` for selecting
the A3C format number.

Namespace:

```csharp
using HslCommunication.Profinet.Melsec;
```

Constructors:

```csharp
new MelsecA3CNet()

new MelsecA3CNetOverTcp()
new MelsecA3CNetOverTcp(string ipAddress, int port)
```

For TCP pass-through, prefer the `(ipAddress, port)` constructor. The
parameterless `MelsecA3CNetOverTcp()` form inherits `127.0.0.1:5000`.

| Class | Transport |
| --- | --- |
| `MelsecA3CNet` | Serial |
| `MelsecA3CNetOverTcp` | TCP pass-through |

Properties:

```csharp
byte Station
bool SumCheck
int Format
bool EnableWriteBitToWordRegister
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Format` is `1` by default and can be `1`, `2`, `3`, or `4`. `SumCheck` controls
the A3C sum check and defaults to `true`.

Connection:

```csharp
// Serial
MelsecA3CNet plc = new MelsecA3CNet();
plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
plc.Open();

// TCP pass-through
MelsecA3CNetOverTcp plcTcp = new MelsecA3CNetOverTcp("192.168.1.10", 6000);
OperateResult connect = plcTcp.ConnectServer();
```

Methods:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult Write(string address, byte[] value)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult Write(string address, bool[] value)
OperateResult RemoteRun()
OperateResult RemoteStop()
OperateResult<string> ReadPlcType()
```

Typed methods inherited from `DeviceCommunication` are also available.

## Address rules

`MelsecA3CNet` and `MelsecA3CNetOverTcp` use `McAddressData.ParseMelsecFrom`, so
they accept the same address rules as the QnA 3E MC clients. The HslCommunication
Demo uses `GetMcAddress()` for these forms. See `references/melsec_mc_api.md`.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `McA3CServer`
- client page / form type: `FormMelsec3COverTcp`
- transport: TCP, server port `6011`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6011" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="0" EnableWriteBitToWordRegister="支持使用位写入字寄存器(实际读字，修改位，写字)" Sumcheck="true" MelsecFormat="0" Interval="1000" SleepTime="0" Type="FormMelsec3COverTcp" Name="p_FormMelsec3COverTcp" Guid="026f56f250a1484d9c0ec474cfa27953" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6011` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `0` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |
| `EnableWriteBitToWordRegister` | `支持使用位写入字寄存器(实际读字，修改位，写字)` | Allow bit writes into word registers. |
| `Sumcheck` | `true` | Append/verify the checksum byte. |
| `MelsecFormat` | `0` | Frame format enum index for the A-series protocol. **A negative value (page never initialised) breaks the connection.** |

`MelsecFormat` is `0` on the page by default (see the table above). Pass it explicitly only if the PLC
needs a different A3C frame format.

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormMelsec3COverTcp",
    "name": "FormMelsec3COverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6011",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "0",
      "EnableWriteBitToWordRegister": "支持使用位写入字寄存器(实际读字，修改位，写字)",
      "Sumcheck": "true",
      "MelsecFormat": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

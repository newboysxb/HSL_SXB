# Omron HostLink clients API subset

Namespace:

```csharp
using HslCommunication.Profinet.Omron;
```

Constructors:

```csharp
new OmronHostLink()

new OmronHostLinkOverTcp()
new OmronHostLinkOverTcp(string ipAddress, int port)
```

For TCP pass-through, prefer the `(ipAddress, port)` constructor. The
parameterless `OmronHostLinkOverTcp()` form inherits `127.0.0.1:5000`.

| Class | Transport |
| --- | --- |
| `OmronHostLink` | Serial |
| `OmronHostLinkOverTcp` | TCP pass-through |

Properties:

```csharp
byte ICF
byte DA2
byte SA2
byte SID
byte ResponseWaitTime
byte UnitNumber
int ReadSplits
OmronPlcType PlcType
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

Connection:

```csharp
// Serial. HostLink normally uses 9600 baud, 7 data bits, even parity, 1 stop bit.
OmronHostLink plc = new OmronHostLink();
plc.SerialPortInni("COM3", 9600);
plc.Open();

// TCP pass-through
OmronHostLinkOverTcp plcTcp = new OmronHostLinkOverTcp("192.168.1.10", 9600);
OperateResult connect = plcTcp.ConnectServer();
```

Methods:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address)
OperateResult Write(string address, byte[] value)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult Write(string address, bool[] value)
```

Typed methods inherited from `DeviceCommunication` are also available.

## Address rules

The address examples are the same as FINS, plus station-number syntax:

```text
D0 / DM0
C0 / CIO0
W0 / WR0
H0 / HR0
A0 / AR0
E0.0 / EM0.0
E100 / EM100
TIM0
CNT0
IR0
DR0
CF1.2
s=2;D0
```

See `references/omron_fins_api.md` for detailed area descriptions.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `OmronHostLinkServer`
- client page / form type: `FormOmronHostLinkOverTcp`
- transport: TCP, server port `6005`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6005" ConnectTimeOut="5000" ReceiveTimeOut="10000" DataFormat="2" Station="0" UnitNumber="0" PCUnitNumber="0" DeviceId="0" Interval="1000" SleepTime="0" Type="FormOmronHostLinkOverTcp" Name="p_FormOmronHostLinkOverTcp" Guid="05d54bc61d394c6fbc2c4ce41c0f20ae" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6005` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `DataFormat` | `2` | Byte/word order enum index. Some protocol servers only accept a specific value. |
| `Station` | `0` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |
| `UnitNumber` | `0` | -  |
| `PCUnitNumber` | `0` | -  |
| `DeviceId` | `0` | -  |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormOmronHostLinkOverTcp",
    "name": "FormOmronHostLinkOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6005",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "DataFormat": "2",
      "Station": "0",
      "UnitNumber": "0",
      "PCUnitNumber": "0",
      "DeviceId": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

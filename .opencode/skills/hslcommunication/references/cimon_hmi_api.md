# Cimon HMI Protocol clients API subset

`CimonHmiProtocol` is the CIMON PLC HMI protocol client.

The default port is `10260`.

Namespace:

```csharp
using HslCommunication.Profinet.Cimon;
```

Constructors:

```csharp
new CimonHmiProtocol()
new CimonHmiProtocol(string ip, int port = 10260)
```

Prefer the `(ip, port)` constructor. The parameterless constructor sets
`127.0.0.1:10260`.

## Properties

```csharp
byte FrameNo
```

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

`CimonHmiProtocol` uses `ReverseWordTransform`.

## Connection

`CimonHmiProtocol` is a TCP client and uses `ConnectServer()`.

```csharp
CimonHmiProtocol plc = new CimonHmiProtocol("192.168.1.10", 10260)
{
    FrameNo = 1
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("D100", 2);
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples below match the HslCommunicationDemo CIMON address examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `X0` | Input relay | Range `X0 ~ X511F`; bit access `X100.0` |
| `Y0` | Output relay | Range `Y0 ~ Y511F`; bit access `Y100.0` |
| `M0` | Internal relay | Range `M0 ~ M999F`; bit access `M100.0` |
| `K0` | Keep relay | Range `K0 ~ K999F`; bit access `K100.0` |
| `L0` | Link relay | Range `L0 ~ L999F`; bit access `L100.0` |
| `T0` | Timer status | Range `T0 ~ T4095`; bit access `T100.0` |
| `C0` | Counter status | Range `C0 ~ C4095`; bit access `C100.0` |
| `F0` | Internal flag | Range `F0 ~ F999F`; bit access `F100.0` |
| `D100.1` | Data register bit | Reads a bit from a data register |
| `TC0` | Timer current value | |
| `CC0` | Counter current value | |
| `TS0` | Timer | |
| `CS0` | Counter | |
| `D0` | Data register | Range `D0 ~ D31999` |
| `F0` | F register | Range `F0 ~ F127` |
| `Z0` | Z register | Range `Z0 ~ Z127` |
| `S0` | S register | Range `S0 ~ S99` |
| `f=2;D0` | Frame-prefixed address | Carries an explicit frame/station number |

The bit suffix accepts hexadecimal digits `0`-`F`. For example, `X100.A`
means bit 10 and `X100.F` means bit 15. The word number before the dot is
decimal.

## Read methods

`CimonHmiProtocol` inherits the raw read API from `DeviceCommunication`:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
```

Inherited typed read methods such as `ReadInt16`, `ReadInt32`, `ReadFloat`, and
`ReadString` are available through `DeviceCommunication`.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] value)
```

Inherited typed write methods such as `Write(string, short)`,
`Write(string, int)`, and `Write(string, float)` are available through
`DeviceCommunication`.

## Async methods

Async versions exist for the core read/write methods, including `ReadAsync`,
`ReadBoolAsync`, `WriteAsync`, and the inherited typed async
methods from `DeviceCommunication`.

## Notes

- Verify the frame number against the PLC configuration.
- Use `f=<frame>;address` when a specific frame number must be used for one
  read/write operation.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `CimonServer`
- client page / form type: `FormCimonHmiProtocol`
- transport: TCP, server port `10260`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="10260" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="1" Interval="1000" SleepTime="0" Type="FormCimonHmiProtocol" Name="p_FormCimonHmiProtocol" Guid="45e037ade3754ec4a740dee99567d311" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `10260` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `1` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormCimonHmiProtocol",
    "name": "FormCimonHmiProtocol (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "10260",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "1"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

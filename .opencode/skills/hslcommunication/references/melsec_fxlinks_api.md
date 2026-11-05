# Melsec FxLinks clients API subset

Namespace:

```csharp
using HslCommunication.Profinet.Melsec;
```

Constructors:

```csharp
new MelsecFxLinks()

new MelsecFxLinksOverTcp()
new MelsecFxLinksOverTcp(string ipAddress, int port)
```

For TCP pass-through, prefer the `(ipAddress, port)` constructor. The
parameterless `MelsecFxLinksOverTcp()` form inherits `127.0.0.1:5000`.

| Class | Transport |
| --- | --- |
| `MelsecFxLinks` | Serial, usually RS485 |
| `MelsecFxLinksOverTcp` | TCP pass-through |

Properties:

```csharp
byte Station
byte WaittingTime
bool SumCheck
int Format
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

Connection:

```csharp
// Serial
MelsecFxLinks plc = new MelsecFxLinks();
plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
plc.Open();

// TCP pass-through
MelsecFxLinksOverTcp plcTcp = new MelsecFxLinksOverTcp("192.168.1.10", 6000);
OperateResult connect = plcTcp.ConnectServer();
```

Methods:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult Write(string address, byte[] value)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult Write(string address, bool[] value)
OperateResult StartPLC(string parameter = "")
OperateResult StopPLC(string parameter = "")
OperateResult<string> ReadPlcType(string parameter = "")
```

`StartPLC`, `StopPLC`, and `ReadPlcType` require commercial authorization and
will return `InsufficientPrivileges` when the library is not activated.

Typed methods inherited from `DeviceCommunication` are also available.

## Address rules

This list matches the HslCommunicationDemo FxLinks address examples.

| Address | Type | Notes |
| --- | --- | --- |
| `M0` | Internal relay | Bit/word |
| `X0` | Input relay | Octal address |
| `Y0` | Output relay | Octal address |
| `S0` | Step relay | Bit/word |
| `TS0` | Timer contact | Bit |
| `CS0` | Counter contact | Bit |
| `D0` | Data register | Word; use `D100.0` for bit access |
| `R0` | File register | Word |
| `TN0` | Timer current value | Word |
| `CN0` | Counter current value | Word |
| `s=2;D0` | Data register | Carry an explicit station number |

X and Y are parsed directly in base 8 by this protocol implementation.
`X10` means PLC `X10`, which is decimal address 8, not hexadecimal `0x10`.
Do not describe X/Y as hexadecimal for this client.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `FxLinksServer`
- client page / form type: `FormMelsecLinksOverTcp`
- transport: TCP, server port `6013`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6013" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="0" Timeout="0" SumCheck="true" MelsecFormat="0" Interval="1000" SleepTime="0" Type="FormMelsecLinksOverTcp" Name="p_FormMelsecLinksOverTcp" Guid="f66db8ae46b04054b0e876017f8a741f" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6013` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `0` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |
| `Timeout` | `0` | -  |
| `SumCheck` | `true` | Append/verify the checksum byte. |
| `MelsecFormat` | `0` | Frame format enum index for the A-series protocol. **A negative value (page never initialised) breaks the connection.** |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormMelsecLinksOverTcp",
    "name": "FormMelsecLinksOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6013",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "0",
      "Timeout": "0",
      "SumCheck": "true",
      "MelsecFormat": "0"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

# Siemens PPI clients API subset

`SiemensPPI` and `SiemensPPIOverTcp` implement the Siemens PPI protocol for
S7-200 series PLC serial communication.

- `SiemensPPI` is the serial version.
- `SiemensPPIOverTcp` is the TCP pass-through version.

Namespace:

```csharp
using HslCommunication.Profinet.Siemens;
```

Both classes use `ReverseBytesTransform`.

## SiemensPPI serial client

Constructors:

```csharp
new SiemensPPI()
```

Properties:

```csharp
byte Station
string PortName
int BaudRate
bool RtsEnable
```

Connection:

```csharp
SiemensPPI plc = new SiemensPPI
{
    Station = 2
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.Even);
plc.RtsEnable = true;
OperateResult open = plc.Open();

if (open.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("M100", 2);
}

plc.Close();
```

Use `Open()` and `Close()` for the serial client.

## SiemensPPIOverTcp client

Constructors:

```csharp
new SiemensPPIOverTcp()
new SiemensPPIOverTcp(string ipAddress, int port)
```

Prefer the `(ipAddress, port)` constructor. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

Properties:

```csharp
byte Station
string IpAddress
int Port
```

Connection:

```csharp
SiemensPPIOverTcp plc = new SiemensPPIOverTcp("192.168.1.10", 102)
{
    Station = 2
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("M100", 2);
}

plc.ConnectClose();
```

## Address rules

The examples below match the HslCommunicationDemo Siemens PPI address examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `I0` | Input | Bit example: `I1.6` |
| `Q0` | Output | Bit example: `Q1.6` |
| `M0` | Merker/internal memory | Bit example: `M1.6` |
| `DB1.0` | Data block | Bit example: `DB1.0.1` |
| `V0` | Data register | Equivalent to `DB1.0` |
| `T0` | Timer | Smart 200 test passed |
| `C0` | Counter | Smart 200 test passed |
| `AI0` | Analog input | Word access |
| `AQ0` | Analog output | Word access |
| `SYS0` | System internal address | Bit example: `SYS1.6` |
| `S0` | Internal register | Bit example: `S1.6` |
| `SM0` | Special internal register | Bit example: `SM1.6` |
| `s=1;M0` | Station-prefixed address | Carries station number in the address |

## Read methods

Both clients expose the same core PPI read API:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte> ReadByte(string address)
```

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] value)
OperateResult Write(string address, byte value)
```

## PLC control methods

```csharp
OperateResult Start(string parameter = "")
OperateResult Stop(string parameter = "")
OperateResult<string> ReadPlcType(string parameter = "")
```

## Async methods

Explicit async methods include:

```csharp
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<byte>> ReadByteAsync(string address)
Task<OperateResult> WriteAsync(string address, byte value)
```

`SiemensPPIOverTcp` also provides:

```csharp
Task<OperateResult> StartAsync(string parameter = "")
Task<OperateResult> StopAsync(string parameter = "")
Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "")
```

## Notes

- PPI communication is split into two operations and uses an internal lock.
- `SiemensPPIOverTcp` async methods may not have the same synchronization lock;
  avoid concurrent async calls from multiple threads.
- The protocol is primarily intended for S7-200 series PLCs.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `SiemensPPIServer`
- client page / form type: `FormSiemensPPIOverTcp`
- transport: TCP, server port `7005`
- verified address for read/write: `M100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="7005" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="2" Interval="1000" SleepTime="0" Type="FormSiemensPPIOverTcp" Name="p_FormSiemensPPIOverTcp" Guid="748ace7bddd942f2b5bb244718d73db1" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `7005` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `2` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormSiemensPPIOverTcp",
    "name": "FormSiemensPPIOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "7005",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "2"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

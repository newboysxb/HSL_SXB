# Siemens Fetch/Write clients API subset

`SiemensFetchWriteNet` uses the Siemens Fetch/Write protocol. It requires
protocol configuration on the PLC side and has no universal default port.
The tested reference configuration is for a CP343 Ethernet module. The PLC
itself may or may not support this protocol depending on the CPU/module.

Configuration reference:

[http://www.hsltechnology.cn:7900/Doc/HslCommunication](http://www.hsltechnology.cn:7900/Doc/HslCommunication)

Use the Siemens connection configuration section, then find the third chapter
for the Fetch/Write setup.

Namespace:

```csharp
using HslCommunication.Profinet.Siemens;
```

Constructors:

```csharp
new SiemensFetchWriteNet()
new SiemensFetchWriteNet(string ipAddress, int port)
```

The parameterless constructor inherits `127.0.0.1:5000` from `DeviceTcpNet`.
Because Fetch/Write has no default port, always prefer the `(ipAddress, port)`
constructor.

## Connection

`SiemensFetchWriteNet` is a TCP client and uses `ConnectServer()`.

```csharp
SiemensFetchWriteNet plc = new SiemensFetchWriteNet("192.168.1.10", 2000);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("M100", 2);
}
plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

`SiemensFetchWriteNet` uses `ReverseBytesTransform`.

## Address rules

The examples below match the HslCommunicationDemo Fetch/Write address examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `I0` | Input | Byte/word-oriented access |
| `Q0` | Output | Byte/word-oriented access |
| `M0` | Merker/internal memory | Byte/word-oriented access |
| `DB1.0` | Data block | Byte/word-oriented access |
| `T0` | Timer | Byte/word-oriented access |
| `C0` | Counter | Byte/word-oriented access |

Direct bit addresses such as `M100.0` are not supported by this client. Read the
containing byte and extract the required bit manually.

## Byte read/write methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte> ReadByte(string address)
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, byte value)
```

`length` is the number of bytes to read.

Example:

```csharp
OperateResult<byte[]> read = plc.Read("M100", 2);
if (read.IsSuccess)
{
    byte high = read.Content[0];
    byte low = read.Content[1];
}

OperateResult write = plc.Write("DB1.0", new byte[] { 0x12, 0x34 });
```

## Async methods

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult<byte>> ReadByteAsync(string address)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult> WriteAsync(string address, byte value)
```

## Important limitations

- Fetch/Write does not support direct `bool` read/write operations.
- Use byte-oriented reads and writes, then extract or modify bits manually.
- The PLC-side protocol must be configured before connecting.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `FetchWriteServer`
- client page / form type: `FormSiemensFW`
- transport: TCP, server port `7004`
- verified address for read/write: `M100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="7004" ConnectTimeOut="5000" ReceiveTimeOut="10000" Interval="1000" SleepTime="0" Type="FormSiemensFW" Name="p_FormSiemensFW" Guid="a0d0462a4293479c9600867f080e669a" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `7004` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormSiemensFW",
    "name": "FormSiemensFW (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "7004",
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

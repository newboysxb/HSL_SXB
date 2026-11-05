# Melsec FxSerial clients API subset

Namespace:

```csharp
using HslCommunication.Profinet.Melsec;
```

Constructors:

```csharp
new MelsecFxSerial()

new MelsecFxSerialOverTcp()
new MelsecFxSerialOverTcp(string ipAddress, int port)
```

For TCP pass-through, prefer the `(ipAddress, port)` constructor. The
parameterless `MelsecFxSerialOverTcp()` form inherits `127.0.0.1:5000`.

| Class | Transport |
| --- | --- |
| `MelsecFxSerial` | Programming-port serial |
| `MelsecFxSerialOverTcp` | TCP pass-through |

Properties:

```csharp
bool IsNewVersion
bool AutoChangeBaudRate   // serial class only
bool UseGOT               // TCP class only
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

Connection:

```csharp
// Serial
MelsecFxSerial plc = new MelsecFxSerial
{
    IsNewVersion = true
};
plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
plc.Open();

// TCP pass-through
MelsecFxSerialOverTcp plcTcp = new MelsecFxSerialOverTcp("192.168.1.10", 6000)
{
    IsNewVersion = true
};
OperateResult connect = plcTcp.ConnectServer();
```

Methods:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult Write(string address, byte[] value)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] value)
OperateResult ActivePlc()
```

For the new-protocol mode (`IsNewVersion = true`), writing a `bool[]` to bit
devices supports arbitrary array lengths in the current V13 source. Use the
normal typed write instead of manually packing every point into registers.

Typed methods inherited from `DeviceCommunication` are also available.

## Address rules

This list matches the HslCommunicationDemo FxSerial address examples.

| Address | Type | Notes |
| --- | --- | --- |
| `M0` | Internal relay | Bit/word; old protocol limits `M0..M1023` |
| `X0` | Input relay | Octal address |
| `Y0` | Output relay | Octal address |
| `S0` | Step relay | Bit/word |
| `TS0` | Timer contact | Bit |
| `TC0` | Timer coil | Bit |
| `CS0` | Counter contact | Bit |
| `CC0` | Counter coil | Bit |
| `D0` | Data register | Word; use `D100.0` for bit access |
| `TN0` | Timer current value | Word |
| `CN0` | Counter current value | Word |

X and Y are parsed directly in base 8 by this protocol implementation.
`X10` means PLC `X10`, which is decimal address 8, not hexadecimal `0x10`.
Do not describe X/Y as hexadecimal for this client.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `FxSerialServer`
- client page / form type: `FormMelsecSerialOverTcp`
- transport: TCP, server port `6014`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6014" ConnectTimeOut="5000" ReceiveTimeOut="10000" NewVersion="true" UseGOT="false" Interval="1000" SleepTime="0" Type="FormMelsecSerialOverTcp" Name="p_FormMelsecSerialOverTcp" Guid="05274205ad58442f95c00f6fe723eaef" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6014` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `NewVersion` | `true` | -  |
| `UseGOT` | `false` | -  |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormMelsecSerialOverTcp",
    "name": "FormMelsecSerialOverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6014",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "NewVersion": "true",
      "UseGOT": "false"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

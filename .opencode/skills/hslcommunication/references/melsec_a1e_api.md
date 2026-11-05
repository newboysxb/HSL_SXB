# Melsec A1E clients API subset

Namespace:

```csharp
using HslCommunication.Profinet.Melsec;
```

Constructors:

```csharp
new MelsecA1ENet()
new MelsecA1ENet(string ipAddress, int port)

new MelsecA1EAsciiNet()
new MelsecA1EAsciiNet(string ipAddress, int port)
```

Prefer the `(ipAddress, port)` constructors. The parameterless forms inherit
`127.0.0.1:5000` from `DeviceTcpNet`.

| Class | Frame format | Transport |
| --- | --- | --- |
| `MelsecA1ENet` | Binary A1E | TCP |
| `MelsecA1EAsciiNet` | ASCII A1E | TCP |

These classes target FX3U-style Ethernet modules. They do not implement the QnA
3E random-read methods such as `ReadRandom`.

Properties:

```csharp
byte PLCNumber
int ConnectTimeOut
int ReceiveTimeOut
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

Connection:

```csharp
OperateResult ConnectServer()
OperateResult ConnectClose()
Task<OperateResult> ConnectServerAsync()
Task<OperateResult> ConnectCloseAsync()
```

Regular read/write methods inherited from `DeviceCommunication` are available,
including `Read`, `ReadBool`, `ReadInt16`, `ReadFloat`, `Write`, and async
variants.

UDP transport can be enabled by replacing the communication pipe:

```csharp
using HslCommunication.Core.Pipe;

MelsecA1ENet plc = new MelsecA1ENet("192.168.1.10", 6000);
plc.CommunicationPipe = new PipeUdpNet("192.168.1.10", 6000);
```

## Address rules

This list matches the HslCommunicationDemo Melsec A1E address examples.

| Address | Type | Notes |
| --- | --- | --- |
| `M0` | Internal relay | Bit/word |
| `X0` | Input relay | PLC notation is octal; see the X/Y rule below |
| `Y0` | Output relay | PLC notation is octal; see the X/Y rule below |
| `S0` | Step relay | Bit/word |
| `F0` | Annunciator | Bit/word |
| `B0` | Link relay | Hexadecimal address |
| `TS0` | Timer contact | Bit |
| `TC0` | Timer coil | Bit |
| `CS0` | Counter contact | Bit |
| `CC0` | Counter coil | Bit |
| `D0` | Data register | Word |
| `W0` | Link register | Hexadecimal word address |
| `R0` | File register | Word |
| `TN0` | Timer current value | Word |
| `CN0` | Counter current value | Word |

For X/Y, `X10` without a leading zero is parsed as hexadecimal `0x10` by this
A1E client. Use `X010` when the PLC address is octal `X10`; do not present the
plain form `X10` to users as the normal PLC input address.

The QnA 3E addresses `SM`, `SD`, `SB`, `SW`, `DX`, `DY`, `L`, `V`, `Z`, and
`ZR` are not part of the A1E example list and should not be assumed here.

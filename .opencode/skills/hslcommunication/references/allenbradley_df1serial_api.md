# Allen-Bradley DF1 Serial clients API subset

`AllenBradleyDF1Serial` implements the Allen-Bradley DF1 serial protocol.
It is half-duplex and is used with MicroLogix 1000, SLC 500, SLC 5/03, and
SLC 5/04 controllers.

Namespace:

```csharp
using HslCommunication.Profinet.AllenBradley;
```

Constructor:

```csharp
new AllenBradleyDF1Serial()
```

## Properties

```csharp
byte Station
byte DstNode
byte SrcNode
CheckType CheckType
string PortName
int BaudRate
```

Common serial properties such as `RtsEnable`, `ReceiveEmptyDataCount`, and
`IsClearCacheBeforeRead` are inherited from `DeviceSerialPort`.

`AllenBradleyDF1Serial` uses `RegularByteTransform` and defaults to CRC16
checking.

## Connection

Use `SerialPortInni(...)` and `Open()`.

```csharp
AllenBradleyDF1Serial plc = new AllenBradleyDF1Serial
{
    Station = 2,
    DstNode = 1,
    SrcNode = 2
};

plc.SerialPortInni("COM3", 19200, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();

if (open.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("N7:0", 2);
}

plc.Close();
```

## Address rules

The base address examples match the SLC address examples. In addition, DF1
addresses can carry station, destination, and source node information.

| Address | Meaning | Notes |
| --- | --- | --- |
| `A9:0` | ASCII | |
| `B9:0` | Bit | |
| `N9:0` | Integer | |
| `F9:0` | Floating point | |
| `S9:0` | Status | |
| `ST1:0` | String | |
| `C9:0` | Counter | |
| `I9:0` | Input | |
| `O9:0` | Output | |
| `R9:0` | Control | |
| `T9:0` | Timer | |
| `L9:0` | Long integer | |
| `s=2;N9:0` | Station-prefixed address | |
| `s=2;dst=1;src=2;N9:0` | Full node address | Carries station, destination, and source |

## Read/write methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult Write(string address, byte[] value)
```

`length` is the number of bytes to read. Inherited typed methods such as
`ReadInt16`, `ReadInt32`, `ReadFloat`, `Write(string, short)`, and
`Write(string, float)` are available through `DeviceCommunication` and use the
raw byte read/write implementation.

## Async methods

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
```

Other inherited async typed methods are also available through
`DeviceCommunication`.

## Notes

- DF1 is serial and half-duplex.
- Verify station, destination node, source node, baud rate, and check type
  against the PLC configuration.

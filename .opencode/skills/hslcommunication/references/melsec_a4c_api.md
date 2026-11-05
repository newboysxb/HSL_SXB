# Melsec A4C clients API subset

`MelsecA4CNet` and `MelsecA4CNetOverTcp` implement the QnA-compatible 4C frame
protocol for Mitsubishi PLCs.

`MelsecA4CNet` is the serial version. `MelsecA4CNetOverTcp` is the TCP
pass-through version. Both share the same address rules.

This protocol requires HslCommunication `13.0.0` or later.

Namespace:

```csharp
using HslCommunication.Profinet.Melsec;
```

## Class and transport table

| Class | Base | Transport |
| --- | --- | --- |
| `MelsecA4CNet` | `MelsecA3CNet` | Serial |
| `MelsecA4CNetOverTcp` | `MelsecA3CNetOverTcp` | TCP pass-through |

## Constructors

```csharp
new MelsecA4CNet()

new MelsecA4CNetOverTcp()
new MelsecA4CNetOverTcp(string ipAddress, int port)
```

`MelsecA4CNet` has only the parameterless serial constructor.

For TCP, prefer `(ipAddress, port)`. The parameterless form inherits
`127.0.0.1:5000`.

## Properties

```csharp
byte Station
bool SumCheck
int Format
bool EnableWriteBitToWordRegister
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Format` supports `1`, `2`, `3`, `4`, and `5`.
`SumCheck` controls the A4C sum check and defaults to `true`.

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.

## Connection

Serial:

```csharp
MelsecA4CNet plc = new MelsecA4CNet
{
    Station = 0,
    SumCheck = true,
    Format = 1
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.Close();
```

TCP:

```csharp
MelsecA4CNetOverTcp plc = new MelsecA4CNetOverTcp("192.168.1.10", 6000)
{
    Station = 0,
    SumCheck = true,
    Format = 1
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist for the TCP client.

The Demo TCP port is `6000`. The parameterless TCP constructor does not set
port `6000`; like other TCP clients, it inherits `127.0.0.1:5000`.

## Address rules

The address examples match the HslCommunicationDemo Melsec MC address
declarations.

| Address | Type | Notes |
| --- | --- | --- |
| `M0` | Internal relay | Bit/word |
| `X0` | Input relay | PLC notation is octal; see the X/Y rule below |
| `Y0` | Output relay | PLC notation is octal; see the X/Y rule below |
| `SM0` | Special relay | Bit/word |
| `S0` | Step relay | Bit/word |
| `L0` | Latch relay | Bit/word |
| `F0` | Annunciator | Bit/word |
| `V0` | Edge relay | Bit/word |
| `B0` | Link relay | Hexadecimal |
| `SB0` | Special link relay | Hexadecimal |
| `DX0` | Direct input | Hexadecimal |
| `DY0` | Direct output | Hexadecimal |
| `TS0` | Timer contact | Bit |
| `TC0` | Timer coil | Bit |
| `SS0` | Cumulative timer contact | Bit |
| `SC0` | Cumulative timer coil | Bit |
| `CS0` | Counter contact | Bit |
| `CC0` | Counter coil | Bit |
| `D0` | Data register | Word |
| `SD0` | Special data register | Word |
| `W0` | Link register | Hexadecimal |
| `SW0` | Special link register | Hexadecimal |
| `R0` | File register | Word |
| `Z0` | Index register | Word |
| `ZR0` | ZR file register | Word |
| `TN0` | Timer current value | Word |
| `SN0` | Cumulative timer current value | Word |
| `CN0` | Counter current value | Word |

For Mitsubishi PLC X/Y addresses, prefer the leading-zero HSL form. For
example, PLC `X10` is octal and must be written as `X010` in this client.
Writing `X10` means hexadecimal `0x10` and addresses a different point.

For register bit access, use `D100.1`.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
```

Inherited typed reads:

```csharp
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
```

## PLC control methods

```csharp
OperateResult RemoteRun()
OperateResult RemoteStop()
OperateResult<string> ReadPlcType()
```

## Async methods

`MelsecA4CNetOverTcp` provides native async core methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] values)
Task<OperateResult> RemoteRunAsync()
Task<OperateResult> RemoteStopAsync()
Task<OperateResult<string>> ReadPlcTypeAsync()
```

`MelsecA4CNet` also exposes inherited async typed read/write methods from
`DeviceCommunication`. These methods wrap the synchronous implementation when
the serial base class does not override the async command-building methods.

Other inherited async read/write methods from `DeviceCommunication` are also
available.

## Notes

- This is the QnA-compatible 4C frame protocol.
- `Format` supports `1`, `2`, `3`, `4`, and `5`.
- The Demo TCP port is `6000`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `McA4CServer`
- client page / form type: `FormMelsec4COverTcp`
- transport: TCP, server port `6012`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6012" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="0" EnableWriteBitToWordRegister="支持使用位写入字寄存器(实际读字，修改位，写字)" Sumcheck="true" MelsecFormat="0" Interval="1000" SleepTime="0" Type="FormMelsec4COverTcp" Name="p_FormMelsec4COverTcp" Guid="0cd022b50de24e789bd8b20e62f1bb53" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6012` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `0` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |
| `EnableWriteBitToWordRegister` | `支持使用位写入字寄存器(实际读字，修改位，写字)` | Allow bit writes into word registers. |
| `Sumcheck` | `true` | Append/verify the checksum byte. |
| `MelsecFormat` | `0` | Frame format enum index for the A-series protocol. **A negative value (page never initialised) breaks the connection.** |

`MelsecFormat` is `0` on the page by default (see the table above). Pass it explicitly only if the PLC
needs a different A4C frame format.

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormMelsec4COverTcp",
    "name": "FormMelsec4COverTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6012",
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

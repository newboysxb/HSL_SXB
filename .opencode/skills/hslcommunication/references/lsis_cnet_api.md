# Lsis Cnet clients API subset

`LSCnet` and `LSCnetOverTcp` implement the LSIS Cnet frame protocol for the
XGB Cnet I/F module. `LSCnet` is the serial version. `LSCnetOverTcp` is the
TCP pass-through version. Both share the same address rules and high-level
read/write API.

Namespace:

```csharp
using HslCommunication.Profinet.LSIS;
```

## Class and transport table

| Class | Base | Transport | Typical port |
| --- | --- | --- | --- |
| `LSCnet` | `DeviceSerialPort` | Serial | Serial only |
| `LSCnetOverTcp` | `DeviceTcpNet` | TCP pass-through | No universal default; Demo uses `2000` |

## Constructors

```csharp
new LSCnet()

new LSCnetOverTcp()
new LSCnetOverTcp(string ipAddress, int port)
```

For TCP, prefer `(ipAddress, port)`. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

```csharp
byte Station
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Station` defaults to `0x05` in the class. The HslCommunicationDemo UI uses
`1`; confirm the station configured on the PLC side.

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.
The protocol word length is 2 bytes.

TCP client common properties include `IpAddress`, `Port`, `ConnectTimeOut`,
and `ReceiveTimeOut`.

## Connection

`LSCnet` is serial only.

```csharp
LSCnet plc = new LSCnet
{
    Station = 1
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<byte> read = plc.ReadByte("MB100");
}

plc.Close();
```

The Demo default serial parameters are `9600`, `8` data bits, no parity, and
`1` stop bit.

`LSCnetOverTcp` uses `ConnectServer()`.

```csharp
LSCnetOverTcp plc = new LSCnetOverTcp("192.168.1.10", 2000)
{
    Station = 1
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("MW100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo LSIS Cnet address declarations.

Every area prefix can be followed by a size suffix:

| Suffix | Meaning |
| --- | --- |
| `X` | Bit |
| `B` | Byte, 1 byte |
| `W` | Word, 2 bytes |
| `D` | Double word, 4 bytes |
| `L` | Long word, 8 bytes |

Supported area prefixes:

| Prefix | Examples |
| --- | --- |
| `P` | `PX100`, `PB100`, `PW100`, `PD100`, `PL100` |
| `M` | `MX100`, `MB100`, `MW100`, `MD100`, `ML100` |
| `L` | `LX100`, `LB100`, `LW100`, `LD100`, `LL100` |
| `K` | `KX100`, `KB100`, `KW100`, `KD100`, `KL100` |
| `F` | `FX100`, `FB100`, `FW100`, `FD100`, `FL100` |
| `T` | `TX100`, `TB100`, `TW100`, `TD100`, `TL100` |
| `C` | `CX100`, `CB100`, `CW100`, `CD100`, `CL100` |
| `D` | `DX100`, `DB100`, `DW100`, `DD100`, `DL100` |
| `S` | `SX100`, `SB100`, `SW100`, `SD100`, `SL100` |
| `Q` | `QX100`, `QB100`, `QW100`, `QD100`, `QL100` |
| `I` | `IX100`, `IB100`, `IW100`, `ID100`, `IL100` |
| `N` | `NX100`, `NB100`, `NW100`, `ND100`, `NL100` |
| `U` | `UX100`, `UB100`, `UW100`, `UD100`, `UL100` |
| `Z` | `ZX100`, `ZB100`, `ZW100`, `ZD100`, `ZL100` |
| `R` | `RX100`, `RB100`, `RW100`, `RD100`, `RL100` |

Additional rules:

| Address | Notes |
| --- | --- |
| `M0` | A bare address with no suffix is treated as `MW0` by default |
| `MW100.0` | Bit access in a word register |
| `s=2;M0` | Carry an explicit station number for one operation |

The `s=<station>;address` prefix overrides the client `Station` for that
individual operation.

For raw `Read(string address, ushort length)`, `length` is the number of bytes.
The protocol supports up to `254` bytes per request; larger reads are split
automatically.

`Read(string[] address)` reads multiple individual addresses and returns their
raw bytes concatenated in input order. The helper splits the address list every
16 addresses automatically. For example, the Demo uses
`MB100;MB200;MW300`, which returns one byte, one byte, and two bytes in that
order.

## Read methods

```csharp
OperateResult<byte> ReadByte(string address)
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<bool> ReadCoil(string address)
OperateResult<bool[]> ReadCoil(string address, ushort length)
```

`ReadCoil` and `ReadBool` are the same operation.

Inherited typed reads from `DeviceCommunication` are also available:

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
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
```

## Write methods

```csharp
OperateResult Write(string address, byte value)
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult WriteCoil(string address, bool value)
```

Inherited typed writes from `DeviceCommunication` are also available:

```csharp
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

The inherited `Write(string, bool[])` and `WriteAsync(string, bool[])` methods
are not implemented for the Cnet protocol and return `NotSupportedFunction`.
Use single bool writes or raw byte writes instead.

## Async methods

Both classes provide native async core methods:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult<byte[]>> ReadAsync(string[] address)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult<bool>> ReadCoilAsync(string address)
Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length)
Task<OperateResult> WriteCoilAsync(string address, bool value)
```

`LSCnetOverTcp` additionally provides:

```csharp
Task<OperateResult<byte>> ReadByteAsync(string address)
Task<OperateResult> WriteAsync(string address, byte value)
```

Inherited typed async read/write methods from `DeviceCommunication` are also
available.

## Notes

- Use `Open()` after `SerialPortInni(...)` for `LSCnet`.
- Use `ConnectServer()` for `LSCnetOverTcp`.
- Confirm the TCP port against the serial-to-Ethernet converter. The Demo
  default is `2000`, but the class has no universal default port.
- `ReadBool(address, length)` reads bool arrays. Writing bool arrays is not
  supported by this protocol implementation.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `LSisServer`
- client page / form type: `FormLsisCnet`
- transport: **serial**
- verified address for read/write: `MB0`

```xml
<Device PipeSetting="SerialPipe" ComPort="COM6" BaudRate="9600" DataBit="8" StopBit="1" Parity="0" Station="1" Interval="1000" SleepTime="0" Type="FormLsisCnet" Name="p_FormLsisCnet" Guid="bde75f0edb2d4bd09d2bebe64d0097d7" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `SerialPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `ComPort` | `COM6` | Serial port name, for example `COM3`. `PortName` is accepted as an alias. |
| `BaudRate` | `9600` | Serial baud rate. |
| `DataBit` | `8` | Serial data bits. |
| `StopBit` | `1` | Serial stop bits. |
| `Parity` | `0` | Serial parity (0 = none). |
| `Station` | `1` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormLsisCnet",
    "name": "FormLsisCnet (AI)",
    "parameters": {
      "PipeSetting": "SerialPipe",
      "ComPort": "COM6",
      "BaudRate": "9600",
      "DataBit": "8",
      "StopBit": "1",
      "Parity": "0",
      "Station": "1"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

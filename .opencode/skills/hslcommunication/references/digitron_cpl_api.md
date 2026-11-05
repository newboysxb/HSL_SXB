# Digitron CPL API subset

`DigitronCPL` and `DigitronCPLOverTcp` implement the Yamatake digital
indicating-controller CPL protocol, currently tested with `SDC40B`.

`DigitronCPL` is the serial version. `DigitronCPLOverTcp` is the TCP
pass-through version. Both share the same address rules.

Namespace:

```csharp
using HslCommunication.Profinet.Yamatake;
```

## Class and transport table

| Class | Base | Transport | Typical port |
| --- | --- | --- | --- |
| `DigitronCPL` | `DeviceSerialPort` | Serial | Serial only |
| `DigitronCPLOverTcp` | `DeviceTcpNet` | TCP pass-through | Constructor default `502`; Demo uses `2000` |

## Constructors

```csharp
new DigitronCPL()

new DigitronCPLOverTcp()
new DigitronCPLOverTcp(string ipAddress, int port = 502, byte station = 0x01)
```

For TCP, prefer `(ipAddress, port, station)`. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Properties

```csharp
byte Station
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`Station` defaults to `1`. `ByteTransform` is `RegularByteTransform` and
defaults to `DataFormat.DCBA`.

TCP client common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

`DigitronCPL` is serial only.

```csharp
DigitronCPL plc = new DigitronCPL
{
    Station = 1
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("506");
}

plc.Close();
```

The Demo default serial parameters are `9600`, `8` data bits, no parity, and
`1` stop bit.

`DigitronCPLOverTcp` uses `ConnectServer()`.

```csharp
DigitronCPLOverTcp plc = new DigitronCPLOverTcp("192.168.1.10", 2000, 1);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("506");
}

plc.ConnectClose();
```

The constructor default port is `502`, but the Demo UI uses `2000`.

## Address rules

The examples match the HslCommunicationDemo DigitronCPL address declarations.

| Address | Meaning | Notes |
| --- | --- | --- |
| `501` | Alarm status 1 | Read-only |
| `504` | Status 1 / run operation | Read/write |
| `506` | PV (PV1) | Read-only |
| `2001` | P-0 proportional band 0 | Read/write |
| `2002` | I-0 integral time 0 | Read/write |
| `2003` | D-0 derivative time 0 | Read/write |
| `s=2;501` | Station-prefixed address | |

The CPL address parser expects numeric addresses.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
```

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
```

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
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

Bool read/write methods are not implemented for this protocol.

## Async methods

`DigitronCPLOverTcp` provides:

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
```

Other inherited async core and typed read/write methods from
`DeviceCommunication` are available. For `DigitronCPL`, inherited async methods
fall back to wrapping the synchronous implementation.

## Notes

- Use `Open()` after `SerialPortInni(...)` for `DigitronCPL`.
- Use `ConnectServer()` for `DigitronCPLOverTcp`.
- Confirm the TCP port against the serial-to-Ethernet converter. The Demo uses
  `2000`; the class constructor default is `502`.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

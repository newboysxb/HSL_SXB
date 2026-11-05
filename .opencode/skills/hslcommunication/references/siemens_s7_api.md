# Siemens S7 clients API subset

`SiemensS7Net` is the standard Siemens S7 TCP client used for S7-200, S7-200
Smart, S7-300, S7-400, S7-1200, and S7-1500 PLCs. The default TCP port is
`102`.

Namespace:

```csharp
using HslCommunication.Profinet.Siemens;
```

If using the advanced `S7AddressData[]` overload directly, also add:

```csharp
using HslCommunication.Core.Address;
```

Constructors:

```csharp
new SiemensS7Net(SiemensPLCS siemens)
new SiemensS7Net(SiemensPLCS siemens, string ipAddress)
```

The constructor always sets `Port = 102`. Pass the IP address when creating the
object, or set `IpAddress` afterwards.

Supported PLC types:

```csharp
SiemensPLCS.S1200
SiemensPLCS.S300
SiemensPLCS.S400
SiemensPLCS.S1500
SiemensPLCS.S200Smart
SiemensPLCS.S200
```

For S7-300/S7-400, check `Rack` and `Slot`. For S7-200/S7-200 Smart, check
`LocalTSAP` and `DestTSAP` when the default connection fails.

## Connection

`SiemensS7Net` is a TCP client and uses `ConnectServer()`.

```csharp
SiemensS7Net plc = new SiemensS7Net(SiemensPLCS.S1200, "192.168.1.10");
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("M100");
}
plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

```csharp
byte Slot
byte Rack
byte ConnectionType
int LocalTSAP
int DestTSAP
int PDULength
```

`SiemensS7Net` uses `ReverseBytesTransform`.

## Address rules

The examples below match the HslCommunicationDemo Siemens S7 address examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `I0` | Input | Bit example: `I1.6` |
| `Q0` | Output | Bit example: `Q1.6` |
| `M0` | Merker/internal memory | Bit example: `M1.6`, word/dword: `MW100`, `MD100` |
| `DB1.0` | Data block | Bit example: `DB1.0.1`, dword example: `DB1.DBD100` |
| `V0` | S7-200 V memory | Equivalent to `DB1.0` |
| `SM0` | System flags | Mainly S7-200 family |
| `P0` | Peripheral memory | Also supports `PIW0`, `PQW0` |
| `T0` | Timer | Tested with S7-200 Smart |
| `C0` | Counter | Tested with S7-200 Smart |
| `AI0` | Analog input | Word access |
| `AQ0` | Analog output | Word access |

Raw `Read(address, length)` uses byte length. Typed reads such as
`ReadInt16(address, length)` use element count.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address, ushort[] length)
OperateResult<byte[]> Read(S7AddressData[] s7Addresses)

OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte> ReadByte(string address)

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

OperateResult<string> ReadString(string address)
OperateResult<string> ReadString(string address, Encoding encoding)
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
OperateResult<string> ReadWString(string address)
```

`ReadString("DB1.0")` reads an S7 string directly. The first two bytes are the
maximum length and current length; HslCommunication handles them internally, so
normally do not pass `length`.

## Multiple data block read example

Use `Read(string[] address, ushort[] length)` to read multiple separate blocks
in one call. The two arrays must have the same length. Returned bytes are
ordered by the input addresses.

Addresses must be byte-oriented. Valid examples are `M100`, `I100`, `DB1.0`,
and `M100.0`. Bit addresses whose bit number is not zero, such as `M100.1`,
`DB1.100.2`, or `Q2.1`, are invalid for this multi-block method and will not
return the expected value.

```csharp
OperateResult<byte[]> read = plc.Read(
    new string[] { "M100", "DB1.0", "Q0" },
    new ushort[] { 2, 10, 1 });

if (read.IsSuccess)
{
    byte[] m100 = new byte[2];
    byte[] db1 = new byte[10];
    Array.Copy(read.Content, 0, m100, 0, m100.Length);
    Array.Copy(read.Content, 2, db1, 0, db1.Length);
    byte q0 = read.Content[12];
}
```

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string[] address, List<byte[]> data)

OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, byte value)

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
OperateResult Write(string address, string value, Encoding encoding)
OperateResult WriteWString(string address, string value)
```

Bulk `bool[]` write is implemented by read-modify-write and has a race risk.
Prefer raw byte writes when the PLC may update adjacent bits concurrently.

## DateTime methods

```csharp
OperateResult<DateTime> ReadDateTime(string address)
OperateResult<DateTime> ReadDTLDataTime(string address)
OperateResult<DateTime> ReadDate(string address)

OperateResult Write(string address, DateTime dateTime)
OperateResult WriteDTLTime(string address, DateTime dateTime)
OperateResult WriteDate(string address, DateTime dateTime)
```

## PLC control and special methods

```csharp
OperateResult<string> ReadOrderNumber()

OperateResult HotStart()
OperateResult ColdStart()
OperateResult Stop()

OperateResult ForceBool(string address, bool value)
OperateResult CancelAllForce()
```

`HotStart`, `ColdStart`, `Stop`, `ForceBool`, and `CancelAllForce` are mainly
for S7-200 Smart.

## Async methods

Async versions exist for the core read/write methods, including `ReadAsync`,
`ReadBoolAsync`, `ReadByteAsync`, `ReadStringAsync`, `WriteAsync`,
`WriteWStringAsync`, `ReadOrderNumberAsync`, `HotStartAsync`, `ColdStartAsync`,
and `StopAsync`.

## PUT/GET troubleshooting

If reads return an error indicating the Siemens PUT/GET access is not enabled,
ask the user to enable PUT/GET in the PLC hardware configuration and download
it again. Refer to the Siemens connection configuration section in the online
help:

[http://www.hsltechnology.cn:7900/Doc/HslCommunication](http://www.hsltechnology.cn:7900/Doc/HslCommunication)

### S7-200 SMART secure communication case

Known model case:

```text
PLC model: 6ES7 288-1ST40-0AA2
S7-200 SMART CPU
```

When `ConnectServer()` fails and the reported message is similar to "remote
host forcibly closed an existing connection" (`远程主机强迫关闭了一个现有的连接`),
ask the user to check the PLC communication/security settings in the Siemens
S7-200 SMART programming software.

This newer S7-200 SMART model may enable secure communication by default.
Open the PLC communication or security setting, uncheck the secure
communication option, download the new configuration to the PLC, and try
`ConnectServer()` again. If the PLC does not reconnect immediately, power-cycle
the PLC after downloading the setting.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `SiemensS7Server`
- client page / form type: `FormSiemensS1200`
- transport: TCP, server port `102`
- verified address for read/write: `M100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="102" ConnectTimeOut="5000" ReceiveTimeOut="10000" Rack="0" Slot="0" ConnectType="" LocalTSAP="" Interval="1000" SleepTime="0" Type="FormSiemensS1200" Name="p_FormSiemensS1200" Guid="10305835383047aa9b9ff8fd5ec9e810" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `102` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Rack` | `0` | -  |
| `Slot` | `0` | Module slot number. |
| `ConnectType` | `` | -  |
| `LocalTSAP` | `` | -  |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormSiemensS1200",
    "name": "FormSiemensS1200 (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "102",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Rack": "0",
      "Slot": "0",
      "ConnectType": "",
      "LocalTSAP": ""
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

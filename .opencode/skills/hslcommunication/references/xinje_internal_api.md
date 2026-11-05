# XinJE internal TCP API subset

`XinJEInternalNet` implements XinJE's internal TCP protocol, which is based on
Modbus TCP but supports a larger address range and more address types.

The default port is `502`.

Namespace:

```csharp
using HslCommunication.Profinet.XINJE;
```

## Constructors

```csharp
new XinJEInternalNet()
new XinJEInternalNet(string ipAddress, int port = 502, byte station = 0x01)
```

Prefer `(ipAddress, port, station)`. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

The constructor default station is `1`. The Demo UI defaults the station text
to `0`; configure the value according to the PLC.

## Properties

```csharp
byte Station
DataFormat DataFormat
bool IsStringReverse
SoftIncrementCount MessageId
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ByteTransform` is `RegularByteTransform` with `DataFormat.CDAB`.
`DataFormat` and `IsStringReverse` are convenience properties over
`ByteTransform`.

TCP common properties include `IpAddress`, `Port`, `ConnectTimeOut`, and
`ReceiveTimeOut`.

## Connection

```csharp
XinJEInternalNet plc = new XinJEInternalNet("192.168.1.10", 502, 1);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("D100");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Address rules

The examples match the HslCommunicationDemo XinJE internal TCP address
declarations.

### Bit addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `M100` | Internal relay | `M0-M69999` |
| `X107` | Input coil | Octal address |
| `Y107` | Output coil | Octal address |
| `S100` | Process relay | `S0-S7999` |
| `HS100` | Process relay | `HS0-HS999` |
| `SM100` | Special relay | `SM0-SM4999` |
| `T100` | Timer | `T0-T4999` |
| `C100` | Counter | `C0-C4999` |
| `ET100` | Precision timer | `ET0-ET39` |
| `SEM100` | SFC instruction | `SEM0-SEM31` |
| `HM100` | Internal relay | `HM0-HM11999` |
| `HT100` | Timer | `HT0-HT1999` |
| `HC100` | Counter | `HC0-HC1999` |
| `HSC100` | High-speed counter | `HSC0-HSC39` |

### Word addresses

| Address | Meaning | Notes |
| --- | --- | --- |
| `D100` | Data register | `D0-D69999` |
| `ID0` | Main/extension input module | `ID0-ID99`, `ID10000` etc. |
| `QD0` | Main/extension output module | `QD0-QD99`, `QD10000` etc. |
| `SD0` | Special register | `SD0-SD4999` |
| `TD0` | Timer current value | `TD0-TD4999` |
| `CD0` | Counter current value | `CD0-CD4999` |
| `ETD0` | Precision timer current value | `ETD0-ETD39` |
| `HD0` | Data register | `HD0-HD24999` |
| `HSD0` | Special register | `HSD0-HSD1023` |
| `HTD0` | Timer current value | `HTD0-HTD1999` |
| `HCD0` | Counter current value | `HCD0-HCD1999` |
| `HSCD0` | High-speed counter current value | `HSCD0-HSCD39` |
| `FD0` | FlashROM register | `FD0-FD8191` |
| `SFD0` | Special FlashROM register | `SFD0-SFD5999` |
| `FSD0` | Special secure register | `FSD0-FSD47` |

The station can be overridden for an individual operation with `s=<station>;`
prefix.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
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

## Async methods

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
Task<OperateResult> WriteAsync(string address, bool value)
Task<OperateResult> WriteAsync(string address, bool[] values)
```

Inherited typed async methods from `DeviceCommunication` are also available.

## Notes

- `XinJEInternalNet` is TCP only.
- The protocol is based on Modbus TCP but uses XinJE internal command codes for
  larger address ranges.
- The low-level command-building and static helper methods are intentionally
  omitted from this reference.

## Creating this client inside HslCommunicationDemo (AI Gateway)

This block was produced by running the Demo's own client page for this protocol
against a matching Demo virtual server, and verifying **both directions**: the value written
by the client is visible on the server, and a value written on the server is read back by the
client. The XML below is what that page really generates, so it is a known-good starting point.

- Demo virtual-server type: `XinJEInternalServer`
- client page / form type: `FormXinJEInternalTcp`
- transport: TCP, server port `6009`
- verified address for read/write: `D100`

```xml
<Device PipeSetting="TcpPipe" IP="127.0.0.1" Port="6009" ConnectTimeOut="5000" ReceiveTimeOut="10000" Station="1" Interval="1000" SleepTime="0" Type="FormXinJEInternalTcp" Name="p_FormXinJEInternalTcp" Guid="5fcda2330b604e7d8dc1c614fe700b14" />
```

Parameters of that page (values as the verified page held them after loading):

| Parameter | Value | Meaning |
| --- | --- | --- |
| `PipeSetting` | `TcpPipe` | Which pipe the page uses: `TcpPipe`, `UdpPipe`, `SerialPipe`, `MqttPipe`, `DTU`... |
| `IP` | `127.0.0.1` | Peer IP address. Also accepts `IpAddress` as an alias. |
| `Port` | `6009` | Peer TCP/UDP port. |
| `ConnectTimeOut` | `5000` | Connect timeout in ms. |
| `ReceiveTimeOut` | `10000` | Receive timeout in ms. |
| `Station` | `1` | Station / slave number. Defaults differ per protocol - keep the value shown unless the PLC needs another. |

Create it with one call, then connect (create never connects by itself):

```json
{
  "operation": "client.create",
  "parameters": {
    "type": "FormXinJEInternalTcp",
    "name": "FormXinJEInternalTcp (AI)",
    "parameters": {
      "IpAddress": "127.0.0.1",
      "Port": "6009",
      "PipeSetting": "TcpPipe",
      "ConnectTimeOut": "5000",
      "ReceiveTimeOut": "10000",
      "Station": "1"
    }
  }
}
```

Then `{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}`, read/write through
`/api/read` and `/api/write` with that `clientId`, and finish with `client.disconnect` then
`client.remove`.

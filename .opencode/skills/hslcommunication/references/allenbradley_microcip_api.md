# Allen-Bradley Micro800 CIP clients API subset

`AllenBradleyMicroCip` is a specialized CIP client for Micro800 series
controllers. It inherits `AllenBradleyNet`, so most API behavior is the same as
the UCMM CIP client, but the Demo configuration is smaller.

The usual port is `44818`.

Namespace:

```csharp
using HslCommunication.Profinet.AllenBradley;
```

Constructors:

```csharp
new AllenBradleyMicroCip()
new AllenBradleyMicroCip(string ipAddress, int port = 44818)
```

Prefer the `(ipAddress, port)` constructor. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Connection

`AllenBradleyMicroCip` is a TCP client and uses `ConnectServer()`.

```csharp
AllenBradleyMicroCip plc = new AllenBradleyMicroCip("192.168.1.10", 44818)
{
    Slot = 0
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<short> read = plc.ReadInt16("A1");
}

plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

`AllenBradleyMicroCip` inherits all `AllenBradleyNet` properties. For Micro800,
the Demo mainly configures:

```csharp
byte Slot
```

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

## Address rules

Use the same CIP tag address examples as `AllenBradleyNet`.

| Address | Meaning | Notes |
| --- | --- | --- |
| `A1` | Global tag | Type must match the PLC tag definition |
| `type=0xDA;A2` | Global tag with explicit type | Use when the tag type is special |
| `x=0x52;A3[0]` | Fragment read | Use for very large data |
| `class=0x6b;0xf68f` | Symbol instance | `class=107;63119` is equivalent |
| `Program:MainProgram.A1` | Local tag | Prefix with the program name |
| `slot=2;A1` | Global tag with slot | Overrides the default slot |
| `i=A[0]` | Bool-array style access | Use when the tag is an integer-backed bit array |
| `B[0]` | Array element | Use an index for array tags |
| `C[0,1]` | Multi-dimensional array element | Use comma-separated indexes |

## Read/write methods

`AllenBradleyMicroCip` inherits the full `AllenBradleyNet` method set. See
`references/allenbradley_cip_api.md` for the complete list. Key methods include:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte[]> Read(string[] address)
OperateResult<byte[]> Read(string[] address, ushort[] length)
OperateResult<bool> ReadBool(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<byte> ReadByte(string address)

OperateResult<short> ReadInt16(string address)
OperateResult<int> ReadInt32(string address)
OperateResult<float> ReadFloat(string address)
OperateResult<double> ReadDouble(string address)

OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, byte value)
OperateResult Write(string address, short value)
OperateResult Write(string address, int value)
OperateResult Write(string address, float value)
OperateResult Write(string address, double value)

OperateResult<ushort, byte[]> ReadTag(string address, ushort length = 1)
OperateResult<AbTagValue[]> ReadTags(string[] address, ushort[] length)
OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
OperateResult<byte[]> ReadEipFromServer(params byte[][] eip)
OperateResult<AbTagItem[]> TagEnumerator()
OperateResult<AbTagItem[]> StructTagEnumerator(AbTagItem structTag)
```

Async versions exist for the core read/write methods and CIP helpers.

## Structure tag behavior

The same structure tag rule as `AllenBradleyNet` applies:

```text
[structure-id-low][structure-id-high][actual structure bytes...]
```

Raw structure reads must preserve the two leading structure ID bytes when
writing the complete structure back.

## Notes

- Use the Micro800 Demo page for the smaller configuration set.
- Verify the actual tag type with `ReadTags` or HslCommunicationDemo before
  writing special typed tags.

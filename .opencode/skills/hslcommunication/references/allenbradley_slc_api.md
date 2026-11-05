# Allen-Bradley SLC clients API subset

`AllenBradleySLCNet` targets Allen-Bradley SLC series controllers and is also
used with the 1747 model.

The usual port is `44818`.

Namespace:

```csharp
using HslCommunication.Profinet.AllenBradley;
```

Constructors:

```csharp
new AllenBradleySLCNet()
new AllenBradleySLCNet(string ipAddress, int port = 44818)
```

Prefer the `(ipAddress, port)` constructor. The parameterless form inherits
`127.0.0.1:5000` from `DeviceTcpNet`.

## Connection

`AllenBradleySLCNet` is a TCP client and uses `ConnectServer()`.

```csharp
AllenBradleySLCNet plc = new AllenBradleySLCNet("192.168.1.10", 44818);
OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("A9:0", 2);
}
plc.ConnectClose();
```

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

Common inherited network properties include `IpAddress`, `Port`,
`ConnectTimeOut`, `ReceiveTimeOut`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

```csharp
uint SessionHandle
```

`AllenBradleySLCNet` uses `RegularByteTransform`.

## Address rules

The examples below match the HslCommunicationDemo SLC address examples.

| Address | Meaning | Notes |
| --- | --- | --- |
| `A9:0` | ASCII | Bit access: `A9:0/1` or `A9:0.1` |
| `B9:0` | Bit | Bit access: `B9:0/1` or `B9:0.1` |
| `N9:0` | Integer | Bit access: `N9:0/1` or `N9:0.1` |
| `F9:0` | Floating point | Bit access: `F9:0/1` or `F9:0.1` |
| `S9:0` | Status | `S:0` is equivalent to `S2:0` |
| `ST1:0` | String | |
| `C9:0` | Counter | Bit access: `C9:0/1` or `C9:0.1` |
| `I9:0` | Input | Bit access: `I9:0/1` or `I9:0.1` |
| `O9:0` | Output | Bit access: `O9:0/1` or `O9:0.1` |
| `R9:0` | Control | Bit access: `R9:0/1` or `R9:0.1` |
| `T9:0` | Timer | Bit access: `T9:0/1` or `T9:0.1` |
| `L9:0` | Long integer | Bit access: `L9:0/1` or `L9:0.1` |

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool> ReadBool(string address)
```

The inherited typed read methods such as `ReadInt16`, `ReadInt32`, `ReadFloat`,
and `ReadDouble` are available through `DeviceCommunication` and use the raw
`Read` implementation.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
```

The inherited typed write methods such as `Write(string, short)`,
`Write(string, int)`, and `Write(string, float)` are available through
`DeviceCommunication`.

## Async methods

```csharp
Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
Task<OperateResult<bool>> ReadBoolAsync(string address)
Task<OperateResult> WriteAsync(string address, byte[] value)
Task<OperateResult> WriteAsync(string address, bool value)
```

## Notes

- The client is tested with 1747 SLC hardware.
- Use the Demo SLC page to verify the actual data-file layout before production.

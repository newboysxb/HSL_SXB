# Custom protocol by inheritance

Custom protocols can be implemented by referencing HslCommunication at compile
time, inheriting a device base class, and overriding the read/write or
command-building methods.

This differs from the plugin approach:

| Approach | Dependency | Loading |
| --- | --- | --- |
| `DevicePluginNet` plugin | No compile-time dependency needed for basic hooks | Loaded by reflection from a DLL |
| Inheritance | Must reference the HslCommunication library | Compiled directly into the project |

## Class hierarchy

The important base classes are:

```text
DeviceCommunication
  -> DeviceTcpNet
  -> DeviceSerialPort
```

Most built-in protocol classes inherit from `DeviceTcpNet` or
`DeviceSerialPort`.

## Project setup

Create a C# class library or application and reference HslCommunication, for
example through NuGet:

```text
HslCommunication
```

Then create a class:

```csharp
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Core.Device;
using HslCommunication.Core.IMessage;
using HslCommunication.Core.Pipe;

public class MyDevice : DeviceTcpNet
{
    public MyDevice(string ipAddress, int port)
        : base(ipAddress, port)
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }
}
```

`DeviceTcpNet(ipAddress, port)` already creates a `PipeTcpNet`. For a serial
device, inherit `DeviceSerialPort` and call `SerialPortInni(...)` plus `Open()`.

## Message parser

Implement `INetMessage`, usually by inheriting `NetMessageBase`. This tells the
library how to know when one complete response has been received.

For a fixed-length header followed by a body:

```csharp
public class MyMessage : NetMessageBase, INetMessage
{
    public int ProtocolHeadBytesLength => 6;

    public int GetContentLengthByHeadBytes()
    {
        if (HeadBytes == null || HeadBytes.Length < 6) return 0;
        return HeadBytes[4] * 256 + HeadBytes[5];
    }
}
```

The exact head length and remaining-length calculation must match the protocol.

For one fixed end character:

```csharp
protected override INetMessage GetNewNetMessage() =>
    new SpecifiedCharacterMessage(0x16);
```

For two end characters:

```csharp
protected override INetMessage GetNewNetMessage() =>
    new SpecifiedCharacterMessage(0x13, 0x10);
```

If the protocol needs a parser, override `GetNewNetMessage`:

```csharp
protected override INetMessage GetNewNetMessage() => new MyMessage();
```

Without a parser, `ReadFromCoreServer` can still be used for simple
request/response messages, but fragmented or variable-length responses should
always use a proper `INetMessage`.

## Method 1: override Read/Write directly

Override the public read/write methods and call `ReadFromCoreServer` manually:

```csharp
public override OperateResult<byte[]> Read(string address, ushort length)
{
    byte[] request = BuildReadRequest(address, length);

    OperateResult<byte[]> read = ReadFromCoreServer(request);
    if (!read.IsSuccess) return read;

    return OperateResult.CreateSuccessResult(ExtractReadData(read.Content));
}

public override OperateResult Write(string address, byte[] value)
{
    byte[] request = BuildWriteRequest(address, value);
    return ReadFromCoreServer(request);
}
```

`BuildReadRequest`, `BuildWriteRequest`, and `ExtractReadData` in this example
are placeholders for the protocol-specific frame builders and parsers.

After `Read` and `Write` are overridden, inherited typed methods such as
`ReadInt16`, `ReadFloat`, and `Write` become available automatically.

## Method 2: override the command-building hooks

Instead of overriding `Read` and `Write`, let the base class drive the process
and override the protected hooks:

```csharp
public class MyDevice : DeviceTcpNet
{
    public MyDevice(string ipAddress, int port)
        : base(ipAddress, port)
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }

    protected override INetMessage GetNewNetMessage() => new MyMessage();

    protected override OperateResult<List<byte[]>> BuildDeviceReadCommand(
        string address,
        ushort length,
        bool isBit)
    {
        // Build one or more request frames.
        byte[] command = BuildReadRequest(address, length);
        return OperateResult.CreateSuccessResult(new List<byte[]> { command });
    }

    protected override OperateResult<byte[]> ParseDeviceReadResult(
        string address,
        byte[] buildCommand,
        byte[] content)
    {
        // Remove protocol header/checksum bytes and return raw device data.
        return OperateResult.CreateSuccessResult(ExtractReadData(content));
    }

    protected override OperateResult<byte[]> BuildDeviceWriteCommand(
        string address,
        byte[] value)
    {
        return OperateResult.CreateSuccessResult(BuildWriteRequest(address, value));
    }

    protected override OperateResult CheckDeviceResponse(
        string address,
        byte[] buildCommand,
        byte[] content)
    {
        // Return failure when the response contains a protocol error.
        return OperateResult.CreateSuccessResult();
    }
}
```

Supported protected hooks:

```csharp
OperateResult<List<byte[]>> BuildDeviceReadCommand(string address, ushort length, bool isBit)
OperateResult<byte[]> BuildDeviceWriteCommand(string address, bool[] value)
OperateResult<byte[]> BuildDeviceWriteCommand(string address, byte[] value)
OperateResult CheckDeviceResponse(string address, byte[] buildCommand, byte[] content)
OperateResult<byte[]> ParseDeviceReadResult(string address, byte[] buildCommand, byte[] content)
OperateResult<bool[]> ParseDeviceReadBoolResult(string address, byte[] buildCommand, byte[] content, ushort length)
```

`ReadBool` support requires `BuildDeviceReadCommand` with `isBit = true` and a
working `ParseDeviceReadBoolResult`. Bool write requires the
`BuildDeviceWriteCommand(string, bool[])` overload.

`WordLength` controls how inherited typed reads calculate byte length:

```csharp
// Set inside the custom class.
WordLength = 1;
```

## Connection hooks

Override `InitializationOnConnect` when the protocol needs handshake packets
after the socket is connected:

```csharp
protected override OperateResult InitializationOnConnect()
{
    OperateResult<byte[]> read = ReadFromCoreServer(handshakeRequest);
    if (!read.IsSuccess) return read;

    return base.InitializationOnConnect();
}
```

If `CommunicationPipe.UseServerActivePush` is enabled, call
`base.InitializationOnConnect()` so the base class can start the background
receive loop.

Override `ExtraOnDisconnect` when the protocol needs a closing packet before
disconnect:

```csharp
protected override OperateResult ExtraOnDisconnect()
{
    return ReadFromCoreServer(logoutRequest);
}
```

## Server active push

When the device can actively push data, enable active push on the pipe and
override `DecideWhetherQAMessage`:

```csharp
public MyDevice(string ipAddress, int port)
    : base(ipAddress, port)
{
    CommunicationPipe.UseServerActivePush = true;
}

protected override bool DecideWhetherQAMessage(
    CommunicationPipe pipe,
    OperateResult<byte[]> receive)
{
    // Return true when this is the response to a normal request.
    // Return false when this is an active push message.
    return !IsActivePushMessage(receive.Content);
}
```

`CommunicationPipe.UseServerActivePush` starts a background receive loop, and
`DecideWhetherQAMessage` decides which messages belong to the question/answer
flow and which are server push messages.

Set `UseServerActivePush` before calling `ConnectServer()` or `Open()`.

## Notes

- The inheritance approach must reference HslCommunication at compile time.
- Override `GetNewNetMessage` whenever responses can be fragmented or contain a
  variable-length body.
- `DeviceTcpNet` uses `ConnectServer()` and `ConnectClose()`.
- `DeviceSerialPort` uses `SerialPortInni(...)` and `Open()`.
- If only raw `ReadFromCoreServer` is used, implement a parser before relying
  on typed methods in production.
- Low-level command-building hooks are part of the extension contract and are
  intentionally listed here.

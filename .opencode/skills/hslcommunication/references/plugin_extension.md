# DevicePluginNet plugin extension

HslCommunication V13.0.0 and later supports plugin-style protocol
extensions. A protocol can be packaged in a separate DLL and loaded by
`DevicePluginNet` without changing the HslCommunication library source.

Namespace:

```csharp
using HslCommunication.Core.Device;
```

## Choose the communication mode

`DevicePluginNet` supports two plugin designs. Choose one before writing the
plugin.

| Mode | Use it when | Responsibilities |
| --- | --- | --- |
| HSL pipe/command mode | The protocol can use `PipeTcpNet`, `PipeUdpNet`, or `PipeSerialPort` | The plugin builds and parses frames; HslCommunication manages the pipe |
| Fully custom mode | The protocol owns a client or session that cannot be driven by the HSL pipe | The plugin manages connect, close, timeout, retry, concurrency, and direct read/write |

For a normal request/response protocol, prefer HSL pipe/command mode. It keeps
the normal connection lifecycle and communication lock. The `MyModbus` plugin
is the reference implementation.

Use fully custom mode only when a third-party client must own the session. The
`OpcUaDevice` plugin is the reference implementation. Direct `Read`,
`ReadBool`, and `Write` hooks bypass the normal HSL read/write lifecycle, so
the plugin owns those responsibilities.

## Plugin DLL structure

The assembly must contain a type named `PluginsHelper` with a public static
method named `DeviceDefinitions`. The helper type itself does not have to be
static or public, but `DeviceDefinitions` must be public.

```csharp
using System;
using System.Collections.Generic;

namespace MyCompany.MyProtocol
{
    public class PluginsHelper
    {
        public static string Company { get; set; } = "My Company";
        public static string Http { get; set; } = "https://www.example.com";
        public static string Version { get; set; } = "1.0.0";
        public static string Description { get; set; } = "My protocol plugin";
        public static string Framework { get; set; } = "net462";
        public static int Language { get; set; }
        public static DateTime ReleaseDate { get; set; } = DateTime.Now;
        public static byte[] Icon16 { get; set; }

        public static List<object> DeviceDefinitions()
        {
            return new List<object>
            {
                new
                {
                    DeviceName = "MyDevice",
                    Description = "My protocol device",
                    DevicePara = typeof(MyDeviceParameter),
                    DeviceObject = typeof(MyDevice)
                }
            };
        }
    }
}
```

`DeviceDefinitions()` may return `List<object>` or `object[]`. The definition
properties read by `DevicePluginNet` are:

```csharp
DeviceName
Description
DevicePara
DeviceObject
```

`DeviceObject` is required. `DevicePara` is optional. Both types must be
instantiable through parameterless constructors.

`CreateFromFile(file, deviceName, parameters)` creates the parameter and device
objects, applies the optional dictionary to public properties of the parameter
object, and then calls `SetParameters` if that method exists.

If the device defines `SetParameters(T parameter)`, use exactly one public
overload and make `T` the type declared as `DevicePara`:

```csharp
public void SetParameters(MyDeviceParameter parameter)
{
    this.parameter = parameter;
}
```

A `void` return is valid. If the method returns `string`, return an empty or
null string for success and a non-empty error message for failure. Dictionary
property lookup is case-insensitive. An unknown property name throws during
plugin creation, and a non-writable property is ignored.

## Mode 1: HSL pipe/command mode

In this mode, `DevicePluginNet` performs the normal connect, reconnect,
timeout, and communication-lock handling. The plugin implements the protocol
message and command hooks.

### Parser and command hooks

```csharp
public int ProtocolHeadBytesLength()

public int GetContentLengthByHeadBytes(byte[] send, byte[] head)

public bool CheckReceiveDataComplete(byte[] send, byte[] content)

public List<byte[]> BuildDeviceReadCommand(
    string address,
    ushort length,
    bool isBit)

public byte[] BuildDeviceWriteCommand(
    string address,
    bool[] value)

public byte[] BuildDeviceWriteCommand(
    string address,
    byte[] value)

public string CheckDeviceResponse(
    string address,
    byte[] buildCommand,
    byte[] content)

public byte[] ParseDeviceReadResult(
    string address,
    byte[] buildCommand,
    byte[] content)

public bool[] ParseDeviceReadBoolResult(
    string address,
    byte[] buildCommand,
    byte[] content,
    ushort length)
```

`CheckDeviceResponse` returns an empty or null string for success and a
non-empty error message for failure. Implement only the write overloads the
protocol supports.

For a fixed header, implement `ProtocolHeadBytesLength` and
`GetContentLengthByHeadBytes`. For a delimiter or variable-length response,
use `ProtocolHeadBytesLength() => -1`,
`GetContentLengthByHeadBytes(...) => 0`, and a correct
`CheckReceiveDataComplete`. If no parser rule is implemented, the default
head length is `-1`, so response completion is accepted immediately. Command
mode therefore needs an explicit framing rule.

### Handshake

To send a handshake or initialization request after the HSL pipe is connected,
implement:

```csharp
public int InitializationOnConnect()
```

Return `0` for success. In the current source, the synchronous connection path
treats a non-zero value as a connection failure, while the asynchronous path
ignores the returned integer. For consistent behavior, return `0` and throw an
exception when initialization fails.

The plugin can expose these writable properties. `DevicePluginNet` injects the
matching helper when the device object is created:

```csharp
public Func<byte[], bool, bool, byte[]> ReadFromCoreServerFunc { get; set; }

public Func<byte[], byte[]> ReadFromCoreServer { get; set; }

#if !NET35 && !NET20
public Func<byte[], bool, bool, Task<byte[]>> ReadFromCoreServerAsyncFunc { get; set; }

public Func<byte[], Task<byte[]>> ReadFromCoreServerAsync { get; set; }
#endif
```

The four-parameter delegate uses the current HSL pipe and lets the caller
choose whether a response is expected and whether pack/unpack is used. The
two-parameter delegate is a raw request/response call.

Do not implement `ConnectServer` or `ConnectClose` in this mode unless the
plugin must replace the HSL connection lifecycle. A custom `ConnectServer`
bypasses the base connection path and therefore bypasses
`InitializationOnConnect`. A custom `ConnectClose` returns without calling the
base pipe close.

## Mode 2: Fully custom communication

In this mode, the plugin owns the connection and session. Implement
`ConnectServer` and `ConnectClose`, then implement the raw direct hooks or the
typed hooks required by the protocol.

### Connection hooks

```csharp
public string ConnectServer()

public string ConnectClose()

#if !NET35 && !NET20
public Task<string> ConnectServerAsync()

public Task<string> ConnectCloseAsync()
#endif
```

An empty or null string means success. A non-empty string becomes the failure
message.

Synchronous and asynchronous connection hooks are bound independently. If the
application uses both `ConnectServer()` and `ConnectServerAsync()`, or both
close methods, implement each hook that should use the plugin-owned session.

### Raw direct hooks

```csharp
public byte[] Read(string address, ushort length)

public bool[] ReadBool(string address, ushort length)

public string Write(string address, byte[] value)

public string Write(string address, bool[] value)

#if !NET35 && !NET20
public Task<byte[]> ReadAsync(string address, ushort length)

public Task<bool[]> ReadBoolAsync(string address, ushort length)

public Task<string> WriteAsync(string address, byte[] value)

public Task<string> WriteAsync(string address, bool[] value)
#endif
```

An empty or null write result means success. A read exception is converted by
`DevicePluginNet` into a failed `OperateResult`.

Synchronous and asynchronous read/write hooks are also bound independently.
Implement both families when both APIs will be used; providing only the
synchronous hook does not make the asynchronous API use that plugin method.

### Complete typed direct hooks

The following sync hooks are bound exactly when their public method name and
parameter types match. The corresponding async hook uses the same method name
with an `Async` suffix and a `Task<T>` return type. For example,
`ReadInt16(string)` corresponds to `ReadInt16Async(string)`.

Typed hooks return their final .NET value, for example a `short` or `short[]`,
not raw device bytes. If only the raw `Read(string, ushort)` hook is
implemented, the inherited typed read methods use the HslCommunication byte
transform to convert the returned bytes.

```csharp
bool ReadBool(string address)
byte ReadByte(string address)
short ReadInt16(string address)
short[] ReadInt16(string address, ushort length)
ushort ReadUInt16(string address)
ushort[] ReadUInt16(string address, ushort length)
int ReadInt32(string address)
int[] ReadInt32(string address, ushort length)
uint ReadUInt32(string address)
uint[] ReadUInt32(string address, ushort length)
long ReadInt64(string address)
long[] ReadInt64(string address, ushort length)
ulong ReadUInt64(string address)
ulong[] ReadUInt64(string address, ushort length)
float ReadFloat(string address)
float[] ReadFloat(string address, ushort length)
double ReadDouble(string address)
double[] ReadDouble(string address, ushort length)
string ReadString(string address, ushort length, Encoding encoding)

string Write(string address, bool value)
string Write(string address, byte value)
string Write(string address, short value)
string Write(string address, short[] value)
string Write(string address, ushort value)
string Write(string address, ushort[] value)
string Write(string address, int value)
string Write(string address, int[] value)
string Write(string address, uint value)
string Write(string address, uint[] value)
string Write(string address, long value)
string Write(string address, long[] value)
string Write(string address, ulong value)
string Write(string address, ulong[] value)
string Write(string address, float value)
string Write(string address, float[] value)
string Write(string address, double value)
string Write(string address, double[] value)
string Write(string address, string value, Encoding encoding)
```

The current source does not bind `sbyte` hooks. `ReadString` uses the `ushort`
length and `Encoding` signature.

When the Demo is used with a fully custom plugin, add this property to the
parameter object so the Demo hides the HSL pipe controls:

```csharp
public bool DisablePipeSetting { get; private set; } = true;
```

This property is a Demo integration convention. `DevicePluginNet` does not use
it to provide connection, locking, or retry behavior. The plugin must handle
those concerns itself, including concurrent calls.

`DevicePluginNet.Dispose()` disposes the HSL pipe, but does not automatically
dispose the plugin object. Release plugin-owned clients and sessions from
`ConnectClose` or another explicit cleanup path.

## Common plugin properties

`DevicePluginNet` reads these properties from the device object when they are
present:

```csharp
public string DataFormat { get; set; } = "CDAB";

public ushort WordLength { get; set; } = 1;
```

`DataFormat` supports `ABCD`, `BADC`, `CDAB`, and `DCBA`.
`WordLength` follows the HslCommunication convention: `1` means an address
normally occupies two bytes, and `2` means an address occupies one byte.

The Demo recognizes these optional members:

```csharp
public List<object> GetDeviceAddressExamples()

public string DefaultCommunicationPipe()

public bool DisablePipeSetting { get; }
```

`DefaultCommunicationPipe` supports values such as:

```text
TCP:127.0.0.1:502
UDP:127.0.0.1:502
COM3-9600-8-N-1
```

`GetDeviceAddressExamples()` returns anonymous objects with these fields:

```csharp
AddressExample
AddressType
BitEnable
WordEnable
Mark
```

The first example is used by the Demo as its default address.

## Load the plugin

```csharp
using System;
using System.Collections.Generic;
using HslCommunication;
using HslCommunication.Core.Device;
using HslCommunication.Core.Pipe;

Dictionary<string, object> parameters =
    new Dictionary<string, object>();
parameters["Station"] = (byte)1;

DevicePluginNet plc = DevicePluginNet.CreateFromFile(
    @"E:\plugins\MyProtocol\MyProtocol.dll",
    "MyDevice",
    parameters);

if (plc == null)
    throw new Exception("The plugin device definition was not found.");

// HSL pipe/command mode: configure the pipe used by the plugin.
plc.CommunicationPipe = new PipeTcpNet("127.0.0.1", 502)
{
    ConnectTimeOut = 5000,
    ReceiveTimeOut = 10000
};

OperateResult connect = plc.ConnectServer();
if (!connect.IsSuccess)
    throw new Exception(connect.Message);

OperateResult<short> read = plc.ReadInt16("100");
if (read.IsSuccess)
    Console.WriteLine(read.Content);

plc.ConnectClose();
```

When `deviceName` is null or empty, `CreateFromFile` uses the first definition.
For a fully custom plugin, the parameter object should expose
`DisablePipeSetting`, and the plugin's `ConnectServer` and `ConnectClose`
methods perform the actual session lifecycle.

The plugin DLL is loaded with `Assembly.LoadFrom`. Keep dependency DLLs beside
the plugin DLL or make sure the host application can resolve them.

## Validation checklist

- `PluginsHelper.DeviceDefinitions()` is public and returns `List<object>` or
  `object[]`.
- The definition uses `DeviceObject` and the exact device and parameter types.
- `SetParameters` has one unambiguous public overload when provided.
- HSL pipe/command mode implements a framing rule and exact hook signatures.
- Fully custom mode owns connect, close, timeout, retry, concurrency, and
  resource cleanup, and sets `DisablePipeSetting = true` for the Demo.
- `CreateFromFile` may return null, so check it before accessing the device.

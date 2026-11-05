# Siemens WebApi clients API subset

`SiemensWebApi` reads and writes Siemens PLC tags over the PLC Web API. It is
mainly used with S7-1500 PLCs when the "Enable web server on this module"
option is enabled in the PLC configuration.

The default HTTPS port is `443`. For HTTP access, use port `80` and set
`UseHttps = false`.

Namespace:

```csharp
using HslCommunication.Profinet.Siemens;
```

If using multi-tag JSON reads, also add:

```csharp
using Newtonsoft.Json.Linq;
```

Constructors:

```csharp
new SiemensWebApi()
new SiemensWebApi(string ipAddress, int port = 443)
```

The parameterless constructor uses `127.0.0.1:443` and enables HTTPS by default.

## Connection

`SiemensWebApi` must log in before reading or writing.

```csharp
SiemensWebApi plc = new SiemensWebApi("192.168.1.10", 443)
{
    UserName = "admin",
    Password = "password"
};

OperateResult connect = plc.ConnectServer();
if (connect.IsSuccess)
{
    OperateResult<string> mode = plc.ReadOperatingMode();
}

plc.ConnectClose();
```

`ConnectClose()` calls `Logout()` so the current token becomes invalid.

`ConnectServerAsync()` and `ConnectCloseAsync()` also exist.

## Properties

Common inherited network properties include `IpAddress`, `Port`, `UserName`,
`Password`, `UseHttps`, `DefaultContentType`, `ByteTransform`, `LogNet`, and
`AutoReConnect`.

```csharp
string Token
```

`Token` is obtained automatically by `ConnectServer()`. A valid token can also
be assigned manually when reconnect-free operation is required.

`SiemensWebApi` uses `ReverseBytesTransform`.

## Address rules

Addresses are PLC tag names and do not have a fixed area format. Tag values must
be visible to the PLC Web API.

Examples from HslCommunicationDemo:

```text
"全局DB".Static_14
"全局DB".Static_21
```

Multiple tags are separated by a semicolon:

```text
"全局DB".Static_14;"全局DB".Static_21
```

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<byte> ReadByte(string address)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length)
OperateResult<JToken[]> Read(string[] address)
```

For raw `Read(address, length)`, the `length` parameter is ignored; the returned
byte array must be parsed according to the tag type. For `ReadBool`, `length`
selects the number of bool values. For `ReadString`, the `length` parameter is
ignored and the tag is read as a JSON value.

## Multi-tag read example

```csharp
OperateResult<JToken[]> read = plc.Read(new string[]
{
    "\"全局DB\".Static_14",
    "\"全局DB\".Static_21"
});

if (read.IsSuccess)
{
    foreach (JToken token in read.Content)
    {
        Console.WriteLine(token.ToString());
    }
}
```

If any tag fails, the whole multi-tag read returns failure.

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, byte value)
OperateResult Write(string address, bool[] value)
OperateResult Write(string address, string value)
OperateResult Write(string address, DateTime dateTime)
```

String writes support Chinese correctly when the PLC tag is a `WString`.

## DateTime methods

```csharp
OperateResult<DateTime> ReadDateTime(string address)
```

## Special methods

```csharp
OperateResult<string> ReadOperatingMode()
OperateResult<double> ReadVersion()
OperateResult ReadPing()
OperateResult Logout()
```

`ReadOperatingMode()` can return values such as `STOP`, `STARTUP`, `RUN`,
`HOLD`, or `-`.

## Async methods

Async versions exist for the same operations, including `ConnectServerAsync`,
`ConnectCloseAsync`, `ReadAsync`, `ReadByteAsync`, `ReadBoolAsync`,
`ReadStringAsync`, multi-tag `ReadAsync(string[])`, `WriteAsync`,
`ReadOperatingModeAsync`, `ReadDateTimeAsync`, `ReadVersionAsync`,
`ReadPingAsync`, and `LogoutAsync`.

## Authorization

Multi-tag `Read(string[])`, `ReadVersion()`, and `ReadPing()` require commercial
authorization. Free builds return `InsufficientPrivileges` for those paths.

## Web server configuration

The PLC Web server must be enabled. If the user is not sure how to configure
it, refer to the Siemens connection configuration section in the online help:

[http://www.hsltechnology.cn:7900/Doc/HslCommunication](http://www.hsltechnology.cn:7900/Doc/HslCommunication)

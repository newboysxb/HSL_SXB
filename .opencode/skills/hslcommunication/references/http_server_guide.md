# HTTP server guide

HslCommunication provides `HttpServer` for simple Web API hosting without an
ASP.NET host. It can serve static data, JSON, files, file uploads, and methods
registered as HTTP RPC endpoints.

## Choose the implementation style

Use the following rule before writing the API:

- Use `HandleRequestFunc` when the API surface is small, when requests need
  custom low-level routing, or when the handler must handle files, images,
  uploads, or protocol-specific response behavior directly.
- When there are more than three API operations and they are naturally
  RPC-like, with named operations, parameters, and return values, prefer public
  methods marked with `[HslMqttApi]` and registered with
  `RegisterHttpRpcApi`.
- Keep related operations in one class or one service object, and use
  `ApiTopic` to group them into a readable path such as `GroupA/Calculate`.
- Use static methods for stateless utility APIs and instance methods when the
  service needs object state, such as a connection, cache, or configuration.

This keeps the HTTP entry point small and lets HslCommunication handle method
discovery, parameter conversion, and JSON results.

Important classes:

| Class | Purpose |
| --- | --- |
| `HttpServer` | HTTP server |
| `HttpUploadFile` | Uploaded file information |
| `NetworkWebApiBase` | C# HTTP client used to call the server |
| `MqttCredential` | Account used by login access control |

Namespaces:

```csharp
using HslCommunication.Enthernet;
using HslCommunication.Core.Net;
using HslCommunication.Reflection;
```

## Step 1: start a basic HttpServer

```csharp
HttpServer server = new HttpServer();
server.HandleRequestFunc = HandleRequest;
server.IsCrossDomain = true;
server.Start(5000);

Console.ReadLine();
```

Handler:

```csharp
private static object HandleRequest(
    HttpListenerRequest request,
    HttpListenerResponse response,
    string data)
{
    if (request.RawUrl == "/GetA")
    {
        response.AddHeader("Content-type", "application/json; charset=utf-8");
        return new { A = 123, B = 1.23f, C = "asf" }.ToJsonString();
    }

    return null;
}
```

`HandleRequestFunc` returns `object`, so text, JSON strings, or binary arrays can
be returned directly. Returning an `object` handler requires HslCommunication
V12.0.3 or later.

## Step 2: call the API from C#

```csharp
NetworkWebApiBase webapi = new NetworkWebApiBase("127.0.0.1", 5000);

OperateResult<string> read = webapi.Get("/GetA");
if (read.IsSuccess)
{
    Console.WriteLine(read.Content);
}
```

With login access control:

```csharp
NetworkWebApiBase webapi =
    new NetworkWebApiBase("127.0.0.1", 5000, "admin", "123456");
```

## Step 3: return images and files

```csharp
private static object HandleRequest(
    HttpListenerRequest request,
    HttpListenerResponse response,
    string data)
{
    if (request.RawUrl.StartsWith("/images/"))
    {
        string fileName = "D:\\" + request.RawUrl.Substring(8);
        byte[] content = File.ReadAllBytes(fileName);

        response.ContentType = "image/png";
        return content;
    }

    return null;
}
```

Do not map user input directly to files in production. Always restrict the path
and reject path traversal.

## Step 4: handle file upload

The server supports one file per upload request.

```csharp
HttpServer server = new HttpServer();
server.HandleFileUpload = HandleFileUpload;
server.Start(5000);
```

```csharp
private static string HandleFileUpload(
    HttpListenerRequest request,
    HttpListenerResponse response,
    HttpUploadFile file)
{
    File.WriteAllBytes(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.FileName),
        file.Content);

    return "upload success";
}
```

`HttpUploadFile`:

```csharp
string FileName
string Name
byte[] Content
```

Use the raw URL when different upload paths need different behavior:

```csharp
if (request.RawUrl.StartsWith("/Upload/File/"))
{
    // save file
}
```

## Step 5: enable login access control

```csharp
HttpServer server = new HttpServer();
server.HandleRequestFunc = HandleRequest;
server.IsCrossDomain = true;

server.SetLoginAccessControl(new MqttCredential[]
{
    new MqttCredential("admin", "123456")
});

server.Start(5000);
```

Client:

```csharp
NetworkWebApiBase webapi =
    new NetworkWebApiBase("127.0.0.1", 5000, "admin", "123456");

OperateResult<string> read = webapi.Get("/GetA");
```

## Step 6: register class methods as HTTP APIs

This is the preferred style when the API has more than three RPC-like
operations. It is also valid for smaller APIs when the method-and-attribute
model is clearer than a central request switch.

```csharp
HttpServer server = new HttpServer();
server.RegisterHttpRpcApi(typeof(Program));
server.IsCrossDomain = true;
server.Start(5000);
```

```csharp
[HslMqttApi(HttpMethod = "GET")]
public static int Calculate(int a, int b)
{
    return a * 10 + b * 2;
}

[HslMqttApi(HttpMethod = "POST")]
public static int Add(int a, int b)
{
    return a + b;
}
```

Default API path:

```text
/Program/Calculate?a=1&b=2
/Program/Add
```

Remove the class name from the path:

```csharp
server.RegisterHttpRpcApi("", typeof(Program));
```

Then:

```text
/Calculate?a=1&b=2
```

## Step 7: customize API paths and descriptions

```csharp
[HslMqttApi(
    ApiTopic = "GroupA/Calculate",
    Description = "Calculate value",
    HttpMethod = "GET")]
public static int Calculate(int a, int b)
{
    return a * 10 + b * 2;
}
```

Multi-level paths such as `A/B/C/D/Calculate` are supported.

## Step 8: register instance objects and nested APIs

```csharp
HttpServer server = new HttpServer();
Account account = new Account();

server.RegisterHttpRpcApi(account);
server.Start(8000);
```

```csharp
public class Account
{
    [HslMqttApi(ApiTopic = "Account/Login", HttpMethod = "POST")]
    public int Login(string name, string password)
    {
        return 1;
    }
}
```

For nested properties:

```csharp
[HslMqttApi(PropertyUnfold = true)]
public static Account Account { get; set; } = new Account();
```

## Step 9: pass request data into a registered method

```csharp
[HslMqttApi(HttpMethod = "POST")]
public string RequestHeaderTest(HttpListenerRequest request, int a)
{
    return request.RawUrl + " -> " + a;
}
```

The framework injects supported framework types such as
`HttpListenerRequest` automatically.

## Step 10: unregister APIs

```csharp
server.UnRegisterHttpRpcApi(typeof(Program));
server.UnRegisterHttpRpcApi(account);

// unregister one API
server.UnRegisterHttpRpcApiSingle("Account/Login");
```

## Step 11: register a PLC object as Web API

HslCommunication device objects expose methods marked with `[HslMqttApi]`, so
they can be registered directly:

```csharp
MelsecMcNet plc = new MelsecMcNet("127.0.0.1", 6000);

HttpServer server = new HttpServer();
server.RegisterHttpRpcApi("plc1", plc);
server.IsCrossDomain = true;
server.Start(5000);
```

The PLC API path will look like:

```text
/plc1/ReadInt32?address=D100
```

Before registering a device in production, connect it first and confirm the
device can communicate.

## Step 12: protect the PLC from HTTP concurrency

HTTP endpoints can be called concurrently. A PLC communication object usually
has one underlying connection and a lock, so burst calls can pile up and cause
timeouts.

Wrap the PLC and reject calls while the connection is in an error state:

```csharp
public class MyPlc
{
    private readonly MelsecMcNet plc;
    private int callCount;

    public MyPlc(MelsecMcNet plc)
    {
        this.plc = plc;
    }

    [HslMqttApi(HttpMethod = "POST")]
    public OperateResult<short> ReadInt16(string address = "D100")
    {
        if (plc.CommunicationPipe.IsConnectError() && callCount > 0)
        {
            return new OperateResult<short>("PLC is busy, try later");
        }

        Interlocked.Increment(ref callCount);
        OperateResult<short> read = plc.ReadInt16(address);
        Interlocked.Decrement(ref callCount);

        return read;
    }
}
```

Register the wrapper:

```csharp
server.RegisterHttpRpcApi("plc1", new MyPlc(plc));
```

For frequently requested values, read the PLC once in a background thread and
return the cached result from the HTTP API:

```csharp
public class CachedPlc
{
    private volatile OperateResult<short> value =
        new OperateResult<short>("not ready");

    public CachedPlc(MelsecMcNet plc)
    {
        Thread thread = new Thread(() =>
        {
            while (true)
            {
                HslHelper.ThreadSleep(1000);
                value = plc.ReadInt16("D100");
            }
        });

        thread.IsBackground = true;
        thread.Start();
    }

    [HslMqttApi(HttpMethod = "POST")]
    public OperateResult<short> ReadD100()
    {
        return value;
    }
}
```

Caching avoids repeated PLC reads, but it also removes dynamic address
selection from the API. Choose the approach according to the actual use case.

## Notes

- `HttpServer.Start(port)` starts a listener on that port.
- `HttpServer` supports custom handlers and automatically registered RPC APIs.
- For more than three RPC-like operations, prefer `[HslMqttApi]` methods and
  `RegisterHttpRpcApi` over extending `HandleRequestFunc`.
- Registered methods must be public and marked with `[HslMqttApi]`.
- Set `HttpMethod = "GET"` or `HttpMethod = "POST"` on the API.
- `SetLoginAccessControl` enables HTTP basic login checking.
- `NetworkWebApiBase` is a convenient C# client for calling these endpoints.
- Returned binary data such as images should be returned as `byte[]`.
- File upload currently supports one file per request.

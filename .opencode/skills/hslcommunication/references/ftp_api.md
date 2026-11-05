# FTP client and server API subset

`FtpClient` and `FtpServer` implement FTP file exchange between two programs or
two computers. `FtpClient` connects to an FTP server, and `FtpServer` hosts
files on a local directory.

Namespace:

```csharp
using HslCommunication.Enthernet.Ftp;
```

## FtpClient

`FtpClient` inherits `TcpNetCommunication`. Only the parameterless constructor
is provided; assign `IpAddress` and `Port` after construction.

```csharp
FtpClient ftpClient = new FtpClient
{
    IpAddress = "192.168.1.10",
    Port = 12345,
    Username = "admin",
    Password = "123456"
};
```

Properties:

```csharp
string IpAddress
int Port
int ConnectTimeOut
int ReceiveTimeOut
string Username
string Password
string SystemType
ILogNet LogNet
```

`Username` defaults to `anonymous`. When `Password` is empty, the client sends
`hslcommunication@example.com` internally for anonymous login. `SystemType` is
read-only and is filled after a successful connection.

Connection methods:

```csharp
OperateResult ConnectServer()
Task<OperateResult> ConnectServerAsync()
OperateResult ConnectClose()
Task<OperateResult> ConnectCloseAsync()
```

`ConnectServer()` performs the FTP login handshake and then closes the control
connection. File methods such as `ListFiles`, `DownloadFile`, and `UploadFile`
open their own control and data connections internally.

File methods:

```csharp
OperateResult<FtpFileItem[]> ListFiles(string parent = "/")
OperateResult DownloadFile(string remoteFile, string localFile, Action<long, long> processReport = null, HslCancelToken cancelToken = null)
OperateResult UploadFile(string localFile, string remoteFile, Action<long, long> processReport = null, HslCancelToken cancelToken = null)
OperateResult DeleteFile(string remoteFile)
OperateResult DeleteDirectory(string path, bool clearFiles = false)
OperateResult CreateDirectory(string path)
OperateResult RenameFolder(string oldPath, string newPath)
OperateResult RenameFile(string oldName, string newName)
```

Remote paths use `/` separators and normally start with `/`, for example
`/A/123.txt`. `DeleteDirectory(path)` deletes only an empty directory;
`DeleteDirectory(path, clearFiles: true)` first deletes all files and
subdirectories inside it.

`RenameFolder` does not support moving a directory to a different parent
folder. `RenameFile(oldName, newName)` uses the same implementation.

`processReport` receives already transferred bytes and total bytes. Set
`cancelToken.IsCancelled = true` to cancel an upload or download.

`FtpFileItem` returned by `ListFiles`:

```csharp
string Name
long Size
DateTime CreateTime
bool IsDirectory
```

## FtpServer

`FtpServer` inherits `CommunicationTcpServer`.

```csharp
FtpServer ftpServer = new FtpServer
{
    CurrentDirectory = @"D:\FtpFiles",
    AllowAnonymous = true,
    DownloadOnly = false
};

ftpServer.SetPasvPortRange("7000-8000");
ftpServer.ServerStart(12345);
```

Properties:

```csharp
string CurrentDirectory
FtpSession[] Sessions
bool AllowAnonymous
bool DownloadOnly
Func<string, string, FtpSession, OperateResult> CheckAccountFunction
int Port
ILogNet LogNet
bool IsStarted
```

`CurrentDirectory` is the physical root directory. If it does not exist, the
server creates it automatically. `AllowAnonymous` defaults to `true`.
`DownloadOnly` defaults to `false`; when `true`, upload, delete, rename, and
create-directory operations are rejected for all accounts.

Methods:

```csharp
void SetPasvPortRange(string range)
void ServerStart(int port)
void ServerStart()
void ServerClose()
```

`SetPasvPortRange` accepts a single port or a range such as `"7000-8000"`.
The built-in default range is `50000-60000`. When the server is behind a
firewall or NAT, configure the range and open the ports in the firewall.

Account validation:

```csharp
ftpServer.CheckAccountFunction = (name, pwd, session) =>
{
    if (name == "admin" && pwd == "123456")
    {
        return OperateResult.CreateSuccessResult();
    }

    return new OperateResult(500, "user account or password wrong");
};
```

The delegate can change `session.CanUpload` and `session.CanEdit` to control
per-account permissions.

`FtpSession`:

```csharp
string UserName
string CurrentDirectory
string ClientId
bool CanUpload
bool CanEdit
object Tag
```

`Sessions` returns the currently connected FTP sessions and can be refreshed by
a timer.

## Notes

- Use `ServerStart(port)` on the server and then connect with `FtpClient`.
- File transfer methods are synchronous. Run them on a background thread when
  used from a WinForms/WPF UI.
- FTP uses a control connection plus a temporary PASV data connection. Confirm
  the PASV port range and firewall rules when uploads/downloads fail.
- See `examples/ftp_client_server_console.cs` for a complete server/client
  console example.

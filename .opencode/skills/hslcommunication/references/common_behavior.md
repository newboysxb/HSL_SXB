# Common behavior for HslCommunication clients

All supported HslCommunication PLC clients listed in `SKILL.md` derive from
`BinaryCommunication` and `DeviceCommunication`. They share the same
`OperateResult` handling, typed read/write overloads, logging, reconnection,
raw byte reads, and attribute-based read/write behavior.

Protocol-specific differences are documented in the individual reference files.

## Configurable connection values

When the user asks for configurable or complete-project code, do not require
the real IP address, port, station, password, or AMS NetId before generating
the source. Treat these values as runtime configuration:

- Generate a configuration model or options object with the required fields.
- Generate the loading and validation code for the user's configuration source,
  such as `appsettings.json`, XML, a database, or an existing settings object.
- Generate a protocol factory or resolver when more than one protocol may be
  selected.
- Use placeholders or configuration bindings instead of machine-specific
  literals in project code.

For a one-off sample or interactive connection test, concrete values are
appropriate and may be written directly into the example.

When a configured connection fails, whether the Skill should ask the user to
confirm the values is context-dependent. It is not mandatory. If the user is
interactively testing a specific device, asking them to check and update the
values is reasonable. If the code belongs to a production service or a
multi-device project, the generated program should log or return the
`OperateResult`, update the connection status, and follow the configured
retry/backoff policy. Only ask the current user again when they explicitly want
to diagnose the failed connection or update the configuration source.

## OperateResult

Every read or write returns `OperateResult` or `OperateResult<T>`.

Always check `IsSuccess` before accessing `.Content`:

```csharp
OperateResult<short> read = plc.ReadInt16("D100");
if (read.IsSuccess)
{
    Console.WriteLine(read.Content);
}
else
{
    Console.WriteLine(read.Message);
}
```

`OperateResult` and `OperateResult<T>` also expose a mutable `object Tag`
property for application-defined metadata. It is useful for correlating a
result with a point name, request ID, or task identifier, and wrapper APIs can
carry it through their results:

```csharp
OperateResult<short> read = plc.ReadInt16("D100");
read.Tag = "temperature";
```

## Address table parsing

When the user provides `.xlsx` or `.csv`, use a real spreadsheet parser.
Normalize headers to lowercase and trim whitespace.

Required columns:

- `name`
- `address`
- `dataType`
- `length`

Optional columns:

- `description`
- `scale`
- `unit`
- `functionCode`
- `defaultValue`

`length` is the number of elements for typed reads. For `string`, `length` is
the number of protocol words or registers to read; follow the selected protocol
reference.

`defaultValue` is optional application metadata used to initialize or reset a
point. It is not a protocol field; convert it to the selected point's actual
data type before writing.

## Non-decimal address components

Do not assume every PLC address number is decimal. The parser behavior is
protocol-specific:

| Protocol family | Component | Base and examples |
| --- | --- | --- |
| Mitsubishi MC/A1E/A4C/A3C | X/Y | Hexadecimal by default; use a leading `0` for PLC octal notation. PLC `X10` -> `X010`; HSL `X10` means hex `0x10`. |
| Mitsubishi MC/A1E/A4C/A3C | `B`, `SB`, `DX`, `DY`, `W`, `SW` | Hexadecimal |
| Mitsubishi FXSerial/FxLinks | X/Y | Octal. `X10` means octal 10 (decimal 8). |
| Delta DVP/legacy series | X/Y | Octal |
| Delta AS series | X/Y word number | Decimal; dotted bit suffix is hexadecimal `0`-`F` |
| Xinje, WeCon, MegMeet, Vigor | X/Y | Octal |
| Inovance H3U/H5U/Easy, EasyNet, ComputerLink | X/Y | Octal |
| Inovance EasyNet | `U`, `UB`, `UW` | Hexadecimal digits |
| Keyence MC | `X`, `Y`, `B`, `ZR`, `W` | Hexadecimal |
| Keyence Nano | `B`, `W`, `VB` | Hexadecimal |
| ToyoPuc | Device number portion | Hexadecimal |
| Panasonic Mewtocol/MC | Compact bit digit and dotted bit suffix | Hexadecimal `0`-`F`; `X3.A` means bit 10 |
| General `word.bit` syntax | Bit suffix | Accepts `0`-`F`; uppercase `A`-`F` are hexadecimal. `D100.F` is bit 15. Some parsers only detect uppercase letters before choosing hexadecimal parsing, so prefer uppercase. This applies to parsers such as LS CNet/FastEnet, CIMON, Memobus, Beckhoff ADS, Fuji SPB/SPH, Omron FINS, Keyence Nano, and other clients that use `HslHelper.CalculateBitStartIndex`. |
| Omron HostLink C-Mode and Omron FINS | `E`/`EM` block number | Hexadecimal; `E10.100` means EM block `0x10`, word `100` |
| Modbus-derived clients | Numeric address with `H` suffix | Hexadecimal marker added by HslCommunication; `100H` means decimal `256` |

Always load the selected protocol reference before generating an address whose
numbering base may differ from decimal.

## Data type mapping

Use these methods unless the selected protocol reference says otherwise:

| Data type | Method |
| --- | --- |
| `bool` | `ReadBool(address)` or `ReadBool(address, length)` |
| `short` | `ReadInt16` |
| `ushort` | `ReadUInt16` |
| `int` | `ReadInt32` |
| `uint` | `ReadUInt32` |
| `float` | `ReadFloat` |
| `double` | `ReadDouble` |
| `long` | `ReadInt64` |
| `ulong` | `ReadUInt64` |
| `string` | `ReadString(address, length)` |
| `byte[]` | `Read(address, length)` |

Modbus coils may use `ReadCoil`; Modbus discrete inputs may use
`ReadDiscrete`. See the Modbus references for the exact method names.

## PLC type names vs C# types

Many PLC type names differ from C# type names.

Common mappings:

| PLC type | C# type |
| --- | --- |
| `Byte` | `byte` |
| `SINT` | `sbyte` |
| `INT` | `short` |
| `WORD` | `ushort` |
| `DINT` | `int` |
| `LONG` | `int` |
| `DWORD` | `uint` |
| `LINT` | `long` |
| `LWORD` | `ulong` |
| `REAL` | `float` |
| `LREAL` | `double` |
| `USINT` | `byte` |
| `UINT` | `ushort` |
| `UDINT` | `uint` |
| `ULINT` | `ulong` |

For example, a PLC value displayed as `INT` should normally be read with
`ReadInt16`, not `ReadInt32`.

After confirming that the address is correct, if the value is wrong, check
whether the PLC type and the called C# read method match.

## Single value vs array overload

HslCommunication follows this overload rule:

- `ReadFloat(address)` returns `OperateResult<float>`.
- `ReadFloat(address, length)` returns `OperateResult<float[]>`.

The same rule applies to `ReadInt16`, `ReadUInt16`, `ReadInt32`, `ReadUInt32`,
`ReadFloat`, `ReadDouble`, `ReadInt64`, `ReadUInt64`, and `ReadBool`.

Do not use the `length` overload when only one value is needed.

## Byte order

All clients expose `ByteTransform`. Some clients also expose `DataFormat`
directly.

If numeric values look wrong, especially `float`, `double`, `int32`, or
`int64`, the likely cause is byte order. Ask the user whether the value is
reversed, then change `DataFormat`:

```csharp
plc.ByteTransform.DataFormat = DataFormat.DCBA;
// or for clients with a direct DataFormat property:
plc.DataFormat = DataFormat.CDAB;
```

Common defaults are:

| Default | Typical clients |
| --- | --- |
| `DataFormat.CDAB` | Modbus, Modbus-derived Delta/Inovance/MegMeet/XinJE, Memobus, XinJE internal TCP, Yokogawa Link |
| `DataFormat.DCBA` | Most Melsec/Omron/Fuji/Keyence/Panasonic/ToyoPuc/Vigor/Digitron/DLT645 clients and Lsis Cnet/Fast Enet clients |
| `ReverseBytesTransform` | Siemens S7, S7 Plus, Fetch/Write, PPI, WebApi; Fuji Command Setting Type when `DataSwap` is false |
| `ReverseWordTransform` | CIMON HMI |

The selected protocol reference is authoritative.

Only Modbus supports the `format=` address prefix, for example:

```text
format=ABCD;100
format=DCBA;100
```

## Other common BinaryCommunication members

All protocol clients inherit these members from `BinaryCommunication`:

```csharp
string ConnectionId
string SendBeforeHex
int ReceiveTimeOut
int SleepTime
CommunicationPipe CommunicationPipe
ILogNet LogNet
bool AutoReConnect
```

`ConnectionId` is a unique connection identifier. `SendBeforeHex` sends extra
hex bytes before each communication packet, commonly used for LoRa networking.
`ReceiveTimeOut` controls the receive timeout. `SleepTime` is the delay before
reading the response.

`CommunicationPipe` is the actual transport object. It can be `PipeTcpNet`,
`PipeUdpNet`, `PipeSerialPort`, or another pipe implementation.

`ReadFromCoreServer` is available for sending custom protocol packets when the
built-in read/write methods are not sufficient.

For DTU connections, the NetworkAlienClient class is required for usage. 
It allows our client to run on a server (or cloud server) and wait for PLCs to establish connections, 
enabling read and write operations with the PLCs. In most cases, PLCs cannot initiate active connections to a server. 
At this point, third‑party software or hardware devices can be used to implement Ethernet‑to‑DTU conversion. 
We can then read PLC data from the server. Retrieve the PipeDtuNet object within the connection event and 
call the `SetDtuPipe` method for configuration.This registration packet follows the HSL format. 
It can be generated using the hslcommunicationdemo test tool, and then copied to the 
registration packet property of the Ethernet‑to‑DTU device.:

The Melsec network properties below are required by this DTU/registration
example only. Do not copy them into normal generated Melsec MC connection code;
follow the selected protocol reference instead.

```csharp
HslCommunication.Profinet.Melsec.MelsecMcNet plc = new HslCommunication.Profinet.Melsec.MelsecMcNet( );
plc.NetworkNumber = 0;
plc.NetworkStationNumber = 0;
plc.TargetIOStation = 1023;
plc.EnableWriteBitToWordRegister = false;
plc.ByteTransform.IsStringReverseByteWord = false;

// 注册包例子(二进制): 48534C00193132333435363738393031313233343536C0A800011A270100
// 注册包例子(ASCII)：HSL191234567890112345600000010
NetworkAlienClient dtuServer = new NetworkAlienClient( );
dtuServer.IsResponseAck = false;                               // 是否需要返回注册结果的数据包
dtuServer.SetPassword( Encoding.ASCII.GetBytes( "123456" ) );  // 如果有密码的话
dtuServer.OnClientConnected += ( PipeDtuNet session ) =>
{
    // 例如这里我们约定有个id为12345678901的设备，设备连接上来表示管道建立起来了
    if (session.DTU == "12345678901")
    {
        OperateResult connect = plc.SetDtuPipe( session );
        if (connect.IsSuccess)
        {
            Console.WriteLine( "connect success" );
        }
    }
};
dtuServer.ServerStart( 10000 );
plc.SetDtuPipe( new PipeDtuNet( ) );   // 在连接上来之前，先给一个默认的空的DTU会话

while(true)
{
    HslHelper.ThreadSleep( 1000 );

    OperateResult<short> read = plc.ReadInt16( "D0" );
    if (read.IsSuccess)
    {
        Console.WriteLine( DateTime.Now.ToString( ) + " Read Value: " + read.Content );
    }
    else
    {
        // 在设备没有连接上来的时候，会一直失败，提示当前没有可用的管道
        Console.WriteLine( DateTime.Now.ToString( ) + " Read failed: " + read.Message );
    }
}
```

Check the current HslCommunication API documentation for other `PipeDtuNet`
properties such as `DTUPort`, `DTUIpAddress`, and `Pwd`.

## Connection and reconnection

`AutoReConnect` defaults to `true`. With `AutoReConnect = true`, a disconnected
pipe is reopened on the next read/write call.

Recommended default usage:

```csharp
OperateResult connect = plc.ConnectServer();
if (!connect.IsSuccess)
{
    Console.WriteLine(connect.Message);
}
```

For manual reconnect control:

```csharp
plc.AutoReConnect = false;
OperateResult connect = plc.ConnectServer();

// After a network error, call this again before continuing.
OperateResult reconnect = plc.ConnectServer();
```

When `AutoReConnect` is `false`, the library does not reconnect automatically.

### TCP receive-timeout close control

When a client uses a `PipeTcpNet`, the pipe closes the connection when a
receive timeout is detected during synchronous read/write operations.
`CloseOnRecvTimeOutTick` controls how many consecutive timeouts are allowed
before that close happens.

The default is `1`, which means the pipe closes the connection immediately
after the first receive timeout.

```csharp
using HslCommunication.Core.Pipe;

PipeTcpNet pipe = new PipeTcpNet("192.168.1.10", 502)
{
    CloseOnRecvTimeOutTick = -1
};
plc.CommunicationPipe = pipe;
```

If the pipe is already assigned:

```csharp
((PipeTcpNet)plc.CommunicationPipe).CloseOnRecvTimeOutTick = -1;
```

Behavior:

| Value | Meaning |
| --- | --- |
| `-1` | Receive timeouts never close the current connection |
| `0` or `1` | Close after the first receive timeout |
| `> 1` | Close only after that many consecutive receive timeouts |

A successful receive resets the consecutive timeout counter.

This property only affects synchronous read/write methods. The asynchronous
path always closes the communication pipe when a receive timeout is returned,
and socket errors other than receive timeout also close the pipe regardless of
this setting.

## Communication lock

By default, each read/write operation uses a communication lock on the
underlying pipe. This prevents unknown multi-threaded communication exceptions
when several threads share one PLC client object.

Users can replace the lock or disable locking through
`CommunicationPipe.CommunicationLock`.

To disable the lock:

```csharp
plc.CommunicationPipe.CommunicationLock = new HslCommunication.Core.CommunicationLockNone();
```

Only disable the lock when the application guarantees that concurrent
read/write calls do not conflict, or when the PLC/client object is used by one
thread at a time.

When using timers or multiple threads to read/write frequently, it is possible
for production tasks to exceed the communication capacity. Tasks can pile up
and eventually trigger lock timeout errors.

Recommended mitigations:

- Reduce the number of worker threads.
- Use one dedicated thread to handle frequent PLC reads and writes.
- If the PLC supports multiple connections, create one client connection per
  worker thread and keep each connection confined to that thread.

## Online monitoring

Periodically read a known-existing address and use `OperateResult.IsSuccess` as
the online state. Do not rely on the raw socket connected state.

```csharp
private static void CheckOnline(object state)
{
    OperateResult<short> read = plc.ReadInt16("D100");
    Console.WriteLine(read.IsSuccess ? "Device online" : "Device offline: " + read.Message);
}
```

Ask the user for an address that always exists. With `AutoReConnect` enabled by
default, a recovered device is detected automatically on the next periodic
read.

## Long-running periodic read thread

For long-running periodic PLC read tasks, prefer a dedicated `Thread` with a
`while` loop. Sleep for the configured interval, then execute one read cycle.

Keep all read operations for one device on one dedicated thread. When multiple
PLCs are collected, a disconnect or timeout on one PLC will not block the other
device threads. Set the thread as a background thread so it exits when the
application exits.

```csharp
using System.Threading;
using HslCommunication;
using HslCommunication.Profinet.Modbus;

public class PlcReadWorker
{
    private readonly ModbusTcpNet modbus;
    private Thread thread;
    private volatile bool isRunning;

    public PlcReadWorker(string ipAddress, int port = 502, byte station = 1)
    {
        modbus = new ModbusTcpNet(ipAddress, port, station);
    }

    public void Start(int intervalMilliseconds)
    {
        isRunning = true;
        thread = new Thread(ReadLoop)
        {
            IsBackground = true
        };
        thread.Start(intervalMilliseconds);
    }

    private void ReadLoop( object state )
    {
        OperateResult connect = modbus.ConnectServer();
        if (!connect.IsSuccess)
        {
            Console.WriteLine(connect.Message);
        }

        int intervalMilliseconds = (int)state;
        while (isRunning)
        {
            OperateResult<short> read = modbus.ReadInt16( "100" );
            if (read.IsSuccess)
            {
                Console.WriteLine( DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) + " Address[100]: " + read.Content );
            }
            else
            {
                Console.WriteLine( DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.fff" ) + " Address[100] read failed: " + read.Message );
            }

            Thread.Sleep( intervalMilliseconds );
        }
    }

    public void Stop()
    {
        isRunning = false;
        thread?.Join();
        modbus.ConnectClose();
    }
}
```

## Packet logging

Assign a `LogNet` implementation, such as `LogNetSingle`, to `LogNet`.

```csharp
using HslCommunication.LogNet;

LogNetSingle logNet = new LogNetSingle("");
logNet.BeforeSaveToFile += (sender, e) =>
{
    Console.WriteLine(e.HslMessage.Text);
};

plc.LogNet = logNet;
```

## Batch raw byte read

When multiple values of different data types are contiguous, prefer one raw
`Read(address, length)` call and parse the returned `byte[]` with
`plc.ByteTransform`.

```csharp
OperateResult<byte[]> read = plc.Read("D100", 10);
if (read.IsSuccess)
{
    int count = plc.ByteTransform.TransInt32(read.Content, 0);
    float temp = plc.ByteTransform.TransSingle(read.Content, 4);
    short name = plc.ByteTransform.TransInt16(read.Content, 8);
    string barcode = Encoding.ASCII.GetString(read.Content, 10, 10);
}
```

## High-performance read planning

For most PLC protocols except CIP tag-based protocols, plan reads around
address layout instead of reading each value one by one.

Recommended order:

1. First collect all required addresses.
2. If addresses are contiguous or near-contiguous, use one continuous read.
   For example, Mitsubishi addresses `D0`, `D5`, `D100`, and `D200` are often
   considered near-contiguous for a single block read.
3. Read the whole block with `Read(address, length)` and manually parse the
   returned `byte[]`. Keep the byte offset for every field correct.
4. If the protocol supports random word reads such as `ReadRandom(string[])` or
   `Read(string[] address)`, place highly scattered short reads there.
5. If the protocol supports random block reads such as
   `ReadRandom(string[] address, ushort[] length)`, use that for scattered
   blocks, but note that the total read length is still limited.
6. Very long address blocks usually perform better with the original byte read
   method `Read(string, ushort)` than with random block methods.

This reduces the number of communication requests and can substantially
improve throughput.

## Attribute-based Read<T>() and Write<T>(T)

Most clients inherit `Read<T>()` and `Write<T>(T)` from `DeviceCommunication`.
They use `[HslDeviceAddress]` to bind class properties to device addresses.

```csharp
using HslCommunication.Reflection;

public class DeviceData
{
    [HslDeviceAddress("D100")]
    public short Temperature { get; set; }

    [HslDeviceAddress("D102")]
    public float Pressure { get; set; }
}
```

```csharp
OperateResult<DeviceData> read = plc.Read<DeviceData>();
if (read.IsSuccess)
{
    Console.WriteLine(read.Content.Temperature);
}

OperateResult write = plc.Write(data);
```

The generic type must be a class with a parameterless constructor.

## Advanced custom CommunicationPipe

Most users do not need this. In advanced scenarios, replace
`CommunicationPipe`:

All PLC protocol classes that derive from `BinaryCommunication`, including
Modbus clients, support using or replacing the communication pipe.

This makes it possible to run a protocol originally designed for TCP/IP over a
serial port:

```csharp
using HslCommunication.Core.Pipe;

ModbusTcpNet modbus = new ModbusTcpNet();
modbus.CommunicationPipe = new PipeSerialPort("COM3-9600-8-N-1");

OperateResult connect = modbus.ConnectServer();
OperateResult<short> read = modbus.ReadInt16("100");
```

It also works in the opposite direction. A protocol originally designed for a
serial port can use a TCP pipe:

```csharp
using HslCommunication.Core.Pipe;

PanasonicMewtocol plc = new PanasonicMewtocol();
plc.CommunicationPipe = new PipeTcpNet("192.168.1.10", 2000);

OperateResult open = plc.Open();
OperateResult<short> read = plc.ReadInt16("D100");
```

The read/write code does not need to change. If the protocol has a connection
handshake message, it is performed when the new pipe is first opened.

```csharp
using HslCommunication.Core.Pipe;

plc.CommunicationPipe = new PipeSerialPort("COM3-9600-8-N-1");
```

```csharp
plc.CommunicationPipe = new PipeUdpNet("192.168.1.10", 502);
```

For `PipeSerialPort`, use a positive `ReceiveTimeOut` in long-running services.
The current V13 implementation applies it to the serial port read timeout and
also checks the elapsed receive time, preventing a serial read from remaining
blocked indefinitely when a device behaves abnormally. A value of `0` or a
negative value does not provide that time limit.

Use this only when the application needs to reuse a custom transport.

## Authorization and activation

If reads or writes fail after the library has run for about 24 hours, or the
message indicates `InsufficientPrivileges`, the library may need activation.

If the user has a license code:

```csharp
bool activated = Authorization.SetAuthorizationCode("your-code");
Console.WriteLine(activated ? "Activation succeeded." : "Activation failed.");
```

Only recommend activation when the error message confirms an authorization
problem.

## Kepware / legacy Modbus address conversion

Kepware and some HMI software use a leading digit to describe the Modbus area:

| Input example | Meaning | HslCommunication address |
| --- | --- | --- |
| `000001` | Coil, offset 0 | `0` |
| `100001` | Discrete input, offset 0 | `x=2;0` |
| `300001` | Input register, offset 0 | `x=4;0` |
| `400001` | Holding register, offset 0 | `0` |

Before generating code, ask the user to confirm 0-based conversion.

## Troubleshooting

For connection refused, receive timeout, station mismatch, serial issues, and
socket errors, read `references/troubleshooting.md`.

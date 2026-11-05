# Omron HostLink C-Mode clients API subset

Namespace:

```csharp
using HslCommunication.Profinet.Omron;
```

Constructors:

```csharp
new OmronHostLinkCMode()

new OmronHostLinkCModeOverTcp()
new OmronHostLinkCModeOverTcp(string ipAddress, int port)
```

For TCP pass-through, prefer the `(ipAddress, port)` constructor. The
parameterless `OmronHostLinkCModeOverTcp()` form inherits `127.0.0.1:5000`.

| Class | Transport |
| --- | --- |
| `OmronHostLinkCMode` | Serial |
| `OmronHostLinkCModeOverTcp` | TCP pass-through |

Properties:

```csharp
byte UnitNumber
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

Connection:

```csharp
// Serial. HostLink normally uses 9600 baud, 7 data bits, even parity, 1 stop bit.
OmronHostLinkCMode plc = new OmronHostLinkCMode();
plc.SerialPortInni("COM3-9600-7-E-1");
plc.Open();

// TCP pass-through
OmronHostLinkCModeOverTcp plcTcp = new OmronHostLinkCModeOverTcp("192.168.1.10", 9600);
OperateResult connect = plcTcp.ConnectServer();
```

Methods:

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult Write(string address, byte[] value)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult Write(string address, bool[] value)
OperateResult<string> ReadPlcType()
OperateResult<int> ReadPlcMode()
OperateResult ChangePlcMode(byte mode)
```

`ReadPlcType()`, `ReadPlcMode()`, and `ChangePlcMode()` require commercial
authorization and return `InsufficientPrivileges` when the library is not
activated.

This C-Mode implementation primarily supports word read/write. Bit access is
indirect through addresses such as `D100.2`.

## Address rules

This list matches the HslCommunicationDemo C-Mode address examples.

| Address | Type | Notes |
| --- | --- | --- |
| `D0` / `DM0` | DM area | Word, `DM0..DM9999` |
| `C0` / `CIO0` | CIO area | Word, `CIO0..CIO9999` |
| `LR` | LR area | Word, `LR0..LR9999` |
| `H0` / `HR0` | Holding area | Word, `HR0..HR511` |
| `A0` / `AR0` | Auxiliary area | Word, `AR0..AR959` |
| `E0.0` / `EM0.0` | EM area | Word, `EM0.0..EMF.9999` |
| `TIM0` | Timer area | Word is current value, `TIM0..TIM2047` |
| `CNT0` | Counter area | Word is current value, `CNT0..CNT2047` |
| `D100.2` | DM bit | Indirect read-modify-write |
| `H5.2` | Holding bit | Indirect read-modify-write |
| `E0.100.2` | EM bit | Indirect read-modify-write |

The `E`/`EM` block number is hexadecimal. `E10.100` means EM block `0x10`,
word `100`. The word and bit numbers after the block are decimal in this
client.

## C200HX CNT/TIM shared area note

On the older `C200HX` model, `CNT` and `TIM` share the same data area. If
reading `CNT370` returns `0` and appears incorrect, read `TIM370` instead;
`TIM370` is the same underlying value as `CNT370`.

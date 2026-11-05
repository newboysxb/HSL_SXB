# Virtual PLC and device server guide

Important routing distinction:

- This guide is for writing HslCommunication C# code that creates a virtual
  server inside the user's own program.
- If the user says "in HslCommunicationDemo", "in the Demo program", "在
  HslCommunicationDemo 里", or "在 Demo 程序里" and asks to create, open,
  start, stop, configure or test a virtual PLC/server, do not implement a new
  server class and do not use desktop GUI automation. Load
  `references/ai_gateway_guide.md` and use `POST {base}/api/operations`.
- In that Demo case, `virtualServer.create` opens or reuses the original Demo
  server window, and start/stop operations invoke the original window button
  actions.

HslCommunication includes virtual server classes for testing clients without a
physical PLC. Most derive from `DeviceServer`; the virtual device keeps an
in-process data pool and responds to the same protocol frames as the real
device. Use this reference when the user asks for a PLC simulator, virtual
server, protocol mock, or local test device. Load the corresponding client
protocol reference as well for the exact address syntax.

## Common DeviceServer workflow

```csharp
MelsecMcServer server = new MelsecMcServer( isBinary: true );
server.Port = 6000;
server.ServerStart( );

server.Write( "D100", (short)1234 );
OperateResult<short> read = server.ReadInt16( "D100" );

server.ServerClose( );
```

Common transport options from `DeviceServer`:

- `ServerStart(port)` starts TCP.
- `ServerStart(port, modeTcp: false)` starts UDP.
- `ServerStart(tcpPort, udpPort)` starts both modes.
- `StartSerialSlave("COM3-9600-8-N-1")` starts a serial slave.
- `EnableWrite = false` makes the virtual device read-only.
- `ActiveTimeSpan` controls the idle disconnect period.
- `SaveDataPool` and `LoadDataPool` persist the data pool when the concrete
  server overrides the persistence methods.
- `SetTrustedIpAddress` restricts clients.
- `UseSSL` enables TLS for servers that support it.
- `OnDataReceived` and `OnDataSend` expose raw protocol frames.
- Override `ReadFromCoreServer(PipeSession, byte[])` to implement custom
  response logic before falling back to the base protocol parser.

For real device address syntax, use the matching client reference. The virtual
server normally uses the same address parser as its client class. The notes
below only call out differences that are confirmed in the server source.

## Server-side API map

Every class in this guide inherits the common read/write API from
`DeviceServer -> DeviceCommunication -> BinaryCommunication`. Use the methods
below directly on the server object; they operate on the virtual data pool
without requiring a network client.

### Common inherited methods

| Data category | Single-value read | Array/block read | Write |
| --- | --- | --- | --- |
| Raw bytes | `Read(address, ushort length)` → `OperateResult<byte[]>` | Same method | `Write(address, byte[] value)` → `OperateResult` |
| Boolean | `ReadBool(address)` → `OperateResult<bool>` | `ReadBool(address, ushort length)` → `OperateResult<bool[]>` | `Write(address, bool value)` or `Write(address, bool[] value)` |
| 16-bit signed | `ReadInt16(address)` | `ReadInt16(address, ushort length)` | `Write(address, short value)` or `Write(address, short[] value)` |
| 16-bit unsigned | `ReadUInt16(address)` | `ReadUInt16(address, ushort length)` | `Write(address, ushort value)` or `Write(address, ushort[] value)` |
| 32-bit signed | `ReadInt32(address)` | `ReadInt32(address, ushort length)` | `Write(address, int value)` or `Write(address, int[] value)` |
| 32-bit unsigned | `ReadUInt32(address)` | `ReadUInt32(address, ushort length)` | `Write(address, uint value)` or `Write(address, uint[] value)` |
| 64-bit signed | `ReadInt64(address)` | `ReadInt64(address, ushort length)` | `Write(address, long value)` or `Write(address, long[] value)` |
| 64-bit unsigned | `ReadUInt64(address)` | `ReadUInt64(address, ushort length)` | `Write(address, ulong value)` or `Write(address, ulong[] value)` |
| 32-bit floating | `ReadFloat(address)` | `ReadFloat(address, ushort length)` | `Write(address, float value)` or `Write(address, float[] value)` |
| 64-bit floating | `ReadDouble(address)` | `ReadDouble(address, ushort length)` | `Write(address, double value)` or `Write(address, double[] value)` |
| String | `ReadString(address, ushort length)` or `ReadString(address, ushort length, Encoding encoding)` | Same methods | `Write(address, string value)` or the length/encoding overloads |

Additional inherited helpers:

- `ReadCustomer<T>(address)` and `WriteCustomer<T>(address, data)` for
  `IDataTransfer` objects.
- `Read<T>()`, `Write<T>(data)`, and `ReadStruct<T>(address, length)` for
  attribute-based or structure mapping.
- `Wait(...)` overloads for waiting for a boolean or numeric value.
- `...Async` variants of the read/write methods when the target framework
  supports `Task`.

### Special methods by server

Only the classes below add server-specific operations; the other virtual
servers use the common API above without additional read/write methods.

| Server | Special methods or behavior |
| --- | --- |
| `ModbusTcpServer` | `ReadCoil`, `WriteCoil`, `ReadDiscrete`, `WriteDiscrete`, `AddSubcription`, `RemoveSubcrption`, and `RegisteredAddressMapping`. |
| `XinJEServer` | Inherits `ModbusTcpServer`; the same API is used for its `M`, `D`, `SD`, `SM`, and `HD` areas. |
| `AllenBradleyServer` | `AddTagValue(...)` for tags and `CreateTagWithWrite` for optional auto-creation. |
| `OmronCipServer` | Inherits CIP tag methods; overrides string tag handling. |
| `BeckhoffAdsServer` | `AddTagValue(...)` for `s=TagName` tags, in addition to memory-area access. |
| `SiemensS7Server` | `AddDbBlock(db)` and `AddDbBlock(db, length)` for DB blocks beyond DB1/DB2/DB3. |
| `SiemensPPIServer` | Inherits `SiemensS7Server`; adds `Station`. |
| `YokogawaLinkServer` | `StartProgram()` and `StopProgram()`. |
| `DLT645Server` / `DLT645With1997Server` / `DLT698Server` | Protected `AddDltTag` and `AddDltAsciiTag` methods for implementing custom meter tag tables in a derived server. |

The common API is sufficient for memory-pool servers. For tag-based CIP and ADS
servers, the tag must exist before the typed read/write call. For S7 DB access,
the DB block must exist before the typed read/write call.

## Server families and special setup

### Memory-pool servers

These servers create internal buffers and map protocol addresses directly to
them:

- `ModbusTcpServer`
- `MelsecMcServer`
- `MelsecA1EServer`
- `MelsecA3CServer`
- `MelsecA4CServer`
- `MelsecFxSerialServer`
- `MelsecFxLinksServer`
- `SiemensS7Server`
- `SiemensPPIServer`
- `SiemensFetchWriteServer`
- `OmronFinsServer`
- `OmronFinsUdpServer`
- `OmronHostLinkServer`
- `OmronHostLinkCModeServer`
- `AllenBradleyPcccServer`
- `KeyenceNanoServer`
- `PanasonicMewtocolServer`
- `MemobusTcpServer`
- `LSisServer`
- `GeSRTPServer`
- `YokogawaLinkServer`
- `FatekProgramServer`
- `ToyoPucServer`
- `VigorServer`
- `InovanceEasyNetServer`
- `XinJEServer`
- `FujiSPBServer`
- `FujiSPHServer`
- `FujiCommandSettingTypeServer`
- `CimonServer`
- `DigitronCPLServer`

Most have a parameterless constructor. The important constructor and setup
differences are:

| Class | Special constructor or setup |
| --- | --- |
| `MelsecMcServer` | `MelsecMcServer(bool isBinary = true)`; chooses binary or ASCII MC framing. Buffers include `M`, `X`, `Y`, `L`, `B`, `S`, `F`, `D`, `W`, `R`, `ZR`. |
| `MelsecA1EServer` | `MelsecA1EServer(bool isBinary = true)`, inherited from `MelsecMcServer`. |
| `MelsecA3CServer` | Calls `base(false)`; exposes `Station`, `SumCheck`, and `Format`. |
| `MelsecA4CServer` | Inherits `MelsecA3CServer`; it is an A4C framed server. |
| `MelsecFxSerialServer` | Exposes `IsNewVersion`; this must match the client and PLC/programming-port version. |
| `MelsecFxLinksServer` | Exposes `Station`, `SumCheck`, and `Format`. |
| `SiemensS7Server` | Supports `I`, `Q`, `M`, `DB1`, `DB2`, `DB3`, `V`, timer, and counter areas. DB1, DB2, and DB3 are created automatically; use `AddDbBlock(db)` or `AddDbBlock(db, length)` for others. |
| `SiemensPPIServer` | Inherits the S7 server and adds `Station`. |
| `SiemensFetchWriteServer` | Uses the Fetch/Write server frame and S7-style memory areas. |
| `AllenBradleyPcccServer` | Uses PCCC command framing rather than CIP tag registration; address forms such as `N7:0` follow the PCCC client reference. |
| `OmronFinsServer` | Creates DM, CIO, Work, Hold, Auxiliary, EM, timer/counter, and related buffers. Supports `DataFormat` and `CopySID`. |
| `OmronFinsUdpServer` | Inherits the FINS server but is intended for UDP server mode. |
| `OmronHostLinkServer` | Inherits the FINS server and adds `UnitNumber`. |
| `OmronHostLinkCModeServer` | Inherits the FINS server and adds `UnitNumber`. |
| `LSisServer` | Has `LSisServer()` and `LSisServer(string CpuType)`; `SetCpuType` selects the CPU family mapping. |
| `YokogawaLinkServer` | Adds `StartProgram()` and `StopProgram()` control operations. |
| `ModbusTcpServer` | Adds station handling and data-pool controls. See the Modbus-specific notes below. |
| `XinJEServer` | Inherits `ModbusTcpServer`; adds `M`, `D`, `SD`, `SM`, and `HD` areas while retaining Modbus address handling for the base paths. |
| `CimonServer` | Uses separate bit and word buffers. Bit areas are `X`, `Y`, `M`, `K`, `L`, `C`, `T`; word areas are `D`, `F`, `Z`, `S`. It also exposes `FrameNo`. |
| `DigitronCPLServer` | Uses one word data pool and `Station`; the wire protocol uses fixed-length numeric addresses and `RS`/`WS` commands. |

### Tag-based servers

Tag-based servers do not accept arbitrary PLC memory addresses. Add the tags
before starting the server:

```csharp
AllenBradleyServer server = new AllenBradleyServer( );
server.AddTagValue( "A", (short)123 );
server.AddTagValue( "Run", true );
server.ServerStart( 44818 );
```

`AllenBradleyServer` and `OmronCipServer` use the tag dictionary. The tag names
must match the client's address exactly, including case where the client uses
symbolic tag access. `OmronCipServer` overrides string tag handling and stores
the leading string-length information used by CIP strings. `CreateTagWithWrite`
can create a missing tag when a write operation is performed; do not rely on
that for a preconfigured production simulation. For arrays and strings, use the
typed `AddTagValue` overloads so the CIP type and maximum length are correct.

### Beckhoff ADS tags and memory areas

`BeckhoffAdsServer` supports both memory areas and tags:

- `M` memory maps to the `0x4020` area.
- `I` memory maps to the `0xF020` area.
- `Q` memory maps to the `0xF030` area.
- Symbolic access using `s=TagName` uses `AddTagValue("TagName", value)`.

Use the typed `AddTagValue` overloads for numeric arrays and strings. Array
access such as `TagName[0]` is handled by the server's tag address logic.

### DLT meter servers

`DLT645Server`, `DLT645With1997Server`, and `DLT698Server` are address-tag
servers, not general-purpose register servers. They create a fixed set of
energy-meter tags in `CreateAddressTags()`:

- `DLT645Server`: DLT645-2007 address tags such as `02010100` and `00000000`.
- `DLT645With1997Server`: overrides the tag table with DLT645-1997 addresses
  such as `9010`, `9020`, and `B611`.
- `DLT698Server`: creates DLT698 object addresses such as `00000200`,
  `20000200`, and `40010200`; it also uses `ReverseBytesTransform`.

`AddDltTag` and `AddDltAsciiTag` are protected. To add custom meter points,
derive from the desired DLT server and override `CreateAddressTags()` or add
tags in the derived constructor. `Station`, `StringReverse`, `EnableCodeFE`,
and `StationMatch` control meter addressing and response behavior.

## Address support summary

Representative server-side address forms are listed below. The selected client
protocol reference remains authoritative and should be loaded for a complete
address table.

| Server family | Representative addresses and notes |
| --- | --- |
| `ModbusTcpServer` | Coils and registers use the normal Modbus forms such as `0`, `100`, `x=2;100`, `x=4;100`, and `s=2;100`. `StationDataIsolation` separates data by station; `StationCheck` controls station matching; `UseModbusRtuOverTcp` selects RTU-over-TCP framing; `EnableWriteMaskCode` controls mask writes. |
| `MelsecMcServer` / `MelsecA1EServer` | `M100`, `X100`, `Y100`, `D100`, `W100`, `R100`, `ZR100`, `B100`, `S100`, `F100`, `L100`. Binary/ASCII and frame format must match the client. |
| `MelsecA3CServer` / `MelsecA4CServer` | A3C/A4C serial-compatible frame addresses; `Station`, `Format`, and `SumCheck` must match the client. |
| `MelsecFxSerialServer` | FX programming-port addresses; `IsNewVersion` must match the client. |
| `MelsecFxLinksServer` | FX Links addresses; `Station`, `Format`, and `SumCheck` must match the client. |
| `SiemensS7Server` | `I100`, `Q100`, `M100`, `DB1.100`, `V100`, `T100`, `C100`. `DB1`, `DB2`, and `DB3` exist automatically; other DB blocks require `AddDbBlock`. |
| `SiemensPPIServer` | S7/PPI addresses with `Station`; use the PPI client reference for the allowed subset. |
| `SiemensFetchWriteServer` | S7/Fetch-Write memory areas; use the Fetch/Write client reference. |
| `OmronFinsServer` | `DM100`, `CIO100`, `W100`, `H100`, `A100`, `E0_100`, plus timer/counter areas. |
| `OmronHostLinkServer` / `OmronHostLinkCModeServer` | The Omron area syntax plus `UnitNumber`; use the corresponding HostLink client reference. |
| `AllenBradleyServer` / `OmronCipServer` | Symbolic tags registered with `AddTagValue`; no arbitrary memory address is accepted. |
| `AllenBradleyPcccServer` | PCCC addresses such as `N7:0`; use the PCCC client reference. |
| `BeckhoffAdsServer` | `M100`, `I100`, `Q100`, and symbolic `s=TagName` after `AddTagValue`. |
| `KeyenceNanoServer` | Keyence Nano upper-link addresses; use the Nano client reference. |
| `PanasonicMewtocolServer` | Mewtocol addresses with station handling; use the Mewtocol client reference. |
| `MemobusTcpServer` | Memobus address areas; use the Memobus client reference. |
| `LSisServer` | `P`, `Q`, `M`, `I`, `U`, `D`, and `T` areas; CPU type affects address interpretation. |
| `GeSRTPServer` | GE SRTP address areas; use the GE client reference. |
| `YokogawaLinkServer` | Yokogawa Link addresses; use the Yokogawa client reference. |
| `FatekProgramServer` | Fatek register/coil address forms; use the Fatek client reference. |
| `ToyoPucServer` | Toyo PUC addresses; use the Toyo client reference. |
| `VigorServer` | Vigor PLC addresses; use the Vigor client reference. |
| `InovanceEasyNetServer` | Inovance EasyNet addresses; use the EasyNet client reference. |
| `FujiSPBServer`, `FujiSPHServer`, `FujiCommandSettingTypeServer` | Use the corresponding Fuji client address format. |
| `CimonServer` | Bit areas `X/Y/M/K/L/C/T`; word areas `D/F/Z/S`; address offsets are internal to the server. |
| `DigitronCPLServer` | Numeric word addresses and `RS`/`WS` commands; station and data type W/S must match the client. |
| `DLT645Server`, `DLT645With1997Server`, `DLT698Server` | Fixed DLT address tags, not generic register addresses. Use the DLT client reference and the server tag table. |

## Important implementation notes

- `EnableWrite` is enforced by the server; set it to `false` for read-only
  simulations.
- Calling `Write` on the server directly writes the virtual data pool and does
  not require a network client.
- Server-side and client-side address syntax are intentionally the same for most
  memory-pool protocols; a mismatch is usually a frame-format, station, or
  address-family mismatch rather than a different server API.
- For `SiemensS7Server`, DB blocks beyond DB1/DB2/DB3 are not implicitly
  available. Add them before the client accesses them.
- For `AllenBradleyServer`, `OmronCipServer`, and `BeckhoffAdsServer`, add tags
  before the client reads or writes them unless `CreateTagWithWrite` is
  intentionally enabled.
- For DLT servers, custom points require a subclass because the tag-addition
  methods are protected.
- For serial virtual servers, `ServerStart` and `StartSerialSlave` are
  independent options; use the transport that matches the test client.

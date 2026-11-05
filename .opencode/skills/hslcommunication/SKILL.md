---
name: hslcommunication
description: Generate and troubleshoot HslCommunication C# code for PLCs, energy meters, Modbus, Siemens, Mitsubishi, Omron, Allen-Bradley, Beckhoff, Keyence, Delta, Inovance, and other industrial protocols. Use for live reads/writes, address tables, online-status checks, SECS/HSMS, HTTP/WebApi, MQTT, WebSocket, FTP, Python/pythonnet, DevicePluginNet, and custom protocol extensions. When HslCommunicationDemo is running, use its local AI Gateway to read/write connected devices, manage point tables, create/start/stop virtual PLC servers, run PLC-style logic programs, generate simulation data, and verify an upper computer from its frame log.
---

# HslCommunication: generated code and Demo operation

This skill covers two different jobs. Decide which one the request belongs to **before** reading
anything else - the wrong branch wastes the whole turn:

- **Generate code** - copy-paste C# that uses the HslCommunication library against a PLC, meter,
  device, or a server class you write yourself. Selected by the protocol index below.
- **Operate a running HslCommunicationDemo** - read/write an already connected device, open and
  connect a client page, create/start/stop a virtual server, import points, run a logic program or
  data simulation, inject faults, verify an upper computer from its frames. This goes through the
  Demo's local HTTP AI gateway, **not** through generated code.

If the request mentions HslCommunicationDemo, "in the Demo", "在 Demo 程序里", or a device that is
already connected in the Demo, it is the second job.

## Fast intent router

| User intent | Load | Start at |
| --- | --- | --- |
| Read, write, collect or diagnose a device already connected in the Demo | `references/ai_gateway_guide.md` | `Step 1: verify the gateway` -> `Step 5` |
| Collect values periodically (100 ms, 1 s, ...) and analyse the dataset | `references/ai_gateway_guide.md` | `Step 5: periodic collection and analysis`, `High-frequency reads: use collect, never loop read` |
| Read or replace the Demo address table, or get device instantiation code | `references/ai_gateway_guide.md` | `Step 3: get the current address table`, `Step 3.1`, `Step 3.2`, `Generate read/write code from a Demo device` |
| Add or delete tags on a Beckhoff ADS virtual server (tag names come from the PLC project, not free text) | `references/ai_gateway_guide.md` + `references/beckhoff_ads_api.md` | `virtualServer.tags.list` / `add` / `remove` |
| See what the gateway did in this session, in order | `references/ai_gateway_guide.md` | `Get the operation queue (GET /api/activity)` |
| Create/start/stop a virtual server, or read/write its point table | `references/ai_gateway_guide.md` | `Virtual PLC server automation` |
| Open a Demo client page, connect it, read/write through it, close it | `references/ai_gateway_guide.md` (+ the protocol reference for the page's default settings) | `Client device page automation (client.*)` |
| List the tags that exist inside a Beckhoff PLC (tag names are defined in the PLC project) | `references/ai_gateway_guide.md` + `references/beckhoff_ads_api.md` | `client.browseSymbols` (auto-switches to the 浏览变量 tab) |
| Create/edit/start/test a logic program (server **or** client page) | `references/ai_gateway_guide.md` + `references/logic_program_guide.md` | `Driving a page's logic program (POST /api/ui/logic)` |
| Create periodic, step, ramp, random, sine or expression-driven data | `references/ai_gateway_guide.md` + `references/data_simulation_guide.md` | `Virtual PLC server automation`, `Step 4.1` |
| Inject delay/drop/corrupt/disconnect to test timeouts and reconnect | `references/ai_gateway_guide.md` | `Fault injection of a virtual server` |
| Verify an upper computer from what it actually sent | `references/ai_gateway_guide.md` | `Poll upper-computer traffic`, `报文日志分析` |
| Save / list / re-open / delete Demo saved pages (保存列表) | `references/ai_gateway_guide.md` | `保存列表：保存 / 打开 / 删除已保存的页面 (page.*)` |
| Bring a tab to the front, or look at the Demo window | `references/ai_gateway_guide.md` | `Switching tabs at the start of every testing stage`, `Seeing the Demo` |
| Run a multi-step regression, or verify logic/simulation/faults end to end | `references/ai_gateway_guide.md` | `Scenario automation: many verified steps in one call` |
| Generate normal C# client code for a named PLC protocol | `references/common_behavior.md` + the one matching protocol reference | - |
| Write a virtual PLC/protocol server in the user's own project | `references/virtual_plc_server_guide.md` + the matching protocol reference | - |
| Build an HTTP/Web API service | `references/http_server_guide.md` | - |
| Use MQTT, WebSocket, SECS/HSMS or FTP | The matching transport guide only | - |
| Extend with a plugin or an inherited custom protocol | `references/plugin_extension.md` or `references/custom_protocol_inheritance.md` | - |
| Call HslCommunication from Python | `references/python_interop.md` | - |
| A connection or read/write operation fails | The already selected primary reference + `references/troubleshooting.md` | - |

Routing precedence:

1. If the request operates the running Demo, the Demo AI Gateway wins over code generation.
2. Logic programs and data simulation are subroutes of the Demo gateway, not separate topics.
3. A virtual server written in the user's own project is **not** a Demo operation.
4. Load one primary reference plus at most one closely related subguide. Load a protocol reference
   when generating client code **or when its address syntax / default settings are needed** - which
   includes creating a Demo client page, because `client.create` needs that page's XML defaults. Do
   not load it merely to read or write a device that is already connected in the Demo.
5. Multi-step verification of the running Demo is still one Demo-gateway route: express it as a
   single `/api/scenario/run` instead of many single calls.

`references/ai_gateway_guide.md` opens with a `Contents` table that lists every section; search for
a section title instead of reading the file top to bottom.

## Top-level rules

1. Generate code only for the classes listed in this skill or in the matching protocol reference.
2. Always check `OperateResult<T>.IsSuccess` before accessing `.Content`.
3. Prefer explicit network constructors such as `(ipAddress, port)`. Parameterless network
   constructors inherit `127.0.0.1:5000`, not the protocol's usual port.
4. Open the transport the way the selected client expects:
   - TCP/UDP clients based on `DeviceTcpNet`: `ConnectServer()`.
   - Serial clients: `Open()` after `SerialPortInni(...)`.
   - Special UDP cases follow the selected protocol reference.
5. Never guess address syntax. Copy or adapt the examples in the selected protocol reference and,
   when available, the HslCommunicationDemo address declarations. Never assume the numeric part of an
   address is decimal; check the base rules in `references/common_behavior.md` and the selected
   protocol reference.
6. Do not silently convert Kepware or legacy Modbus addresses. Ask the user to confirm first.
7. Only recommend commercial activation when `OperateResult.Message` clearly indicates an
   authorization or `InsufficientPrivileges` problem.
8. **Demo work goes through the gateway, never through the desktop.** No mouse coordinates, no
   clicking on screenshots, no RPA or `SendKeys`. The gateway opens or reuses the original Demo
   window and calls its original button actions in-process. Inside a Demo task, also switch the
   visible tab at the start of **every** testing stage (`GET/POST /api/ui/tabs`, by `title`, never by
   a hard-coded index) - the user is watching the screen and cannot follow work that happens on a
   page that is not in front. Do not take a screenshot for this; the tab switch is the observable
   step. Details: `Switching tabs at the start of every testing stage`.

## Connection input rules

Connection values are not always required at code-generation time. Determine the output style first:

- For a one-off sample, interactive test or code that connects to a specific device now, collect the
  protocol/brand, transport values, station or device identity, and a real address when needed.
- For configurable code, a complete project, a service or a multi-device application, do not require
  the current IP address, port, station, password or AMS NetId. Generate configuration fields,
  placeholders, a factory or protocol resolver, and load the values from the project's configuration
  source at runtime.

If the user explicitly asks for a runnable connection test, concrete values are appropriate. If the
user asks for reusable or configurable code, do not block generation waiting for deployment values.

## External API service routing

When the user asks for a service that exposes APIs to other programs and does not name a protocol:

- Prefer `HttpServer` for request/response Web APIs and load `references/http_server_guide.md`.
- Do not default to MQTT or WebSocket merely because they can also exchange API messages; choose
  them when the requirement explicitly includes device push, subscriptions, persistent connections
  or bidirectional real-time messaging.
- Choose SECS/HSMS, FTP or another protocol family only when the user names it or the use case
  clearly requires it.
- For `HttpServer` implementations with more than three operations and naturally RPC-like semantics,
  prefer public methods marked `[HslMqttApi]` registered through `RegisterHttpRpcApi` instead of
  growing a large `HandleRequestFunc` switch.

## Protocol index

Rows whose family starts with `Demo` are gateway features, not library classes. Demo **virtual-server type names** (for example `CipServer`, `PcccServer`, `McA3CServer`, `XinJEInternalServer`) are also not library classes - pass them to `virtualServer.create` as the `type` value and do not `new` them in generated code.

| Brand / protocol family | Representative classes | Reference |
| --- | --- | --- |
| Modbus | `ModbusTcpNet`, `ModbusUdpNet`, `ModbusRtu`, `ModbusAscii`, `ModbusRtuOverTcp`, `ModbusAsciiOverTcp` | `references/modbus_tcp_api.md`, `references/modbus_variants_api.md` |
| Demo AI Gateway | Connected Demo devices plus virtual-server create/start/stop/read/write operations over local HTTP | `references/ai_gateway_guide.md` |
| Demo virtual-server fault injection | Inject delay and drop, corrupt (serial only) and disconnect (network only) into the responses of a virtual server to verify the timeout, retry and reconnect logic of a host application over `virtualServer.faultInjection.*` | `references/ai_gateway_guide.md` |
| Demo virtual-server logic program | PLC-style rules, conditions, actions, TON timers, non-blocking `delay`, `call` sub programs and runtime logs - **gateway feature, not a library class** | `references/logic_program_guide.md` |
| Demo scenario automation | One `POST /api/scenario/run` runs an ordered list of typed steps (operation / read / write / logic / wait / sleep / assert / note) with assertions, always runs the teardown, and returns one report. Only one run at a time (`scen_busy`). A `wait` step's `until.path` may be relative to the polled response or absolute (`steps.<waitId>.value`); an `assert` path must be absolute. | `references/ai_gateway_guide.md` |
| Demo data simulation | Periodic step, ramp, random, sine and expression-generated values written through a virtual-server or connected-device simulation tab | `references/data_simulation_guide.md` |
| Virtual PLC / device servers | `DeviceServer`, `ModbusTcpServer`, `MelsecMcServer`, `SiemensS7Server`, `AllenBradleyServer`, and other protocol servers, for user-written C# code rather than operating the Demo program | `references/virtual_plc_server_guide.md` |
| WeCon over Modbus | `ModbusTcpNet`, `ModbusUdpNet`, `ModbusRtu`, `ModbusAscii`, `ModbusRtuOverTcp`, `ModbusAsciiOverTcp` with `ModbusMappingAddress.WeCon_Lx5v` | `references/wecon_api.md` |
| FTP | `FtpClient`, `FtpServer` | `references/ftp_api.md` |
| HTTP/WebApi | `HttpServer`, `NetworkWebApiBase` | `references/http_server_guide.md` |
| MQTT | `MqttServer`, `MqttClient`, `MqttSyncClient` | `references/mqtt_guide.md` |
| WebSocket | `WebSocketServer`, `WebSocketClient` | `references/websocket_guide.md` |
| SECS/HSMS | `SecsHsms`, `SecsHsmsServer` | `references/secs_hsms_guide.md` |
| Python interop | Python + pythonnet calling the C# HslCommunication assembly | `references/python_interop.md` |
| Plugin extension | `DevicePluginNet` (HSL pipe/command mode or fully custom mode) | `references/plugin_extension.md` |
| Custom protocol by inheritance | `DeviceCommunication`, `DeviceTcpNet`, `DeviceSerialPort` overrides | `references/custom_protocol_inheritance.md` |
| Mitsubishi Melsec MC | `MelsecMcNet`, `MelsecMcAsciiNet`, `MelsecMcUdp`, `MelsecMcAsciiUdp` | `references/melsec_mc_api.md` |
| Mitsubishi A1E | `MelsecA1ENet`, `MelsecA1EAsciiNet` | `references/melsec_a1e_api.md` |
| Mitsubishi A3C | `MelsecA3CNet`, `MelsecA3CNetOverTcp` | `references/melsec_a3c_api.md` |
| Mitsubishi A4C | `MelsecA4CNet`, `MelsecA4CNetOverTcp` | `references/melsec_a4c_api.md` |
| Mitsubishi FxLinks | `MelsecFxLinks`, `MelsecFxLinksOverTcp` | `references/melsec_fxlinks_api.md` |
| Mitsubishi FxSerial | `MelsecFxSerial`, `MelsecFxSerialOverTcp` | `references/melsec_fxserial_api.md` |
| Omron FINS | `OmronFinsNet`, `OmronFinsUdp` | `references/omron_fins_api.md` |
| Omron HostLink | `OmronHostLink`, `OmronHostLinkOverTcp` | `references/omron_hostlink_api.md` |
| Omron HostLink C-Mode | `OmronHostLinkCMode`, `OmronHostLinkCModeOverTcp` | `references/omron_hostlink_cmode_api.md` |
| Omron CIP | `OmronCipNet`, `OmronConnectedCipNet` | `references/omron_cip_api.md` |
| Allen-Bradley CIP | `AllenBradleyNet`, `AllenBradleyConnectedCipNet` | `references/allenbradley_cip_api.md` |
| Allen-Bradley Micro800 | `AllenBradleyMicroCip` | `references/allenbradley_microcip_api.md` |
| Allen-Bradley PCCC | `AllenBradleyPcccNet` | `references/allenbradley_pccc_api.md` |
| Allen-Bradley SLC | `AllenBradleySLCNet` | `references/allenbradley_slc_api.md` |
| Allen-Bradley DF1 | `AllenBradleyDF1Serial` | `references/allenbradley_df1serial_api.md` |
| Beckhoff ADS | `BeckhoffAdsNet` | `references/beckhoff_ads_api.md` |
| CIMON HMI | `CimonHmiProtocol` | `references/cimon_hmi_api.md` |
| Delta Modbus | `DeltaSerial`, `DeltaSerialAscii`, `DeltaSerialOverTcp`, `DeltaSerialAsciiOverTcp`, `DeltaTcpNet` | `references/delta_api.md` |
| Inovance Modbus | `InovanceSerial`, `InovanceSerialOverTcp`, `InovanceTcpNet` | `references/inovance_api.md` |
| Inovance extended | `InovanceComputerLink`, `InovanceConnectedCipNet`, `InovanceEasyNet` | `references/inovance_computerlink_api.md`, `references/inovance_connectedcip_api.md`, `references/inovance_easynet_api.md` |
| Fatek | `FatekProgram`, `FatekProgramOverTcp` | `references/fatek_program_api.md` |
| Fuji SPB | `FujiSPB`, `FujiSPBOverTcp` | `references/fuji_spb_api.md` |
| Fuji SPH | `FujiSPHNet` | `references/fuji_sph_api.md` |
| Fuji Command Setting Type | `FujiCommandSettingType` | `references/fuji_commandsettingtype_api.md` |
| GE SRTP | `GeSRTPNet` | `references/ge_srtp_api.md` |
| Lsis Cnet | `LSCnet`, `LSCnetOverTcp` | `references/lsis_cnet_api.md` |
| Lsis Fast Enet | `LSFastEnet` | `references/lsis_fastenet_api.md` |
| Keyence Nano | `KeyenceNanoSerial`, `KeyenceNanoSerialOverTcp` | `references/keyence_nano_api.md` |
| Keyence KV Old | `KeyenceKvOld` | `references/keyence_kvold_api.md` |
| Keyence MC | `KeyenceMcNet`, `KeyenceMcAsciiNet` | `references/keyence_mc_api.md` |
| MegMeet | `MegMeetSerial`, `MegMeetSerialOverTcp`, `MegMeetTcpNet` | `references/megmeet_api.md` |
| Panasonic Mewtocol | `PanasonicMewtocol`, `PanasonicMewtocolOverTcp` | `references/panasonic_mewtocol_api.md` |
| Panasonic MC | `PanasonicMcNet` | `references/panasonic_mc_api.md` |
| ToyoPuc | `ToyoPuc` | `references/toyo_puc_api.md` |
| Vigor | `VigorSerial`, `VigorSerialOverTcp` | `references/vigor_api.md` |
| XinJE | `XinJESerial`, `XinJESerialOverTcp`, `XinJETcpNet` | `references/xinje_api.md` |
| XinJE internal TCP | `XinJEInternalNet` | `references/xinje_internal_api.md` |
| Digitron CPL | `DigitronCPL`, `DigitronCPLOverTcp` | `references/digitron_cpl_api.md` |
| Memobus | `MemobusTcpNet`, `MemobusUdpNet` | `references/memobus_api.md` |
| Yokogawa Link | `YokogawaLinkTcp` | `references/yokogawa_link_api.md` |
| DLT645 | `DLT645`, `DLT645OverTcp` | `references/dlt645_api.md` |
| DLT645-1997 | `DLT645With1997`, `DLT645With1997OverTcp` | `references/dlt645_1997_api.md` |
| DLT698 | `DLT698`, `DLT698OverTcp`, `DLT698TcpNet` | `references/dlt698_api.md` |
| Siemens S7 | `SiemensS7Net` | `references/siemens_s7_api.md` |
| Siemens S7 Plus | `SiemensS7Plus` | `references/siemens_s7plus_api.md` |
| Siemens Fetch/Write | `SiemensFetchWriteNet` | `references/siemens_fetchwrite_api.md` |
| Siemens PPI | `SiemensPPI`, `SiemensPPIOverTcp` | `references/siemens_ppi_api.md` |
| Siemens WebApi | `SiemensWebApi` | `references/siemens_webapi_api.md` |

If the user asks for an unsupported protocol, say this skill does not support
it yet and do not generate guessed code.

## Required inputs

### Sample or connection-test output

Collect these concrete values before writing the code:

1. Protocol or PLC brand/model.
2. Transport values:
   - network: `ipAddress`, `port`
   - serial: `portName`, `baudRate`, `dataBits`, `stopBits`, `parity`
3. Device identity settings such as station, CPU number, network number, PLC
   number, or AMS NetId. Use only the values listed in the selected reference.
4. Target style: synchronous console/WinForms code or async code.
5. Either an address table file, or a list of
   `{ name, address, dataType, length }`.

For online monitoring, also ask for a known-existing address that can be read
periodically.

### Configurable or complete-project output

Collect the design information that affects the generated code:

1. Protocol class or stable protocol key, and whether the protocol is fixed or
   selected at runtime.
2. Configuration source and schema, such as `appsettings.json`, XML, a
   database, a remote configuration service, or an existing options object.
3. Configuration field names and types for IP/host, port, station, network or
   CPU identity, credentials, and timeouts.
4. Address-table source, health-check address or tag, retry/backoff policy,
   logging, and connection-status requirements.
5. Target framework and application style, such as console service, WinForms,
   WPF, ASP.NET Core, or a background worker.

The current IP address, port, station, and password are not required at
code-generation time in this mode. Generate configuration bindings or
placeholders and let the deployed application supply the values. Do not ask
the operator to confirm these values on every connection or retry.

## Workflow

1. Apply "Fast intent router" first and select the primary reference.
2. Identify whether the request is a runnable sample/test or configurable
   project code. Use the rules in "Connection input rules".
3. For generated PLC client code, load `references/common_behavior.md` and only
   the matching protocol reference. For Demo Gateway, transport, plugin,
   custom-protocol or Python tasks, use the selected guide first and load
   `common_behavior.md` only when needed.
4. If an address table is provided, parse it with a real spreadsheet parser for
   `.xlsx` or `.csv`. Do not fabricate columns or methods.
5. Generate code using only the APIs listed in the selected protocol reference.
6. Use `references/address_formats.md` for Modbus address syntax, and the
   selected reference's address table for non-Modbus protocols.
7. Check `OperateResult<T>.IsSuccess` before accessing `.Content`.
8. Open the transport using the selected client's correct method.
9. For tasks requiring periodic data reading, implementation using the 
   thread class `System.Threading.Thread` is preferred. Once the thread starts, 
   connect to the PLC immediately, then enter a `While` loop to read data, 
   reusing the PLC object within the loop. Only when the reading interval 
   is longer than two minutes is it recommended to establish a connection 
   at the start of each cycle and close the connection upon cycle completion.
10. When reading a large number of addresses, check whether these addresses are contiguous. 
   For protocols such as Mitsubishi MC protocol with partially contiguous 
   addresses (e.g., D100, D101, D105, D106, etc.), batch reading via the `Read(string, ushort)` 
   method is preferred. Verify that the read operation succeeds first, then parse the actual 
   data values. Use the `ByteTransform` property for data parsing  by default.
11. When outputting code, ensure good readability, clear logic, and appropriate comments.
12. Output the generated code and note that the project must reference
   HslCommunication.

When `ConnectServer()` fails, read `references/troubleshooting.md` and decide
the interaction from the current task:

- For a sample or interactive connection test, explain the most likely cause
  and ask the user to confirm or update the connection values before retrying.
- For configurable or complete-project code, do not automatically ask the user
  to confirm the connection values. The generated program should log or return
  the `OperateResult`, update its connection status, and follow the configured
  retry/backoff policy. Ask the current user again only if they explicitly want
  help diagnosing the failed connection or updating the configuration source.

For serial or UDP clients, inspect the read/write result instead.

## Code skeletons

- Single or timed read: use `examples/modbus_tcp_console.cs`.
- Address-table-driven read: use `examples/address_table_console.cs`.
- Batch raw byte read: see `examples/modbus_tcp_batch_read.cs`.
- Attribute-based read: see `examples/modbus_tcp_attribute_read.cs`.
- Packet logging: see `examples/modbus_tcp_logging_console.cs`.
- FTP server/client: use `examples/ftp_client_server_console.cs`.
- Python calling C# HslCommunication: use `examples/python_call_hsl_modbus.py`.
- Virtual PLC and protocol servers: use
  `references/virtual_plc_server_guide.md`.
- Creating/starting/stopping a virtual server inside HslCommunicationDemo: use
  `references/ai_gateway_guide.md` and `/api/operations`.
- Writing/starting/testing PLC-style rules inside a Demo virtual server: use
  `references/logic_program_guide.md` and `/api/logic/*`.
- Creating periodic or expression-driven values in a Demo virtual server or on
  a connected real device: use `references/data_simulation_guide.md` and
  `/api/operations`.
- Verifying several Demo operations together, or a regression that must clean up
  after itself: use `references/ai_gateway_guide.md` and `POST /api/scenario/run`
  (live progress via `GET /api/scenario/status`).

Copy the appropriate example and replace connection values, addresses, and tags
with the selected protocol. These examples use literal values so they can run
immediately. When the user asks for configurable or complete-project code,
replace those literals with configuration fields or placeholders instead of
requiring the current deployment values.

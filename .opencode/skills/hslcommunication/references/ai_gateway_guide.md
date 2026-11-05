# HslCommunicationDemo AI Gateway guide

Load this guide when HslCommunicationDemo is running on this machine and the user asks the AI to
**operate the Demo**: read or write a connected device, create/start/stop a virtual server, drive a
client page, import a point table, run a logic program or data simulation, inject faults, verify an
upper computer through the traffic log, or run a multi-step regression. For *generated* PLC client
code this is the wrong guide - use a protocol reference instead.

The Demo contains a built-in HTTP/JSON AI gateway (`AIGatewayService.cs`, built on the
HslCommunication `HttpServer` class). It starts with the Demo when AI Server auto-start is enabled;
otherwise the user opens the AI Server page and presses Start. The default address is:

```text
http://127.0.0.1:8731/
```

The user may configure another host/IP/port - never assume the default URL when one is given. The
menu label may read `AI MCP Server` in some builds; the endpoints below are what this workspace
implements. The gateway sets `IsCrossDomain = true`, so no CORS setup is needed. It exists only in
the Demo build that ships `AIGatewayService.cs`; a stock binary does not start it.

Three rules summarized once here; each is detailed in the section named in the Contents table:

1. **Operate the Demo through this HTTP API, never through the desktop.** No mouse coordinates, no
   clicking on screenshots, no RPA/`SendKeys`. The gateway opens or reuses the original Demo window
   and calls its original button actions in-process.
2. **Switch the visible tab before each testing stage** (`/api/ui/tabs`), because the user cannot
   follow work that happens on a page that is not in front.
3. **`{base}` means the resolved gateway URL**, for example `http://127.0.0.1:8731/`.

## Contents

Section names below are the exact `##` headings; search for the title to jump.

| Section | Covers |
| --- | --- |
| Intent routing: choose the API before reading details | The endpoint decision table for this whole guide. |
| Choose an HTTP-capable tool first | Pick a real HTTP tool on the Demo host before calling anything. |
| Resolve the gateway URL | Default `127.0.0.1:8731` vs a user-supplied address. |
| Web API connection failure retry flow | What to do when the gateway itself does not answer. |
| Device read/write failure: retry once or report address error | Distinguish a transport failure from a wrong address. |
| Run on the Demo host, not inside a sandbox | The gateway binds to loopback; a sandbox cannot reach it. |
| Reading device data needs no confirmation gate | Read freely; only writes need the AI Server switch. |
| Step 1: verify the gateway | `GET /api/health` |
| Step 2: list connected devices | `GET /api/devices` (connected devices + unconnected client pages) |
| Step 3: get the current address table | `GET/POST /api/points` |
| Step 3.1: import a connected device's point table | `POST /api/points/import` (`replace`/`append`/`update`/`upsert`) |
| Step 3.2: get the device instantiation example code | `GET/POST /api/devicecode` |
| Generate read/write code from a Demo device | Compose code from `/api/devicecode` + `/api/points` |
| Step 4: read one value | `GET/POST /api/read` |
| Step 4.1: write one value or an array | `POST /api/write` and the write switch |
| Step 5: periodic collection and analysis | `POST /api/collect` then `GET /api/collection` |
| High-frequency reads: use collect, never loop read | Why an agent loop of `/api/read` is wrong |
| Virtual PLC server automation | `virtualServer.*`, the type list, reuse/`requestedNotApplied` |
| Client device page automation (`client.*`) | `client.create`/`connect`/`remove`, page ids, error codes |
| Manage the tags of a Beckhoff ADS virtual server | `virtualServer.tags.list` / `add` / `remove`, array length semantics |
| Browse the tag names that live inside the PLC (`client.browseSymbols`) | Beckhoff ADS: switch to the 浏览变量 tab, upload the symbol table, return the tag list |
| Get the operation queue (`GET /api/activity`) | Every request the gateway handled, newest last |
| Poll upper-computer traffic (`GET /api/traffic`) | Frame log of a virtual server, `afterSeq` polling |
| 报文日志分析 / decoded address tail (`DeviceServer.AnalysisLogMessage`) | The ` --- Read("D100", 2)` tail: default, parsing, on/off |
| 保存列表：保存 / 打开 / 删除已保存的页面 (`page.*`) | `page.save` / `page.list` / `page.open` / `page.delete`: the AI equivalent of the top-right 保存 button, double-clicking a saved entry, and right-click 删除设备 |
| Coding workflow for agents | The end-to-end recipe for curve/collection tasks |
| Gateway behavior and limitations | Permissions, clamping, what the gateway does **not** do |
| Fault injection of a virtual server | `virtualServer.faultInjection.*`, transport-specific faults |
| Scenario automation: many verified steps in one call | `/api/scenario/run`, steps, assertions, teardown, reports |
| Switching tabs at the start of every testing stage (`GET/POST /api/ui/tabs`) | List/switch tabs, title matching, error codes |
| Driving a page's logic program (`POST /api/ui/logic`) | Logic program on a client page vs `/api/logic/*` |
| Seeing the Demo: screenshots for the agent | `GET /api/windows`, `GET /api/screenshot` |

## Intent routing: choose the API before reading details

Read this table first and select the exact endpoint for the user request:

| User intent | Endpoint | Key parameters |
| --- | --- | --- |
| Check whether the Demo gateway is online | `GET {base}/api/health` | none |
| Show Demo devices: connected ones plus client pages that are open but not connected (`connected`, `pageOnly`) | `GET {base}/api/devices` | none |
| Save the open server/client page into the Demo save list (保存列表) | `POST {base}/api/operations` | `page.save`, `page` (clientId/serverId/title), `name` |
| List the Demo save list | `POST {base}/api/operations` | `page.list` |
| Re-open a saved page by name (like double-clicking it) | `POST {base}/api/operations` | `page.open`, `name` |
| Delete a saved page (or a whole saved group) | `POST {base}/api/operations` | `page.delete`, `name`/`guid`, `prefix: true` for a group |
| Discover gateway operations and virtual-server types | `GET {base}/api/capabilities` | none |
| List AI-created and UI-opened virtual servers | `GET {base}/api/servers` | none |
| Poll upper-computer read/write frames against a virtual server | `GET {base}/api/traffic` | `serverId`, optional `afterSeq`, `limit` |
| See and drive the Demo window itself (tabs, screenshots, scenario monitor) | `/api/windows`, `/api/ui/tabs`, `/api/screenshot` | `window`, `device`/`serverId`, `title`/`index` |
| Run many verified steps in one call, or a self-cleaning regression | `POST {base}/api/scenario/run` | `steps`, `teardown`; progress via `GET /api/scenario/status` |
| See what the agent just did, in order | `GET {base}/api/activity` | none |
| Understand the operation catalog of a subsystem | `GET {base}/api/logic/schema`, `/api/scenario/schema`, `/api/faultinjection/schema` | none |
| Create/start/stop a virtual PLC server or operate its points | `POST {base}/api/operations` | `operation`, `parameters` |
| Write, start, stop or inspect a virtual-server logic program | `/api/logic/*` | `serverId`, `program` |
| Test a client against timeouts, drops, bad frames or disconnects | `POST {base}/api/operations` | `virtualServer.faultInjection.*`, `serverId`, `types` |
| Store, list, read or delete a callable sub program of a virtual server | `/api/logic/programs*` | `serverId`, `name`, `program` |
| Import/list/export/start/stop virtual-server data simulation | `POST {base}/api/operations` | `virtualServer.simulate.*`, `serverId`, `items` |
| Import/list/export/start/stop connected-device data simulation | `POST {base}/api/operations` | `device.simulate.*`, `deviceId`, `items` |
| Get the current Demo address table of one device | `GET/POST {base}/api/points` | `device`/`deviceId`/`guid`/`clientId` |
| Replace/add/update/upsert a connected device's point table | `POST {base}/api/points/import` | `device`/`deviceId`/`guid`/`clientId`, `mode`, `points` |
| Get the instantiation example code of one device | `GET/POST {base}/api/devicecode` | `device` |
| Generate read/write code for a Demo device | `/api/devicecode` first, then `/api/points` | `device` |
| Read one value right now (device page or AI-created client page) | `GET/POST {base}/api/read` | `device` = guid from `/api/devices` **or `clientId` from `client.create`**, `address`, optional `type` (default `short`) |
| Write one value or array right now (device page or AI-created client page) | `POST {base}/api/write` | `device` = guid from `/api/devices` **or `clientId` from `client.create`**, `address`, `value`, optional `type` (default `short`) |
| Read/write a client page by its own id | `POST {base}/api/operations` | `client.read` / `client.write`, `clientId`, `address`, `type`, `value` |
| List the PLC's own tags (Beckhoff ADS) | `POST {base}/api/operations` | `client.browseSymbols`, `clientId`, optional `switchTab` |
| Collect values periodically (100 ms, 1 s, etc.) | `POST {base}/api/collect` | `device`/`deviceId`/`guid`/`clientId`, `address`, optional `type`/`intervalMs`/`seconds` |
| Get the collected dataset and analysis | `GET {base}/api/collection` | `id` = the `collectionId` from `/api/collect` |
| Stop an active collection | `GET/POST {base}/api/stop` | `id` = the `collectionId` from `/api/collect` |
| Connection/Web API failure | See the failure retry flow below | update gateway URL from user |
| Device read/write failure | See the retry/address-error decision below | inspect the returned `message` |

`{base}` is the resolved gateway URL, for example
`http://127.0.0.1:8731/`.

Rules for choosing quickly:

- One-time value read: use `/api/read`.
- One-time value write: use `/api/write`; it requires the Demo
  `Allow write plc data?` setting to be enabled.
- Reading or writing through a page you created with `client.create`: either
  `client.read` / `client.write` with that page's `clientId`, or `/api/read` /
  `/api/write` with the same id passed as `device`. The two routes share one
  implementation, so the same `address` / `type` / `value` work on both; only
  `client.read` and `/api/read` skip the write switch.
- Virtual PLC testing without real hardware: call `/api/capabilities`, then
  create and operate the server through `/api/operations`.
- Writing or testing internal PLC-style logic in a Demo virtual server: load
  `references/logic_program_guide.md` and use `/api/logic/schema`,
  `/api/logic/upload` (response includes static-validation `checks`),
  `/api/logic/start`, `/api/logic/status` (response includes structured
  execution `trace`, `subPrograms` and `pendingDelays`) and `/api/logic/log`.
  Store a `call` target with `/api/logic/programs/save` before uploading the
  caller. Do not use mouse automation or invent actions outside the documented
  DSL.
  - Verifying the timeout, retry, reconnect or frame-validation logic of a host
    application: use the fault injection of a virtual server,
    `virtualServer.faultInjection.start/stop/status/reset` through
    `/api/operations`, and read `GET {base}/api/faultinjection/schema` first.
    The faults are injected by the server itself, so the client keeps using the
    normal port.
- Creating step, ramp, sine, random or incrementing data for a virtual server:
  load `references/data_simulation_guide.md` and use
  `virtualServer.simulate.list/import/export/start/stop`.
- Creating the same periodic data on an already connected real device: load
  `references/data_simulation_guide.md` and use
  `device.simulate.list/import/export/start/stop` with the `deviceId` returned
  by `/api/devices`. This writes through the original Demo device connection.
- If the sentence contains Demo/HslCommunicationDemo plus
  create/open/start/stop virtual PLC/server, this rule always wins over the
  normal virtual-server code-generation guide. The Demo gateway opens or
  reuses the original server window; no desktop automation is required.
- Address table exists in Demo: call `/api/points` first and use the returned
  points instead of guessing addresses.
- Import or merge points into a connected device's table: use
  `/api/points/import`, not `virtualServer.points.import`.
- Device instantiation code is needed: call `/api/devicecode` and use the
  returned `code` text.
- The user asks for generated code based on a Demo device: call
  `/api/devicecode` first, then `/api/points`, then generate the code from the
  returned device code and point definitions.
- Repeated reads or curve/analysis: use `/api/collect`, never repeat
  `/api/read` in an agent loop.
- Every collection starts with one `/api/collect`, then one delayed
  `/api/collection` call after the requested duration.

## Choose an HTTP-capable tool first

The Demo gateway is a plain HTTP/JSON API. `Invoke-RestMethod`, `curl`, and
other commands in this guide are host command examples, not tool names.

Before calling the gateway, inspect the available tools and choose a real tool
that can reach the local Demo host:

- A generic HTTP request tool.
- A URL/WebFetch tool running on the host or with access to the local address.
- A host terminal or shell with `curl`, PowerShell, or another HTTP client.
- A local code runner that executes on the host machine.

If the first tool reports a parameter error, do not keep retrying with the same
parameters. Inspect the tool schema, then map the HTTP request to a different
available tool:

| HTTP concept | Tool input |
| --- | --- |
| Method | `GET` or `POST` |
| URL | `http://127.0.0.1:8731/...` |
| Query parameters | Part of the URL |
| POST body | JSON text |
| Headers | `Content-Type: application/json; charset=utf-8` for POST |

### UTF-8 encoding for Chinese or non-ASCII content

The gateway decodes POST bodies as UTF-8. If the JSON body contains Chinese
characters (or any non-ASCII text such as point names, descriptions, logic
program names, comments), you MUST send the body as UTF-8 bytes, not as a raw
string. Otherwise Chinese characters arrive as `??` or `??????` on the server.

PowerShell (`Invoke-RestMethod`) sends a string body using ISO-8859-1 by
default, which destroys Chinese. Convert the JSON to a UTF-8 byte array:

```powershell
$json = '{"operation":"virtualServer.points.import","parameters":{"serverId":"GUID","mode":"replace","points":[{"name":"温度","address":"D100","type":"short","description":"工位1温度"}]}}'
$bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:8731/api/operations' `
    -ContentType 'application/json; charset=utf-8' -Body $bytes
```

`curl`, Python `requests`, and most HTTP clients already default to UTF-8 and
do not need this conversion. Only PowerShell string bodies are affected.

Example after `/api/devices` returns the device GUID:

```text
GET http://127.0.0.1:8731/api/read?device=DEVICE_GUID&address=D200&type=short
```

Do not call a tool named `Invoke-RestMethod` unless that exact tool exists in
the current agent.

## Resolve the gateway URL

Use the gateway address from the user request when one is provided; otherwise
use the default:

```text
http://127.0.0.1:8731/
```

After `/api/health` succeeds, read the returned `baseUrl` field and use that
value for every subsequent request. Display the resolved address in the read
target block. Do not ask the user again unless the health check fails or the
address is ambiguous.

## Web API connection failure retry flow

Run the `/api/health` request directly on the Demo host first. When the Web API
test fails, do not keep retrying the same address silently and do not report a
device failure. Show the user a short connection prompt:

```text
Demo AI Gateway connection failed.
Current address: http://127.0.0.1:8731/

Need to change the Demo server address and try again?
```

Wait for the user's answer. If the user provides a new Demo server address,
update the gateway URL used by this task to the user-supplied value, then run
`/api/health` again against the new address. If the health check succeeds,
continue with `/api/devices` and the normal read workflow.

Do not invent a new port or address. If the user only confirms the current
address, retry once with the current address. If the retry still fails, ask the
user to check that Demo is running, the AI Gateway service started, and that
the port is not occupied.

When the original request was to create/start a virtual server inside the
Demo, do not fall back to generated C# server code or desktop GUI automation
after an AI Gateway connection failure. Report the gateway failure and fix the
gateway connection instead.

## Device read/write failure: retry once or report address error

The `/api/read` and `/api/write` responses return the underlying
HslCommunication error in `message`. Classify this message before retrying.

### Retry once for a clear connection-loss error

Retry the exact same read or write request once when `message` contains one of
these connection-loss fragments:

```text
远程主机强迫关闭了一个现有的连接
远程关闭了连接
当前的连接不可用
An existing connection was forcibly closed by the remote host
Remote shutdown of connection
The current connection is not available
```

This commonly happens when a PLC closes an idle connection. Wait about
200-500 ms before the single retry when the host tool supports delay. Keep the
same device GUID, address, type, length, and value. If it succeeds, report that
the connection was re-established. If it fails again, report the connection
error and stop retrying.

`Receive Data Timeout` / `接收数据超时` may be retried once only when the same
device and address previously worked in the current session and the message is
not an address-format error. Do not use a timeout alone as proof that the
network is faulty.

### Do not retry address parsing failures

Address errors are deterministic. Do not retry the same address and do not
guess or silently change it. Return the exact `message` to the user, then ask
them to check the selected protocol's address examples.

The exact text depends on the configured `StringResources.Language`, so match
either the Chinese or English fragments below rather than a single UI language.

The source emits these address-error forms:

```text
Address[<address>]: 输入的地址解析失败，可能是该地址格式不支持或是没有输入正确的地址，请重新输入
Parse [<address>] failed: <exception message>
输入的地址解析失败，可能是该地址格式不支持或是没有输入正确的地址，请重新输入
The entered address fails to be resolved because the address format is not supported or the correct address is not entered. Please try again
```

Specific range or format checks use messages such as:

```text
地址偏移量必须为偶数
当前的位偏移地址必须为16的倍数，例如M0，M16，X20
输入的位地址只能在0-15之间
位读写的起始地址需要为16的倍数，示例：R0.0, R2.0, L3.0, Y4.0
地址值在起始地址为1的情况下，必须大于1
地址值必须小于65536，不能超出modbus的理论范围
地址信息不能为空或是长度为0
地址信息长度不能大于12
地址格式配对失败，请检查是否是少于12个字，且都是0-9或A的数字组成的地址
```

When an address error occurs:

1. Show the original request address and the exact error `message`.
2. If the Demo point table is available, call `/api/points` and compare the
   requested address with the configured point address.
3. Load the selected protocol reference and identify the correct area prefix,
   numeric base, bit syntax, and range.
4. Report the specific reason, such as unsupported area, wrong base, invalid
   bit number, odd byte offset, or out-of-range address.
5. Ask the user to confirm the corrected address. Do not silently replace it
   with a guessed address.

An `Unsupported type: <type>` response is a data-type error, not an address
parse error. Ask the user to correct the requested type instead of retrying the
address.

## Run on the Demo host, not inside a sandbox

The default gateway binds to `127.0.0.1`, so it is reachable only from the host
machine where HslCommunicationDemo is running. If the user configured a
different gateway address, run the requests on that same reachable host or
network. A containerized code runner, remote cloud sandbox, or virtualized
network sandbox normally cannot reach a local gateway.

Prefer execution tools that run directly on the Demo host:

- Host shell/terminal execution.
- Host PowerShell with `Invoke-RestMethod`, or shell with `curl`.
- Any agent tool marked as host or local execution mode.
- Any generic HTTP request tool that can reach the local gateway address.

Do not keep retrying inside a failed sandbox. If the first request fails with a
connection or network sandbox error, switch to host execution immediately and
repeat the same request there.

For an agent that can only ask the user to run commands, provide host
PowerShell commands such as:

```powershell
Invoke-RestMethod -Uri 'http://127.0.0.1:8731/api/health'
Invoke-RestMethod -Uri 'http://127.0.0.1:8731/api/devices'
```

## Reading device data needs no confirmation gate

Show the resolved read target for transparency, but do not wait for approval
before executing the read:

```text
Demo AI Gateway: http://127.0.0.1:8731/
Demo device: S7-1200-01
Device type: SiemensS7Net
Device description: SiemensS7Net[192.168.1.10:102]
PLC address to read: M100
Data type: short
Read length: 1
```

Use the `baseUrl` returned by `/api/health` and the device fields returned by
`/api/devices`. Only ask the user when the target is ambiguous, for example
when multiple connected devices exist or the address/type was not given in the
request.

## Step 1: verify the gateway

The examples in this guide use the default `http://127.0.0.1:8731/`. When the
user provides a different gateway address, replace the base URL in every
request with the resolved address.

```text
GET http://127.0.0.1:8731/api/health
```

Response:

```json
{
  "success": true,
  "service": "HslCommunicationDemo AI Gateway",
  "baseUrl": "http://127.0.0.1:8731/",
  "started": true,
  "writeEnabled": false,
  "deviceCount": 0
}
```

If the request fails, follow the Web API connection failure retry flow above:
ask the user whether to change the Demo server address, update the URL from
the user's answer, and retry. If the running Demo does not include the AI
Gateway source change, fall back to normal generated PLC client code or the
Demo remote-debug pipe pattern.

## Step 2: list connected devices

```text
GET http://127.0.0.1:8731/api/devices
```

Response:

```json
{
  "success": true,
  "count": 1,
  "devices": [
    {
      "guid": "form-guid-value",
      "name": "S7-1200-01",
      "deviceType": "SiemensS7Net",
      "description": "SiemensS7Net[192.168.1.10:102]"
    }
  ]
}
```

Always call `/api/devices` before reading. Prefer the `guid` value as the
device parameter. Names can be empty when the user has not saved the Demo page.

## Step 3: get the current address table

When the Demo page contains a configured data table, return its points:

```text
GET http://127.0.0.1:8731/api/points?device=FORM_GUID
```

Response:

```json
{
  "success": true,
  "device": "S7-1200-01",
  "deviceType": "SiemensS7Net",
  "count": 2,
  "points": [
    {
      "name": "M100_short",
      "address": "M100",
      "type": "short"
    },
    {
      "name": "DB1_value",
      "address": "DB1.0",
      "type": "int",
      "length": 1,
      "unit": "mm",
      "description": "position value"
    }
  ]
}
```

`length` is omitted when the Demo row has no array length. `encoding` is added
for `string` points. If the device has no associated table, the gateway returns
an error message; in that case ask the user to add points in the Demo data
table first.

Use the returned points as the address source for `/api/read` and
`/api/collect`. Never replace them with guessed addresses.

## Step 3.1: import a connected device's point table

This endpoint changes the address table of a device already registered by the
Demo. It is different from `virtualServer.points.import`, which changes a
virtual server's own table.

```json
{
  "device": "FORM_GUID",
  "mode": "replace",
  "points": [
    {
      "name": "M100_short",
      "address": "M100",
      "type": "short",
      "length": -1,
      "unit": "",
      "description": "",
      "defaultValue": "123"
    }
  ]
}
```

Supported modes:

- `replace`: replace the complete device point table.
- `append` or `add`: append the supplied points.
- `update`: update points by `address`; fail without applying changes if any
  supplied address does not exist.
- `upsert`: update existing addresses and append missing addresses.

The response reports `count`, `added`, `updated` and `replaced`. The gateway
refreshes the Demo DataTableControl on the UI thread, so the imported rows are
visible in the device's data-table tab after the call.

The device may be identified with `device`, `guid` or `deviceId` (all four lookups below accept any of the three). Point-name labels (`pointName`) are resolved by the virtual-server operations only (`virtualServer.points.read/write`); the device-side endpoints take an explicit `address` and `type`. The points
array may be top-level or inside `parameters`; `items` is also accepted as an
alias for `points`. Prefer the example above with top-level `device`, `mode`
and `points`.

Always inspect the JSON `success` and `verified` fields. HTTP status `200` does
not prove that rows were imported. When `success=false`, report its `message`
and correct the request; do not tell the user that the point table was written.

This is a Demo configuration mutation and requires the AI Server write switch.

## Step 3.2: get the device instantiation example code

When the Demo page provides a code example for the connected device, return its
current C# instantiation code:

```text
GET http://127.0.0.1:8731/api/devicecode?device=DEVICE_GUID
```

Response:

```json
{
  "success": true,
  "device": "Melsec-01",
  "deviceType": "MelsecMcNet",
  "language": "csharp",
  "code": "MelsecMcNet plc = new MelsecMcNet(\"192.168.1.10\", 6000);"
}
```

Use the `code` value as the constructor/instantiation template when generating
a separate program. Treat the returned code as authoritative and do not change
its protocol class, IP address, port, or device properties unless the user
confirms a correction or there are obvious errors. If the device has no associated code control, the
gateway returns an error and the user should open the code example tab in Demo.

## Generate read/write code from a Demo device

When the user wants generated read/write code based on a device connected in
Demo, use this order:

1. Call `/api/devices` and resolve the device GUID.
2. Call `/api/devicecode` and keep the returned `code` as the authoritative
   instantiation code for that device.
3. Call `/api/points` to get the real point names, addresses, and data types
   when the Demo data table is configured.
4. Generate the read/write code by combining the returned instantiation code
   with the returned points.

Do not recreate the protocol constructor from memory when `/api/devicecode`
returns a code value. The Demo code is the reference for the protocol class,
connection address, port, and any required properties.

Example generated flow:

```text
GET /api/devices
GET /api/devicecode?device=DEVICE_GUID
GET /api/points?device=DEVICE_GUID
```

Use the instantiation code as the class member or local declaration, then add
the read method such as `plc.ReadInt16(point.address)`. Preserve the Demo
address syntax and data types returned by `/api/points`.

## Step 4: read one value

`device` accepts a device guid from `GET /api/devices`, or a `clientId` returned by
`client.create` (i.e. an AI-created client page that was connected successfully). Both are read
through the exact same endpoint - there is no separate "client read" API. It does **not** accept a
`serverId`: virtual-server data is read through `virtualServer.points.read` (see the virtual-server
section), never `/api/read`.

GET example:

```text
GET http://127.0.0.1:8731/api/read?device=FORM_GUID&address=M100&type=short
```

POST example:

```json
POST http://127.0.0.1:8731/api/read

{
  "device": "FORM_GUID",
  "address": "M100",
  "type": "short",
  "length": 1
}
```

Response:

```json
{
  "success": true,
  "timestamp": "2026-09-03T10:00:00.1234567+08:00",
  "elapsedMs": 12,
  "device": "S7-1200-01",
  "address": "M100",
  "type": "short",
  "value": -123
}
```

`elapsedMs` is the time the Demo spent on the local device read operation, in
milliseconds. It excludes HTTP request handling, so it is the value to use when
deciding whether a slow or fluctuating result is caused by the PLC/network.
When calling `/api/read` repeatedly, keep a short history of `elapsedMs` and
only report a network problem when the value stays high or shows large spikes
while the same address works normally through other tools.

Supported `type` values:

| type | C# read method | meaning |
| --- | --- | --- |
| `bool` | `ReadBool` | Boolean value or array |
| `short`, `int16` | `ReadInt16` | 16-bit signed integer |
| `ushort`, `uint16` | `ReadUInt16` | 16-bit unsigned integer |
| `int`, `int32` | `ReadInt32` | 32-bit signed integer |
| `uint`, `uint32` | `ReadUInt32` | 32-bit unsigned integer |
| `long`, `int64` | `ReadInt64` | 64-bit signed integer |
| `ulong`, `uint64` | `ReadUInt64` | 64-bit unsigned integer |
| `float` | `ReadFloat` | Single-precision float |
| `double` | `ReadDouble` | Double-precision float |
| `string`, `string_ascii` | `ReadString` | ASCII string |
| `string_utf8` | `ReadString` | UTF-8 string |
| `string_unicode` | `ReadString` | UTF-16 string |
| `string_gb2312` | `ReadString` | GB2312 string |
| `bytes`, `bytearray`, `hex` | `Read` | Raw bytes returned as hex text |

## Step 4.1: write one value or an array

Remote write is disabled by default. The Demo AI Server page has an
`Allow write plc data?` checkbox backed by
`Program.Settings.AllowRemoteWriteAiServer`. The gateway exposes the current
state as `writeEnabled` in `/api/health` and service information responses.

`device` works exactly like `/api/read`: a device guid from `GET /api/devices`, or a `clientId`
returned by `client.create`. Writing to an AI-created client page uses the same endpoint and body.
It does **not** accept a `serverId` - write virtual-server data with
`virtualServer.points.write` instead.

Use `POST /api/write`; query-string writes are intentionally rejected:

```json
POST http://127.0.0.1:8731/api/write

{
  "device": "FORM_GUID",
  "address": "M100",
  "type": "short",
  "value": 123
}
```

Successful response:

```json
{
  "success": true,
  "timestamp": "2026-09-22T15:30:00.1234567+08:00",
  "elapsedMs": 8,
  "device": "S7-1200-01",
  "address": "M100",
  "type": "short",
  "value": 123
}
```

If remote write is disabled, the gateway returns:

```json
{
  "success": false,
  "code": "write_disabled",
  "message": "Remote write is disabled. Enable 'Allow write plc data?' in the Demo AI Server settings first."
}
```

The write type names are the same as `/api/read`:

| type | JSON value examples |
| --- | --- |
| `bool` | `true`, `false`, `[true, false]` |
| `short`, `int16` | `123`, `[1, 2, 3]` |
| `ushort`, `uint16` | `123`, `[1, 2, 3]` |
| `int`, `int32` | `123`, `[1, 2, 3]` |
| `uint`, `uint32` | `123`, `[1, 2, 3]` |
| `long`, `int64` | `123`, `[1, 2, 3]` |
| `ulong`, `uint64` | `123`, `[1, 2, 3]` |
| `float` | `1.25`, `[1.25, 2.5]` |
| `double` | `1.25`, `[1.25, 2.5]` |
| `string`, `string_ascii`, `string_utf8`, `string_unicode`, `string_gb2312` | `"HELLO"`; optional `length` and `encoding` |
| `bytes`, `bytearray`, `hex` | `[1, 2, 255]` or `"01 02 FF"` |

For string writes, `length` is optional. When supplied, the library pads or
expands the encoded value to that length. For byte writes, the JSON array or
hexadecimal string itself defines the write length.

Rules:

- Never invent an address. Use the address examples in the protocol reference,
  the address table supplied by the user, or ask the user for the exact Demo
  address first.
- For typed numeric values, `length` is the element count, not the byte count.
- For raw `bytes`/`hex`, `length` is the byte count.
- For strings, omit `length` for Siemens S7 so the library reads the leading
  length header automatically. Other protocols normally require an explicit
  byte `length`.
- When `type` starts with `string_`, the correct encoding is selected
  automatically. An explicit `encoding` parameter is optional.
- URL-encode the address when it contains characters such as `&`, `+`, `?`,
  or `%` in a GET request.

## Step 5: periodic collection and analysis

For a one-second M100 short collection over 60 seconds:

```text
POST http://127.0.0.1:8731/api/collect
```

```json
{
  "device": "FORM_GUID",
  "address": "M100",
  "type": "short",
  "intervalMs": 1000,
  "seconds": 60
}
```

Response:

```json
{
  "success": true,
  "collectionId": "c-20260903100000-1",
  "endpoint": "/api/collection?id=c-20260903100000-1"
}
```

Wait about `seconds` seconds, then call the collection endpoint once:

```text
GET http://127.0.0.1:8731/api/collection?id=c-20260903100000-1
```

Response contains samples and numeric analysis:

```json
{
  "success": true,
  "collectionId": "c-20260903100000-1",
  "running": false,
  "sampleCount": 60,
  "errorCount": 0,
  "samples": [
    { "t": "2026-09-03T10:00:00.1234567+08:00", "elapsedMs": 8, "value": -123 }
  ],
  "analysis": {
    "count": 60,
    "min": -128,
    "max": -100,
    "avg": -114.5,
    "stddev": 6.2
  }
}
```

The `samples` array is abbreviated in the example above. The real response
contains one object per successful sample, and each object includes `elapsedMs`
for the local read duration. When the user asks why a periodic read is slow or
fluctuating, inspect the `elapsedMs` sequence: sustained high values suggest a
PLC/network bottleneck, while values that jump after running normally for a
long time are worth reporting as a network or PLC event rather than assuming
the Demo read logic is the cause.

Stop a running collection:

```text
POST http://127.0.0.1:8731/api/stop
```

```json
{ "id": "c-20260903100000-1" }
```

The key is `id`, but the **value is the `collectionId`** returned by `/api/collect` - the two names
differ, so do not send `{"collectionId": ...}` and do not send a device guid. `POST /api/collect`
answers with the id and a ready-made `endpoint` for polling it:

```json
{
  "success": true,
  "collectionId": "c-20261015093315-8",
  "device": "plc1",
  "address": "D100",
  "type": "short",
  "intervalMs": 200,
  "seconds": 3,
  "endpoint": "/api/collection?id=c-20261015093315-8"
}
```

`GET /api/collection` takes the same `id`. Omitting it answers
`gw_param_missing: id is required` on either endpoint. A collection also stops on its own after
`seconds`, so `/api/stop` is only needed to end one early.

## High-frequency reads: use collect, never loop read

For a 100 ms periodic read, do not generate an agent loop that calls
`/api/read` once per sample. That pattern is slow and creates hundreds of
separate HTTP/thought steps. Always delegate the whole sampling session to the
Demo background collector:

```json
POST http://127.0.0.1:8731/api/collect

{
  "device": "FORM_GUID",
  "address": "M100",
  "type": "short",
  "intervalMs": 100,
  "seconds": 5
}
```

The agent should follow this exact procedure:

1. Ask the user for the collection duration when it was not provided. For
   testing and curve analysis, recommend 3..10 seconds instead of an unlimited
   100 ms loop.
2. Call `/api/collect` once with `intervalMs: 100`.
3. Do not poll `/api/collection` repeatedly while thinking. Wait once for
   about `seconds * 1000` milliseconds, then call `/api/collection` once.
4. If the returned `running` is still true, wait one additional second and call
   `/api/collection` again. Do not retry more than a few times.
5. Treat the returned `samples` array and `analysis` object as one dataset.
   Do not explain or reason about each 100 ms sample separately.
6. Generate the curve from `samples`, and use `analysis` for min, max, average,
   and standard deviation.

**Check `sampleCount` before reading `analysis`.** When every read in the window
failed, the response has `samples: []`, a non-zero `errorCount`, and **no
`analysis` object at all** (there is nothing to analyse). Read
`sampleCount`/`errorCount` first; if `sampleCount` is 0, diagnose the read failure
(the `errorCount` and the device error codes) instead of reporting a curve of
zero samples as a result.

For example, with `seconds: 5`, wait about 5 seconds before the first
`/api/collection` call. The expected sample count is approximately 50 for a
100 ms interval.

The current Demo gateway clamps `intervalMs` to a minimum of 100 milliseconds.
If the user asks for a period below 100 ms, tell them the current gateway
supports 100 ms or higher, and do not report a smaller sampling period.

## Virtual PLC server automation

The gateway can create and control virtual PLC servers without changing the
normal Demo UI workflow. The dedicated server types are:

| Type | Default port | Default address | Notes |
| --- | --- | --- | --- |
| `SiemensS7Server` | `102` | `M100` | DB1, DB2 and DB3 are built in; add other DB blocks with `dbBlocks`. |
| `ModbusTcpServer` | `502` | `100` | TCP and serial can run simultaneously. |
| `MelsecMcServer` | `6000` | `D100` | Set `isBinary=false` for ASCII mode. |
| `OmronFinsServer` | `9600` | `D100` | TCP and serial can run simultaneously. |

`/api/capabilities` currently reports 37 types. Besides the four above, the families are:

- Mitsubishi: `McA3CServer`, `McA4CServer`, `FxSerialServer`, `FxLinksServer` (A1E has no type of
  its own - it is the `ServerMode = 2` variant of the MC window behind `MelsecMcServer`).
- Omron: `OmronCipServer`, `OmronUdpServer` (FINS over UDP), `OmronHostLinkServer`,
  `OmronHostLinkCModeServer`.
- Rockwell/Allen-Bradley: `CipServer` (AllenBradleyServer), `PcccServer` (AllenBradleyPcccServer).
- Fuji: `FujiCSTServer` (Command Setting Type), `FujiSPBServer`, `FujiSPHServer`.
- Others: `KeyenceNanoServer`, `GeSRTPServer`, `CimonServer`, `PanasonicMewtocolServer`,
  `FatekProgrameServer`, `YokogawaLinkServer`, `InovanceEasyNetServer`, `MemobusTcpServer`,
  `VigorServer`, `XinJEInternalServer`, `BeckhoffAdsServer`, `LSisServer`,
  `RkcTemperatureControllerServer`, `TurckReaderServer`, `ToyoPucServer`, `DLT645Server`,
  `DLT645With1997Server`, `DLT698Server`.

Do not guess a type name: read `serverTypes[].type` from `/api/capabilities` and pass it back
unchanged.

`GET {base}/api/capabilities` is the authoritative list. It also returns other
Demo virtual server forms that follow the common
`ServerSettingControl + UserControlReadWriteServer + DeviceServer` structure.
These additional servers use the generic UI adapter: common TCP/UDP, local
address, IPv4/IPv6 and serial parameters are applied, while protocol-specific
controls keep their current Demo window values. Use the `type` value returned
by `/api/capabilities` when creating them.

A virtual server lives inside the Demo process and holds no state on disk, so it does **not** come
back after the Demo restarts: `GET /api/servers` is empty again and a re-created server gets a
**new `serverId`**. Never reuse a `serverId` (or a `device:<guid>` page id) across a restart - read
the current one from `/api/servers` / `client.list` in the same session before using it.

First call `GET {base}/api/capabilities`; it reports the supported parameters,
write permission state, and operation names. Call `GET {base}/api/servers` to
list servers already opened by the Demo UI or created through the API. Always
use the returned `serverId`; do not identify a server by its window title.

`virtualServer.create` opens the corresponding original Demo server window if
it is not already open. If a window of the same type is already open, the
gateway reuses it instead of creating another instance. Start, serial-start and
stop operations are executed through the original window controls, so the
normal Demo UI state, logging, address table and code example remain active.

For the request "在 Demo 程序里创建一个三菱虚拟服务器，端口号 6001，并启动
服务", the expected quick path is:

1. `GET {base}/api/health`.
2. `GET {base}/api/capabilities`.
3. `POST {base}/api/operations` with `virtualServer.create`.
4. Use `type: "MelsecMcServer"`, `tcpPort: 6001`, `startTcp: true`.
5. Report the returned `serverId`, the port that is really in use, and the
   original Demo window state.

Do not answer this request with generated server C# code, and do not simulate
mouse operations on the desktop.

**`create` reuses an open window of the same type.** If a window of that type is
already open - in particular when its server is already running - the gateway
reuses it and the port, serial port and other start-time settings in the request
**cannot be applied to the running server**. The response then carries:

```jsonc
{
  "success": true,
  "serverId": "...",
  "server": { "tcpPort": 502, ... },            // the values really in use
  "requestedNotApplied": {                       // only present on a mismatch
    "tcpPort": { "requested": 6001, "actual": 502,
                 "reason": "the network server is already running on port 502" }
  },
  "warning": "The server window was reused and the server is already running, so the parameters in requestedNotApplied did not take effect. ..."
}
```

Always read the port out of `server` (or check for `requestedNotApplied`) instead
of assuming the port you asked for is the port you got. To really use a different
port, stop the reused server first (`virtualServer.stop`) and create it again.
The same applies to `startTcp` on an already running server: it answers
`code: "already_started"` inside `tcpStart`.

To inject faults into an already running virtual server (delay, drop, corrupt,
disconnect) see "Fault injection of a virtual server" at the end of this guide.

All lifecycle and point mutations use one endpoint:

```json
{
  "operation": "virtualServer.create",
  "parameters": {
    "type": "SiemensS7Server",
    "name": "s7-test",
    "tcpPort": 102,
    "startTcp": true,
    "serialPort": "COM4-9600-8-N-1",
    "startSerial": false,
    "dbBlocks": [
      { "dbNumber": 10, "size": 1024 }
    ]
  }
}
```

`tcpPort` and `udpPort` must be inside **1..65535**. An out-of-range or
non-numeric value is rejected **before** the server window is opened or reused:

```json
{ "success": false, "message": "tcpPort must be between 1 and 65535." }
```

Do not work around that error by opening the window first: a port that reaches the
window's port box stays there, and the next time anything presses *start TCP*
the framework throws *"the specified argument is out of range"* and WinForms shows
it as a **modal** dialog - which blocks the Demo UI, and with it every later
gateway call, until a human dismisses it. Rejecting the value up front is what
keeps a bad port from ever reaching the control.

The same endpoint provides these operations:

```text
virtualServer.list
virtualServer.status
virtualServer.create
virtualServer.startTcp
virtualServer.startSerial
virtualServer.stopTcp
virtualServer.stopSerial
virtualServer.stop
virtualServer.remove
virtualServer.points.list
virtualServer.points.import
virtualServer.points.read
virtualServer.points.write
virtualServer.points.writeDefaults
virtualServer.clients.list
```

TCP and serial are independent. For example, after creating a Modbus or MC
server with `startTcp=true`, start a serial endpoint with:

```json
{
  "operation": "virtualServer.startSerial",
  "parameters": {
    "serverId": "SERVER_GUID",
    "serialPort": "COM4-9600-8-N-1"
  }
}
```

`virtualServer.stopSerial` closes only serial sessions. `virtualServer.stopTcp`
closes the network listener only when no serial session is running; if both are
running, stop serial first or call `virtualServer.stop`. `virtualServer.stop`
closes both; use `virtualServer.remove` when the automation-created server
should also be removed from the registry.

Point tables use the same types accepted by the Demo data table, such as
`bool`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`,
`string` and `byte`. Import points with:

```json
{
  "operation": "virtualServer.points.import",
  "parameters": {
    "serverId": "SERVER_GUID",
    "mode": "replace",
    "points": [
      { "name": "temperature", "address": "D100", "type": "short", "defaultValue": "123" },
      { "name": "run", "address": "M100", "type": "bool", "defaultValue": "true" }
    ]
  }
}
```

`virtualServer.points.import` only stores the row definitions. It does not write
any value into the server, so right after an import the address still holds whatever
the server had before (usually 0). Push the `defaultValue` of every row into the
server with `virtualServer.points.writeDefaults`, or write the addresses you
need with `virtualServer.points.write`.

`virtualServer.points.list` returns the definitions (`name`, `address`, `type`,
`length`, `unit`, `description`, `expression`, `defaultValue`) and no live
value; read a value with `virtualServer.points.read` (or
`virtualServer.points.write` to set one). Note: the device-side endpoints
`/api/read` / `/api/write` do **not** accept a `serverId` - they resolve only
device/client pages registered in `DemoDevice` (a guid from `/api/devices` or a
`clientId` from `client.list`). To read a virtual server's data, always use
`virtualServer.points.read` with the `serverId`.

Write a point and optionally verify it by reading the same virtual server
back:

```json
{
  "operation": "virtualServer.points.write",
  "parameters": {
    "serverId": "SERVER_GUID",
    "address": "D100",
    "type": "short",
    "value": 456,
    "verify": true
  }
}
```

All create/start/stop/remove/import/write/default-write operations require the
Demo AI Server write switch. Read, status and list operations do not require
that switch. If the user asks only to read from an existing server, proceed
without asking for a write confirmation.

`virtualServer.points.read` / `virtualServer.points.write` accept a label too:
pass `pointName` (alias `point`) with a name from `virtualServer.points.list`
instead of `address`/`type`. Matching is case-insensitive, by name or by
address, for example `{"serverId":"...","pointName":"temperature"}`. The
device-side `/api/read` / `/api/write` do **not** resolve labels - they need an
explicit `address` and `type`.


### Beckhoff ADS tags (`virtualServer.tags.*`)

A Beckhoff ADS virtual server keeps its tags inside the server itself (they are what a real PLC
project would provide), so the agent manages them through three operations instead of the generic
point table:

| Operation | Purpose | Read-only |
| --- | --- | --- |
| `virtualServer.tags.list` | List every tag: name, type, array length, byte size, current value | yes |
| `virtualServer.tags.add` | Create one or more tags with default values (0 / False) | no (needs the write switch) |
| `virtualServer.tags.remove` | Delete tags by name | no (needs the write switch) |

Add a single tag:

```json
{"operation":"virtualServer.tags.add","parameters":{"serverId":"<serverId>","name":"MAIN.counter","type":"int","length":0}}
```

`length` is the **array length**; `0` means a single value, `3` means `int[3]`. Supported types:
`bool`, `short`, `ushort`, `int`, `uint`, `float`, `long`, `ulong`, `double`.

Add several at once (each item is validated independently, failures are reported per item):

```json
{"operation":"virtualServer.tags.add","parameters":{"serverId":"<serverId>",
  "tags":[{"name":"MAIN.a1","type":"bool","length":0},{"name":"MAIN.f1","type":"float","length":2}]}}
```

Delete:

```json
{"operation":"virtualServer.tags.remove","parameters":{"serverId":"<serverId>","names":["MAIN.a1","MAIN.f1"]}}
```

Behaviour worth knowing:

- `add` replies with `added` / `failed` arrays plus the new `count`; a duplicate name or an unknown
  type is reported per item instead of failing the whole call.
- `remove` replies with `removed` / `missing`; deleting a name that does not exist is not an error by
  itself (`success` is false only when nothing was removed).
- Both operations **switch the page to the 标签列表 tab and refresh its grid**, so the user sees the
  change immediately.
- The tag list is stored in the page's XML (`<BeckhoffTags>`), so saving the page and loading it
  again restores the tags exactly (name / type / array length). Values are runtime data and start at
  their defaults after a restart.
- A tag is read or written from a Beckhoff client with `address = "s=" + name`.
- **Array elements are not addressable over ADS.** Read or write the array as a whole
  (`address = "s=MAIN.arr"` with `length = arrayLength`) and slice it yourself. `s=MAIN.arr[2]`
  answers `0x710 Symbol not found` over ADS on the virtual server; only the server-side direct
  API (`virtualServer.points.read` / `points.write`) accepts the `[n]` subscript.

## Client device page automation (`client.*`)

`virtualServer.*` drives the Demo's **virtual server** windows. The `client.*` family drives the
**device / client** pages - the forms you normally open by hand to talk to a real PLC. Use it when the
task is "open that protocol's client page, connect it, read/write a few values, then close it".

The page is created from a **device configuration XML**, i.e. exactly what the Demo itself writes when
it saves a device, and the parameters are applied by the page's own `LoadXmlParameter`. There is no
per-protocol adapter code, so the parameter list always matches the page.

| Operation | Purpose | Write switch needed |
| --- | --- | --- |
| `client.list` | every client device known to the Demo plus every AI-created page, and the creatable `types[]` | no |
| `client.params` | parameter list; with `clientId` it reads back the values an **open page** currently holds, plus the XML that page generates | no |
| `client.status` | one client's state; **`clientId` is required** - without it the call answers `gw_param_missing`, so use `client.list` to enumerate | no |
| `client.read` | read through this client: `clientId` + `address` + `type` (+ optional `length`, `encoding`) | no |
| `client.write` | write through this client: `clientId` + `address` + `type` + `value` (+ optional `length`, `encoding`) | yes |
| `client.create` | create the page from XML. **It never connects by itself** | yes |
| `client.connect` | run the page's own connect button and report the outcome | yes |
| `client.disconnect` | close the device connection | yes |
| `client.remove` | close the page, which also removes it from the device list | yes |

> **Two equivalent ways to read/write through a client page.** `client.read` / `client.write` and
> `POST /api/read` / `POST /api/write` run the **same implementation** (`DemoValueOperation`), so the
> same `address`, `type`, `value`, `length` and `encoding` behave identically. Use whichever fits:
>
> ```
> {"operation":"client.read","parameters":{"clientId":"<clientId>","address":"D100","type":"short"}}
> {"operation":"client.write","parameters":{"clientId":"<clientId>","address":"D100","type":"short","value":1234}}
>
> POST /api/read   { "device": "<clientId>", "address": "D100", "type": "short", "length": 1 }
> POST /api/write  { "device": "<clientId>", "address": "D100", "value": 1234, "type": "short" }
> ```
>
> - `client.read` is read-only and needs no write switch; `client.write` counts as a write and requires
>   `AllowRemoteWriteAiServer`.
> - `client.params` / `client.status` never read PLC data - they only report page configuration.
> - The device endpoints accept the page's `clientId` under any of the aliases
>   `guid` / `device` / `deviceId` / `clientId`, on `GET /api/read`, `POST /api/write`,
>   `GET /api/points`, `POST /api/points/import`, `GET/POST /api/devicecode` and `POST /api/collect`.
>   They resolve against the same registry as `client.list`.
> - **`GET/POST /api/stop` is the exception**: it identifies a *collection task*, not a device, and
>   takes `id` (the value returned by `/api/collect`). Passing a `clientId` there answers
>   `gw_param_missing: id is required`.
> - `GET /api/points` on a client page usually reports `success=true` with `count=0`, because a client
>   page normally has no configured address table. That is not evidence that the page has no points -
>   pass an explicit `address` to `client.read` / `client.write` instead of relying on that table.
> - A page that is open but not connected answers `cli_not_connected`; call `client.connect` first.

### Typical flow

1. `client.params` with `type` to learn that protocol's parameters, or copy the verified XML from the
   protocol reference for that family (the matching `references/*_api.md`).
2. `client.create` with `type` + `parameters`. Read `appliedParameters` in the response; if
   `requestedNotApplied` is present, the page stored something else - fix the request before connecting.
3. `client.connect`. `success` is true only when the device was registered, which happens only after a
   real successful connect.
4. Read/write with `client.read` / `client.write` (target `clientId`), or with `POST /api/read` /
   `POST /api/write` (target the same `clientId` under the alias `device`). Both routes share one
   implementation, so either is correct.
5. `client.disconnect`, then `client.remove`.

```json
{"operation":"client.create","parameters":{"type":"FormModbus","name":"plc1","parameters":{"IpAddress":"192.168.1.10","Port":"502"}}}
{"operation":"client.connect","parameters":{"clientId":"<clientId>"}}
{"operation":"client.read","parameters":{"clientId":"<clientId>","address":"D100","type":"short","length":1}}
{"operation":"client.write","parameters":{"clientId":"<clientId>","address":"D100","type":"short","value":1234}}
POST /api/read   { "device": "<clientId>", "address": "D100", "type": "short", "length": 1 }
POST /api/write  { "device": "<clientId>", "address": "D100", "value": 1234, "type": "short" }
{"operation":"client.disconnect","parameters":{"clientId":"<clientId>"}}
{"operation":"client.remove","parameters":{"clientId":"<clientId>"}}
```

### Naming and matching

- `type` accepts the form class name (`FormModbus`) or the display name without the `Form` prefix
  (`Modbus`). Matching ignores case, `_`, `-` and spaces. An ambiguous prefix is rejected rather than
  guessed - `Melsec` matches several forms and fails with `cli_type_not_found`.
- `clientId` (aliases: `guid`, `client`, `device`) accepts the guid or the page title.
- Parameters accept the XML attribute name (`IP`, `Station`) or an `Xml`-prefixed form
  (`XmlIpAddress`). `IpAddress` maps to `IP`, and `PortName` maps to `ComPort` **when the request uses a
  serial pipe or the page template already has `ComPort`**. Two keys that resolve to the same attribute
  are rejected with `cli_param_conflict` instead of one silently winning.

Some TCP pages (`FormModbus`, `FormMelsecBinary`, `FormSiemensS1200`) can be switched to a serial pipe,
but their saved template only contains the network fields. For those, pass `PipeSetting=SerialPipe`
together with `ComPort`; use `ComPort` rather than `PortName` to avoid depending on alias resolution.

### Three things worth knowing

- **A serial-capable TCP page needs its serial settings to match the peer.** `FormMelsecBinary`
  defaults to `9600-7-E-1`; a `MelsecMcServer` serial slave started as `COM5-9600-8-N-1` needs
  `DataBit=8` and `Parity=0`. With the mismatched defaults the client reports connected and the peer
  does answer, yet every read fails with a frame-header error - it reads like a protocol fault.
- **Read-only measurement points accept a write without error.** Meter protocols (DLT698, DLT645)
  expose measurements; `write.success` is true but the value is unchanged. Always confirm a write with
  a following read instead of trusting the write response.
- **`client.list` / `/api/devices` list every page the AI registry knows about: the connected
  devices plus the client pages created with `client.create` that are not connected yet
  (`connected:false`, `pageOnly:true`).** A page a human opened from the left device tree is
  **not** in either list, so `count: 0` is still not proof that nothing is open - confirm with
  `GET /api/windows` and look for a `device:<guid>` entry.
  Closing behaves the opposite way: `client.remove` closes the window, so `success:true` with
  `removed:true` really means the page is gone (the window disappears from `/api/windows`);
  `removed:false` means the close did not complete. A page the human opened cannot be addressed
  by this API at all - only the user can close it.
  The page guid is **regenerated when the Demo restarts**, so re-read it from `client.list`
  immediately before `client.remove` / `client.disconnect` instead of reusing an id from an earlier
  run - otherwise the call succeeds against a different page and silently leaves the target open.
  `client.create` returns the same id that later appears as `guid` in `client.list` and
  `/api/devices`, so within one Demo session those three values agree.

### Error codes

`gw_param_missing`, `gw_param_invalid`, `cli_type_not_found`, `cli_not_found`, `cli_not_connected`,
`cli_param_conflict`, `cli_template_failed`, `cli_create_failed`, `cli_no_connect_button`,
`cli_connect_failed`, `cli_disconnect_failed`, `cli_read_failed`, `cli_write_failed`.

`cli_not_connected` means the page exists but holds no live connection - call `client.connect` first.
`cli_read_failed` / `cli_write_failed` carry the protocol-level message (an unsupported `type`, an
address the peer rejects, and so on); correct the request instead of retrying it unchanged.

### Watching what the agent does

Reads and writes issued through `/api/read` and `/api/write` run on a **device page**, and the
HslCommunication device logs every send/receive to the `LogNet` the page received from the Demo
(`FormSaveList` assigns it when the page is created, and the page forwards it to its device). Those
entries go to the Demo's shared `LogNetSingle` with an empty file path, i.e. they are not written to
disk and are only visible through the UI. Open the Demo's **"报文日志" / "MsgLog"** menu item to display
them live; the window is not shown by default, and it only renders while its `ShowLog` flag is on.

Screen captures of a page are available through `GET /api/screenshot?window=device:<guid>` - the
`<guid>` being the `device:` entry reported by `GET /api/windows`, which is the page guid from
`client.list`.
### Browsing the PLC's own tags (`client.browseSymbols`)

Some protocols keep their tag names inside the device - Beckhoff ADS is the first one in this
Demo - so the agent must **ask the PLC** for the list instead of inventing names. On a
`BeckhoffAdsNet` page:

```json
{"operation":"client.browseSymbols","parameters":{"clientId":"<clientId>"}}
```

What it does, in order:

1. Resolves the connected client page and refuses anything that is not a `BeckhoffAdsNet` page
   (`cli_browse_unsupported`).
2. **Switches that page to the 浏览变量 / Browse Tag tab** so the user watches the result arrive
   (same rule as every other feature). Pass `"switchTab": false` to skip the switch.
3. Runs the same two uploads the tab's 浏览 button runs: `BrowseSymbolStructInfosAsync` (struct
   definitions) and `BrowseSymbolInfosAsync` (the tag table).
4. Refreshes the on-screen tree with the result, and returns the same data.

```jsonc
{
  "success": true, "clientId": "...", "device": "FormBeckhoffAdsNet (AI)",
  "count": 12, "structCount": 17, "elapsedMs": 16,
  "tab": { "title": "浏览变量", "activated": true },
  "symbols": [ { "name": "CoEModeSwitch.bBusy", "type": "BOOL", "size": 1 } ],
  "structs": [ { "name": "ST_Test", "type": "", "size": 8, "children": [] } ],
  "note": "Same data as clicking 浏览 on the 浏览变量 tab. Read a symbol with address 's=<name>' ..."
}
```

Rules:

- The returned `symbols[].name` is the **only** reliable tag name; read it with `client.read` and
  `address = "s=" + name` (for example `s=MAIN.a`). Guessing a name returns a symbol-not-found
  error from the PLC.
- `structs[]` carries `children` (struct members); `symbols[]` is flat and uses `type` to tell you
  which struct definition applies.
- Both arrays are capped (3000 / 1000 nodes, depth 6) so a huge tag table cannot blow up the
  response; browse again with a narrower look when you need more.
- It is **read-only**: no write switch needed. It does need the page to be **connected** first
  (`cli_not_connected`).
- Errors: `cli_not_found`, `cli_not_connected`, `cli_browse_unsupported` (not a Beckhoff page, or
  the page has no browse tab), `cli_browse_failed` (the PLC rejected the upload).



## Get the operation queue (`GET /api/activity`)

Every request the gateway handles is appended to an in-memory operation queue, newest last, holding
the most recent **100** steps. `GET /api/activity` returns that queue. Use it to see what the agent
has actually done on this Demo host - which pages it opened, which values it read or wrote, and which
calls failed - without needing to watch the UI.

```
GET /api/activity
```

No parameters, read-only, and it does not need the write switch. The queue is process-wide, so it
keeps filling even while no AI Server window is open, and it survives across requests until the Demo
is restarted.

```json
{
  "success": true,
  "count": 3,
  "capacity": 100,
  "activities": [
    {
      "seq": 1,
      "time": "2026-10-08 22:38:49.895",
      "method": "GET",
      "route": "api/health",
      "operation": "api/health",
      "target": "",
      "detail": "",
      "success": true,
      "code": "",
      "message": "",
      "elapsedMs": 24,
      "guid": ""
    },
    {
      "seq": 2,
      "time": "2026-10-08 22:38:50.765",
      "method": "POST",
      "route": "api/operations",
      "operation": "client.create",
      "target": "FormModbus",
      "detail": "IpAddress=127.0.0.1  Port=502",
      "success": true,
      "code": "",
      "message": "",
      "elapsedMs": 806,
      "guid": ""
    }
  ],
  "note": "Newest last. This is the same buffer the AI Server window shows on its AI activity tab."
}
```

| Field | Meaning |
| --- | --- |
| `count` | how many steps are currently queued (at most `capacity`) |
| `capacity` | queue size, fixed at `100` |
| `activities` | the queue, **oldest first**, newest last |
| `activities[].seq` | 1-based counter, keeps increasing; gaps at the front mean older steps were dropped |
| `activities[].time` | local time of the request |
| `activities[].method` | HTTP method |
| `activities[].route` | HTTP route, for example `api/operations` |
| `activities[].operation` | the operation that actually ran; for `/api/operations` this is the `operation` field from the body, for example `client.create` |
| `activities[].target` | what it acted on: device/server name with a readable description when it can be resolved, otherwise the raw id |
| `activities[].detail` | short parameter summary, for example `100 short  = 4321` or `port=5502` |
| `activities[].success` | whether the request reported success |
| `activities[].code` | error code on failure, for example `cli_not_found`, `cli_type_not_found`, `gw_param_missing` |
| `activities[].message` | error message on failure |
| `activities[].elapsedMs` | how long the request took |
| `activities[].guid` | guid of the affected device or server page, empty when the step does not target one |

Notes:

- The queue is bounded at 100 steps; once it is full the oldest entries are dropped, so `seq` of the
  first element rises over time.
- `target` prefers a readable name. When a step removes a page (`client.remove`,
  `virtualServer.remove`) the page is already gone by the time the step is queued, so `target` falls
  back to the raw guid - read the `guid` field there.
- Call it after a batch of operations to audit what the agent did, or before starting so you can
  compare the queue length afterwards.

## Poll upper-computer traffic (`GET /api/traffic`)

When an upper-computer program connects to a virtual server and reads/writes addresses, the server
logs every frame it receives and sends. `GET /api/traffic` exposes those frames so the agent can
observe *what the upper computer actually did* — which addresses it read, what values it wrote, and
whether it received the expected response — without watching the UI.

```
GET /api/traffic?serverId=5e12494110a241389a8f82a6a46caa94&afterSeq=0&limit=500
```

| Parameter | Required | Meaning |
| --- | --- | --- |
| `serverId` | yes | target virtual server, from `GET /api/servers` |
| `afterSeq` | no | return only events with `seq > afterSeq`. Use the `latestSeq` from the previous response to poll incrementally. Default `0`. |
| `limit` | no | max events to return. Default `500`; a value `<= 0` or `> 5000` silently falls back to `500` (it is **not** clamped to 5000). |

```json
{
  "success": true,
  "serverId": "5e12494110a241389a8f82a6a46caa94",
  "count": 2,
  "latestSeq": 1024,
  "afterSeq": 1000,
  "events": [
    {
      "seq": 1001,
      "time": "2026-10-12 22:30:15.123",
      "text": "<192.168.1.5:54321> 接收 : 00 01 00 00 00 06 01 03 00 64 00 02 --- Read(\"100\", 2)"
    },
    {
      "seq": 1002,
      "time": "2026-10-12 22:30:15.125",
      "text": "<192.168.1.5:54321> 发送 : 00 01 00 00 00 03 01 03 04 00 2A 00 00"
    }
  ],
  "note": "..."
}
```

How to read one line:

1. `<IP:port>` identifies the session; `接收` (Receive) = request from the upper computer;
   `发送` (Send) = response from the virtual server.
2. The hex body is the frame. A **`接收` line may end with ` --- <decoded tail>`** — see
   "报文日志分析" below. Split on ` --- ` first and decode only the left part; the tail is not
   part of the frame.
3. A `接收` followed by a `发送` within a few milliseconds is a matched request/response pair.
4. The buffer keeps the last **2000** frames per server; older frames are dropped.

Typical closed-loop workflow:

1. Start the virtual server and set up a logic program / point table.
2. Note the `latestSeq` (or call with `afterSeq=0` to start fresh).
3. Have the upper computer perform its action (e.g. write a recipe, poll a tag).
4. Call `GET /api/traffic?serverId=...&afterSeq=<prevLatestSeq>&limit=500`.
5. Inspect the frames: did the upper computer read the expected address? Did it write the expected
   value? Did the server respond? This verifies the upper computer's behavior end-to-end.

Decode raw hex yourself only when the tail is absent, or when you must check CRC, station, fragment
length or a byte the tail does not show. When the tail is present, the address and value are already
decoded - do not re-derive them from hex, and never report the tail as part of the frame.

Traffic capture is independent of the "日志" (log) checkbox: frames are recorded even when the UI log
view is off. The **analysis tail** is a different switch - the next section.

## 报文日志分析 / decoded address tail (`DeviceServer.AnalysisLogMessage`)

`AnalysisLogMessage` makes a protocol server append a decoded address line to every **received**
frame (send lines never get one). It is the fastest way to answer "did the upper computer read the
right address?" without decoding hex.

| Frame (left) | Tail (right) | Meaning |
| --- | --- | --- |
| `... 01 03 00 64 00 02` | `--- Read("100", 2)` | Modbus read, 2 registers at 100 |
| `... 01 06 00 64 04 D2` | `--- Write("100", (ushort)1234)` | Modbus write |
| `... 01 20 80 00 00 64 00 01` | `--- Read("D100", 1)` | MC / XinJE extension read |
| `... 0F 00 04 00 AA 02 07 89 01 00 D2 04` | `--- Write("N7:1", (ushort)1234)` | Allen-Bradley PCCC write |
| `... 01 03 00 64` | `--- ReadBool("M100", 1)` | bit read (protocol dependent) |

Rules that matter:

- **On by default** wherever it is implemented (the Demo checkbox "日志报文分析" starts checked).
  Do not assume the hex is all you get.
- **Parse defensively**: split `text` on `' --- '`; the left side is the frame, the right side the
  tail. There is at most one occurrence and only on `接收` lines.
- **Turning it off**: `virtualServer.create` accepts `analysisLogMessage: false`, but only for the
  five server types whose Demo window reads that parameter: `CipServer`, `MelsecMcServer`,
  `OmronFinsServer`, `OmronUdpServer`, `SiemensS7Server`. For every other type the parameter is
  ignored - only the window checkbox ("日志报文分析") changes it.
- **The value is sticky per window.** `virtualServer.create` reuses an open window of the same type,
  so a window that once ran with `analysisLogMessage:false` stays off; `virtualServer.stop` /
  `startTcp` do not reset it either. Pass the parameter explicitly whenever the value matters.
- **Not every server implements it.** 28 of the 37 virtual-server types do:
  `SiemensS7Server`, `MelsecMcServer` (3E binary/ASCII, and the A1E mode), `McA3CServer`,
  `McA4CServer`, `FxSerialServer`, `FxLinksServer`, `OmronFinsServer`, `OmronUdpServer`,
  `OmronHostLinkServer`, `OmronHostLinkCModeServer`, `OmronCipServer`, `CipServer`, `PcccServer`,
  `KeyenceNanoServer`, `GeSRTPServer`, `FujiCSTServer`, `FujiSPBServer`, `FujiSPHServer`,
  `CimonServer`, `PanasonicMewtocolServer`, `FatekProgrameServer`, `YokogawaLinkServer`,
  `InovanceEasyNetServer`, `MemobusTcpServer`, `VigorServer`, `XinJEInternalServer`,
  `FetchWriteServer` (Siemens S7 Fetch/Write), `SiemensPPIServer`.
  These nine produce **no tail** even with the switch on: `ModbusTcpServer` (the plain Modbus
  virtual server), `LSisServer`, `BeckhoffAdsServer`, `DLT645Server`, `DLT645With1997Server`,
  `DLT698Server`, `RkcTemperatureControllerServer`, `ToyoPucServer`, `TurckReaderServer`.
- Client pages created with `client.*` have no such tail; their frame log lives on the `报文读取`
  tab of the page and is not exposed by this API.
- The tail is produced by the server, so it records **what the server understood**, not what the
  client library intended. When they disagree (wrong station, mis-addressed tag, wrong length), the
  tail is the more reliable witness for an upper-computer test.

## 保存列表：保存 / 打开 / 删除已保存的页面 (`page.*`)

Every server window and client window has a **保存 (Save)** button in its top-right corner. It asks for
a name and stores the page - connection values, point table, data simulation table, logic program and
protocol-specific options - in the Demo save list (保存列表), which is persisted to
`<Demo directory>\devices.xml`. Double-clicking an entry in that list re-creates the page from the
saved XML. `page.*` gives the agent the same four actions (save / list / open / delete) without a dialog:

| Operation | Meaning | Key parameters |
| --- | --- | --- |
| `page.save` | Save the current page exactly like the top-right 保存 button. Saving again under the **same name** overwrites that entry; saving under a **new name** adds a new entry and keeps the old one (same as the manual button). | `page` = `clientId` / `serverId` / window title; `name` = save name (defaults to the window title) |
| `page.list` | List the save list (read-only, no write switch needed). | none |
| `page.open` | Open a saved entry exactly like double-clicking it: reuse the window if it is already open, otherwise re-create it from the XML. | `name` (accepts `group:name` too) |
| `page.delete` | Delete one saved entry, exactly like right-clicking it in the save list and choosing 删除设备. | `name` (or `guid`); `prefix: true` deletes every entry of a group instead of one |

```jsonc
// save the page that is currently open
POST {base}/api/operations  {"operation":"page.save","parameters":{"serverId":"<id>","name":"mc-bench"}}
// -> { "success": true, "name": "mc-bench", "guid": "...", "type": "FormMcServer",
//      "kind": "server", "file": "...\\devices.xml", "serverId": "..." }

// what is in the save list
POST {base}/api/operations  {"operation":"page.list"}
// -> { "success": true, "count": 30, "pages": [ {"name":"mc-bench","type":"FormMcServer","guid":"..."} ] }

// open it again (after a Demo restart too)
POST {base}/api/operations  {"operation":"page.open","parameters":{"name":"mc-bench"}}
// -> { "success": true, "kind": "server", "formType": "FormMcServer", "reused": false,
//      "serverId": "...", "started": false, "savedAttributes": { "Port": "6001", ... } }

// delete one entry (or the whole group with prefix:true)
POST {base}/api/operations  {"operation":"page.delete","parameters":{"name":"mc-bench"}}
// -> { "success": true, "name": "mc-bench", "deleted": 1, "remaining": 27, "file": "...\\devices.xml" }
POST {base}/api/operations  {"operation":"page.delete","parameters":{"name":"分厂一","prefix":true}}
// -> { "success": true, "group": "分厂一", "deleted": 3, "remaining": 24 }
```

Rules that matter:

- **`page.open` does not start or connect anything.** It only recreates the page with the saved
  settings, which is exactly what a double-click does. Continue with:
  - server page -> `virtualServer.startTcp` / `startSerial` with the returned `serverId`;
  - client page -> `client.connect` with the returned `clientId`, then `client.read` / `client.write`.
- **The returned id is new every time a page is created.** A saved page opened in a fresh Demo gets a
  new `serverId` / `clientId` even though its `Guid` in the XML stays the same; always use the id from
  the `page.open` response.
- **`reused: true`** means the window was already open, so no settings were re-read from disk - the
  live page wins. Stop or close it first when you want a clean restore.
- **Saved entries survive a Demo restart** (they are files, not memory). This is the only way to get a
  configured page back across restarts: `virtualServer.*` state and `client.*` pages do not survive.
- **`page.delete` removes the entry only.** The window it belongs to keeps running - deleting is
  bookkeeping, not a disconnect. Use `virtualServer.stop` / `client.remove` to close the live page.
  Deleting an entry that is currently open means the next `page.save` with that name creates a fresh
  entry (a new guid); the open page keeps its old binding until you save again.
  `prefix: true` mirrors right-clicking a group node, so "分厂一" removes every saved page under it.
- **`page.save` needs the write switch** (`Allow write plc data?`), because it persists Demo
  configuration; `page.list` does not.
- A page that was opened from the save list keeps reporting the same saved name on a later
  `page.save` with the same name, so "open -> test -> save again" updates the entry in place.
- Error codes: `page_not_found` (the target page is not open), `page_saved_not_found` (no saved entry
  with that name; the response carries `availableNames`), `page_save_failed`, `page_open_failed`,
  `page_list_failed`, `page_delete_failed`.

## Coding workflow for agents

When an agent is asked to produce a live curve analysis:

1. Call `/api/health`.
2. Call `/api/devices`.
3. If the Demo data table is configured, call `/api/points` and use its rows
   as the address source.
4. Resolve the Demo gateway URL, device, target address, and data type.
5. Display the resolved target for transparency, then proceed without waiting
   for confirmation.
6. If multiple devices are listed or the target is ambiguous, ask the user
   which device/address to use before reading.
7. Use the address and the protocol reference for the address syntax; do not
   guess an address.
8. Start `/api/collect`, wait once for about the requested duration, then read
   `/api/collection?id=...`.
9. Use the returned samples and `analysis` to generate the final curve,
   table, or report. Do not fabricate a curve when the read failed.

When the request asks for repeated reads such as every 100 ms or every second,
skip step-by-step reasoning per sample. Start one `/api/collect` task, wait
once, and analyze the final dataset in a single step.

## Gateway behavior and limitations

- The gateway only exposes connected Demo devices. Reads are enabled by
  default; remote writes require the Demo AI Server permission switch.
- Virtual server lifecycle and point mutation operations also require the same
  write switch. Read/status/list operations remain available without it.
- Device point-table import (`/api/points/import`) changes Demo configuration
  and therefore also requires the write switch.
- Connected-device data simulation (`device.simulate.*`) writes to the real PLC
  through the existing Demo connection. Import, start and stop therefore
  require the write switch; list and export do not.
- Servers opened through the normal Demo UI are registered automatically after
  their start button succeeds; use `/api/servers` to obtain their `serverId`.
- The gateway opens or reuses the original Demo server window and invokes its
  original controls internally. Agents must not add desktop mouse automation
  or screenshot-clicking as a workaround.
- The virtual-server logic engine supports only the actions and condition
  helpers listed in `references/logic_program_guide.md`. It does not execute
  arbitrary C#, external scanner/HTTP/database actions. `call` runs another
  **stored** logic program of the same window (`/api/logic/programs*`), which is
  still an in-process rule program, not an external service.
- A logic program may store and call sub programs, so the logic endpoints are
  `/api/logic/schema`, `/api/logic/upload`, `/api/logic/start`, `/api/logic/stop`,
  `/api/logic/status`, `/api/logic/log`, `/api/logic/delete`, `/api/logic/list`
  and `/api/logic/programs` (`save`/`get`/`delete`). Store a sub program before
  uploading the caller that calls it.
- The data simulation engine only runs the expression-driven rows from
  `DataSimulateControl`; it does not invoke external scanners, HTTP services,
  databases or MES systems.
- Device objects are removed automatically when the corresponding Demo page is
  closed.
- `intervalMs` is clamped to 100..3600000 milliseconds and `seconds` is clamped
  to 1..86400.
- The periodic task runs on one background thread per collection request.
- The Demo process must stay open while the agent reads data.
- The endpoint is bound to `127.0.0.1` and is intended for local agents only.
  Do not expose it to the public internet without authentication.

### Fault injection of a virtual server

`virtualServer.faultInjection.*` injects faults into the responses of one virtual
server, so a host application can be tested against timeouts, drops, bad frames
and disconnects without hardware. Read `GET {base}/api/faultinjection/schema`
for the machine readable description; the essentials:

| Operation | Meaning |
| --- | --- |
| `virtualServer.faultInjection.status` | Settings, counters and the session split of one server (read only, works with the write switch off) |
| `virtualServer.faultInjection.start` | Configure and enable: `types` (`["delay"]`, `["drop","disconnect"]`, or the string `"delay,drop"`), `delayMs` 1..60000, `probability` 1..100, `corruptBytes` 1..1024. Every parameter is optional and keeps its current value, so calling it again retunes a running injection |
| `virtualServer.faultInjection.stop` | Disable it; the server responds normally again immediately |
| `virtualServer.faultInjection.reset` | Clear the counters without changing the settings |

Fault types and the transports they apply to:

| Fault | tcp / udp | serial | Meaning |
| --- | --- | --- | --- |
| `delay` | yes | yes | the response waits `delayMs` before it is sent |
| `drop` | yes | yes | the response is discarded, the client hits its receive timeout |
| `corrupt` | no | yes | the tail of the response is turned into a bad frame |
| `disconnect` | yes | no | the session is closed right after the response |

The two "no" entries are deliberate and are reported by the API:

- **`corrupt` only takes effect on serial sessions.** A serial response usually
  ends with a CRC/LRC, so a tampered tail is a realistic bad frame; TCP and UDP
  have link layer checksums and never deliver a bad frame to the application, so
  nothing is tampered there. `delay` and `drop` apply to every transport. The
  status response reports `corruptSerialOnly` and `corruptEffective` (true
  only while `corrupt` is selected **and** `sessions.serial` > 0), and it adds
  a `hint` when `corrupt` is selected without any serial session, so an agent
  can tell "not effective" from "not started".
- **`disconnect` only takes effect on network sessions (TCP/UDP).** A serial
  session has no connection of its own: closing it would close the serial port of
  the server itself, the whole serial slave would stop answering while the window
  still reports it as running, and the client could not recover. Serial tests
  should therefore use `drop` (device stops answering), which the client sees as
  a timeout in exactly the same way. The status response reports
  `disconnectSerialSkipped` and `disconnectEffective` (the same rule with
  `sessions.tcpOrUdp`), and it adds a `hint` when `disconnect` is selected
  while no network session is connected at all (either nothing is connected yet,
  or only serial sessions are).
- The injection belongs to the server, not to one client: while it is enabled every
  session of that server is affected, including the sessions of other tools.
- Only one fault type is applied to a single response: the type is drawn at random
  among the selected ones, so a response that was delayed is never corrupted at the
  same time. Turn on several types when the goal is "sometimes late, sometimes
  gone", not when a single response must show two faults at once.
- `faultInjection.sessions` is computed live on every call, and it is an
  **instantaneous snapshot**: it counts only the sessions that are open at the
  moment of the call, and it agreed exactly with `virtualServer.clients.list` on
  the bench. Because of that:
  - a short-lived client that connected, did one read and closed (for example the
    probe in a previous step) is **not** in it, so `sessions.tcpOrUdp` may be 0
    even though a client did work a moment ago - use the `statistics` counters,
    not `sessions`, to prove that traffic happened;
  - to assert "a network session exists", keep the client connected across the
    call (a long-running client, or a session the scenario opened itself).
  Prefer `sessions` (or `virtualServer.clients.list`, which also reports each
  session transport) over `server.onlineCount`, which is a cached counter
  refreshed by the server window and can lag right after a reconnect; note that
  `onlineCount` counts serial and network sessions together.
- The faults are injected by the server while it builds the response: the client
  keeps using the normal port and point table, no proxy and no port change. The
  data pool of the server is never modified, only the responses.
- Everything is disabled by default; a server that never had an explicit
  `start` injects nothing and has zero overhead.
- `delay` blocks only the response of that one session; other clients and the
  logic-program scan are unaffected.
- Observe the result with `status` counters (`totalResponses`, `delay`, `drop`,
  `corrupt`, `disconnect`) rather than by reading raw logs, then assert on the
  client side (timeout took effect, reconnect happened, the value stayed
  correct).
  `totalResponses` counts only the responses judged while the injection is
  enabled, so it stays 0 as long as the injection is off.
- `start`, `stop` and `reset` are write operations and need the Demo AI Server
  write switch; `status` is read only.
- Example: `virtualServer.faultInjection.start` with
  `{"types": ["delay","disconnect"], "delayMs": 800, "probability": 30}` makes
  30% of the responses late by 800 ms and some of them end with a disconnect,
  which is the usual way to verify a reconnect implementation.
  On a serial-only bench only `delay` takes effect, because `disconnect` is a
  network fault.

## Scenario automation: many verified steps in one call

Use a scenario when the task needs a whole sequence - configure the points, upload
and start a logic program, wait until a client reads the result, inject 300 ms of
delay, prove the read got slower, inject drops, prove the client times out, then
clean everything up - instead of driving `/api/operations`, `/api/read`,
`/api/write` and `/api/logic/*` one call at a time. One POST executes the sequence
on the Demo host, evaluates the assertions, **always** runs the teardown, and
returns one report (JSON + Markdown) that can be quoted in the answer.

| Endpoint | Meaning |
| --- | --- |
| `GET {base}/api/scenario/schema` | machine readable description: step types, assertion operators, options, limits and one complete example |
| `POST {base}/api/scenario/validate` | static check only, no side effect: exactly the validation a run performs first |
| `POST {base}/api/scenario/run` | execute and return the report (blocks until the scenario is finished) |
| `GET {base}/api/scenario/status` | live progress of the current (or the last) run; add `?includeReport=true` to also get the report |
| `GET/POST {base}/api/scenario/list` | the stored scenarios, with `directory` and a summary per file (`name`, `title`, `tags`, `stepCount`, `operations`, `requiresTargets`, `needsWriteSwitch`, `parseError`) |
| `POST {base}/api/scenario/save` | store a scenario so it can be reused; needs the write switch, and runs `validate` first so only a runnable scenario is stored |
| `GET/POST {base}/api/scenario/get` | read one stored scenario back (`?name=`) |
| `POST {base}/api/scenario/delete` | remove one stored scenario (`{ "name": ... }`) |

### Storing and reusing a scenario

```jsonc
// `name` resolves in this order: ?name= -> request's name -> scenario.name
POST {base}/api/scenario/save { "name": "s7-timeout", "scenario": { /* ... */ } }
POST {base}/api/scenario/save { "scenario": { "name": "s7-timeout", /* ... */ } }   // also fine
POST {base}/api/scenario/save { "name": "s7-timeout", "overwrite": true, "scenario": { /* ... */ } }

GET  {base}/api/scenario/get?name=s7-timeout
POST {base}/api/scenario/delete { "name": "s7-timeout" }
```

- The name is taken from `?name=`, else the request's top-level `name`, else
  **`scenario.name`**. The last fallback matters because a scenario object already
  carries its own `name`, and `run` / `validate` accept that object as-is - so a
  scenario that runs and validates must also be storable without repeating the name
  outside it.
- Whichever name wins becomes the stored name: the file is `<name>.scenario.json`
  and `scenario.name` inside the body is overwritten with it. `get` / `delete` use
  that same value. A `null` name counts as "not provided".
- `targets` is dropped unless you pass `keepTargets: true` (server and device ids
  change on every restart).
- Saving an existing name answers `scen_exists` unless you pass `overwrite: true`.
- Saving is validated: a scenario with validation errors is refused with
  `scen_schema_invalid` and a `checks` array, exactly like `run` would report them.
  When no name can be resolved anywhere the answer is `scen_name_invalid` with
  *"The scenario name is required. Pass it as the request's name, or set name inside
  the scenario object."*
- Stored scenarios live in `<Demo base directory>/Scenarios` and also appear in the
  Demo scenario window's list, so the user can run them by hand.
- Other store errors: `scen_name_invalid`, `scen_too_large`, `scen_save_failed`.

### Request

```json
{
  "scenario": {
    "scenarioVersion": 1,
    "name": "s7-timeout-reconnect",
    "title": "optional human readable title",
    "targets": { "server": "<serverId>", "client": "<deviceId>" },
    "defaults": { "stepTimeoutMs": 20000, "dataType": "short" },
    "setup":    [ /* steps that prepare the bench */ ],
    "steps":    [ /* the checks */ ],
    "teardown": [ /* steps that always run, even after a failure */ ]
  },
  "options": {
    "stopOnFailure": true,
    "stepTimeoutMs": 20000,
    "includeResponses": "failed",
    "evidence": [ "faultInjection", "serverStatus", "logic", "sessions" ],
    "report": "json+markdown"
  }
}
```

`options.dryRun: true` only validates. Run `/api/scenario/validate` (or `dryRun`)
before a long scenario: it reports `checks[]` with `kind` (`error` / `warning`) and
a `code`, for example `scen_step_type_unknown`, `scen_schema_invalid`,
`scen_target_missing`, `scen_path_unknown_step`, `emptyProgram`, `teardownMissing`.

### Step types

| `type` | What it does | Key fields |
| --- | --- | --- |
| `operation` | any `POST /api/operations` call | `operation`, `parameters` |
| `read` / `write` | one read / write, on the client by default or on the server | `target` (`client` / `server`), `address`, `dataType`, `length`, `value` |
| `logic` | logic program actions | `action` (`upload`, `start`, `stop`, `delete`, `programs.list`, `programs.save`, `programs.get`, `programs.delete`), `program`, `name` |
| `wait` | polls a source step until the condition holds | `source` (a step), `until` (an assertion), `timeoutMs`, `intervalMs` |
| `sleep` | fixed wait | `ms` (0..60000) |
| `assert` | assertions only, no call of its own | `assert` |
| `note` | comment, no side effect | `text` |

#### `wait`: paths in `until`

`until` is a single assertion object (or an array) whose `path` may be written
**either relatively or absolutely** - both work:

```jsonc
{ "type": "wait", "id": "wt1",
  "source": { "type": "read", "address": "100", "dataType": "short" },
  "until": { "path": "value", "operator": "equals", "value": 4321 },   // relative
  "timeoutMs": 3000, "intervalMs": 100 }
```

A relative path is resolved against the source step's own response, so `value`,
`success` and `elapsedMs` all work. Absolute `steps.<id>.<field>` paths work too
(`steps.wt1.value`), and `env.*` is always absolute. A relative path cannot reach
the source's inner wrapper fields - write those absolutely, for example
`steps.wt1.response.value`. A path that cannot be
resolved is a failure, never a silent pass - if a `wait` always times out with
`the until condition never became true` and an empty `Last poll:`, the path is
wrong (or the value really never arrived).

`assert` steps, by contrast, must use an absolute path: they read the responses
recorded for earlier steps, so write `steps.<stepId>.<field>` there.

Every step also accepts:

- `label` - a human readable name. It is shown in the Demo's scenario monitor
  window and kept in the report as `steps[].label`, so "which row is the drop
  injection" has an obvious answer.
- `stepTimeoutMs` - overrides `options.stepTimeoutMs` for that single step (use it
  for a step that is expected to be slow, such as a drop test that waits for a
  10 s receive timeout).

The report also carries a generated `steps[].action` for every step
(`virtualServer.faultInjection.start (types=drop; probability=100)`,
`read D100 (client)`, `wait read D104 (client) → value equals 1`, ...) - that is
what the Demo window shows in its Action column.

### Assertions

Shape: `{ "path": "...", "operator": "...", "value": ..., "description": "...", "severity": "error|warning" }`,
or the shorthand `"expect": { "success": true }` for the whole response.

| Operator | Meaning |
| --- | --- |
| `equals` / `notEquals` | scalar comparison (numbers compare numerically, strings exactly) |
| `gte`, `gt`, `lte`, `lt` | numeric comparison |
| `contains` | substring, or membership in an array |
| `matches` | .NET regular expression |
| `exists` / `absent` | the field is present / missing |
| `isTrue` / `isFalse` | boolean check |
| `oneOf` | the value is one of the listed values |

- A path addresses the **raw response of a step**: `steps.rd1.value`,
  `steps.rd1.elapsedMs`, `steps.cl.count`,
  `steps.fs1.faultInjection.statistics.delay`, or `env.serverId` / `env.clientId`.
  Report metadata such as `durationMs`, `status` or `assertions` is **not**
  addressable - assert on the response fields instead.
- An unresolvable path is an error (`scen_path_unknown`), never a silent pass.
- **A step that is expected to fail must assert its own `success` field.** When the
  response reports `success: false`, the step is otherwise recorded as an error
  (`scen_step_error`) and, with the default `stopOnFailure`, the remaining steps are
  marked `skipped`:

```json
{ "type": "read", "id": "rd2", "address": "D100", "stepTimeoutMs": 20000,
  "label": "drop must make the client time out",
  "assert": { "path": "steps.rd2.success", "operator": "isFalse" } }
```

- Session-mix dependent fields (`faultInjection.corruptEffective`,
  `disconnectEffective`, and the whole `faultInjection.sessions` block) must be
  asserted with `severity: "warning"`: they are instantaneous snapshots of **every**
  connected session, so a TCP + serial bench turns them `true` even when the
  transport under test is unaffected, and a client that already disconnected is
  simply absent. The static flags
  (`corruptSerialOnly`, `disconnectSerialSkipped`) are stable and safe as `error`.
- Use `"severity": "warning"` when the expectation may legitimately not hold on the
  bench - for example `corrupt` on a TCP session, which does not break frames (see
  the fault injection section). The scenario still passes and the mismatch is
  counted in `summary.warnings` and listed in the step's `assertions`.

### Teardown rules (learned on the bench)

- `teardown` runs on every outcome, including a validation failure, a step error
  and a timeout. Keep injection, simulation and logic cleanup there.
- `virtualServer.faultInjection.stop` (and `reset` so the counters start from zero
  next time) belongs in the teardown of every scenario that starts an injection.
- `virtualServer.simulate.stop` stops the worker but **keeps the rows** in the
  table. To leave the server exactly as it was, add a step that clears them:
  `{"type":"operation","operation":"virtualServer.simulate.import","parameters":{"mode":"replace","items":[]}}`.
- Point tables are cleared the same way:
  `{"type":"operation","operation":"virtualServer.points.import","parameters":{"mode":"replace","points":[]}}`
  (only do this for points the scenario created itself).
- `points.import` defines rows; it does **not** write the values. Add
  `virtualServer.points.writeDefaults` when the scenario needs the default values
  in the PLC.

### Attaching a screenshot to the report

Set `options.screenshot` when the user wants to *see* the result:

| Value | Meaning |
| --- | --- |
| absent / `false` | no screenshot (default) |
| `true` | capture the scenario monitor when the run finishes |
| a selector string | `main`, `scenario`, `server:<id>`, `device:<guid>` - same selectors as `/api/screenshot` |

The report then carries `screenshot { available, window, title, width, height, bytes,
visible, savedFile, capturedAt }`, and the Markdown summary gets a `Screenshot:` line
with the file path. `options.screenshotIncludeImage: true` additionally inlines
`imageBase64` (only worth it for small windows).

The picture is taken **after** the run has been recorded as finished, so it shows the
final status line, the filled step table and the report box - not the previous frame.
Best-effort by design: if the window is not open the report records
`screenshot.available = false` with `reason` and a suggestion, and **the run result,
`code` and `summary` are untouched**. The picture is taken after the teardown, so it
shows the finished state, and the worst case is one extra render (the capture waits up
to 8 s if a modal dialog blocks the UI thread).
### One run at a time

Only one scenario may run at a time. A second `POST /api/scenario/run` while
another one is in progress is refused immediately with:

```json
{ "success": false, "code": "scen_busy",
  "message": "Another scenario is already running (s7-timeout-reconnect); only one run is allowed at a time.",
  "suggestion": "Call GET /api/scenario/status to watch the running scenario, then retry after it finishes.",
  "status": { "running": true, "name": "...", "completedSteps": 12, "totalSteps": 37 } }
```

Poll `GET /api/scenario/status` instead of retrying blindly:

```json
{ "success": true, "hasRun": true, "running": true, "busy": true, "finished": false,
  "name": "s7-timeout-reconnect", "startedAt": "2026-10-05T23:41:02.1234567+08:00",
  "elapsedMs": 8421, "completedSteps": 12, "totalSteps": 37, "revision": 41,
  "currentStep": { "id": "rd2", "type": "read", "section": "steps",
                   "action": "read D100 (client)", "elapsedMs": 8120 },
  "steps": [ { "no": 1, "id": "imp", "section": "setup", "type": "operation",
               "status": "passed", "durationMs": 4, "code": "", "message": "",
               "action": "virtualServer.points.import (3 point(s); mode=replace)",
               "label": "prepare 3 points" }, ... ] }
```

After the run is finished the same call returns `"running": false,
"finished": true` plus `result { success, code, message, summary { total, passed,
failed, errored, skipped, warnings, teardownFailed } }`, and with
`?includeReport=true` the complete report. A client that lost the run response (a
tool timeout, a second agent) can therefore still read the outcome.

### Watching the run in the UI

The window is also a small test console: its left pane lists the stored scenarios
(`Scenarios` folder) with step count and updated time, lets you pick the server and
the client from the live Demo lists, and runs the selected scenario with one click -
including a pre-flight check that refuses to start when a required target is missing
or when the scenario needs the write switch while it is off. Runs started there use
the same engine, the same mutex and the same snapshot, so `GET /api/scenario/status`
shows them live, and "Save as..." turns whatever ran last (yours or the AI's) into a
stored scenario.
The Demo main window has **AI Tool -> AI Scenario** ("场景监视" / Scenario Monitor,
docked next to the virtual PLC and client windows). It shows the same snapshot as
`/api/scenario/status`: the two-line status bar (progress, current step, elapsed
time, then the result and failure reason), one row per step with **No. / time /
section / step id / action / status / duration / detail**, rows coloured by status,
an automatically scrolled active row, a "failed only" filter and the final report
JSON with copy / save-as. Ask the user to open it when they want to watch an
automated run happen live; it is a single window, closing it only hides it.

### Report

`POST /api/scenario/run` returns:

- `success`, `code`, `message`, `summary { total, passed, failed, errored, skipped,
  warnings, teardownFailed }`
- `steps[]` - per step: `id`, `section`, `type`, `status` (`passed`, `failed`,
  `errored`, `skipped`), `durationMs`, `action`, `label`, `code`, `message`,
  `assertions[]` and, when the response is kept, the raw `response`
- `evidence` - one snapshot per requested area (`faultInjection`, `serverStatus`,
  `logic`, `sessions`) taken **before** the teardown, so the cleanup cannot hide
  the state that was measured
- `markdown` - a ready to paste summary
- `options.includeResponses` controls the raw responses in the report: `failed`
  (default) keeps only the failed steps, `all` keeps every one, `none` keeps none.
  `action` and `label` are always kept.

### Scenario error codes

| Code | Meaning |
| --- | --- |
| `scen_schema_invalid` | the scenario has validation errors; the report carries `checks[]` |
| `scen_step_type_unknown` | a step `type` is not one of the eight types |
| `scen_unknown_operation` | an `operation` step names an operation that is not in `/api/capabilities` |
| `scen_target_missing` | the step needs `targets.server` or `targets.client`, which the scenario does not define |
| `scen_step_error` | a step response reported `success: false` without the scenario accepting that failure |
| `scen_step_timeout` | the step took longer than `stepTimeoutMs` (per step, or the option) |
| `scen_assert_failed` | an assertion with `severity: "error"` did not hold |
| `scen_path_unknown` / `scen_path_unknown_step` | an assertion path cannot be resolved / references a step that is not defined before it |
| `scen_teardown_failed` | a teardown step failed; the run keeps the result of the checks but the bench may not be clean |
| `scen_busy` | another scenario is running; poll `GET /api/scenario/status` |

A permission failure inside a step keeps its own code (`write_disabled` when the
Demo AI Server write switch is off), so the report shows the real cause.

## Switching tabs at the start of every testing stage (`GET/POST /api/ui/tabs`)

Every Demo server window and client window is **tabbed**: each feature lives on its own tab,
and only the tab in front is visible. The agent cannot click a tab, so this call is the only way
to bring a feature page to the front. It lists the tabs of one window and switches to a chosen
one.

> **Switch the tab before every stage of testing.** The user is watching the screen. If you drive
> a feature that lives on page 8 while page 1 is in front, the user sees nothing happen and
> cannot follow the test. So whenever a task has more than one stage - or you move from one
> feature to another - switch that window to the matching tab **first**, then do the work.
> This is a default step, not an optional extra: do it even when the user did not ask for it.
> Do **not** capture a screenshot as part of it. Switching the tab is what makes the run
> observable; take `/api/screenshot` only when the user asks to see the result, or when you
> genuinely need to look at the UI yourself to answer a question.

Common tab names per feature (pass the one that matches what you are about to do):

| You are about to | Switch to |
| --- | --- |
| upload / start / stop a logic program | `逻辑程序` (client and server both have it) |
| set up or check data simulation | `数据模拟` |
| inject delays, drops or corrupt frames | `故障注入` (server windows) |
| import or inspect the point table | `点位变量` |
| run batch reads / frame-message reads | `批量读取` / `报文读取` |
| export collected data | `数据导出` |
| show protocol address syntax | `地址示例` |
| show generated code | `代码示例` |
| read a server window log | `日志信息` (server windows) |

```jsonc
// list the tabs of one window (no side effect)
POST {base}/api/ui/tabs   { "device": "<clientId or serverId>" }

// switch: give title OR index
POST {base}/api/ui/tabs   { "device": "<id>", "title": "逻辑程序" }
POST {base}/api/ui/tabs   { "device": "<id>", "index": 7 }

// GET works too; percent-encode a non-ASCII title
GET  {base}/api/ui/tabs?device=<id>&title=%E9%80%BB%E8%BE%91%E7%A8%8B%E5%BA%8F
```

| Parameter | Meaning |
| --- | --- |
| `device` | target window: a `clientId` from `/api/devices` or `client.list`, a `serverId` from `/api/servers`, or a server name. `clientId`, `serverId`, `target` and `window` are accepted aliases. |
| `title` | tab to switch to. Matched against the current-language title, the English title and the raw designer text; an exact match wins, otherwise a case-insensitive substring match is used (so "逻辑" selects "逻辑程序"). `tab` and `name` are aliases. |
| `index` | 0-based position. Takes precedence over `title`; `0` is valid and means the first tab. |

Omit both `title` and `index` to only list. The response:

```jsonc
{
  "success": true,
  "target": "beeb5a60d51844059b1c30caa1e1ccb5",
  "tabCount": 11,
  "selectedIndex": 4,
  "selectedTitle": "点位变量",
  "changed": true,
  "tabs": [
    { "index": 0, "title": "批量读取", "englishTitle": "Batch Read", "originalText": "批量读取", "selected": false }
  ],
  "screenshotHint": "POST /api/screenshot {"window":"device:..."}"
}
```

Every tab carries `title` (current interface language) **and** `englishTitle`, so you can match
either language no matter how the Demo is configured. Typical sets (they differ per protocol and
per window, so treat them as examples and read the real list):

| Window | Tabs seen in this build |
| --- | --- |
| virtual server (Modbus) | 日志信息/Log, 批量读取/Batch Read, 点位变量/Data Table, 数据模拟/Simulate, 其他功能/Others, **逻辑程序/Logic Program**, 故障注入/Fault Injection, 地址示例/Address Example, 代码示例/Code Example |
| client device (Modbus) | 批量读取/Batch Read, 报文读取/Frame Message Read, 远程调试/Debug Remote, 线程测试/Thread Test, 点位变量/Data Table, 数据模拟/Simulate, 数据导出/Data Export, 特殊功能/Special Function, 地址示例/Address Example, 站号搜索/Station Search, 代码示例/Code Example |

Rules and good practice:

- **Switch at the start of every stage, every time.** A multi-stage test means one switch per
  stage (and one before switching to a different feature), not a single switch at the beginning.
  Re-check with a plain list call if you are unsure which tab is in front.
- **Never hard-code an index.** Tab sets differ per protocol and window (a page can add its own
  tabs), so list once and then switch by `title`.
- Switching only changes which tab is in front. It touches no device communication and needs no
  write switch.
- It works on a page that was created but **not connected** yet, so you can switch before the
  device is up.
- **No screenshot by default.** The switch itself is the observable step. Use `/api/screenshot`
  only when the user asks to see the result, or when you must inspect the UI yourself; then use
  `window` = `device:<guid>` for a client page or `server:<serverId>` for a server page. The `<guid>`
  is the one `GET /api/windows` reports for that page and the same value `client.list` returns.
- Errors: `ui_target_not_found` (no window matches the id), `ui_tab_not_found` (nothing matches
  the title, or `index` out of range - the message states the valid range).
- On the GET form, percent-encode a non-ASCII `title` (for example
  `title=%E9%80%BB%E8%BE%91%E7%A8%8B%E5%BA%8F` for 逻辑程序); raw Chinese in a URL is decoded with
  the wrong code page and will not match. POST with a JSON body has no such limitation and is
  the safer choice.


## Driving a page's logic program (`POST /api/ui/logic`)

Both the **virtual server window** and every **client device window** carry a `逻辑程序` /
`Logic Program` tab, and it is the same control backed by the same engine. `/api/logic/*` only
accepts a virtual server; `/api/ui/logic` accepts either, so it is the one to use when the
program must run against a **client device** (rules then really read and write the PLC).

> Uploading through this endpoint **puts the program on that page's Logic Program tab**, so the
> user sees exactly what you uploaded and can start/stop/edit it by hand afterwards. The order is
> therefore: switch the window to `逻辑程序` with `/api/ui/tabs`, upload, start - and switch again
> at the start of each later stage. No screenshot is needed for this.

```jsonc
// what is on the page right now
POST {base}/api/ui/logic  { "device": "<clientId or serverId>", "action": "status" }

// upload a program (same JSON shape as /api/logic/upload)
POST {base}/api/ui/logic  {
  "device": "<id>", "action": "upload",
  "program": { "name": "demo", "cycleMs": 200, "rules": [ /* ... */ ] }
}

// run it, then check it
POST {base}/api/ui/logic  { "device": "<id>", "action": "start" }
POST {base}/api/ui/logic  { "device": "<id>", "action": "start", "cycleMs": 20 }
POST {base}/api/ui/logic  { "device": "<id>", "action": "status" }
```

| Parameter | Meaning |
| --- | --- |
| `device` | target window: a `clientId` from `/api/devices`, a `serverId` from `/api/servers`, or a server name. `clientId`, `serverId` and `target` are aliases. |
| `action` | `list`, `upload`, `start`, `stop`, `status`, `delete`. Defaults to `status`. |
| `program` | the program as a JSON object, or a string naming an already stored program. |
| `name` | for `delete`, the stored program to remove. |
| `cycleMs` | for `start`: override the scan period for this run (5..60000 ms). **Omit it to keep the program's own `cycleMs`.** Read the effective value back from `status.cycleMs`. |

| `action` | Does |
| --- | --- |
| `list` | stored program names of that page (its own folder, same as the page drop-down) |
| `upload` | parse + validate + compile and put it on the page; returns `warnings` and `checks` |
| `start` / `stop` | run or halt the engine of that page |
| `status` | `uploaded`, `running`, `programName`, `cycleMs`, `lastScanMs`, `watchDogAlarm`, `readErrorCount`, `actionErrorCount`, `lastError`, per-rule `triggerCount` / `lastCondition` / `lastError`, and the recent `logs` |
| `delete` | remove a stored program from that page's folder |

Notes:

- **Upload works before the device is connected.** Compiling needs the point table, not the
  device, so you can author and upload while the page is still offline; the tab shows the rules
  immediately. `start` is what needs the channel: if the page has no device object yet it
  answers `ui_logic_start_failed` with *"No read/write channel is available yet. Connect the
  device on this page first, then start the logic program."*
- **An id that does not exist is refused, not silently accepted.** `upload` on a `clientId` that
  matches no window answers `ui_target_not_found`; the same call made through the server-shaped
  `/api/logic/upload` with a `serverId` that matches no virtual server answers
  `Virtual server not found: <id>`. Do not ignore either one and do not retry: a program
  "uploaded" against a nonexistent id would never be able to start, because there is no page to
  connect - the rejection happens before compiling, so no session is created either.
- A server page and a client page on the same device share one data pool, so their programs can
  drive each other - and can also overwrite each other when they write the same address. There is
  no cross-session write protection.
- `upload`, `start`, `stop` and `delete` need the Demo write switch; `list` and `status` do not.
- Verify the result through the device itself (`POST /api/read` with the same `clientId`), not from
  the UI: `readErrorCount` / `actionErrorCount` / `lastError` / `logs` are the fastest way to see
  whether a rule really works.
- Errors: `ui_target_not_found`, `ui_no_logic_id`, `ui_program_missing`,
  `ui_program_not_found`, `ui_program_invalid` (compile errors), `ui_logic_start_failed`,
  `ui_bad_action`, `write_disabled`.

## Seeing the Demo: screenshots for the agent

A text answer can be wrong about the UI, and the user cannot always describe what they
see. These two read-only endpoints let an agent look at the Demo itself.

| Endpoint | Meaning |
| --- | --- |
| `GET {base}/api/windows` | every window that can be captured: `window` (stable key), `title`, `formType`, `width`, `height`, `visible`, `windowState` |
| `GET {base}/api/screenshot` | render one window and return it as a JSON envelope carrying the PNG (metadata + `imageBase64` + `savedFile`) |

Parameters of `/api/screenshot`:

| Parameter | Default | Meaning |
| --- | --- | --- |
| `window` | `main` | `main`, `scenario`, `server:<serverId or name>`, `device:<guid or name>`, or a form type name. When omitted the capture falls back to `main`, which always exists; pass `scenario` explicitly for the scenario monitor. |
| `maxWidth` | 1280 | downscale to this width; the image is also capped at 1200 px high |
| `save` | true | write the PNG into `<Demo base directory>\Screenshots\` and return `savedFile` |
| `includeImage` | true | also return `imageBase64` |

The response is **JSON, not a raw PNG**, so an agent that pipes the body straight
into an image file gets JSON. Read `savedFile` (the path of the written PNG) or
decode `imageBase64`:

```jsonc
{
  "success": true, "available": true,
  "window": "main", "requestedWindow": "", "title": "HslCommunication 测试工具",
  "formType": "FormMain", "width": 1280, "height": 776, "bytes": 358153,
  "format": "png", "windowState": "Normal", "visible": true,
  "capturedAt": "2026-10-08 11:06:12.046",
  "savedFile": "<Demo base directory>\\Screenshots\\main-20261008-110612-046.png",
  "imageBase64": "iVBORw0KGgo..."
}
```

Set `includeImage=false` when you only need the file path: `imageBase64` of a
full-size window is hundreds of kilobytes of text.

Rules and good practice:

- Both endpoints are **read-only**: they never open, activate, restore or resize a
  window, and the capture itself uses control rendering (not a screen grab), so it
  works even when the window is behind another one.
- If the window does not exist yet the answer is `form_not_open` plus the
  `availableWindows[]` list; either pick another window or ask the user to open it
  (`AI Tool -> AI Scenario`).
- **A window the user opened and then closed can still be captured** - it is hidden,
  not destroyed. Verified on the bench: a hidden scenario monitor renders exactly the
  same content as a visible one, so do not ask the user to reopen it.
- The response always carries `available` as a **boolean** (`true` on success, `false`
  with `code`/`message` on failure) - the same field name and meaning as
  `report.screenshot.available` for `options.screenshot`. On failure, the list of
  capturable windows is `availableWindows[]`.
- `visible` and `windowState` tell you what the capture represents; `savedFile` is
  what you quote in the answer so the user can open the image.
- Use it when the user says "看看效果 / 哪里不对 / 截个图", after a scenario run to
  confirm the monitor, or when you must explain what a device window shows. Keep
  `maxWidth` small (for example 900) when the image is only for your own reading.

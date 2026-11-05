# Demo Virtual PLC Logic Program Guide

Use this reference when the user asks to create, edit, import, start, stop or
test a PLC-style logic program inside a Demo virtual server.

This is different from writing a normal C# HslCommunication client or from
creating a virtual server object in the user's own program.

## Routing

Select this guide when the request contains phrases such as:

- "在 Demo 的虚拟 PLC 里写入流程"
- "给虚拟服务器写逻辑程序"
- "把这些步骤写入 Demo 的逻辑程序"
- "创建一条基本逻辑，M100 为 true 时写 D100"
- "启动 Demo 里的逻辑程序并测试"
- "编辑逻辑程序选项卡中的规则"
- "把这个配方/握手做成子程序，主程序里调用它"
- "延时 200ms 后再执行下一步"

Do not select this guide for:

- Writing a normal C# PLC client with `MelsecMcNet`, `SiemensS7Net`, etc.
- Creating a virtual server class in the user's own program.
- General external workflow orchestration that needs scanners, HTTP, MES,
  databases or multiple devices. The current logic engine only executes
  in-process virtual-server rules.

## Required API Flow

1. Resolve the virtual server with `GET {base}/api/servers`.
2. If it does not exist, create it with
   `POST {base}/api/operations` using `virtualServer.create`.
3. Read the supported DSL with `GET {base}/api/logic/schema`.
4. If a rule uses `call`, store every sub program **first** with
   `POST {base}/api/logic/programs/save` (see "Sub Programs"). Call targets are
   compiled when the caller is uploaded, so a target that does not exist yet is
   only reported as the `callTargetMissing` warning and fails at runtime.
5. Upload a compiled program with `POST {base}/api/logic/upload`. The
   success response includes a `checks` array: structured non-fatal
   validation findings, each with `ruleId`, 1-based `row`, `kind`
   (`warning`/`info`) and `code` (`writeConflict`, `callTargetMissing`,
   `edgeNoSource`, `unusedTag`). Resolve `warning` items before starting;
   `info` items are advisory.
6. Start it with `POST {base}/api/logic/start`. You may pass `cycleMs` to pick
   the scan period for this run:

   ```json
   { "serverId": "VIRTUAL_SERVER_GUID", "cycleMs": 20 }
   ```

   **Without `cycleMs` the program's own `cycleMs` is kept** - the parameter is
   purely an override for that run, so a plain `start` never silently changes the
   period the program was saved with. A supplied value is clamped to 5..60000.
   `/api/logic/status` reports the value actually in use, so read `cycleMs` back
   instead of assuming. One rule can fire at most once per scan, so `cycleMs`
   is also the ceiling on how often a permanently-true rule triggers:
   `cycleMs=20` is about 50 triggers/second, `cycleMs=50` about 20.
7. Test inputs and outputs through `POST {base}/api/operations` using
   `virtualServer.points.write` and `virtualServer.points.read`.
8. Inspect status and logs with:
   - `GET {base}/api/logic/list`
   - `GET/POST {base}/api/logic/status`
   - `GET/POST {base}/api/logic/log`
   - `GET/POST {base}/api/logic/programs` (stored sub programs)
9. Stop with `POST {base}/api/logic/stop`.
10. Remove with `POST {base}/api/logic/delete` (a stored sub program is removed
    with `POST {base}/api/logic/programs/delete`).

All upload, start, stop, delete and program-save operations require the Demo AI
Server write switch to be enabled.

Uploading a program does not start it. The program must be started explicitly.

Uploading also **stops a program that is already running**: the upload replaces
the running session, so call `/api/logic/start` again afterwards and confirm
`running` in the status response.

After editing a stored sub program, upload the caller again: the target is
compiled into the caller's session at upload time.

## Program Shape

```json
{
  "serverId": "VIRTUAL_SERVER_GUID",
  "program": {
    "name": "Example",
    "cycleMs": 50,
    "maxScanMs": 1000,
    "rules": []
  }
}
```

`serverId` must name a virtual server that **really exists** (from
`GET {base}/api/servers`). An unknown id is refused before anything is compiled:

```json
{ "success": false, "message": "Virtual server not found: 00000000000000000000000000000000" }
```

Do not retry that, and do not expect a later `start` to explain it: an upload against
a nonexistent server used to answer `success: true` and register a session that could
never start, because there is no page to connect. Resolve the id first
(`virtualServer.create` when you need a new server), then upload.

Program fields:

| Field | Meaning |
| --- | --- |
| `name` | Display name. |
| `cycleMs` | Scan period, clamped to 5..60000 ms. Default is 50. |
| `maxScanMs` | Watchdog threshold. 0 disables the alarm. A `delay` never trips it (it does not occupy the scan). |
| `rules` | Ordered rule list. Rules execute top-down every scan. |

Hard limits: at most 500 rules per program and 100 actions per rule; a longer
list is rejected with a per-item error at upload.

Rule shape:

```json
{
  "id": "r1",
  "comment": "M100 rising edge writes D100",
  "enable": true,
  "when": "rise(M100)",
  "timeoutMs": 0,
  "actions": [
    {
      "type": "write",
      "address": "D100",
      "dataType": "s",
      "value": "1234"
    }
  ]
}
```

Rule fields:

- `enable` is optional and defaults to `true`.
- When `enable=false`, the rule remains in the program but its condition and
  actions are skipped until it is enabled again.
- `timeoutMs` > 0 makes the rule TON-style: the condition must stay true for
  `timeoutMs` before firing, and while it stays true the rule fires again
  after each `timeoutMs` (periodic tick). A self-reset action (e.g.
  `resetBit` on the trigger bit) prevents repeated firing.

## Conditions

Supported address forms (three disjoint modes, no ambiguity):

- Bare bit address in `when` (no `@`): `M100`, `Q0.0`, `I1.2`, `X5` —
  always an absolute bit address, never a tag.
- Point-table tag: `@TagName` or `@TagName[i]` — must exist in the point
  table; type/length/encoding come from the tag.
- Absolute address with explicit type: `@Address:type[:length]` — never
  resolved as a tag, even when a tag with the same name exists (an upload
  warning is returned in that case; to use the tag instead, write `@Name`
  without a type suffix).

Absolute-address examples:

- PLC data: `@D100:s`, `@D200:i`, `@Code:str[20]`, `@100:s[10]`
- Modbus special prefixes: `@x=4;100:us`, `@s=2;x=4;100:us`,
  `@w=6;x=3;100:us`, `@0x64:us`
- Allen-Bradley (colon inside the address): `@B3:0/0:b`, `@N7:0:i`,
  `@T4:0.PRE:i`, `@C5:0.ACC:i[10]`

The address part of an absolute reference may contain protocol characters
`= ; / . :`. The type after the last `:` must be a plain letter code
(`b s us i ui l ul f d str bytes`; there is no standalone `u` — use `us`
for ushort and `ui` for uint), which keeps the address-internal colon
unambiguous. Invalid codes such as `u`, `word` or `real` are rejected with
a suggestion of the closest valid code (`u` → `us`/`ui`, `word` → `us`/`i`,
`real` → `f`/`d`); follow the suggestion instead of guessing.

Supported expression features:

- Boolean operators: `&&`, `||`, `!`
- Comparisons: `==`, `!=`, `<`, `<=`, `>`, `>=`
- Arithmetic: `+`, `-`, `*`, `/`, `%`
- Parentheses
- String helpers: `len`, `sub`, `contains`, `startsWith`, `trim`, `toUpper`
- Random helpers: `random`, `randomInt`, `randomEAN13`, `randomString`

Examples:

```text
M100
rise(M100)
fall(M200)
rise(@100:b)
rise(100)
M100 && !M101
@D100:s > 10
@D100:s > @D200:s
startsWith(@Code:str[20], "NG")
contains(@Code:str[20], "NG")
```

String notes (a fixed length field reads back with its trailing `0x00` padding, so
`@Code:str[20] == "NG"` is false): compare a fixed field with `startsWith`,
`contains` or `sub`, and remember that for a **write** the DSL `length` is a byte
count (0/omitted = the bytes of the text, `> 0` = a fixed size field), while the
library reads a string by device unit (registers on word devices).

Two addresses can be compared directly when their types are compatible:

```text
@D100:s > @D200:s
@D100:i <= @D200:i
@D100:f < @D200:f
```

For mixed numeric types, the expression is evaluated by DynamicExpresso using
C#-style numeric promotion. Prefer the same type on both sides. Do not rely on
signed/unsigned combinations such as `int` versus `ulong` or `long` versus
`ulong`; report them as risky or unsupported unless the user explicitly
requests a conversion.

## Edge Limitations

The current engine supports `rise` and `fall` only for boolean scalar
addresses:

```text
rise(M100)
fall(M101)
rise(100)
fall(100)
```

For Modbus coils, `rise(100)` and `fall(100)` are shorthand for
`rise(@100:b)` and `fall(@100:b)`.

The current engine does not support expression edges such as:

```text
rise(@D100:s > 10)
fall(@D100:s > @D200:s)
```

If the user asks for a numeric threshold edge, use a spare boolean flag only
when the point table has one:

```text
Rule 1: @D100:s > 10 && !M1000
Action: setBit M1000

Rule 2: @D100:s <= 10
Action: resetBit M1000
```

If no spare flag is available, stop and report that the requested edge cannot
be generated with the current DSL.

## Supported Actions

| Action | Parameters | Notes |
| --- | --- | --- |
| `write` | `address`, `value`, `dataType`, `length` | Value may be a constant, expression, address reference, or array. A text constant must be quoted, e.g. `"\"NG\""`. |
| `copy` | `from`, `fromType`, `address`, `dataType`, `length` | Copies a block. `address` accepts the alias `to`. `length` is required and must be > 0. |
| `setBit` | `address` | Writes boolean true. |
| `resetBit` | `address` | Writes boolean false. |
| `inc` | `address`, `delta` | Integer increment. |
| `dec` | `address`, `delta` | Integer decrement. |
| `delay` | `ms` | 1..10000 ms. Schedules the remaining actions of the rule; it does **not** block the scan (see "Delay"). |
| `log` | `message` | Writes a log entry. `{@Address:type}` is interpolated with an **absolute address** (for example `{@M100:s}`); a point-table tag name such as `{@temperature:s}` is not resolved and produces a read error in the runtime log. |
| `call` | `program` | Runs a stored sub program (see "Sub Programs"). The name must match a stored program / drop-down entry of the same window. |

Array writes use a JSON-like array value:

```json
{
  "type": "write",
  "address": "D100",
  "dataType": "f",
  "length": 3,
  "value": "[1.1,2.2,3.3]"
}
```

The array element count must equal `length`. For the numeric types `length` counts
array elements; for `dataType: "str"` it is a byte count: `length` 0 or omitted
writes exactly the bytes of the text, `length > 0` writes a fixed size field
(padded with `0x00`, truncated when the text is longer).

Action address fields (`address`, `from`, `to`) resolve point-table tag
names first, then absolute addresses. The `dataType` field also accepts
the alias `typeCode`.

Actions write directly into the virtual-server data pool. Modbus
function-code prefixes (`x=`, `w=`) do not restrict writes on the server
side: for example `address: "x=4;100"` writes the input-register area
directly, so the same address can be used both in conditions and as a write
target. Allen-Bradley addresses such as `"B3:0/0"` work the same way.

## Sub Programs (`call`)

A `call` action runs a **stored logic program** once, in place. `program` is the
saved name (the file name without `.logic.json`), not the display name of a
built-in UI example:

```json
{ "type": "call", "program": "RecipeCopy" }
```

Store that program first, otherwise the upload reports `callTargetMissing` and
the call fails at runtime.

Storage: sub programs are the `*.logic.json` files shown in the Logic Program
drop-down of the same window, i.e.
`<Demo base directory>/LogicPrograms/<owner form type name>/<name>.logic.json`.
Manage them through the API, not through the file system:

| Endpoint | Body | Meaning |
| --- | --- | --- |
| `GET/POST /api/logic/programs` | `serverId` | Lists the stored programs (`name`, `ruleCount`, `cycleMs`, `file`), the `programDirectory` and the `missingSubPrograms` of the running session. |
| `POST /api/logic/programs/save` | `serverId`, `name`, `program` | Stores a program (structurally validated). It appears in the drop-down and becomes callable. |
| `GET/POST /api/logic/programs/get` | `serverId`, `name` | Returns one stored program. |
| `POST /api/logic/programs/delete` | `serverId`, `name` | Removes one stored program. |

Semantics:

- When the `call` action fires, every rule of the target is evaluated once, in
  order, with the caller's process image, and its writes are visible to the
  actions after the call in the same scan.
- Sub program rules keep their own TON timers and trigger counters between
  calls; they are not part of the caller's rule table (`ruleCount`).
- Targets are compiled at **upload** time. Store the sub program first, and
  re-upload any caller after editing a target. A target that is not stored is
  not an upload error: it produces the `callTargetMissing` **warning** and logs
  one failure line per activation at runtime. `callTargetMissing` means exactly
  "this call will fail at runtime": only the stored programs of the window (and
  the targets already compiled into the session) count as known, so save the
  target and upload the caller again until the warning is gone.
- Nesting is allowed and resolved breadth-first; recursion is refused at
  runtime (logged, no exception) and the call depth is limited to 8.
- Order and observability: a call writes an entering line and a finishing line
  (with the fired rule count) once per activation of the calling rule, and the
  trace gets `hold`/`resume` events for delays. Logs use the
  `[SubName.ruleId]` prefix for events inside the sub program.
- Because a delayed continuation is owned by the rule (see "Delay"), a delay
  inside a sub program also completes when the caller only calls it once on an
  edge.

## Delay

`delay` does **not** block the scan: it schedules the remaining actions of the
rule and they continue in the first scan at or after `ms` (quantised to
`cycleMs`, so `delay 300` with `cycleMs 50` resumes after 300..350 ms).

```json
{ "type": "delay", "ms": 300 }
```

Rules of use:

- Actions before the delay run immediately; the actions behind it run later, in
  order. Several delays in one rule chain (`1500` then `800` -> the second one
  starts when the first segment finished).
- A delayed continuation belongs to the rule, so it completes even when the
  condition turns false or the rule is disabled. It is dropped by
  `/api/logic/stop` (a stopped program never continues a sequence).
- A rule has at most **one** pending delay. A further trigger while it waits is
  ignored (logged once per activation), so a level condition cannot pile up
  continuations. Expect a hold/resume pair roughly every `ms` for such a rule.
- Use `timeoutMs` (TON) when the intent is "fire N ms after the condition
  becomes true" and `delay` when the intent is a step sequence inside one rule.
- Observability: the runtime log gets
  `delay 300ms: N remaining action(s) continue in a later scan` and
  `delay 300ms elapsed, running the remaining actions`; the trace gets `hold`
  and `resume`; `/api/logic/status` reports `pendingDelays`
  (`ruleId`, `owner`, `delayMs`, `remainMs`, `remainingActions`) and the
  per-rule `pendingDelayMs`. Wait for `pendingDelays` to become empty (or for
  the resume line) before asserting the delayed actions in a test.

## Decomposition Rules

When a user describes a process:

1. List every input, output, state flag, timer and counter.
2. Resolve every address from the virtual-server point table.
3. Express each independent behavior as one rule.
4. Use `rise`/`fall` only for boolean addresses.
5. Use `timeoutMs` for a TON-style delayed action.
6. Use latch flags for numeric threshold edges when spare bits exist.
7. Keep rules ordered so later rules react as expected. Writes reach the device
   immediately, but conditions, expression values and log interpolation use the
   process image read at the beginning of the scan: a value written by a rule is
   seen by the conditions of the NEXT scan. Only `copy`, `inc` and `dec` read the
   device directly and see it in the same scan.
8. Reuse an existing behavior with `call` when a stored sub program already
   describes it; do not duplicate the rules inline. Store the sub program before
   uploading the caller.
9. Do not use arbitrary C#, external scanners, HTTP, database or network
   operations: the engine only executes the action list above.
10. Do not use `delay` for a long wait inside a rule when a TON (`timeoutMs`)
    expresses the same intent; a delay is for a step sequence, not for holding a
    scan.

If any step cannot be expressed with the supported action list, do not invent
a new action. Report the exact step, the reason, and the required future
capability.

Example unsupported response:

```text
无法生成步骤 3：调用外部扫码程序并通过 HTTP 返回条码。
当前逻辑引擎支持 randomEAN13/randomString 等虚拟扫码值，但不支持调用外部
进程或 HTTP 接口。
请将该步骤改为外部程序写入虚拟服务器地址，或等待后续版本支持外部动作。
```

## Testing a Logic Program

After upload and start:

1. Wait at least one `cycleMs`.
2. Set the input through `virtualServer.points.write`.
3. Wait at least one additional scan cycle or the configured `timeoutMs`.
4. Read the output through `virtualServer.points.read`.
5. Check `/api/logic/status` and `/api/logic/log`.

`/api/logic/status` also returns a structured `trace` (newest last, max 200)
and `traceSeq`. Trace events record transitions only: `fired` (first
execution of the current condition-true run, with the `actions` count),
`armed` (condition true, TON `timeoutMs` waiting started), `off` (condition
turned false), `error` (rule exception, with `message`), `hold` (a `delay`
scheduled the remaining actions) and `resume` (the delay expired and the
remaining actions ran). Repeated per-scan executions update `triggerCount` in
the `rules` array without adding trace events, so a complete handshake reads as
`fired` -> `off`.

Status also reports the compiled call targets and the waiting delays:

- `subPrograms`: names compiled into this session (a `call` target that is
  missing appears in `missingSubPrograms` instead).
- `missingSubPrograms`: referenced but not stored targets, reported as the
  `callTargetMissing` check at upload time.
- `pendingDelays` / per-rule `pendingDelayMs`: the `delay` continuations that
  are still waiting. Assert a delayed action by first waiting until it is
  observable in the data pool (or until the rule no longer appears in
  `pendingDelays`); note that a rule whose condition stays true schedules the
  next delay immediately after the previous one resumed, so the rule reappears
  in the list.
- `programDirectory`: the folder of the stored programs of this window.

Both responses include a monotonic `logVersion`. Compare it with the previous
value to detect new runtime logs, especially after the 500-line log limit is
reached.

### Log language

The engine log text follows the Demo window language
(`Program.Language`: 1 = Chinese, 2 = English) and is chosen when the line is
produced. Do not match a log line by an exact English sentence: match on the
`[ruleId]` prefix, on `logVersion`, or on the trace events
(`fired`/`hold`/`resume`/`error`), which are language independent. English and
Chinese examples of the same call line:

```text
call 'RecipeCopy' entered: 2 rule(s).
进入子程序 'RecipeCopy'，共 2 条规则。
```

The message of a `log` action is written verbatim and is not translated.

Example for `M100 -> D100=1234`:

```json
{
  "operation": "virtualServer.points.write",
  "parameters": {
    "serverId": "VIRTUAL_SERVER_GUID",
    "address": "M100",
    "type": "bool",
    "value": true
  }
}
```

Then read:

```json
{
  "operation": "virtualServer.points.read",
  "parameters": {
    "serverId": "VIRTUAL_SERVER_GUID",
    "address": "D100",
    "type": "short"
  }
}
```

The expected result is `1234`.

## UI Relationship

The Demo virtual-server window contains a Logic Program tab. The tab supports:

- Add, edit and delete rules.
- Toggle each rule with the `Enable` checkbox. Disabled rows are shown in dark
  gray and are skipped by the engine.
- Save the current program under
  `<Demo base directory>/LogicPrograms/<FormTypeName>/<name>.logic.json`, load a
  saved program from the drop-down list, and delete a saved program. The
  drop-down is refreshed automatically (about every 2 s), so a program saved
  through `/api/logic/programs/save` appears there without reopening the window.
- Pick the target of a `call` action from that same drop-down (the value field of
  a `call` action becomes a combo box listing the stored programs).
- Import/export `.logic.json`.
- Start/stop the local logic engine.
- Display rule state, trigger count, scan time, watchdog and logs.

In the rule editor, the `length` field of a `write` action defaults to **0** when
the data type is `str` (0 = write the exact byte count of the text) and to 1 for
every other type; a literal text must be quoted (`"NG"`). The field hint under
the value box says so.

AI agents should use `/api/logic/*`, not mouse clicks or direct DataGridView
manipulation. The UI and API share the same `DemoLogicEngineRegistry` session.

# Demo Data Simulation Guide

Use this reference when the user asks to create periodic, step, ramp,
incrementing, random, sine-wave or custom expression-driven data in a Demo
virtual server or on an already connected real device.

This feature writes values periodically through `DataSimulateControl`. It is
different from:

- The logic program engine, which reacts to conditions and executes rules.
- One-off point writes through `virtualServer.points.write` or `/api/write`.
- `DataTableControl`, which stores point definitions but does not generate
  periodic values.

For a real device, the simulation uses the original connection opened by the
Demo device page. It does not create a second hidden PLC connection. Every
generated value is written to the real device, so use it only for an explicit
test request.

## API Operations

All operations use `POST {base}/api/operations`:

Virtual-server simulation:

```text
virtualServer.simulate.list
virtualServer.simulate.import
virtualServer.simulate.export
virtualServer.simulate.start
virtualServer.simulate.stop
```

Connected real-device simulation:

```text
device.simulate.list
device.simulate.import
device.simulate.export
device.simulate.start
device.simulate.stop
```

The virtual-server form uses `serverId`; the connected-device form uses
`deviceId` from `GET {base}/api/devices`.

All import, start and stop operations require the Demo AI Server write switch.
List and export operations are read-only.

### List or Export

```json
{
  "operation": "virtualServer.simulate.list",
  "parameters": {
    "serverId": "VIRTUAL_SERVER_GUID"
  }
}
```

`virtualServer.simulate.export` is a direct alias of the same operation.
`device.simulate.export` is the corresponding alias for a connected device.

### Import

```json
{
  "operation": "virtualServer.simulate.import",
  "parameters": {
    "serverId": "VIRTUAL_SERVER_GUID",
    "mode": "replace",
    "items": [
      {
        "name": "square-wave",
        "address": "D100",
        "time": 1000,
        "expression": "(short)(x%2==0?10:0)",
        "mark": "period 1s"
      }
    ]
  }
}
```

Import modes:

- `replace`: replace all simulation rows.
- `append`: append rows.
- `upsert`: update rows with the same address, append missing addresses.

Stop the simulation before replacing its rows.

For a connected real device, use the same item structure and change only the
owner and operation:

```json
{
  "operation": "device.simulate.import",
  "parameters": {
    "deviceId": "DEVICE_GUID",
    "mode": "replace",
    "items": [
      {
        "name": "D100-square-wave",
        "address": "D100",
        "time": 1000,
        "expression": "(short)(x%2==0?10:0)"
      }
    ]
  }
}
```

### Start and Stop

```json
{
  "operation": "virtualServer.simulate.start",
  "parameters": {
    "serverId": "VIRTUAL_SERVER_GUID"
  }
}
```

```json
{
  "operation": "virtualServer.simulate.stop",
  "parameters": {
    "serverId": "VIRTUAL_SERVER_GUID"
  }
}
```

Starting the simulation reuses the same validation and worker thread as the
Demo UI `Start` button. Stopping reuses the same stop path.

For a connected device, use the same request with
`device.simulate.start` or `device.simulate.stop` and replace `serverId` with
`deviceId`.

**An empty table is refused.** `simulate.start` fails when the table has no usable row:

```json
{ "success": false, "message": "The simulation table is empty. Add at least one row with an address before starting." }
```

(The message follows the Demo language; the Chinese build says
`模拟数据表为空，请先添加至少一行带地址的数据再启动。`)

Import the rows first, then start. A row counts only when it has an address
(the `Address` cell), so a table holding just a blank new row is still "empty".
Before this check existed the call answered `success: true`,
`isSimulationRunning` became `true`, and yet nothing was ever written - which
looks exactly like a broken simulation. If you see `success: false` with this
message, the fix is to import at least one row, not to retry.

## Simulation Item Fields

| Field | Required | Meaning |
| --- | --- | --- |
| `name` | No | Display name. Defaults to the address. |
| `address` | Yes | Address valid for the selected virtual server or real device, such as `D100` or `M100`. |
| `time` | No | Write interval in milliseconds. Default is 100. |
| `expression` | Yes | DynamicExpresso expression. |
| `mark` | No | Description. |
| `encoding` | No | String encoding for string results: ASCII, UTF16, UTF8 or GB2312. |

The expression has two built-in variables:

- `x`: starts at 0 and increments after every successful write.
- `r`: shared `Random` instance from `HslHelper.HslRandom`.

The first write occurs immediately because the runtime initializes the previous
execution time to one day in the past.

### `import` does not validate the rows (verified on the bench)

`virtualServer.simulate.import` / `device.simulate.import` only reject a **missing
`address`** (`gw_param_missing`). Everything else is stored as sent and answered
with `success: true`:

- a syntactically invalid `expression` such as `"这不是表达式("` is accepted;
- an out-of-range `time` such as `99999999` is accepted as-is;
- after `simulate.start` the status still reports `running: true` with an empty
  `lastError`, so **a successful import/start is not evidence that the rows can
  actually run**.

Therefore validate your own expressions before importing: keep `time` within
100..3600000, and confirm the simulated address really changes by reading it back
after at least one `time` interval. If the value never changes, suspect the
expression rather than the gateway.

## Expression Examples

The expressions below are the examples used by the Demo expression-example
dialog.

Square wave:

```text
(short)(x%2==0?10:0)
```

Sawtooth or linear ramp:

```text
(short)(x%100)
```

Sine wave:

```text
(short)(Math.Sin(2*Math.PI*x/100)*100)
```

Random value:

```text
(short)r.Next(100,200)
```

Sine wave with random jitter:

```text
(short)(Math.Sin(2*Math.PI*x/100)*100+100+r.Next(6))
```

Barcode-like string:

```text
"A"+(x%100).ToString().PadLeft(7,'0')
```

Boolean step:

```text
x%2==0
```

Incrementing integer:

```text
(int)x
```

## AI Workflow

When the user asks for simulated data:

1. Decide whether the target is a Demo virtual server or an already connected
   real device.
2. For a virtual server, resolve it with `GET {base}/api/servers` and ensure it
   has been started.
3. For a real device, resolve it with `GET {base}/api/devices` and require
   `simulationAvailable=true`.
4. Read existing rows with `virtualServer.simulate.list` or
   `device.simulate.list`.
5. Convert the requested signal into one or more simulation items.
6. Import them with `virtualServer.simulate.import` or
   `device.simulate.import`.
7. Start with the matching `*.simulate.start` operation.
8. Verify through `virtualServer.points.read`, or `/api/read` for a real
   device.
9. Stop with the matching `*.simulate.stop` operation.

For example, to create a one-second square wave on `D100`:

```json
{
  "operation": "virtualServer.simulate.import",
  "parameters": {
    "serverId": "VIRTUAL_SERVER_GUID",
    "mode": "replace",
    "items": [
      {
        "name": "D100-square-wave",
        "address": "D100",
        "time": 1000,
        "expression": "(short)(x%2==0?10:0)"
      }
    ]
  }
}
```

Then start and verify:

```text
virtualServer.simulate.start
virtualServer.points.read D100 short
```

Wait at least one `time` interval and read again. The value should alternate
between the two configured values.

For a real device, replace `serverId` with the `deviceId`, use
`device.simulate.*`, and verify with:

```text
GET /api/read?device=DEVICE_GUID&address=D100&type=short
```

Do not start real-device simulation if the resolved address is uncertain or if
the write switch is disabled. `device.simulate.list` and
`device.simulate.export` can be used without the write switch.

## Runtime Behavior

- The simulation worker scans the table roughly every 20 ms.
- Each row is written only when its individual `time` interval has elapsed.
- A slow write to one address delays later rows in that same simulation table.
- The runtime writes values serially on one worker thread.
- The table can contain numeric, boolean, byte, array and string expressions.
- A string result uses the row's `encoding` value.
- The current value is displayed in the Demo table's CurrentValue column.
- `virtualServer.status` reports `simulationRunning` and `simulationCount`;
  `simulationCount` is the number of simulation rows, not an execution counter.

## Limitations

- A simulation row and a logic program rule may drive the same address. Nothing
  rejects that combination: both write during the same cycle and the last writer
  of the cycle wins, so the value you read alternates between them. Use different
  addresses unless the overlap is the point of the test.
- Addresses must match the selected virtual-server or real-device protocol.
  For example, `D100` is valid for a MC target but not automatically valid for
  an S7 target.
- The simulation engine does not call scanners, HTTP, databases or MES.
- The simulation table is associated with the original Demo virtual-server or
  device control. It is not a general external process scheduler.
- Long-running or very short intervals should be used with care because all
  rows share one worker thread.

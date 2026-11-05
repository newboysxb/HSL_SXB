# ModbusTcpNet address formats

The address string supports station number and function code prefixes.
Prefixes must appear before the numeric address and are separated by semicolons.

Common examples:

```text
100          holding register 100, function code 03
s=2;100      holding register 100 on station 2
x=4;100      input register 100, function code 04
x=2;100      discrete input 100, function code 02
100H         holding register address 100 in hexadecimal, equal to decimal 256
s=2;x=4;100  input register 100 on station 2
format=DCBA;100 typed reads/writes force DCBA byte order
w=16;100     force Write(string, short) to use function code 0x10
x=7;w=8;100  custom function code: read with 0x07, write with 0x08
file=0;100   file register 100 in file 0, read with 0x14, write with 0x15
```

Starting with v13, `x=6;0` is normalized automatically:

```text
Read("x=6;0")             uses function code 03
Write("x=6;0", value)     uses function code 06
```

Use this form only when the target library behavior is the intended one;
`x=6` is not a distinct Modbus read function code.

Coil and discrete reads:

```text
ReadCoil("100")       function code 01
ReadDiscrete("x=2;100") function code 02
```

Register bit addresses use a decimal point:

```text
ReadBool("100.0")
ReadBool("100.1")
Write("100.2", true)
```

For typed reads and writes, a data format can be specified with `format=`:

```text
format=ABCD;100
format=DCBA;100
format=BADC;100
format=CDAB;100
```

Do not put spaces in the address string.

## Kepware / legacy Modbus address conversion

Legacy notation uses a leading digit to identify the Modbus area:

| Legacy address | Meaning | HslCommunication address |
| --- | --- | --- |
| `000001` | Coil, offset 0 | `0` |
| `100001` | Discrete input, offset 0 | `x=2;0` |
| `300001` | Input register, offset 0 | `x=4;0` |
| `400001` | Holding register, offset 0 | `0` |

The suffix is normally a 0-based offset. If the device uses 1-based offsets,
keep `AddressStartWithZero = false` and enter the register number directly;
for example `1` maps to wire offset `0`.

## Full ModbusTcpNet address examples from HslCommunicationDemo

This table mirrors `TestProject/HslCommunicationDemo/Modbus/Helper.cs`.

| Address | Kind | Notes |
| --- | --- | --- |
| `100` | Coil bool | Some devices display this as `000100` |
| `x=2;100` | Discrete input bool | Some devices display this as `100100` |
| `100` | Holding register word | Some devices display this as `400100` |
| `100H` | Holding register word | `H` suffix means hexadecimal address, `100H` is decimal `256` |
| `x=4;100` | Input register word | Some devices display this as `300100` |
| `100.1` | Register bool operation | Write uses mask function code `0x16` when supported |
| `s=2;100` | Coil bool | Station number can be carried in the address |
| `s=2;100` | Holding register word | Station number can be carried in the address |
| `format=DCBA;100` | Holding register word | Force byte order for int, uint, long, ulong, float, double |
| `s=2;x=4;100` | Input register word | Read input register `100` from station `2` |
| `w=16;100` | Holding register word | `Write(string, short)` uses function code `0x10` |
| `x=7;w=8;100` | Custom function code | Read with `0x07`, write with `0x08` |
| `file=0;100` | File register word | Read with `0x14`, write with `0x15`; `file` is the file number |

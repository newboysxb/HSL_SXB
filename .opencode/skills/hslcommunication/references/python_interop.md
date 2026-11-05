# Calling HslCommunication from Python

The official HslCommunication Python package only provides a small subset of
the functionality. To use the full C# version of HslCommunication from Python,
load the .NET assembly with `pythonnet`.

## Prerequisites

- Windows with .NET Framework 4.5 or later.
- Install `pythonnet`:

```text
pip install pythonnet
```

- Put `HslCommunication.dll` and `Newtonsoft.Json.dll` in the same folder.
- Use an absolute path in `clr.AddReference` and omit the `.dll` extension.
- Match the Python bitness with the installed .NET runtime. For example, use
  64-bit Python when loading the 64-bit .NET Framework runtime.

## Load the assembly

```python
import clr

clr.AddReference(r"C:\path\to\HslCommunication")

from HslCommunication.ModBus import ModbusTcpNet
from HslCommunication import Authorization
```

After loading, import the same protocol namespaces used by the C# protocol
reference. For example:

```python
from HslCommunication.Profinet.Melsec import MelsecMcNet
from HslCommunication.Profinet.Siemens import SiemensS7Net
```

## Result handling

Every read or write returns an `OperateResult` object:

```python
result = modbus.ReadInt16("100")
if result.IsSuccess:
    print(result.Content)
else:
    print(result.Message)
```

Array results are .NET arrays. Convert them to Python lists before printing:

```python
values = [int(x) for x in modbus.ReadInt16("100", 5).Content]
```

## Connect and close

Network clients use `ConnectServer()` and `ConnectClose()`:

```python
modbus = ModbusTcpNet("192.168.1.10", 502)
modbus.ConnectServer()

try:
    print(modbus.ReadInt16("100").Content)
finally:
    modbus.ConnectClose()
```

Serial clients use `SerialPortInni(...)` and `Open()` instead. Follow the
connection rules in the selected protocol reference.

## Read methods

The read methods keep the same names and overloads as C#:

```python
modbus.Read("100", 10)               # byte[]
modbus.ReadBool("100")               # bool
modbus.ReadBool("100", 10)           # bool[]
modbus.ReadCoil("100")               # bool
modbus.ReadCoil("100", 10)           # bool[]
modbus.ReadDiscrete("100")           # bool
modbus.ReadDiscrete("100", 10)       # bool[]
modbus.ReadInt16("100")              # short
modbus.ReadInt16("100", 10)          # short[]
modbus.ReadUInt16("100")             # ushort
modbus.ReadUInt16("100", 10)         # ushort[]
modbus.ReadInt32("100")              # int
modbus.ReadInt32("100", 10)          # int[]
modbus.ReadUInt32("100")             # uint
modbus.ReadUInt32("100", 10)         # uint[]
modbus.ReadFloat("100")              # float
modbus.ReadFloat("100", 10)          # float[]
modbus.ReadDouble("100")             # double
modbus.ReadDouble("100", 10)         # double[]
modbus.ReadInt64("100")              # long
modbus.ReadInt64("100", 10)          # long[]
modbus.ReadUInt64("100")             # ulong
modbus.ReadUInt64("100", 10)         # ulong[]
modbus.ReadString("100", 10)         # string
```

## Write overloads

Python cannot distinguish C# overloads by primitive type. Use
`__overloads__` and the `System` CLR types:

```python
from System import Boolean, Byte, Double, Int16, Int32, Int64, Single, String, UInt16, UInt32, UInt64, Array

modbus.Write.__overloads__[(String, Int16,)]( "100", Int16(4567) )
modbus.Write.__overloads__[(String, Single,)]( "100", Single(1.23) )
modbus.Write.__overloads__[(String, Double,)]( "100", Double(1.23) )
modbus.Write.__overloads__[(String, String,)]( "100", "ABCD" )
```

C# type names map to `System` types:

| C# type | Python type |
| --- | --- |
| `bool` | `Boolean` |
| `byte` | `Byte` |
| `short` | `Int16` |
| `ushort` | `UInt16` |
| `int` | `Int32` |
| `uint` | `UInt32` |
| `long` | `Int64` |
| `ulong` | `UInt64` |
| `float` | `Single` |
| `double` | `Double` |
| `string` | `String` |

Python `float` is not C# `float`; use `Single` for `float` and `Double` for
`double`.

## Arrays

Create a typed .NET array with `Array.CreateInstance`, then fill it:

```python
def create_array(values, clr_type):
    array = Array.CreateInstance(clr_type, len(values))
    for i, value in enumerate(values):
        array[i] = clr_type(value)
    return array

short_array = create_array([1, 2, -1, 1234], Int16)
modbus.Write.__overloads__[(String, Array[Int16],)]( "100", short_array )
```

This pattern works for `Boolean[]`, `Byte[]`, `Int16[]`, `UInt16[]`,
`Int32[]`, `UInt32[]`, `Int64[]`, `UInt64[]`, `Single[]`, and `Double[]`.

## Raw byte parsing

Use `Read(string, length)` and `ByteTransform`:

```python
from System.Text import Encoding

read = modbus.Read("100", 10)
if read.IsSuccess:
    count = modbus.ByteTransform.TransInt32(read.Content, 0)
    temp = modbus.ByteTransform.TransSingle(read.Content, 4)
    name1 = modbus.ByteTransform.TransInt16(read.Content, 8)
    barcode = Encoding.ASCII.GetString(read.Content, 10, 10)
```

## Authorization

Activation methods stay the same as C#:

```python
active = Authorization.SetAuthorizationCode("your-code")
print("active" if active else "activation failed")

result = Authorization.SetHslCertificate(Convert.FromBase64String("base64-cert"))
if result.IsSuccess:
    print("activation succeeded")
else:
    print(result.Message)
```

## Notes

- Use the protocol reference for the exact address syntax and methods.
- The C# API list is authoritative; Python only changes how overloads and
  arrays are called.
- `__overloads__` is only needed when the method has multiple C# overloads,
  such as `Write`.
- See `examples/python_call_hsl_modbus.py` for a complete Modbus example.

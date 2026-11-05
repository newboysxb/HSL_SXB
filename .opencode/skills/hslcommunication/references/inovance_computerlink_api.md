# Inovance Computer Link API subset

`InovanceComputerLink` inherits `MelsecFxLinks` and implements the Inovance
computer-link serial protocol. The class default `Format` is `4`.

Before using the PLC COM1 port, configure `D8126=06h` and `D8120.14=1` on the
PLC side.

Namespace:

```csharp
using HslCommunication.Profinet.Inovance;
```

## Constructor

```csharp
new InovanceComputerLink()
```

## Properties

```csharp
byte Station
byte WaittingTime
bool SumCheck
int Format
IByteTransform ByteTransform
ILogNet LogNet
bool AutoReConnect
```

`ByteTransform` is `RegularByteTransform` and defaults to `DataFormat.DCBA`.
`SumCheck` defaults to `true`. `Station` and `WaittingTime` default to `0`.

## Connection

`InovanceComputerLink` is serial only.

```csharp
InovanceComputerLink plc = new InovanceComputerLink
{
    Station = 0,
    SumCheck = true,
    Format = 4
};

plc.SerialPortInni("COM3", 9600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
OperateResult open = plc.Open();
if (open.IsSuccess)
{
    OperateResult<byte[]> read = plc.Read("D100", 1);
}

plc.Close();
```

Use `Open()` after `SerialPortInni(...)`, not `ConnectServer()`.

## Address examples

The examples match the HslCommunicationDemo Inovance Computer Link address
declarations, which reuse the Melsec FxLinks address list.

| Address | Meaning | Notes |
| --- | --- | --- |
| `M0` | Internal relay | Bit/word |
| `X0` | Input relay | Octal address |
| `Y0` | Output relay | Octal address |
| `S0` | Step relay | Bit/word |
| `TS0` | Timer contact | Bit |
| `CS0` | Counter contact | Bit |
| `D0` | Data register | Word; use `D100.0` for bit access |
| `R0` | File register | Word |
| `TN0` | Timer current value | Word |
| `CN0` | Counter current value | Word |
| `s=2;D0` | Data register | Carries an explicit station number |

This client inherits the MELSEC FX link address parser, whose X/Y addresses
are converted with base 8. `X10` means PLC `X10` (decimal 8), not `0x10`.
Do not apply the Mitsubishi MC leading-zero convention here.

## Read methods

```csharp
OperateResult<byte[]> Read(string address, ushort length)
OperateResult<bool[]> ReadBool(string address, ushort length)
OperateResult<bool> ReadBool(string address)
OperateResult<short> ReadInt16(string address)
OperateResult<short[]> ReadInt16(string address, ushort length)
OperateResult<ushort> ReadUInt16(string address)
OperateResult<ushort[]> ReadUInt16(string address, ushort length)
OperateResult<int> ReadInt32(string address)
OperateResult<int[]> ReadInt32(string address, ushort length)
OperateResult<uint> ReadUInt32(string address)
OperateResult<uint[]> ReadUInt32(string address, ushort length)
OperateResult<float> ReadFloat(string address)
OperateResult<float[]> ReadFloat(string address, ushort length)
OperateResult<double> ReadDouble(string address)
OperateResult<double[]> ReadDouble(string address, ushort length)
OperateResult<long> ReadInt64(string address)
OperateResult<long[]> ReadInt64(string address, ushort length)
OperateResult<ulong> ReadUInt64(string address)
OperateResult<ulong[]> ReadUInt64(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length)
OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
```

## Write methods

```csharp
OperateResult Write(string address, byte[] value)
OperateResult Write(string address, bool value)
OperateResult Write(string address, bool[] values)
OperateResult Write(string address, short value)
OperateResult Write(string address, short[] values)
OperateResult Write(string address, ushort value)
OperateResult Write(string address, ushort[] values)
OperateResult Write(string address, int value)
OperateResult Write(string address, int[] values)
OperateResult Write(string address, uint value)
OperateResult Write(string address, uint[] values)
OperateResult Write(string address, float value)
OperateResult Write(string address, float[] values)
OperateResult Write(string address, double value)
OperateResult Write(string address, double[] values)
OperateResult Write(string address, long value)
OperateResult Write(string address, long[] values)
OperateResult Write(string address, ulong value)
OperateResult Write(string address, ulong[] values)
OperateResult Write(string address, string value)
```

## PLC control methods

```csharp
OperateResult StartPLC(string parameter = "")
OperateResult StopPLC(string parameter = "")
OperateResult<string> ReadPlcType(string parameter = "")
```

`StartPLC`, `StopPLC`, and `ReadPlcType` require commercial authorization and
will return `InsufficientPrivileges` when the library is not activated.

## Async methods

Async core and typed methods from `DeviceCommunication` and the inherited
`ReadBoolAsync(address, length)` overload are available.

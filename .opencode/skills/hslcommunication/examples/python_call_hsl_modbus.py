import clr

from System import Array, Boolean, Byte, Convert, Double, Int16, Int32, Int64, Single, String, UInt16, UInt32, UInt64
from System.Text import Encoding

# Put HslCommunication.dll and Newtonsoft.Json.dll in the same folder.
# The path must not include the .dll extension.
clr.AddReference(r"C:\path\to\HslCommunication")

from HslCommunication import Authorization
from HslCommunication.ModBus import ModbusTcpNet


def print_single_result(result):
    if result.IsSuccess:
        print("Read result: " + str(result.Content))
    else:
        print("Read failed: " + result.Message)


def print_array_result(result):
    if result.IsSuccess:
        print("Read result: " + str([int(x) for x in result.Content]))
    else:
        print("Read failed: " + result.Message)


def create_array(values, clr_type):
    array = Array.CreateInstance(clr_type, len(values))
    for i, value in enumerate(values):
        array[i] = clr_type(value)
    return array


modbus = ModbusTcpNet("192.168.1.10", 502)
connect = modbus.ConnectServer()
if not connect.IsSuccess:
    print("Connect failed: " + connect.Message)
    raise SystemExit(1)

print_single_result(modbus.ReadInt16("100"))
print_array_result(modbus.ReadInt16("100", 5))

modbus.Write.__overloads__[(String, Boolean,)]("100", Boolean(True))
modbus.Write.__overloads__[(String, Array[Boolean],)]("100", create_array([True, False, True], Boolean))
modbus.Write.__overloads__[(String, Int16,)]("100", Int16(4567))
modbus.Write.__overloads__[(String, Array[Int16],)]("100", create_array([1, 2, -1, 1234], Int16))
modbus.Write.__overloads__[(String, UInt16,)]("100", UInt16(50001))
modbus.Write.__overloads__[(String, Array[UInt16],)]("100", create_array([4567, 2, 60000], UInt16))
modbus.Write.__overloads__[(String, Int32,)]("100", Int32(1234567))
modbus.Write.__overloads__[(String, Array[Int32],)]("100", create_array([12345, -12334, 4561316], Int32))
modbus.Write.__overloads__[(String, UInt32,)]("100", UInt32(3200001234))
modbus.Write.__overloads__[(String, Array[UInt32],)]("100", create_array([1, 100, 4100001234], UInt32))
modbus.Write.__overloads__[(String, Int64,)]("100", Int64(1234567890123))
modbus.Write.__overloads__[(String, Array[Int64],)]("100", create_array([1, 100, 1234567890123], Int64))
modbus.Write.__overloads__[(String, UInt64,)]("100", UInt64(1234567890123))
modbus.Write.__overloads__[(String, Array[UInt64],)]("100", create_array([1678, 1234, 1234567890123], UInt64))
modbus.Write.__overloads__[(String, Single,)]("100", Single(1.23))
modbus.Write.__overloads__[(String, Array[Single],)]("100", create_array([1.1, -2.2, 100], Single))
modbus.Write.__overloads__[(String, Double,)]("100", Double(1.23))
modbus.Write.__overloads__[(String, Array[Double],)]("100", create_array([1.1, -2.2, 100], Double))
modbus.Write.__overloads__[(String, String,)]("100", "ABCD")
modbus.Write.__overloads__[(String, Array[Byte],)]("100", create_array([0x12, 0x34, 0x56, 0x78], Byte))

read = modbus.Read("100", 10)
if read.IsSuccess:
    count = modbus.ByteTransform.TransInt32(read.Content, 0)
    temp = modbus.ByteTransform.TransSingle(read.Content, 4)
    name1 = modbus.ByteTransform.TransInt16(read.Content, 8)
    barcode = Encoding.ASCII.GetString(read.Content, 10, 10)
    print("Count: " + str(count))
    print("Temp: " + str(temp))
    print("Name1: " + str(name1))
    print("Barcode: " + barcode)

# Optional activation. SetAuthorizationCode returns bool.
# active = Authorization.SetAuthorizationCode("your-code")
# print("active" if active else "activation failed")

# Optional certificate activation. SetHslCertificate returns OperateResult.
# result = Authorization.SetHslCertificate(Convert.FromBase64String("base64-cert"))
# print(result.IsSuccess)

modbus.ConnectClose()

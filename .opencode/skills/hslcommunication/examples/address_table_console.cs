using System;
using System.Collections.Generic;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.ModBus;

public class ModbusTag
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string DataType { get; set; }
    public int Length { get; set; }
}

public static class AddressTableConsoleExample
{
    public static void Run()
    {
        ModbusTcpNet modbus = new ModbusTcpNet("192.168.1.10", 502, station: 1)
        {
            ConnectTimeOut = 3000,
            ReceiveTimeOut = 3000
        };

        OperateResult connect = modbus.ConnectServer();
        if (!connect.IsSuccess)
        {
            Console.WriteLine("Connect failed: " + connect.Message);
            return;
        }

        List<ModbusTag> tags = new List<ModbusTag>
        {
            new ModbusTag { Name = "temperature", Address = "s=1;100", DataType = "float", Length = 1 },
            new ModbusTag { Name = "running", Address = "s=1;1000", DataType = "bool", Length = 1 },
            new ModbusTag { Name = "barcode", Address = "s=1;200", DataType = "string", Length = 8 }
        };

        foreach (ModbusTag tag in tags)
        {
            ReadTag(modbus, tag);
        }

        modbus.ConnectClose();
    }

    private static void ReadTag(ModbusTcpNet modbus, ModbusTag tag)
    {
        switch (tag.DataType.ToLowerInvariant())
        {
            case "bool":
                OperateResult<bool[]> bools = modbus.ReadBool(tag.Address, (ushort)tag.Length);
                Console.WriteLine(tag.Name + ": " + (bools.IsSuccess ? string.Join(",", bools.Content) : bools.Message));
                break;

            case "float":
                OperateResult<float[]> floats = modbus.ReadFloat(tag.Address, (ushort)tag.Length);
                Console.WriteLine(tag.Name + ": " + (floats.IsSuccess ? string.Join(",", floats.Content) : floats.Message));
                break;

            case "string":
                OperateResult<string> text = modbus.ReadString(tag.Address, (ushort)tag.Length);
                Console.WriteLine(tag.Name + ": " + (text.IsSuccess ? text.Content : text.Message));
                break;

            default:
                Console.WriteLine(tag.Name + ": unsupported dataType " + tag.DataType);
                break;
        }
    }
}

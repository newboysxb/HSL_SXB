using System;
using HslCommunication;
using HslCommunication.ModBus;
using HslCommunication.Reflection;

public class DeviceData
{
    [HslDeviceAddress("100")]
    public short Temperature { get; set; }

    [HslDeviceAddress("102")]
    public float Pressure { get; set; }

    [HslDeviceAddress("1000")]
    public bool Running { get; set; }
}

public static class ModbusTcpAttributeReadExample
{
    public static void Run()
    {
        ModbusTcpNet modbus = new ModbusTcpNet("192.168.1.10", 502, station: 1);

        OperateResult connect = modbus.ConnectServer();
        if (!connect.IsSuccess)
        {
            Console.WriteLine("Connect failed: " + connect.Message);
            return;
        }

        OperateResult<DeviceData> read = modbus.Read<DeviceData>();
        if (read.IsSuccess)
        {
            Console.WriteLine("Temperature: " + read.Content.Temperature);
            Console.WriteLine("Pressure: " + read.Content.Pressure);
            Console.WriteLine("Running: " + read.Content.Running);
        }
        else
        {
            Console.WriteLine("Read failed: " + read.Message);
        }

        modbus.ConnectClose();
    }
}

using System;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.ModBus;

public static class ModbusTcpConsoleExample
{
    private static ModbusTcpNet modbus;
    private static System.Threading.Timer timer;

    public static void Run()
    {
        modbus = new ModbusTcpNet("192.168.1.10", 502, station: 1)
        {
            ConnectTimeOut = 3000,
            ReceiveTimeOut = 3000,
            DataFormat = DataFormat.CDAB,
            AddressStartWithZero = true
        };

        OperateResult connect = modbus.ConnectServer();
        if (!connect.IsSuccess)
        {
            Console.WriteLine("Connect failed: " + connect.Message);
            return;
        }

        timer = new System.Threading.Timer(ReadValues, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

        timer.Dispose();
        modbus.ConnectClose();
    }

    private static void ReadValues(object state)
    {
        OperateResult<float> temperature = modbus.ReadFloat("s=1;100");
        if (temperature.IsSuccess)
        {
            Console.WriteLine("Temperature: " + temperature.Content);
        }
        else
        {
            Console.WriteLine("Read failed: " + temperature.Message);
        }
    }
}

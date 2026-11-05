using System;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.LogNet;
using HslCommunication.ModBus;

public static class ModbusTcpLoggingConsoleExample
{
    public static void Run()
    {
        LogNetSingle logNet = new LogNetSingle("");
        logNet.BeforeSaveToFile += (sender, e) =>
        {
            Console.WriteLine(e.HslMessage.Text);
        };

        ModbusTcpNet modbus = new ModbusTcpNet("192.168.1.10", 502, station: 1)
        {
            ConnectTimeOut = 3000,
            ReceiveTimeOut = 3000,
            DataFormat = DataFormat.CDAB,
            LogNet = logNet
        };

        OperateResult connect = modbus.ConnectServer();
        if (!connect.IsSuccess)
        {
            Console.WriteLine("Connect failed: " + connect.Message);
            return;
        }

        OperateResult<float> read = modbus.ReadFloat("s=1;100");
        if (read.IsSuccess)
        {
            Console.WriteLine("Value: " + read.Content);
        }
        else
        {
            Console.WriteLine("Read failed: " + read.Message);
        }

        modbus.ConnectClose();
    }
}

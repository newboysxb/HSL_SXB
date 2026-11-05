using System;
using System.Text;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.ModBus;

public static class ModbusTcpBatchReadExample
{
    public static void Run()
    {
        ModbusTcpNet modbus = new ModbusTcpNet("192.168.1.10", 502, station: 1)
        {
            ConnectTimeOut = 3000,
            ReceiveTimeOut = 3000,
            DataFormat = DataFormat.CDAB
        };

        OperateResult connect = modbus.ConnectServer();
        if (!connect.IsSuccess)
        {
            Console.WriteLine("Connect failed: " + connect.Message);
            return;
        }

        // Read 10 holding registers starting at 100.
        // The returned byte[] length is 20 bytes.
        OperateResult<byte[]> read = modbus.Read("s=1;100", 10);
        if (read.IsSuccess)
        {
            int count = modbus.ByteTransform.TransInt32(read.Content, 0);
            float temperature = modbus.ByteTransform.TransSingle(read.Content, 4);
            short alarm = modbus.ByteTransform.TransInt16(read.Content, 8);
            string barcode = Encoding.ASCII.GetString(read.Content, 10, 10);

            Console.WriteLine("Count: " + count);
            Console.WriteLine("Temperature: " + temperature);
            Console.WriteLine("Alarm: " + alarm);
            Console.WriteLine("Barcode: " + barcode);
        }
        else
        {
            Console.WriteLine("Read failed: " + read.Message);
        }

        modbus.ConnectClose();
    }
}

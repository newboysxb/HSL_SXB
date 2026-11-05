using System;
using HslCommunication;
using HslCommunication.Profinet.Melsec;

public static class MelsecMcConsoleExample
{
    public static void Run()
    {
        MelsecMcNet melsec = new MelsecMcNet("192.168.1.10", 6000)
        {
            ConnectTimeOut = 3000,
            ReceiveTimeOut = 3000
        };

        OperateResult connect = melsec.ConnectServer();
        if (!connect.IsSuccess)
        {
            Console.WriteLine("Connect failed: " + connect.Message);
            return;
        }

        OperateResult<short> readD = melsec.ReadInt16("D100");
        if (readD.IsSuccess)
        {
            Console.WriteLine("D100: " + readD.Content);
        }
        else
        {
            Console.WriteLine(readD.Message);
        }

        OperateResult<bool> readM = melsec.ReadBool("M100");
        if (readM.IsSuccess)
        {
            Console.WriteLine("M100: " + readM.Content);
        }
        else
        {
            Console.WriteLine(readM.Message);
        }

        OperateResult<byte[]> random = melsec.ReadRandom(new string[] { "D100", "W200", "R300" });
        if (random.IsSuccess)
        {
            short d100 = melsec.ByteTransform.TransInt16(random.Content, 0);
            short w200 = melsec.ByteTransform.TransInt16(random.Content, 2);
            short r300 = melsec.ByteTransform.TransInt16(random.Content, 4);

            Console.WriteLine(d100);
            Console.WriteLine(w200);
            Console.WriteLine(r300);
        }
        else
        {
            Console.WriteLine(random.Message);
        }

        melsec.ConnectClose();
    }
}

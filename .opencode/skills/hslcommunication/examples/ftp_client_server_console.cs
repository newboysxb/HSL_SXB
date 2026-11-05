using System;
using System.IO;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Enthernet.Ftp;

public static class FtpClientServerConsoleExample
{
    public static void Run()
    {
        string root = Path.Combine(Path.GetTempPath(), "HslFtpServer");
        Directory.CreateDirectory(root);

        FtpServer ftpServer = new FtpServer
        {
            CurrentDirectory = root,
            AllowAnonymous = true,
            DownloadOnly = false
        };

        ftpServer.SetPasvPortRange("20000-20010");
        ftpServer.CheckAccountFunction = (name, pwd, session) =>
        {
            if (name == "admin" && pwd == "123456")
            {
                return OperateResult.CreateSuccessResult();
            }

            return new OperateResult(500, "user account or password wrong");
        };

        try
        {
            ftpServer.ServerStart(12345);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Server start failed: " + ex.Message);
            return;
        }

        FtpClient ftpClient = new FtpClient
        {
            IpAddress = "127.0.0.1",
            Port = 12345,
            Username = "admin",
            Password = "123456"
        };

        OperateResult connect = ftpClient.ConnectServer();
        Console.WriteLine("Connect: " + (connect.IsSuccess ? "success" : connect.Message));
        if (connect.IsSuccess)
        {
            Console.WriteLine("System type: " + ftpClient.SystemType);
        }

        OperateResult<FtpFileItem[]> list = ftpClient.ListFiles("/");
        if (list.IsSuccess)
        {
            foreach (FtpFileItem item in list.Content)
            {
                string info = item.IsDirectory ? "Directory" : "File";
                Console.WriteLine(info + ": " + item.Name + ", size: " + item.Size);
            }
        }
        else
        {
            Console.WriteLine("List failed: " + list.Message);
        }

        string localFile = Path.Combine(Path.GetTempPath(), "hsl_ftp_sample.txt");
        File.WriteAllText(localFile, "hello hsl ftp");

        HslCancelToken cancelToken = new HslCancelToken();
        OperateResult upload = ftpClient.UploadFile(localFile, "/sample.txt", ReportProgress, cancelToken);
        Console.WriteLine("Upload: " + (upload.IsSuccess ? "success" : upload.Message));

        string downloadedFile = Path.Combine(Path.GetTempPath(), "hsl_ftp_sample_downloaded.txt");
        OperateResult download = ftpClient.DownloadFile("/sample.txt", downloadedFile, ReportProgress, cancelToken);
        Console.WriteLine("Download: " + (download.IsSuccess ? "success" : download.Message));

        OperateResult rename = ftpClient.RenameFile("/sample.txt", "sample2.txt");
        Console.WriteLine("Rename file: " + (rename.IsSuccess ? "success" : rename.Message));

        OperateResult createDirectory = ftpClient.CreateDirectory("/test");
        Console.WriteLine("Create directory: " + (createDirectory.IsSuccess ? "success" : createDirectory.Message));

        OperateResult renameDirectory = ftpClient.RenameFolder("/test", "test2");
        Console.WriteLine("Rename directory: " + (renameDirectory.IsSuccess ? "success" : renameDirectory.Message));

        OperateResult deleteDirectory = ftpClient.DeleteDirectory("/test2", clearFiles: true);
        Console.WriteLine("Delete directory: " + (deleteDirectory.IsSuccess ? "success" : deleteDirectory.Message));

        OperateResult deleteFile = ftpClient.DeleteFile("/sample2.txt");
        Console.WriteLine("Delete file: " + (deleteFile.IsSuccess ? "success" : deleteFile.Message));

        ftpClient.ConnectClose();
        ftpServer.ServerClose();
    }

    private static void ReportProgress(long already, long total)
    {
        if (total <= 0)
        {
            Console.WriteLine("Already bytes: " + already);
            return;
        }

        int percent = (int)(already * 100L / total);
        Console.WriteLine("Already bytes: " + already + ", total: " + total + ", percent: " + percent + "%");
    }
}

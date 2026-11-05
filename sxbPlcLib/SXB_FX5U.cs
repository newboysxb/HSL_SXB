using System;
using System.Collections.Generic;
using System.Threading;
using HslCommunication;
using HslCommunication.Profinet.Melsec;

namespace sxbPlcLib
{
    /// <summary>
    /// 三菱 FX5U PLC 数据轮询类（MC/SLMP 协议，二进制 3E 帧）。
    /// 启动后由后台线程周期读取指定 D 寄存器，并通过事件输出数据与错误。
    /// </summary>
    public sealed class SXB_FX5U : IDisposable
    {
        private readonly object _lifecycleLock = new object();
        private readonly object _errorLock = new object();
        private readonly List<string> _errorBuffer = new List<string>();

        private Thread _pollThread;
        private MelsecMcNet _plc;
        private CancellationTokenSource _cts;
        private volatile bool _isRunning;

        /// <summary>
        /// 数据接收事件。参数：发送者、本次读取到的字数据（ushort[]）。
        /// </summary>
        public event Action<object, ushort[]> DataReceived;

        /// <summary>
        /// 错误消息事件。参数：发送者、错误消息数组。
        /// 每条消息格式：yyyy-MM-dd HH:mm:ss.fff [错误级别] 具体错误消息。
        /// </summary>
        public event Action<object, string[]> ErrorMessage;

        /// <summary>
        /// 轮询读取的起始地址，默认 D0。
        /// </summary>
        public string ReadAddress { get; set; } = "D0";

        /// <summary>
        /// 每次读取的字数量，默认 10 个 D 寄存器。
        /// </summary>
        public ushort ReadLength { get; set; } = 10;

        /// <summary>
        /// 轮询间隔（毫秒），默认 200 ms。
        /// </summary>
        public int PollInterval { get; set; } = 200;

        /// <summary>
        /// 连接超时（毫秒），默认 3000 ms。
        /// </summary>
        public int ConnectTimeOut { get; set; } = 3000;

        /// <summary>
        /// 接收超时（毫秒），默认 3000 ms。
        /// </summary>
        public int ReceiveTimeOut { get; set; } = 3000;

        /// <summary>
        /// 当前是否正在轮询。
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
        }

        /// <summary>
        /// 启动轮询。重复调用会先停止上一次轮询，再使用新的 IP/端口启动。
        /// </summary>
        /// <param name="ip">PLC IP 地址。</param>
        /// <param name="port">PLC MC 协议端口。</param>
        public void Start(string ip, int port)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                AddError("ERROR", "启动失败：IP 地址不能为空。");
                PublishErrors();
                return;
            }

            if (port <= 0 || port > 65535)
            {
                AddError("ERROR", "启动失败：端口必须在 1-65535 之间，当前值 " + port + "。");
                PublishErrors();
                return;
            }

            if (ReadLength == 0)
            {
                AddError("ERROR", "启动失败：读取长度 ReadLength 必须大于 0。");
                PublishErrors();
                return;
            }

            // 先停掉旧线程，保证同一时间只有一个轮询线程。
            Stop();

            CancellationTokenSource cts = new CancellationTokenSource();
            MelsecMcNet plc = new MelsecMcNet(ip, port)
            {
                ConnectTimeOut = ConnectTimeOut,
                ReceiveTimeOut = ReceiveTimeOut,
                AutoReConnect = true
            };

            lock (_lifecycleLock)
            {
                _cts = cts;
                _plc = plc;
                _isRunning = true;

                _pollThread = new Thread(() => PollLoop(plc, cts))
                {
                    IsBackground = true,
                    Name = "SXB_FX5U_PollThread"
                };
                _pollThread.Start();
            }
        }

        /// <summary>
        /// 停止轮询，等待线程退出并释放连接、线程和取消令牌。
        /// 该方法可重复调用。
        /// </summary>
        public void Stop()
        {
            Thread thread;
            CancellationTokenSource cts;
            MelsecMcNet plc;

            lock (_lifecycleLock)
            {
                thread = _pollThread;
                cts = _cts;
                plc = _plc;

                _pollThread = null;
                _cts = null;
                _plc = null;
                _isRunning = false;

                if (cts != null)
                {
                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        // 已经释放，忽略。
                    }
                }
            }

            if (thread != null && thread != Thread.CurrentThread)
            {
                // 读取操作最多阻塞 ReceiveTimeOut，这里留出足够时间让线程自然退出。
                if (!thread.Join(ConnectTimeOut + ReceiveTimeOut + 2000))
                {
                    AddError("WARN", "停止时轮询线程未在预期时间内退出。");
                }
            }

            if (plc != null)
            {
                try
                {
                    plc.ConnectClose();
                }
                catch (Exception ex)
                {
                    AddError("ERROR", "停止时关闭 PLC 连接失败：" + ex.Message);
                }
            }

            if (cts != null)
            {
                cts.Dispose();
            }

            PublishErrors();
        }

        /// <summary>
        /// 释放资源，等同于 Stop()。
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        private void PollLoop(MelsecMcNet plc, CancellationTokenSource cts)
        {
            CancellationToken token = cts.Token;

            try
            {
                OperateResult connect = plc.ConnectServer();
                if (!connect.IsSuccess)
                {
                    AddError("ERROR", "连接 PLC 失败：" + connect.Message);
                    PublishErrors();
                }

                while (!token.IsCancellationRequested)
                {
                    // 每轮都重新读取属性，允许运行期间调整地址和长度。
                    string address = ReadAddress;
                    ushort length = ReadLength;

                    OperateResult<ushort[]> read = plc.ReadUInt16(address, length);
                    if (read.IsSuccess)
                    {
                        RaiseDataReceived(read.Content);
                    }
                    else
                    {
                        AddError("ERROR", "读取地址 " + address + "（长度 " + length + "）失败：" + read.Message);
                    }

                    PublishErrors();

                    // 分片休眠，保证 Stop() 能快速生效。
                    SleepWithCancellation(token, PollInterval);
                }
            }
            catch (Exception ex)
            {
                AddError("ERROR", "轮询线程发生异常：" + ex.GetType().Name + " - " + ex.Message);
            }
            finally
            {
                try
                {
                    plc.ConnectClose();
                }
                catch (Exception ex)
                {
                    AddError("ERROR", "轮询结束时关闭连接失败：" + ex.Message);
                }

                lock (_lifecycleLock)
                {
                    // 只有当前线程仍是登记线程时才清理状态，避免影响新一轮启动。
                    if (ReferenceEquals(_pollThread, Thread.CurrentThread))
                    {
                        _pollThread = null;
                    }
                }

                _isRunning = false;
                PublishErrors();
            }
        }

        private void RaiseDataReceived(ushort[] data)
        {
            Action<object, ushort[]> handler = DataReceived;
            if (handler == null)
            {
                return;
            }

            try
            {
                handler(this, data);
            }
            catch (Exception ex)
            {
                AddError("ERROR", "数据接收事件处理异常：" + ex.GetType().Name + " - " + ex.Message);
            }
        }

        private void AddError(string level, string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                          + " [" + level + "] "
                          + message;

            lock (_errorLock)
            {
                _errorBuffer.Add(line);
            }
        }

        private string[] TakeErrors()
        {
            lock (_errorLock)
            {
                if (_errorBuffer.Count == 0)
                {
                    return Array.Empty<string>();
                }

                string[] errors = _errorBuffer.ToArray();
                _errorBuffer.Clear();
                return errors;
            }
        }

        private void PublishErrors()
        {
            string[] errors = TakeErrors();
            if (errors.Length == 0)
            {
                return;
            }

            Action<object, string[]> handler = ErrorMessage;
            if (handler == null)
            {
                // 没有订阅者时直接丢弃，避免错误消息无限累积占用内存。
                return;
            }

            try
            {
                handler(this, errors);
            }
            catch
            {
                // 错误事件自身抛出的异常不再向上传递，避免终止轮询线程。
            }
        }

        private static void SleepWithCancellation(CancellationToken token, int milliseconds)
        {
            int interval = Math.Max(10, milliseconds);
            int remaining = interval;

            while (remaining > 0 && !token.IsCancellationRequested)
            {
                int sleep = Math.Min(50, remaining);
                Thread.Sleep(sleep);
                remaining -= sleep;
            }
        }
    }
}

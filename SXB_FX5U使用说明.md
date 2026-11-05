# SXB_FX5U 使用说明

`sxbPlcLib.SXB_FX5U` 是三菱 FX5U（以及支持 MC 协议 3E 二进制帧的 MELSEC 系列 PLC）的数据轮询封装类。它基于 HslCommunication 的 `MelsecMcNet`，用一个后台线程周期性读取连续的 D 寄存器，并通过事件把数据和错误推送给调用方。

---

## 1. 功能特性

- **后台轮询**：调用 `Start` 后自动创建后台线程，按 `PollInterval` 周期读取，不阻塞调用线程。
- **事件驱动**：读取成功触发 `DataReceived`，出现异常触发 `ErrorMessage`。
- **参数可热调**：`ReadAddress`、`ReadLength` 在运行期间修改后，下一轮读取立即生效。
- **安全停止**：内部使用取消令牌 + 分片休眠，`Stop` 能在较短时间内退出线程并关闭连接。
- **错误缓冲**：错误先写入线程安全缓冲区，再批量推送；没有订阅者时自动丢弃，避免内存持续增长。
- **自动重连**：底层 `MelsecMcNet.AutoReConnect = true`，网络恢复后会尝试重新建立连接。

---

## 2. 运行环境

| 项目 | 要求 |
| --- | --- |
| 目标框架 | .NET Framework 4.7.2（`net472`） |
| 语言版本 | C# 7.3 |
| 依赖 | `HslCommunication.dll` 13.0.3、`Newtonsoft.Json.dll` |
| 命名空间 | `sxbPlcLib` |

引用方式二选一：

1. 在解决方案中直接引用 `sxbPlcLib/sxbPlcLib.csproj`（推荐）；
2. 引用编译产物 `sxbPlcLib.dll`，同时把 `RefenceSource` 下的两个 DLL 一起复制到输出目录。

> 由于类库的目标框架是 `net472`，.NET 8/9 项目不能直接引用它。如需在新版 .NET 中使用，请把 `sxbPlcLib.csproj` 改为 `netstandard2.0` 或 `net8.0` 后重新编译。

---

## 3. API 一览

### 3.1 属性

| 属性 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `ReadAddress` | `string` | `"D0"` | 轮询起始地址，支持 MC 协议地址格式，如 `D0`、`D100`、`M0`、`W1A0` |
| `ReadLength` | `ushort` | `10` | 每轮读取的字（16 位寄存器）数量，必须大于 0 |
| `PollInterval` | `int` | `200` | 轮询间隔，单位毫秒，最小按 10 ms 处理 |
| `ConnectTimeOut` | `int` | `3000` | 连接超时，单位毫秒 |
| `ReceiveTimeOut` | `int` | `3000` | 单次收发超时，单位毫秒 |
| `IsRunning` | `bool` | `false` | 只读，当前是否处于轮询状态 |

### 3.2 方法

| 方法 | 说明 |
| --- | --- |
| `Start(string ip, int port)` | 启动轮询。若已在运行，会先 `Stop()` 再以新的参数启动。IP 为空、端口越界或 `ReadLength == 0` 时不会启动，并通过 `ErrorMessage` 报告原因 |
| `Stop()` | 停止轮询，等待线程退出并关闭连接；可重复调用 |
| `Dispose()` | 等价于 `Stop()`，实现 `IDisposable` |

### 3.3 事件

```csharp
public event Action<object, ushort[]> DataReceived;   // 发送者、本次读取到的字数据
public event Action<object, string[]> ErrorMessage;   // 发送者、错误消息数组
```

- `DataReceived` 的第二个参数是**本轮读取的完整数据**，长度等于当轮的 `ReadLength`，索引 `0` 对应 `ReadAddress` 起始地址。
- `ErrorMessage` 中每条消息格式为：

  ```text
  yyyy-MM-dd HH:mm:ss.fff [ERROR] 读取地址 D0（长度 10）失败：...
  ```

  级别目前有 `ERROR` 和 `WARN` 两种。

> 两个事件都在**轮询线程**上触发，不是 UI 线程。在 WinForm/WPF 中更新界面时必须自行 `Invoke`/`Dispatcher` 切换线程；事件处理函数应尽量短，不要在里面执行耗时操作或调用 `Stop()`（会造成线程等待自身）。

---

## 4. 快速开始

### 4.1 控制台示例

```csharp
using System;
using System.Threading;
using sxbPlcLib;

internal static class Program
{
    private static void Main()
    {
        SXB_FX5U plc = new SXB_FX5U
        {
            ReadAddress = "D0",
            ReadLength = 10,
            PollInterval = 200,
            ConnectTimeOut = 3000,
            ReceiveTimeOut = 3000
        };

        // 一定要在 Start 之前订阅，否则启动阶段的错误会被丢弃。
        plc.DataReceived += OnDataReceived;
        plc.ErrorMessage += OnErrorMessage;

        plc.Start("192.168.1.10", 5001);   // 端口以 PLC 实际 MC 协议设置为准
        Console.WriteLine("轮询中，按回车停止...");
        Console.ReadLine();

        plc.Stop();
        plc.Dispose();
    }

    private static void OnDataReceived(object sender, ushort[] data)
    {
        // 注意：这里是轮询线程
        Console.WriteLine("D0={0}, D1={1}, D2={2}", data[0], data[1], data[2]);
    }

    private static void OnErrorMessage(object sender, string[] errors)
    {
        foreach (string error in errors)
        {
            Console.WriteLine(error);
        }
    }
}
```

### 4.2 在 WinForm 中安全更新界面

```csharp
private void OnDataReceived(object sender, ushort[] data)
{
    if (IsDisposed || !IsHandleCreated)
    {
        return;
    }

    BeginInvoke(new Action(() =>
    {
        labelD0.Text = data[0].ToString();
        labelD1.Text = data[1].ToString();
    }));
}
```

---

## 5. 地址与长度

`ReadAddress` 直接交给 `MelsecMcNet.ReadUInt16`，因此使用 HslCommunication 的标准三菱地址写法：

| 软元件 | 示例 | 说明 |
| --- | --- | --- |
| 数据寄存器 D | `D0`、`D100` | 最常用，16 位字 |
| 链接寄存器 W | `W1A0` | 十六进制地址 |
| 内部继电器 M | `M0` | 按字读取时一次读取 16 个位 |
| 输入/输出 X/Y | `X0`、`Y0` | 十六进制，按字读取 |
| 文件寄存器 R/ZR | `R0`、`ZR0` | 需 PLC 支持 |

注意事项：

- `ReadLength` 是**字数**，不是字节数；读取 `D0` 且长度为 10，会得到 D0～D9。
- MC 3E 二进制帧单次读取字数有上限（960 字），建议 `ReadLength` 不超过 960；需要更多数据时请分段读取。
- 读取越界、地址不存在或 PLC 未开放该软元件时，`DataReceived` 不会触发，错误会通过 `ErrorMessage` 返回。

---

## 6. 数据解析示例

`DataReceived` 给出的是原始 `ushort[]`，可按业务需要转换：

```csharp
// 单个 16 位有符号整数
short d0 = unchecked((short)data[0]);

// 32 位整数（低字在前，FX5U 常用顺序；如现场相反请交换下标）
int d0d1 = (int)((uint)(data[1] << 16) | data[0]);

// 32 位浮点数
byte[] buffer = new byte[4];
Buffer.BlockCopy(data, 0, buffer, 0, 4);
float d0d1Float = BitConverter.ToSingle(buffer, 0);

// 位状态（按字读取 M 区时）
bool m0 = (data[0] & 0x0001) != 0;
bool m15 = (data[0] & 0x8000) != 0;
```

> 字节序/字序取决于 PLC 侧的数据排列方式，接入真实设备后请先用已知值校验一次。

---

## 7. 生命周期与线程模型

```text
Start(ip, port)
   │  校验参数 → 停止旧线程 → 创建 MelsecMcNet → 启动后台线程
   ▼
PollLoop
   │  ConnectServer()
   │  while (!取消) {
   │      ReadUInt16(ReadAddress, ReadLength)
   │      ├─ 成功 → DataReceived
   │      └─ 失败 → 写入错误缓冲
   │      PublishErrors() → ErrorMessage
   │      SleepWithCancellation(PollInterval)
   │  }
   ▼
Stop() / Dispose()
   │  取消令牌 → Join 线程 → ConnectClose() → 释放令牌
```

使用约定：

1. **先订阅事件，再 `Start`**。启动阶段的连接错误只推送一次，无订阅者时会被直接丢弃。
2. 重复调用 `Start` 会先停止上一次轮询，不会产生两个线程。
3. `Stop` 内部最多等待 `ConnectTimeOut + ReceiveTimeOut + 2000` 毫秒；若线程未退出，会推送一条 `WARN`。
4. 不要在 `DataReceived` / `ErrorMessage` 中调用 `Stop()` 或 `Dispose()`，也不要在其中长时间阻塞，否则会拖慢轮询甚至造成等待自身。
5. 程序退出前调用 `Dispose()`（或使用 `using`），确保连接和线程被释放。

---

## 8. 错误处理

- 所有异常都被捕获并转成文本，不会抛到调用线程，因此**必须订阅 `ErrorMessage`** 才能发现通信问题。
- 常见错误及排查方向：

| 错误现象 | 可能原因 |
| --- | --- |
| `连接 PLC 失败` | IP/端口错误；PLC 未启用 MC 协议；防火墙拦截；站号或网络类型不匹配 |
| `读取地址 Dxxx（长度 n）失败` | 地址超出 PLC 软元件范围；`ReadLength` 过大；PLC 处于 STOP 或禁止访问状态 |
| `停止时轮询线程未在预期时间内退出` | 网络质量差导致读写阻塞，可适当增大 `ReceiveTimeOut` 后重试 |
| 事件里更新界面抛跨线程异常 | 未使用 `Invoke`/`BeginInvoke` 切回 UI 线程 |
| `启动失败：读取长度 ReadLength 必须大于 0` | `ReadLength` 被设置为 0 |

---

## 9. 典型配置建议

| 场景 | 建议 |
| --- | --- |
| 少量数据、实时性要求高 | `PollInterval = 100`，`ReadLength` 按实际点数设置 |
| 数据点较多 | 分段读取，每段 ≤ 960 字，多段之间留出间隔 |
| 无线/不稳定网络 | 把 `ConnectTimeOut`、`ReceiveTimeOut` 调到 5000 ms 以上，并做好错误提示 |
| 长时间运行 | 在 `ErrorMessage` 中记录日志，定期检查 `IsRunning` 与重连情况 |

---

## 10. 构建

在仓库根目录执行：

```powershell
dotnet build HSL_SXB.sln -c Debug
```

或用 Visual Studio 打开 `HSL_SXB.sln`，还原后直接生成。输出位于 `sxbPlcLib/bin/Debug/net472/`，其中 `sxbPlcLib.dll` 可被其他 .NET Framework 4.7.2 项目引用。

﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core;
using Snap.Hutao.Service.Game.Unlocker;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;
using static Windows.Win32.PInvoke;

namespace Snap.Hutao.Service.Game;

/// <summary>
/// 进程互操作
/// </summary>
internal static class ProcessInterop
{
    /// <summary>
    /// 获取初始化后的游戏进程
    /// </summary>
    /// <param name="options">启动选项</param>
    /// <param name="gamePath">游戏路径</param>
    /// <returns>初始化后的游戏进程</returns>
    public static Process InitializeGameProcess(LaunchOptions options, string gamePath)
    {
        // https://docs.unity.cn/cn/current/Manual/PlayerCommandLineArguments.html
        string commandLine = new CommandLineBuilder()
            .AppendIf("-popupwindow", options.IsBorderless)
            .AppendIf("-window-mode", options.IsExclusive, "exclusive")
            .Append("-screen-fullscreen", options.IsFullScreen ? 1 : 0)
            .Append("-screen-width", options.ScreenWidth)
            .Append("-screen-height", options.ScreenHeight)
            .Append("-monitor", options.Monitor.Value)
            .ToString();

        return new()
        {
            StartInfo = new()
            {
                Arguments = commandLine,
                FileName = gamePath,
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Path.GetDirectoryName(gamePath),
            },
        };
    }

    /// <summary>
    /// 解锁帧率
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="game">游戏进程</param>
    /// <param name="token">取消令牌</param>
    /// <returns>任务</returns>
    public static ValueTask UnlockFpsAsync(IServiceProvider serviceProvider, Process game, CancellationToken token)
    {
        IGameFpsUnlocker unlocker = serviceProvider.CreateInstance<GameFpsUnlocker>(game);
        UnlockTimingOptions options = new(100, 20000, 3000);
        Progress<UnlockerStatus> progress = new(); // TODO: do something.
        return unlocker.UnlockAsync(options, progress, token);
    }

    /// <summary>
    /// 尝试禁用mhypbase
    /// </summary>
    /// <param name="game">游戏进程</param>
    /// <param name="gamePath">游戏路径</param>
    /// <returns>是否禁用成功</returns>
    public static bool DisableProtection(Process game, string gamePath)
    {
        string? gameFolder = Path.GetDirectoryName(gamePath);
        string mhypbaseDll = Path.Combine(gameFolder ?? string.Empty, "mhypbase.dll");

        if (File.Exists(mhypbaseDll))
        {
            using (File.OpenHandle(mhypbaseDll, share: FileShare.None))
            {
                SpinWait.SpinUntil(() => game.MainWindowHandle != 0);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 加载并注入指定路径的库
    /// </summary>
    /// <param name="hProcess">进程句柄</param>
    /// <param name="libraryPathu8">库的路径，不包含'\0'</param>
    [SuppressMessage("", "SH002")]
    public static unsafe void LoadLibraryAndInject(HANDLE hProcess, ReadOnlySpan<byte> libraryPathu8)
    {
        HINSTANCE hKernelDll = GetModuleHandle("kernel32.dll");
        Marshal.ThrowExceptionForHR(Marshal.GetLastPInvokeError());

        FARPROC pLoadLibraryA = GetProcAddress(hKernelDll, "LoadLibraryA"u8);
        Marshal.ThrowExceptionForHR(Marshal.GetLastPInvokeError());

        void* pNativeLibraryPath = default;
        try
        {
            VIRTUAL_ALLOCATION_TYPE allocType = VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE | VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT;
            pNativeLibraryPath = VirtualAllocEx(hProcess, default, unchecked((uint)libraryPathu8.Length + 1), allocType, PAGE_PROTECTION_FLAGS.PAGE_READWRITE);
            Marshal.ThrowExceptionForHR(Marshal.GetLastPInvokeError());

            WriteProcessMemory(hProcess, pNativeLibraryPath, libraryPathu8);
            Marshal.ThrowExceptionForHR(Marshal.GetLastPInvokeError());

            LPTHREAD_START_ROUTINE lpThreadLoadLibraryA = pLoadLibraryA.CreateDelegate<LPTHREAD_START_ROUTINE>();
            HANDLE hLoadLibraryAThread = default;
            try
            {
                hLoadLibraryAThread = CreateRemoteThread(hProcess, default, 0, lpThreadLoadLibraryA, pNativeLibraryPath, 0);
                Marshal.ThrowExceptionForHR(Marshal.GetLastPInvokeError());

                WaitForSingleObject(hLoadLibraryAThread, 2000);
                Marshal.ThrowExceptionForHR(Marshal.GetLastPInvokeError());
            }
            finally
            {
                CloseHandle(hLoadLibraryAThread);
            }
        }
        finally
        {
            VirtualFreeEx(hProcess, pNativeLibraryPath, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
        }
    }

    [SuppressMessage("", "SH002")]
    private static unsafe FARPROC GetProcAddress(HINSTANCE hModule, ReadOnlySpan<byte> lpProcName)
    {
        fixed (byte* lpProcNameLocal = lpProcName)
        {
            return Windows.Win32.PInvoke.GetProcAddress(hModule, new PCSTR(lpProcNameLocal));
        }
    }

    [SuppressMessage("", "SH002")]
    private static unsafe BOOL WriteProcessMemory(HANDLE hProcess, void* lpBaseAddress, ReadOnlySpan<byte> buffer)
    {
        fixed (void* lpBuffer = buffer)
        {
            return Windows.Win32.PInvoke.WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, unchecked((uint)buffer.Length));
        }
    }
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

class Helper
{
    // Constants
    private const int PROCESS_CREATE_THREAD = 0x0002;
    private const int PROCESS_QUERY_INFORMATION = 0x0400;
    private const int PROCESS_VM_OPERATION = 0x0008;
    private const int PROCESS_VM_WRITE = 0x0020;
    private const int PROCESS_VM_READ = 0x0010;

    // P/Invoke declarations
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();

    public static void GetProcessIDByName(out bool Success, out int PID, string ProcessName)
    {
        Success = true; PID = 0;

        Process[] Processes = Process.GetProcessesByName(ProcessName);
        if (Processes.Length == 0) { Success = false; return; };

        PID = Processes[0].Id;
    }

    public static bool InjectDLL(int PID, string DLLPath)
    {
        IntPtr Process = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, PID);
        if (Process == IntPtr.Zero) return false;

        IntPtr AllocMemAddress = VirtualAllocEx(Process, IntPtr.Zero, (uint)(DLLPath.Length + 1), 0x1000, 0x04);
        if (AllocMemAddress == IntPtr.Zero) return false;

        uint BytesWritten;
        bool IsWritten = WriteProcessMemory(Process, AllocMemAddress, System.Text.Encoding.Default.GetBytes(DLLPath), (uint)(DLLPath.Length + 1), out BytesWritten);
        if (!IsWritten) return false;

        IntPtr Kernel32 = GetModuleHandle("kernel32.dll");
        IntPtr LoadLibraryAddr = GetProcAddress(Kernel32, "LoadLibraryA");
        if (LoadLibraryAddr == IntPtr.Zero) return false;

        IntPtr Thread;
        CreateRemoteThread(Process, IntPtr.Zero, 0, LoadLibraryAddr, AllocMemAddress, 0, out Thread);

        return true;
    }
}

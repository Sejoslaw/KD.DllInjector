using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace KD.DllInjector
{
    internal static class DllInjector
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        internal static DllInjectionResult DoWork()
        {
            if (!File.Exists(Settings.PathToDll))
            {
                return DllInjectionResult.DllNotFound;
            }

            uint procId = GetProcessId();
            Console.WriteLine($"Found process with Id: { procId }");

            if (procId == 0)
            {
                return DllInjectionResult.ProcessNotFound;
            }

            if (!TryInject(procId))
            {
                return DllInjectionResult.InjectionFailed;
            }

            return DllInjectionResult.Success;
        }

        private static bool TryInject(uint procId)
        {
            IntPtr procHandler = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, procId);
            if (procHandler == IntPtr.Zero)
            {
                Console.WriteLine($"Can't open process with Id: { procId }");
                return false;
            }

            IntPtr ptrLoadLibraryA = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (ptrLoadLibraryA == IntPtr.Zero)
            {
                Console.WriteLine($"Can't find method: kernel32.dll -> LoadLibraryA");
                return false;
            }

            IntPtr ptrAddress = VirtualAllocEx(procHandler, (IntPtr)null, (IntPtr)Settings.PathToDll.Length, (0x1000 | 0x2000), 0X40);
            if (ptrAddress == IntPtr.Zero)
            {
                Console.WriteLine($"Can't allocate memory for process.");
                return false;
            }

            byte[] pathToDllBytes = Encoding.ASCII.GetBytes(Settings.PathToDll);

            if (WriteProcessMemory(procHandler, ptrAddress, pathToDllBytes, (uint)pathToDllBytes.Length, 0) == 0)
            {
                Console.WriteLine("Can't write process memory.");
                return false;
            }

            if (CreateRemoteThread(procHandler, (IntPtr)null, IntPtr.Zero, ptrLoadLibraryA, ptrAddress, 0, (IntPtr)null) == IntPtr.Zero)
            {
                Console.WriteLine("Can't create remote thread.");
                return false;
            }

            CloseHandle(procHandler);

            return true;
        }

        private static uint GetProcessId()
        {
            uint procId = default(uint);

            Process[] processes = Process.GetProcesses();
            foreach (Process proc in processes)
            {
                if (proc.ProcessName.Equals(Settings.ProcessName))
                {
                    procId = (uint)proc.Id;
                    break;
                }
            }

            return procId;
        }
    }
}
#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace MPExtended.Libraries.Service.Util
{
    public class TranscoderProcess: Process
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [Flags]
        private enum CreateProcessFlags : uint
        {
            NONE = 0,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }



        [Flags()]
        public enum ProcessAccess : int
        {
            PROCESS_ALL_ACCESS = PROCESS_CREATE_THREAD | PROCESS_DUPLICATE_HANDLE | PROCESS_QUERY_INFORMATION | PROCESS_SET_INFORMATION | PROCESS_TERMINATE | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_SYNCHRONIZE,
            PROCESS_CREATE_THREAD = 0x2,
            PROCESS_DUPLICATE_HANDLE = 0x40,
            PROCESS_QUERY_INFORMATION = 0x400,
            PROCESS_SET_INFORMATION = 0x200,
            PROCESS_TERMINATE = 0x1,
            PROCESS_VM_OPERATION = 0x8,
            PROCESS_VM_READ = 0x10,
            PROCESS_VM_WRITE = 0x20,
            PROCESS_SYNCHRONIZE = 0x100000
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool LogonUser(string username, string domain, string password, int logonType, int logonProvider, out IntPtr userToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, int ImpersonationLevel, int TokenType, out IntPtr phNewToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreateProcessAsUserW(IntPtr token, [MarshalAs(UnmanagedType.LPTStr)] string lpApplicationName, [MarshalAs(UnmanagedType.LPTStr)] string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, CreateProcessFlags dwCreationFlags, IntPtr lpEnvironment, [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, int nSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetHandleInformation(IntPtr hObject, int dwMask, uint dwFlags);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("userenv.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int MAXIMUM_ALLOWED = 0x2000000;
        private const int TOKEN_DUPLICATE = 0x0000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_PRIMARY = 1;
        private const int TOKEN_IMPERSONATION = 2;
        private const int SECURITY_IDENTIFICATION = 1;
        private const int SECURITY_IMPERSONATION = 2;
        private const int HANDLE_FLAG_INHERIT = 1;

        private static uint STARTF_USESHOWWINDOW = 0x00000001;
        private static uint STARTF_USESTDHANDLES = 0x00000100;

        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;

        private const int SW_HIDE = 0;
        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        private const int SW_SHOW = 5;

        private IntPtr stdinReadHandle;
        private IntPtr stdinWriteHandle;
        private IntPtr stdoutWriteHandle;
        private IntPtr stderrWriteHandle;
        private IntPtr stdoutReadHandle;
        private IntPtr stderrReadHandle;

        private string GetCommandLine()
        {
            StringBuilder result = new StringBuilder();
            string applicationName = StartInfo.FileName.Trim();
            string arguments = StartInfo.Arguments;
            bool applicationNameIsQuoted = applicationName.StartsWith("\"") && applicationName.EndsWith("\"");
            if (!applicationNameIsQuoted)
            {
                result.Append("\"");
            }
            result.Append(applicationName);
            if (!applicationNameIsQuoted)
            {
                result.Append("\"");
            }
            if ((arguments != null) && (arguments.Length > 0))
            {
                result.Append(" ");
                result.Append(arguments);
            }
            return result.ToString();
        }

        public void Stop()
        {
            IntPtr hProcess = IntPtr.Zero;
            try
            {
                int id = Id;
                Log.Debug("TranscoderProcess: Terminating process pid={0}", id);
                hProcess = OpenProcess(ProcessAccess.PROCESS_TERMINATE, false, id);
                if (hProcess == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "TranscoderProcess: OpenProcess failed");
                if (!TerminateProcess(hProcess, 0))
                    // This shouldn't be a problem normally, the process has probably exited before calling TerminateProcess()
                    Log.Warn("TranscoderProcess: TerminateProcess failed (0x{0:x8})", Marshal.GetLastWin32Error());
            }
            finally
            {
                if (hProcess != IntPtr.Zero)
                    CloseHandle(hProcess);
            }
        }

        public bool StartAsUser(string domain, string username, string password)
        {
            PROCESS_INFORMATION processInformation = new PROCESS_INFORMATION();
            IntPtr userToken = IntPtr.Zero;
            IntPtr token = IntPtr.Zero;

            try 
            {
                if (!LogonUser(username, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out token))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), String.Format("TranscoderProcess: LogonUser {0}\\{1} failed", domain, username));

                if (!DuplicateTokenEx(token, MAXIMUM_ALLOWED | TOKEN_QUERY | TOKEN_DUPLICATE, IntPtr.Zero, SECURITY_IMPERSONATION, TOKEN_PRIMARY, out userToken))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "TranscoderProcess: DuplicateToken failed");

                STARTUPINFO startupInfo = new STARTUPINFO();
                startupInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));
                switch (StartInfo.WindowStyle)
                {
                    case ProcessWindowStyle.Hidden:
                        startupInfo.wShowWindow = SW_HIDE;
                        break;
                    case ProcessWindowStyle.Maximized:
                        startupInfo.wShowWindow = SW_MAXIMIZE;
                        break;
                    case ProcessWindowStyle.Minimized:
                        startupInfo.wShowWindow = SW_MINIMIZE;
                        break;
                    case ProcessWindowStyle.Normal:
                        startupInfo.wShowWindow = SW_SHOW;
                        break;
                }

                CreateStandardPipe(out stdinReadHandle, out stdinWriteHandle, STD_INPUT_HANDLE, true, StartInfo.RedirectStandardInput);
                CreateStandardPipe(out stdoutReadHandle, out stdoutWriteHandle, STD_OUTPUT_HANDLE, false, StartInfo.RedirectStandardOutput);
                CreateStandardPipe(out stderrReadHandle, out stderrWriteHandle, STD_ERROR_HANDLE, false, StartInfo.RedirectStandardError);

                startupInfo.dwFlags = STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW;
                startupInfo.hStdInput = stdinReadHandle;
                startupInfo.hStdOutput = stdoutWriteHandle;
                startupInfo.hStdError = stderrWriteHandle;

                CreateProcessFlags createFlags = CreateProcessFlags.CREATE_NEW_CONSOLE | CreateProcessFlags.CREATE_NEW_PROCESS_GROUP | CreateProcessFlags.CREATE_DEFAULT_ERROR_MODE;
                if (StartInfo.CreateNoWindow)
                    createFlags |= CreateProcessFlags.CREATE_NO_WINDOW;

                String commandLine = GetCommandLine();

                Log.Debug("TranscoderProcess: userToken = {0:x8}", userToken);
                Log.Debug("TranscoderProcess: commandLine = {0}", commandLine);
                Log.Debug("TranscoderProcess: processFlags = {0}", createFlags);

                // Create process as user, fail hard if this is unsuccessful so it can be caught in EncoderUnit
                if (!CreateProcessAsUserW(userToken, null, GetCommandLine(), IntPtr.Zero, IntPtr.Zero, true, createFlags, IntPtr.Zero, null, ref startupInfo, out processInformation))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "TranscoderProcess: CreateProcessAsUser failed");

                Log.Debug("TranscoderProcess: CreateProcessAsUser succeeded, pid={0}", processInformation.dwProcessId);

                if (processInformation.hThread != (IntPtr)(-1))
                {
                    CloseHandle(processInformation.hThread);
                    processInformation.hThread = IntPtr.Zero;
                }

	            if (StartInfo.RedirectStandardInput) 
                {
                    CloseHandle(stdinReadHandle);
                    StreamWriter standardInput = new StreamWriter(new FileStream(new SafeFileHandle(stdinWriteHandle, true), FileAccess.Write, 4096), Console.Out.Encoding);
                    standardInput.AutoFlush = true;
                    typeof(Process).InvokeMember("standardInput", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, this, new object[] { standardInput });
                }

                if (StartInfo.RedirectStandardOutput) 
                {
                    CloseHandle(stdoutWriteHandle);
                    StreamReader standardOutput = new StreamReader(new FileStream(new SafeFileHandle(stdoutReadHandle, true), FileAccess.Read, 4096), StartInfo.StandardOutputEncoding ?? Console.Out.Encoding);
                    typeof(Process).InvokeMember("standardOutput", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, this, new object[] { standardOutput });
                }               
                
                if (StartInfo.RedirectStandardError) 
                {
                    CloseHandle(stderrWriteHandle);
                    StreamReader standardError = new StreamReader(new FileStream(new SafeFileHandle(stderrReadHandle, true), FileAccess.Read, 4096), StartInfo.StandardErrorEncoding ?? Console.Out.Encoding);
                    typeof(Process).InvokeMember("standardError", BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance, null, this, new object[] { standardError });
                }

                // workaround to get process handle as non-public SafeProcessHandle
                Assembly processAssembly = typeof(Process).Assembly;
                Type processManager = processAssembly.GetType("System.Diagnostics.ProcessManager");
                object safeProcessHandle = processManager.InvokeMember("OpenProcess", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, this, new object[] { processInformation.dwProcessId, 0x100000, false });

                typeof(Process).InvokeMember("SetProcessHandle", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, this, new object[] { safeProcessHandle });
                typeof(Process).InvokeMember("SetProcessId", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, this, new object[] { processInformation.dwProcessId });

                return true;
            } 
            finally 
            {
                // cleanup
                if (token != IntPtr.Zero)
                    CloseHandle(token);
                if (userToken != IntPtr.Zero)
                    CloseHandle(userToken);
                if (processInformation.hProcess != IntPtr.Zero)
                    CloseHandle(processInformation.hProcess);
                if (processInformation.hThread != IntPtr.Zero)
                    CloseHandle(processInformation.hThread);
            }
        }

        private void CreateStandardPipe(out IntPtr readHandle, out IntPtr writeHandle, int standardHandle, bool isInput, bool redirect)
        {
            if (redirect)
            {
                bool success;
                SECURITY_ATTRIBUTES security = new SECURITY_ATTRIBUTES();
                security.nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
                security.bInheritHandle = true;
                security.lpSecurityDescriptor = IntPtr.Zero;
                success = CreatePipe(out readHandle, out writeHandle, ref security, 4096);
                if (success)
                    if (isInput)
                        success = SetHandleInformation(writeHandle, HANDLE_FLAG_INHERIT, 0);
                    else
                        success = SetHandleInformation(readHandle, HANDLE_FLAG_INHERIT, 0);
                if (!success)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "TranscoderProcess: could not create standard pipe");
            }
            else
            {
                if (isInput)
                {
                    writeHandle = IntPtr.Zero;
                    readHandle = GetStdHandle(standardHandle);
                }
                else
                {
                    readHandle = IntPtr.Zero;
                    writeHandle = GetStdHandle(standardHandle);
                }
            }
        }
    }
}

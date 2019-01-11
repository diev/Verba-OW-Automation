@echo off
set version=1.3.0.18
rem set net=%windir%\Microsoft.NET\Framework\v3.5
set net=%windir%\Microsoft.NET\Framework\v4.0.30319

set out=bin
set pub=c:\pub
set ka=206194104001

set log1=logs\{0:yyyyMMdd}.log
set log2=P:\PTK PSD\LOG\test_{0:yyyyMMdd}.log
set log3=

set msg={0:HH:mm:ss} {1}
set err=***FAILED!

rem -------------------------------------------------------------------------
md "%out%" 2>nul

rem Escape every ^), ^&, ^<, ^> inside of "echo (...C#...)" below

rem -------------------------------------------------------------------------
set app=Decrypt

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

call :_using
echo if (args.Length ^< 4^) {
echo Console.WriteLine(@"Usage: %app% in\[*] out\ XXXX[SSSSSS] XXXXSSSSSS [ext]"^); return; }
call :_args
echo string id = args[2]; if (id.Length ^> 10^) id = id.Substring(0, 10^);
echo string to = args[3]; if (to.Length ^> 10^) to = to.Substring(0, 10^); if (id.Length == 4^) id += to.Substring(4^);
echo bool changeExt = args.Length ^> 4; string ext = changeExt ? "." + args[4] : string.Empty;
call :_cinit
call :_foreach
echo if (changeExt^) fileOut = Path.ChangeExtension(file, ext^);
echo bool ok = DeCryptFileEx(file, fileOut, to, key^) == 0 ^&^& File.Exists(fileOut^);
echo if (ok^) File.Delete(file^);
call :_print
call :_cdone
)

rem -------------------------------------------------------------------------
set app=Encrypt

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

echo using System.Runtime.InteropServices;
call :_using
echo if (args.Length ^< 4^) {
echo Console.WriteLine(@"Usage: %app% in\[*] out\ XXXXSSSSSS XXXX[SSSSSS] [ext]"^); return; }
call :_args
echo string id = args[2]; if (id.Length ^> 10^) id = id.Substring(0, 10^);
echo string to = args[3]; if (to.Length ^> 10^) to = to.Substring(0, 10^); if (to.Length == 4^) to += id.Substring(4^);
echo bool changeExt = args.Length ^> 4; string ext = changeExt ? "." + args[4] : string.Empty;
call :_cinit
echo IntPtr[] ptr = new IntPtr[] { Marshal.AllocHGlobal(304^) }; Marshal.Copy(key, 0, ptr[0], 304^);
call :_foreach
echo if (changeExt^) fileOut = Path.ChangeExtension(file, ext^);
echo bool ok = EnCryptFileEx(file, fileOut, id, ptr, 1, 0^) == 0 ^&^& File.Exists(fileOut^);
echo if (ok^) File.Delete(file^);
call :_print
echo Marshal.FreeHGlobal(ptr[0]^);
call :_cdone
)

rem -------------------------------------------------------------------------
set app=Sign

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

call :_using
echo if (args.Length ^< 2^) {
echo Console.WriteLine(@"Usage: %app% in\[*] out [%ka%]"^); return; }
call :_args
echo string id = args.Length ^> 2 ? args[2] : "%ka%";
call :_sinit
call :_foreach
echo bool ok = SignFile(file, fileOut, id^) == 0 ^&^& File.Exists(fileOut^);
echo if (ok^) File.Delete(file^);
call :_print
call :_sdone
)

rem -------------------------------------------------------------------------
set app=Unsign

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

call :_using
echo if (args.Length ^< 2^) {
echo Console.WriteLine(@"Usage: %app% in\[*] out\"^); return; }
call :_args
call :_foreach
echo File.Copy(file, fileOut, true^);
echo bool ok = DelSign(fileOut, -1^) == 0;
echo File.Delete(ok ? file : fileOut^);
call :_print
echo }}}
)

rem -------------------------------------------------------------------------
set app=Verify

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

call :_using
echo if (args.Length ^< 2^) {
echo Console.WriteLine(@"Usage: %app% in\[*] out\"^); return; }
call :_args
call :_sinit
call :_foreach
echo File.Copy(file, fileOut, true^);
echo int r; byte count; CheckList list;
echo if ((r = CheckFileSign(fileOut, out count, out list^)^) == 0^) {
echo   for (int i = 0; i ^< (int^)count; i++^) if ((r = list.Signs[i].Status^) ^> 0^) break;
echo   FreeMemory(list^); }
echo bool ok = r == 0;
echo File.Delete(ok ? file : fileOut^);
call :_print
call :_sdone
)

rem -------------------------------------------------------------------------
%1 > "%out%\AssemblyInfo.lib.cs" (

echo using System.Reflection;
echo using System.Runtime.InteropServices;

rem echo [assembly: AssemblyTitle("Verba"^)]
echo [assembly: AssemblyDescription("A Verba-OW library wrapper for PowerShell and standalone use."^)]
echo [assembly: AssemblyConfiguration("Release"^)]
echo [assembly: AssemblyCompany(""^)]
echo [assembly: AssemblyProduct("Verba"^)]
echo [assembly: AssemblyCopyright("Copyright (c) 2018-2019 Dmitrii Evdokimov"^)]
echo [assembly: AssemblyTrademark(""^)]
echo [assembly: AssemblyCulture(""^)]
echo [assembly: ComVisible(false^)]
echo [assembly: AssemblyVersion("%version%"^)]
echo [assembly: AssemblyFileVersion("%version%"^)]
echo [assembly: AssemblyInformationalVersion("%version%"^)]
)

rem -------------------------------------------------------------------------
%1 > "%out%\Crypto.lib.cs" (

echo using System;
echo using System.Runtime.InteropServices;

echo namespace App { partial class Program {

echo [DllImport("wbotho.dll", EntryPoint = "CryptoInit", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CryptoInit(string sec, string pub^);

echo [DllImport("wbotho.dll", EntryPoint = "ExtractKey", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort ExtractKey(string pub, string id, byte[] key^);

echo [DllImport("wbotho.dll", EntryPoint = "DeCryptFileEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort DeCryptFileEx(string fileIn, string fileOut, string id, byte[] key^);

echo [DllImport("wbotho.dll", EntryPoint = "EnCryptFileEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort EnCryptFileEx(string fileIn, string fileOut, string id, IntPtr[] keys, uint keysCount, ulong flags^);

echo [DllImport("wbotho.dll", EntryPoint = "CryptoDone", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CryptoDone(^); }}
)

rem -------------------------------------------------------------------------
%1 > "%out%\Sign.lib.cs" (

echo using System;
echo using System.Runtime.InteropServices;

echo namespace App { partial class Program {

echo [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi^), Serializable] struct CheckStatus {
echo [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13^)] string Name;
echo [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 121^)] string Alias;
echo byte Position; public byte Status; uint Date; }

echo [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi^)] struct CheckList {
echo [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct^)] public CheckStatus[] Signs; }

echo [DllImport("wbotho.dll", EntryPoint = "SignInit", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignInit(string sec, string pub^);

echo [DllImport("wbotho.dll", EntryPoint = "SignLogIn", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignLogIn(string sec^);

echo [DllImport("wbotho.dll", EntryPoint = "SignFile", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignFile(string fileIn, string fileOut, [MarshalAs(UnmanagedType.LPStr, SizeConst = 13^)] string id^);

echo [DllImport("wbotho.dll", EntryPoint = "DelSign", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort DelSign(string file, sbyte count^);

echo [DllImport("wbotho.dll", EntryPoint = "check_file_sign", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CheckFileSign(string file, out byte count, out CheckList list^);

echo [DllImport("wbotho.dll", EntryPoint = "FreeMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern void FreeMemory(CheckList list^);

echo [DllImport("wbotho.dll", EntryPoint = "SignLogOut", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignLogOut(^);

echo [DllImport("wbotho.dll", EntryPoint = "SignDone", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignDone(^); }}
)

rem -------------------------------------------------------------------------
echo Compiling...

pushd "%out%"
for %%f in (%make%) do (
  echo - %%~f
  %net%\csc /nologo /out:"%%~f.exe" "%%~f.cs" *.lib.cs
)
echo.
choice /m "Delete temp *.cs files"
if %errorlevel% equ 1 del /f /q *.cs
popd
goto :eof

rem -------------------------------------------------------------------------
:_using
echo using System;
echo using System.IO;
echo using System.Reflection;
echo using System.Text;

echo [assembly: AssemblyTitle("Verba %app%"^)]
echo namespace App { partial class Program { static void Main(string[] args^) {
goto :eof

:_args
echo string path = Path.GetDirectoryName(args[0]^);
echo string mask = Path.GetFileName(args[0]^);
echo string[] files = Directory.GetFiles(path, mask.Length == 0 ? "*" : mask^); if (files.Length == 0^) return;
echo string pathOut = args[1]; Directory.CreateDirectory(pathOut^);

echo Encoding enc = Encoding.GetEncoding(1251^);
echo DateTime now = DateTime.Now;
if not "%log1%" == "" echo string log1 = string.Format(@"%log1%", now^);
if not "%log2%" == "" echo string log2 = string.Format(@"%log2%", now^);
if not "%log3%" == "" echo string log3 = string.Format(@"%log3%", now^);
goto :eof

:_foreach
echo foreach (string file in files^) {
echo string fileOut = Path.Combine(pathOut, Path.GetFileName(file^)^);
goto :eof

:_print
echo string msg = string.Format(@"%app% {0} {1}", file, ok ? fileOut : "%err%"^);
echo Console.WriteLine(msg^);

echo msg = string.Format("%msg%", DateTime.Now, msg^) + Environment.NewLine;
if not "%log1%" == "" echo File.AppendAllText(log1, msg, enc^);
if not "%log2%" == "" echo File.AppendAllText(log2, msg, enc^);
if not "%log3%" == "" echo File.AppendAllText(log3, msg, enc^);
echo }
goto :eof

:_cinit
echo string pub = @"%pub%"; CryptoInit(pub, pub^);
echo byte[] key = new byte[304]; if (ExtractKey(pub, to, key^) ^> 0^) return;
goto :eof

:_cdone
echo CryptoDone(^); }}}
goto :eof

:_sinit
echo string pub = @"%pub%"; SignInit(pub, pub^); SignLogIn(pub^);
goto :eof

:_sdone
echo SignLogOut(^); SignDone(^); }}}
goto :eof

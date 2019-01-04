@echo off
set version=1.3.0.17
rem set net=%windir%\Microsoft.NET\Framework\v3.5
set net=%windir%\Microsoft.NET\Framework\v4.0.30319

set out=bin
set pub=c:\pub
set ka=206594104002

rem -------------------------------------------------------------------------
md "%out%" 2>nul

rem Escape every ^), ^&, ^<, ^> inside of "echo (...C#...)" below

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
set app=Decrypt

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

echo using System;
echo using System.IO;
echo using System.Reflection;

echo [assembly: AssemblyTitle("Verba %app%"^)]
echo namespace App { partial class Program { static void Main(string[] args^) {

echo if (args.Length ^< 4^) {
echo   Console.WriteLine(@"Usage: %app% in\[*] out\ XXXX[SSSSSS] XXXXSSSSSS [ext]"^); return; }

echo string path = Path.GetDirectoryName(args[0]^);
echo string mask = Path.GetFileName(args[0]^);
echo string[] files = Directory.GetFiles(path, mask.Length == 0 ? "*" : mask^); if (files.Length == 0^) return;
echo string pathOut = args[1]; Directory.CreateDirectory(pathOut^);
echo string id = args[2]; if (id.Length ^> 10^) id = id.Substring(0, 10^);
echo string to = args[3]; if (to.Length ^> 10^) to = to.Substring(0, 10^); if (id.Length == 4^) id += to.Substring(4^);
echo bool changeExt = args.Length ^> 4; string ext = changeExt ? "." + args[4] : string.Empty;

echo string pub = @"%pub%"; CryptoInit(pub, pub^);
echo byte[] key = new byte[304]; if (ExtractKey(pub, id, key^) ^> 0^) return;

echo foreach (string file in files^) {
echo   string fileOut = Path.Combine(pathOut, changeExt ? Path.GetFileNameWithoutExtension(file^) + ext : Path.GetFileName(file^)^);
echo   bool ok = DeCryptFileEx(file, fileOut, to, key^) == 0 ^&^& File.Exists(fileOut^);
echo   if (ok^) { Console.WriteLine(@"%app% {0} {1}", file, fileOut^); File.Delete(file^); }
echo   else { Console.WriteLine(@"%app% {0} FAILED!", file^); }}

echo CryptoDone(^); }}}
)

rem -------------------------------------------------------------------------
set app=Encrypt

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

echo using System;
echo using System.IO;
echo using System.Reflection;
echo using System.Runtime.InteropServices;

echo [assembly: AssemblyTitle("Verba %app%"^)]
echo namespace App { partial class Program { static void Main(string[] args^) {

echo if (args.Length ^< 4^) {
echo   Console.WriteLine(@"Usage: %app% in\[*] out\ XXXXSSSSSS XXXX[SSSSSS] [ext]"^); return; }

echo string path = Path.GetDirectoryName(args[0]^);
echo string mask = Path.GetFileName(args[0]^);
echo string[] files = Directory.GetFiles(path, mask.Length == 0 ? "*" : mask^); if (files.Length == 0^) return;
echo string pathOut = args[1]; Directory.CreateDirectory(pathOut^);
echo string id = args[2]; if (id.Length ^> 10^) id = id.Substring(0, 10^);
echo string to = args[3]; if (to.Length ^> 10^) to = to.Substring(0, 10^); if (to.Length == 4^) to += id.Substring(4^);
echo bool changeExt = args.Length ^> 4; string ext = changeExt ? "." + args[4] : string.Empty;

echo string pub = @"%pub%"; CryptoInit(pub, pub^);
echo byte[] key = new byte[304]; if (ExtractKey(pub, to, key^) ^> 0^) return;
echo IntPtr[] ptr = new IntPtr[] { Marshal.AllocHGlobal(304^) }; Marshal.Copy(key, 0, ptr[0], 304^);

echo foreach (string file in files^) {
echo   string fileOut = Path.Combine(pathOut, changeExt ? Path.GetFileNameWithoutExtension(file^) + ext : Path.GetFileName(file^)^);
echo   bool ok = EnCryptFileEx(file, fileOut, id, ptr, 1, 0^) == 0 ^&^& File.Exists(fileOut^);
echo   if (ok^) { Console.WriteLine(@"%app% {0} {1}", file, fileOut^); File.Delete(file^); }
echo   else { Console.WriteLine(@"%app% {0} FAILED!", file^); }}

echo Marshal.FreeHGlobal(ptr[0]^); CryptoDone(^); }}}
)

rem -------------------------------------------------------------------------
set app=Sign

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

echo using System;
echo using System.IO;
echo using System.Reflection;

echo [assembly: AssemblyTitle("Verba %app%"^)]
echo namespace App { partial class Program { static void Main(string[] args^) {

echo if (args.Length ^< 2^) {
echo   Console.WriteLine(@"Usage: %app% in\[*] out [%ka%]"^); return; }

echo string path = Path.GetDirectoryName(args[0]^);
echo string mask = Path.GetFileName(args[0]^);
echo string[] files = Directory.GetFiles(path, mask.Length == 0 ? "*" : mask^); if (files.Length == 0^) return;
echo string pathOut = args[1]; Directory.CreateDirectory(pathOut^);
echo string id = args.Length ^> 2 ? args[2] : "%ka%";

echo string pub = @"%pub%"; SignInit(pub, pub^); SignLogIn(pub^);

echo foreach (string file in files^) {
echo   string fileOut = Path.Combine(pathOut, Path.GetFileName(file^)^);
echo   bool ok = SignFile(file, fileOut, id^) == 0 ^&^& File.Exists(fileOut^);
echo   if (ok^) { Console.WriteLine(@"%app% {0} {1}", file, fileOut^); File.Delete(file^); }
echo   else { Console.WriteLine(@"%app% {0} FAILED!", file^); }}

echo SignLogOut(^); SignDone(^); }}}
)

rem -------------------------------------------------------------------------
set app=Unsign

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

echo using System;
echo using System.IO;
echo using System.Reflection;

echo [assembly: AssemblyTitle("Verba %app%"^)]
echo namespace App { partial class Program { static void Main(string[] args^) {

echo if (args.Length ^< 2^) {
echo   Console.WriteLine(@"Usage: %app% in\[*] out\"^); return; }

echo string path = Path.GetDirectoryName(args[0]^);
echo string mask = Path.GetFileName(args[0]^);
echo string[] files = Directory.GetFiles(path, mask.Length == 0 ? "*" : mask^); if (files.Length == 0^) return;
echo string pathOut = args[1]; Directory.CreateDirectory(pathOut^);

echo foreach (string file in files^) {
echo   string fileOut = Path.Combine(pathOut, Path.GetFileName(file^)^); File.Copy(file, fileOut, true^);
echo   bool ok = DelSign(fileOut, -1^) == 0 ^&^& File.Exists(fileOut^);
echo   if (ok^) { Console.WriteLine(@"%app% {0} {1}", file, fileOut^); File.Delete(file^); }
echo   else { Console.WriteLine(@"%app% {0} FAILED!", file^); File.Delete(fileOut^); }}}}}
)

rem -------------------------------------------------------------------------
set app=Verify

set make=%make% "%app%"
%1 > "%out%\%app%.cs" (

echo using System;
echo using System.IO;
echo using System.Reflection;

echo [assembly: AssemblyTitle("Verba %app%"^)]
echo namespace App { partial class Program { static void Main(string[] args^) {

echo if (args.Length ^< 2^) {
echo   Console.WriteLine(@"Usage: %app% in\[*] out\"^); return; }

echo string path = Path.GetDirectoryName(args[0]^);
echo string mask = Path.GetFileName(args[0]^);
echo string[] files = Directory.GetFiles(path, mask.Length == 0 ? "*" : mask^); if (files.Length == 0^) return;
echo string pathOut = args[1]; Directory.CreateDirectory(pathOut^);

echo string pub = @"%pub%"; SignInit(pub, pub^); SignLogIn(pub^);

echo foreach (string file in files^) {
echo   string fileOut = Path.Combine(pathOut, Path.GetFileName(file^)^); File.Copy(file, fileOut, true^);

echo   int r; byte count; CheckList list;
echo   if ((r = CheckFileSign(fileOut, out count, out list^)^) == 0^) {
echo     for (int i = 0; i ^< (int^)count; i++^) if ((r = list.Signs[i].Status^) ^> 0^) break;
echo     FreeMemory(list^); }

echo   bool ok = r == 0 ^&^& File.Exists(fileOut^);
echo   if (ok^) { Console.WriteLine(@"%app% {0} {1}", file, fileOut^); File.Delete(file^); }
echo   else { Console.WriteLine(@"%app% {0} FAILED!", file^); File.Delete(fileOut^); }}

echo SignLogOut(^); SignDone(^); }}}
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

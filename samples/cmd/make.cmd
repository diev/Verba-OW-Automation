@echo off
set net=%windir%\Microsoft.NET\Framework\v4.0.30319
set out=bin
set pub=c:\pub
set ka=206594104002

rem -------------------------------------------------------------------------
md %out% 2>nul

rem -------------------------------------------------------------------------
%1 > %out%\decrypt.cs (

echo using System;
echo using System.IO;
echo using System.Runtime.InteropServices;

echo class Program { static void Main^(string[] args^) {

echo string pathIn  = args[0];
echo string mask    = args[1];
echo string pathOut = args[2];
echo string ext     = args[3];
echo string id      = args[4];
echo string to      = args[5];
echo bool move      = args[6] == ^"1^";

echo string pub = @"%pub%"; CryptoInit^(pub, pub^);
echo byte[] key = new byte[304]; if ^(ExtractKey^(pub, id, key^) ^> 0^) return;
echo Directory.CreateDirectory^(pathOut^); bool changeExt = !ext.Equals^(^".*^"^);

echo foreach ^(string file in Directory.GetFiles^(pathIn, mask^)^) {
echo string fileOut = Path.Combine^(pathOut, changeExt ? Path.GetFileNameWithoutExtension^(file^) + ext : Path.GetFileName^(file^)^);
echo if ^(DeCryptFileEx^(file, fileOut, to, key^) == 0 ^&^& move ^&^& File.Exists^(fileOut^)^) File.Delete^(file^); }

echo CryptoDone^(^); }

echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"CryptoInit^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CryptoInit^(string sec, string pub^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"ExtractKey^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort ExtractKey^(string pub, string id, byte[] key^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"DeCryptFileEx^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort DeCryptFileEx^(string fileIn, string fileOut, string id, byte[] key^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"CryptoDone^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CryptoDone^(^); }
)

rem -------------------------------------------------------------------------
%1 > %out%\encrypt.cs (

echo using System;
echo using System.IO;
echo using System.Runtime.InteropServices;

echo class Program { static void Main^(string[] args^) {

echo string pathIn  = args[0];
echo string mask    = args[1];
echo string pathOut = args[2];
echo string ext     = args[3];
echo string id      = args[4];
echo string to      = args[5];
echo bool move      = args[6] == ^"1^";

echo string pub = @"%pub%"; CryptoInit^(pub, pub^);
echo byte[] key = new byte[304]; if ^(ExtractKey^(pub, to, key^) ^> 0^) return;
echo IntPtr[] ptr = new IntPtr[] { Marshal.AllocHGlobal^(304^) }; Marshal.Copy^(key, 0, ptr[0], 304^);
echo Directory.CreateDirectory^(pathOut^); bool changeExt = !ext.Equals^(^".*^"^);

echo foreach ^(string file in Directory.GetFiles^(pathIn, mask^)^) {
echo string fileOut = Path.Combine^(pathOut, changeExt ? Path.GetFileNameWithoutExtension^(file^) + ext : Path.GetFileName^(file^)^);
echo if ^(EnCryptFileEx^(file, fileOut, id, ptr, 1, 0^) == 0 ^&^& move ^&^& File.Exists^(fileOut^)^) File.Delete^(file^); }

echo Marshal.FreeHGlobal^(ptr[0]^); CryptoDone^(^); }

echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"CryptoInit^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CryptoInit^(string sec, string pub^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"ExtractKey^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort ExtractKey^(string pub, string id, byte[] key^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"EnCryptFileEx^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort EnCryptFileEx^(string fileIn, string fileOut, string id, IntPtr[] keys, uint keysCount, ulong flags^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"CryptoDone^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CryptoDone^(^); }
)

rem -------------------------------------------------------------------------
%1 > %out%\sign.cs (

echo using System;
echo using System.IO;
echo using System.Runtime.InteropServices;

echo class Program { static void Main^(string[] args^) {

echo string pathIn  = args[0];
echo string mask    = args[1];
echo string pathOut = args[2];
echo bool move      = args[3] == ^"1^";

echo string pub = @"%pub%"; SignInit^(pub, pub^); SignLogIn^(pub^);
echo Directory.CreateDirectory^(pathOut^);

echo foreach ^(string file in Directory.GetFiles^(pathIn, mask^)^) {
echo string fileOut = Path.Combine^(pathOut, Path.GetFileName^(file^)^);
echo if ^(SignFile^(file, fileOut, "%ka%"^) == 0 ^&^& move ^&^& File.Exists^(fileOut^)^) File.Delete^(file^); }

echo SignLogOut^(^); SignDone^(^); }

echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignInit^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignInit^(string sec, string pub^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignLogIn^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignLogIn^(string sec^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignFile^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignFile^(string fileIn, string fileOut, [MarshalAs^(UnmanagedType.LPStr, SizeConst = 13^)] string id^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignLogOut^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignLogOut^(^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignDone^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignDone^(^); }
)

rem -------------------------------------------------------------------------
%1 > %out%\unsign.cs (

echo using System;
echo using System.IO;
echo using System.Runtime.InteropServices;

echo class Program { static void Main^(string[] args^) {

echo string pathIn  = args[0];
echo string mask    = args[1];
echo string pathOut = args[2];
echo bool move      = args[3] == ^"1^";

echo Directory.CreateDirectory^(pathOut^);
echo foreach ^(string file in Directory.GetFiles^(pathIn, mask^)^) {
echo string fileOut = Path.Combine^(pathOut, Path.GetFileName^(file^)^);
echo File.Copy^(file, fileOut, true^);
echo if ^(DelSign^(fileOut, -1^) == 0 ^&^& move ^&^& File.Exists^(fileOut^)^) File.Delete^(file^);
echo else File.Delete^(fileOut^); }}

echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"DelSign^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort DelSign^(string file, sbyte count^); }
)

rem -------------------------------------------------------------------------
%1 > %out%\verify.cs (

echo using System;
echo using System.IO;
echo using System.Runtime.InteropServices;

echo class Program { static void Main^(string[] args^) {

echo string pathIn  = args[0];
echo string mask    = args[1];
echo string pathOut = args[2];
echo bool move      = args[4] == ^"1^";

echo string pub = @"%pub%"; SignInit^(pub, pub^); SignLogIn^(pub^);
echo Directory.CreateDirectory^(pathOut^);

echo foreach ^(string file in Directory.GetFiles^(pathIn, mask^)^) {
echo string fileOut = Path.Combine^(pathOut, Path.GetFileName^(file^)^);
echo File.Copy^(file, fileOut, true^);

echo int r; byte count; CheckList list;
echo if ^(^(r = CheckFileSign^(fileOut, out count, out list^)^) == 0^) {
echo for ^(int i = 0; i ^< ^(int^)count; i++^) if ^(^(r = list.Signs[i].Status^) ^> 0^) break;
echo FreeMemory^(list^); }

echo if ^(r == 0 ^&^& move ^&^& File.Exists^(fileOut^)^) File.Delete^(file^);
echo else File.Delete^(fileOut^); }

echo SignLogOut^(^); SignDone^(^); }

echo [StructLayout^(LayoutKind.Sequential, CharSet = CharSet.Ansi^), Serializable] struct CheckStatus {
echo [MarshalAs^(UnmanagedType.ByValTStr, SizeConst = 13^)] string Name;
echo [MarshalAs^(UnmanagedType.ByValTStr, SizeConst = 121^)] string Alias;
echo byte Position; public byte Status; uint Date; }

echo [StructLayout^(LayoutKind.Sequential, CharSet = CharSet.Ansi^)] struct CheckList {
echo [MarshalAs^(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct^)] public CheckStatus[] Signs; }

echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignInit^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignInit^(string sec, string pub^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignLogIn^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignLogIn^(string sec^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"check_file_sign^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort CheckFileSign^(string file, out byte count, out CheckList list^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"FreeMemory^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern void FreeMemory^(CheckList list^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignLogOut^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignLogOut^(^);
echo [DllImport^(^"wbotho.dll^", EntryPoint = ^"SignDone^", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true^)]
echo static extern ushort SignDone^(^); }
)

rem -------------------------------------------------------------------------
for %%f in (%out%\*.cs) do call :compile %%f
goto :eof

:compile
%net%\csc /nologo /out:%out%\%~n1.exe %1
del /f /q %1
goto :eof

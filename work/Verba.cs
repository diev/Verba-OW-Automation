// Copyright (c) 2018 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/Verba-OW-Automation

// Используйте %WINDIR%\Microsoft.NET\Framework\v3.5\csc /?
// для Microsoft(R) .NET Framework версии 3.5
// или %WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc для версии .NET 4+

using System;
using System.Runtime.InteropServices;

namespace Verba
{
    #region Structures
    /// <summary>
    /// Запись в таблице слотов драйвера
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), Serializable]
    public struct UsrKeysInfo
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string Num;         // char num[11];
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string Nump;        // char nump[13]
        public ushort KeysStatus;  // T16bit keys_status
        public byte VersionHigh;   // T8bit version_high
        public byte VersionLow;    // T8bit version_low
        public uint KeySlotNumber; // T32bit KeySlotNumber
    }

    /// <summary>
    /// Таблица слотов драйвера (0-15)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SlotsTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public UsrKeysInfo[] Slots;
    }

    /// <summary>
    /// Запись о состоянии проверки подписи
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi), Serializable]
    public struct CheckStatus
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string Name;        // char Name[NAME_LEN + 1];
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 121)]
        public string Alias;       // char Alias[ALIAS_LEN + 1];
        public byte Position;      // T8bit Position;
        public byte Status;        // T8bit Status;
        public uint Date;          // T32bit Date;
                                   // UTC time_t, use DateTime(1970, 1, 1).ToLocalTime().AddSeconds(time_t)
    }

    /// <summary>
    /// Список записей о состоянии проверки каждой подписи
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CheckList
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct)]
        public CheckStatus[] Signs;
    }

    /// <summary>
    /// Запись одного открытого ключа *.pub или *.lfx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct OpenKey
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 304)]
        public string Record;
    }

    /// <summary>
    /// Массив указателей на записи открытых ключей
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct KeyList
    {
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct)]
        public OpenKey[] Keys;
    }
    #endregion Structures

    /// <summary>
    /// Wrapper-класс методов DLL
    /// </summary>
    public static class Wbotho
    {
        #region Crypt
        /// <summary>
        /// 8.2 Инициализация функций шифрования
        /// </summary>
        /// <param name="sec">Строка пути к секретным ключам</param>
        /// <param name="pub">Строка пути к каталогу OPENKEY или FAXKEY (NULL, если ключи симметричные)</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI CryptoInit (char* path, char* base_path);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "CryptoInit", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CryptoInit(string sec, string pub);

        /// <summary>
        /// 8.2 Завершение функций шифрования
        /// </summary>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI CryptoDone(void);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "CryptoDone", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CryptoDone();

        /// <summary>
        /// 8.3.1 Зашифрование файла
        /// </summary>
        /// <param name="fileIn">Исходный открытый файл</param>
        /// <param name="fileOut">Зашифрованный файл</param>
        /// <param name="id">Свой идентификатор (XXXX)</param>
        /// <param name="list">Массив криптографических номеров получателей</param>
        /// <param name="ser">Номер подсети (серии) SSSSSS, куда направляется файл</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI EnCryptFile (char* file_in, char* file_out, T16bit node_From, P16bit node_To, char* ser);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "EnCryptFile", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort EnCryptFile(string fileIn, string fileOut, ushort id, ushort[] list,
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 7)] string ser);

        /// <summary>
        /// 8.3.2 Расшифрование файла
        /// </summary>
        /// <param name="fileIn">Исходный зашифрованный файл</param>
        /// <param name="fileOut">Расшифрованный файл</param>
        /// <param name="id">Номер получателя</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI DeCryptFile (char* file_in, char* file_out, T16bit abonent);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "DeCryptFile", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort DeCryptFile(string fileIn, string fileOut, ushort id);

        /// <summary>
        /// 8.3.4 Получение списка получателей зашифрованного файла
        /// </summary>
        /// <param name="file">Зашифрованный файл</param>
        /// <param name="count">Число получателей</param>
        /// <param name="list">Массив номеров получателей</param>
        /// <param name="ser">Номер серии получателей</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI GetCryptKeysF(char* file_name, P16bit abonents, P16bit* user_list, char* ser);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "GetCryptKeysF", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort GetCryptKeysF(string file, out ushort count, out ushort[] list, //IntPtr toList,
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 7)] string ser);
        // FreeMemory(list) finally!

        /// <summary>
        /// 8.3.5 Зашифрование файла (расширенное)
        /// </summary>
        /// <param name="fileIn">Исходный открытый файл</param>
        /// <param name="fileOut">Зашифрованный файл</param>
        /// <param name="id">Свой идентификатор (XXXXSSSSSS)</param>
        /// <param name="keys">Массив указателей на открытые ключи получателей</param>
        /// <param name="keysCount">Количество получателей</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI EnCryptFile (char* file_in, char* file_out, char* From, 
        /// void** open_keys_array, T16bit open_keys_quantity, T32bit flags);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "EnCryptFileEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort EnCryptFileEx(string fileIn, string fileOut, string id, KeyList keys, uint keysCount, ulong flags);

        /// <summary>
        /// 8.3.6 Расшифрование файла (расширенное)
        /// </summary>
        /// <param name="fileIn">Исходный зашифрованный файл</param>
        /// <param name="fileOut">Расшифрованный файл</param>
        /// <param name="id">Номер получателя (XXXXSSSSSS)</param>
        /// <param name="key">Указатель на открытый ключ отправителя</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI DeCryptFileEx (char* file_in, char* file_out, char* abonent, void* pub_key);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "DeCryptFileEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort DeCryptFileEx(string fileIn, string fileOut, string id, OpenKey key);
        #endregion Crypt

        #region Sign
        /// <summary>
        /// 8.2 Инициализация функций подписи
        /// </summary>
        /// <param name="sec">Путь к файлу с секретным ключом подписи</param>
        /// <param name="pub">Путь к базе открытых ключей подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignInit (char* pathToSecret, char* pathToBase);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignInit", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignInit(string sec, string pub);

        /// <summary>
        /// 8.2 Завершение работы с библиотеками подписи
        /// </summary>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignDone (void);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignDone", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignDone();

        /// <summary>
        /// 8.7.1 Загрузка ключа подписи в оперативную память
        /// </summary>
        /// <param name="sec">Путь к файлу с секретным ключом подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignLogIn (char* path);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignLogIn", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignLogIn(string sec);

        /// <summary>
        /// 8.7.2 Удаление ключа подписи из оперативной памяти
        /// </summary>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignLogOut (void);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignLogOut", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignLogOut();

        /// <summary>
        /// 8.8.1 Подпись файла с добавлением подписи в конец подписываемого файла
        /// </summary>
        /// <param name="fileIn">Исходный файл для подписывания</param>
        /// <param name="fileOut">Подписанный файл</param>
        /// <param name="id">Идентификатор абонента (КА)</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignFile (char* src_file_name, char* dst_file_name, char* name);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignFile", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignFile(string fileIn, string fileOut,
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 13)] string id);

        /// <summary>
        /// 8.8.3 Проверка подписи, добавленной в конец исходного файла
        /// </summary>
        /// <param name="file">Полное имя файла</param>
        /// <param name="count">Число обнаруженных подписей</param>
        /// <param name="list">Массив результатов проверки каждой подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI check_file_sign (char* file_name, P8bit count, Check_Status_Ptr* stat_array);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "check_file_sign", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CheckFileSign(string file, out byte count, out CheckList list);
        // FreeMemory(list) finally!

        /// <summary>
        /// 8.8.4 Проверка подписи, добавленной в конец исходного файла (расширенная)
        /// </summary>
        /// <param name="file">Полное имя файла</param>
        /// <param name="count">Число обнаруженных подписей</param>
        /// <param name="list">Массив результатов проверки каждой подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI check_file_sign_ex(char* file_name, void** open_keys_array, unsigned long open_keys_quantity, 
        /// unsigned char* count, Check_Status_Ptr* status_array);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "check_file_sign_ex", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CheckFileSignEx(string file, KeyList keys, uint keysCount, out byte count, out CheckList list);
        // FreeMemory(list) finally!

        /// <summary>
        /// 8.8.6 Удаление подписи, добавленной в конец исходного файла
        /// </summary>
        /// <param name="file">Полное имя файла</param>
        /// <param name="count">Количество удаляемых подписей, (-1) - удалить все подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI DelSign (char* file_name, T8bit count);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "DelSign", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort DelSign(string file, sbyte count);
        #endregion Sign

        #region Free
        /// <summary>
        /// 8.12 Освобождение памяти, используемой при работе СКЗИ
        /// </summary>
        /// <param name="lpMemory">Указатель, полученный в функциях библиотеки</param>
        /// <remarks>extern void WINAPI FreeMemory (void* lpMemory);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "FreeMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void FreeMemory(IntPtr lpMemory);

        /// <summary>
        /// 8.12 Освобождение памяти, используемой при работе СКЗИ
        /// </summary>
        /// <param name="list">Указатель, полученный в GetCryptKeysF()</param>
        [DllImport("wbotho.dll", EntryPoint = "FreeMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void FreeMemory(ushort[] list);

        /// <summary>
        /// 8.12 Освобождение памяти, используемой при работе СКЗИ
        /// </summary>
        /// <param name="list">Указатель, полученный в CheckFileSign()</param>
        [DllImport("wbotho.dll", EntryPoint = "FreeMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void FreeMemory(CheckList list);
        #endregion Free

        #region Spr
        /// <summary>
        /// 9.3.5 Добавление открытого ключа в справочник
        /// </summary>
        /// <param name="pub">Строка пути к каталогу OPENKEY или FAXKEY (если их нет - создаются)</param>
        /// <param name="key">Блок памяти размером 304 байт, в котором находится открытый ключ</param>
        /// <param name="id">Строка идентификатора ключа (XXXXSSSSSS[YY])</param>
        /// <param name="se">байт, указывающий, в какой справочник добавляется ключ ('S' или 'E')</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI AddOpenKey (char* base_dir, void* open_key, char* my_ID, char S_or_E);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "AddOpenKey", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort AddOpenKey(string pub, OpenKey key, string id, byte se);

        /// <summary>
        /// 9.3.13 Выработка имитовставки для справочника
        /// </summary>
        /// <param name="pub">Строка пути к каталогу OPENKEY или FAXKEY</param>
        /// <param name="ser">Строка с номером серии открытого ключа, на котором подписывается справочник</param>
        /// <param name="id">Строка идентификатора ключа (XXXXSSSSSS[YY]), на котором будет выработана имитовставка</param>
        /// <param name="se">байт, указывающий, для какого справочника ('S' или 'E')</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignSpr (char* base_dir, char* ser, char* my_ID, char S_or_E);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignSpr", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignSpr(string pub, string ser, string id, byte se);

        /// <summary>
        /// 9.3.15 Считывание открытого ключа из справочника в память
        /// </summary>
        /// <param name="pub">Строка пути к каталогу OPENKEY или FAXKEY</param>
        /// <param name="id">Строка идентификатора ключа (XXXXSSSSSS[YY])</param>
        /// <param name="key">Блок памяти размером 304 байт, в который считывается указанный открытый ключ</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI ExtractKey (char* base_dir, char* open_key_ID, void* key);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "ExtractKey", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort ExtractKey(string pub, string id, out OpenKey key);
        #endregion Spr

        #region Key
        /// <summary>
        /// 9.4.2 Загрузка ключей в драйвер ASYNCR
        /// </summary>
        /// <param name="keyDev">Строка с именем ключевого устройства</param>
        /// <param name="keyId">Идентификатор ключа или пустая строка ("")</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI InitKey (char* key_dev, char* key_ID);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "InitKey", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort InitKey(string keyDev, string keyId);

        /// <summary>
        /// 9.4.3 Выгрузка ключей из драйвера ASYNCR
        /// </summary>
        /// <param name="keyId">Идентификатор ключа</param>
        /// <param name="flag">Признак возможности выгрузки ключа из "слота" 0: FALSE-выгрузка запрещена, TRUE-разрешена</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI ResetKeyEx (char* key_ID, int flag);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "ResetKeyEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort ResetKeyEx(string keyId, bool flag);

        /// <summary>
        /// 9.4.4 Получение списка ключей, загруженных в драйвер ASYNCR
        /// </summary>
        /// <param name="keysList">Массив структур с информацией о прогруженных ключах</param>
        /// <param name="count">Количество загруженных ключевых слотов</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI GetDrvInfo(USR_KEYS_INFO* keys_info, P32bit nKeySlots);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "GetDrvInfo", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort GetDrvInfo(ref SlotsTable table, out uint count);
        #endregion Key
    }

    /// <summary>
    /// Класс методов для поточных PowerShell Process{}
    /// </summary>
    public static class Posh
    {
        /// <summary>
        /// Зашифровать файл
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Зашифрованный файл</param>
        /// <param name="id">Номер отправителя (серия опционально)</param>
        /// <param name="to">Номера получателей (той же серии) ...</param>
        public static int Encrypt(string fileIn, string fileOut, string id, params string[] to)
        {
            string ser = "";
            if (id.Length == 10)
            {
                id = id.Substring(0, 4);
                ser = id.Substring(4);
            }
            ushort[] nodes = new ushort[to.Length + 1];
            for (int i = 0; i < to.Length; i++)
            {
                nodes[i] = ushort.Parse(to[i]);
            }
            nodes[to.Length] = 0;
            return Wbotho.EnCryptFile(fileIn, fileOut, ushort.Parse(id), nodes, ser);
        }

        /// <summary>
        /// Расшифровать файл
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Расшифрованный файл</param>
        /// <param name="id">Номер получателя</param>
        public static int Decrypt(string fileIn, string fileOut, string id)
        {
            return Wbotho.DeCryptFile(fileIn, fileOut, ushort.Parse(id));
        }

        /// <summary>
        /// Подписать файл
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Подписанный файл</param>
        /// <param name="id">Код аутентификации (КА)</param>
        public static int Sign(string fileIn, string fileOut, string id)
        {
            return Wbotho.SignFile(fileIn, fileOut, id);
        }

        /// <summary>
        /// Проверить все подписи в конце файла (новое: они реально проверяются)
        /// </summary>
        /// <param name="file">Файл с подписями</param>
        public static int Verify(string file)
        {
            int ret;
            byte count;
            CheckList list;
            if ((ret = Wbotho.CheckFileSign(file, out count, out list)) > 0)
            {
                return ret;
            }
            for (int i = 0; i < count; i++)
            {
                switch (list.Signs[i].Status)
                {
                    case 0: //CORRECT
                        break;
                    case 1: //NOT_CORRECT
                        ret = 26;
                        break;
                    case 2: //OKEY_NOT_FOUND
                        ret = 6;
                        break;
                }
            }
            Wbotho.FreeMemory(list);
            return ret;
        }

        /// <summary>
        /// Удалить все подписи в конце файла (новое: без параметра с числом их, -1 всегда)
        /// </summary>
        /// <param name="file">Файл с подписями</param>
        public static int Unsign(string file)
        {
            return Wbotho.DelSign(file, -1);
        }
    }

    /// <summary>
    /// Класс методов совместимости с консольной утилитой Wftesto
    /// </summary>
    public static class Wftesto
    {
        /// <summary>
        /// Зашифровать файл
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Зашифрованный файл</param>
        /// <param name="sec">Путь к секретному ключу</param>
        /// <param name="pub">Путь к открытым ключам</param>
        /// <param name="id">Номер отправителя (серия опционально)</param>
        /// <param name="to">Номера получателей (той же серии) ...</param>
        /// <remarks>
        /// wftesto.exe e file.txt file.cry a:\ c:\pub 0001 0002
        /// wftesto.exe e file.txt file.cry a:\ c:\pub 0001 0003 0004
        /// wftesto.exe e file.txt file.cry c:\key\ c:\pub 0005[999999] 0006 0007
        /// </remarks>
        public static int Encrypt(string fileIn, string fileOut, string sec, string pub, string id, params string[] to)
        {
            int ret;
            if ((ret = Wbotho.CryptoInit(sec, pub)) > 0)
            {
                Console.WriteLine("CryptoInit error : {0}", ret);
                return ret;
            }
            if ((ret = Posh.Encrypt(fileIn, fileOut, id, to)) > 0)
            {
                Console.WriteLine("EnCryptFile error : {0}", ret);
                return ret;
            }
            Wbotho.CryptoDone();
            Console.WriteLine("File {0} encrypted to {1}", fileIn, fileOut);
            return ret;
        }

        /// <summary>
        /// Расшифровать файл
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Расшифрованный файл</param>
        /// <param name="sec">Путь к секретному ключу</param>
        /// <param name="pub">Путь к открытым ключам</param>
        /// <param name="id">Номер получателя</param>
        /// <remarks>wftesto.exe d file.cry file.txt c:\key c:\pub 1810</remarks>
        public static int Decrypt(string fileIn, string fileOut, string sec, string pub, string id)
        {
            int ret;
            if ((ret = Wbotho.CryptoInit(sec, pub)) > 0)
            {
                Console.WriteLine("CryptoInit error : {0}", ret);
                return ret;
            }
            if ((ret = Posh.Decrypt(fileIn, fileOut, id)) > 0)
            {
                Console.WriteLine("DeCryptFile error : {0}", ret);
                return ret;
            }
            Wbotho.CryptoDone();
            Console.WriteLine("File {0} decrypted to {1}", fileIn, fileOut);
            return ret;
        }

        /// <summary>
        /// Получить список получателей зашифрованного файла
        /// </summary>
        /// <param name="file">Зашифрованный файл</param>
        /// <remarks>wftesto.exe g file.cry</remarks>
        public static int GetAbonents(string file)
        {
            int ret;
            if ((ret = Wbotho.CryptoInit("", "")) > 0)
            {
                Console.WriteLine("CryptoInit error : {0}", ret);
                return ret;
            }
            ushort count;
            ushort[] list;
            if ((ret = Wbotho.GetCryptKeysF(file, out count, out list, "")) > 0)
            {
                Console.WriteLine("GetCryptKeysF error : {0}", ret);
                return ret;
            }
            Wbotho.CryptoDone();
            Console.WriteLine("File {0} is encrypted for these abonents :", file); //новое: исправлено слово
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine("ID{0} - {1}", i + 1, list[i]);
            }
            Wbotho.FreeMemory(list);
            return ret;
        }

        /// <summary>
        /// Подписать файл
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Подписанный файл</param>
        /// <param name="sec">Путь к секретному ключу</param>
        /// <param name="id">Код аутентификации (КА)</param>
        /// <remarks>wftesto.exe s file.txt file.sig a: 000122222201</remarks>
        public static int Sign(string fileIn, string fileOut, string sec, string id)
        {
            int ret;
            if ((ret = Wbotho.SignInit(sec, "")) > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            if ((ret = Posh.Sign(fileIn, fileOut, id)) > 0)
            {
                Console.WriteLine("SignFile error : {0}", ret);
                return ret;
            }
            Wbotho.SignDone();
            Console.WriteLine("File {0} signed to {1}", fileIn, fileOut);
            return ret;
        }

        /// <summary>
        /// Проверить все подписи в конце файла (новое: они реально проверяются)
        /// </summary>
        /// <param name="file">Файл с подписями</param>
        /// <param name="pub">Путь к открытым ключам</param>
        /// <remarks>wftesto.exe v file.txt c:\pub</remarks>
        public static int Verify(string file, string pub)
        {
            int ret;
            if ((ret = Wbotho.SignInit("", pub)) > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            byte count;
            CheckList list;
            if ((ret = Wbotho.CheckFileSign(file, out count, out list)) > 0)
            {
                Console.WriteLine("check_file_sign error : {0}", ret);
                return ret;
            }
            Wbotho.SignDone();
            for (int i = 0; i < count; i++)
            {
                Console.Write("{0} - ", list.Signs[i].Alias);
                switch (list.Signs[i].Status)
                {
                    case 0: //CORRECT
                        Console.WriteLine("sign is OK");
                        break;
                    case 1: //NOT_CORRECT
                        Console.WriteLine("sign is corrupted");
                        ret = 26;
                        break;
                    case 2: //OKEY_NOT_FOUND
                        Console.WriteLine("key not found");
                        ret = 6;
                        break;
                }
            }
            Wbotho.FreeMemory(list);
            Console.WriteLine("File {0} verified", file);
            return ret;
        }

        /// <summary>
        /// Удалить все подписи в конце файла (новое: без параметра с числом их)
        /// </summary>
        /// <param name="file">Файл с подписями</param>
        /// <remarks>wftesto.exe u file.txt</remarks>
        public static int Unsign(string file)
        {
            int ret;
            if ((ret = Wbotho.SignInit("", "")) > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            if ((ret = Posh.Unsign(file)) > 0)
            {
                Console.WriteLine("DelSign error : {0}", ret);
                return ret;
            }
            Wbotho.SignDone();
            Console.WriteLine("Sign deleted in file {0}", file);
            return ret;
        }

        /// <summary>
        /// Загрузить в память драйвера ключ с внешнего ключевого устройства
        /// </summary>
        /// <param name="id">Номер ключа шифрования или КА</param>
        /// <param name="dev">Ключевое устройство (A: или другое)</param>
        /// <remarks>wftesto.exe i [000566666601] [ruToken]</remarks>
        public static int InitKey(string id, string dev)
        {
            int ret;
            if ((ret = Wbotho.InitKey(dev, id)) > 0)
            {
                Console.WriteLine("InitKey error : {0}", ret);
                return ret;
            }
            Console.WriteLine("InitKey done : {0}", ret);
            return ret;
        }

        /// <summary>
        /// Выгрузить ключ из памяти драйвера (новое: слот 0 разрешен также)
        /// </summary>
        /// <param name="id">Номер ключа</param>
        /// <remarks>wftesto.exe r 000566666601</remarks>
        public static int ResetKey(string id)
        {
            int ret;
            if ((ret = Wbotho.ResetKeyEx(id, true)) > 0)
            {
                Console.WriteLine("ResetKey error : {0}", ret);
                return ret;
            }
            Console.WriteLine("ResetKey done : {0}", ret);
            return ret;
        }

        /// <summary>
        /// Выгрузить все ключи из памяти драйвера (новое)
        /// </summary>
        /// <remarks>wftesto.exe r</remarks>
        public static int ResetKeys()
        {
            int ret;
            SlotsTable table = new SlotsTable();
            table.Slots = new UsrKeysInfo[16];
            uint count;
            if ((ret = Wbotho.GetDrvInfo(ref table, out count)) > 0)
            {
                Console.WriteLine("GetDrvInfo error : {0}", ret);
                return ret;
            }
            for (int i = 0; i < count; i++)
            {
                if (table.Slots[i].Num[0] != '\0') //crypto
                {
                    ret += ResetKey(table.Slots[i].Num);
                }
                else if (table.Slots[i].Nump[0] != '\0') //sign
                {
                    ret += ResetKey(table.Slots[i].Nump);
                }
            }
            return ret == 0 ? ret : 1;
        }

        /// <summary>
        /// Получить перечень загруженных в память драйвера ключей
        /// </summary>
        /// <remarks>wftesto.exe l</remarks>
        public static int ListKeys()
        {
            int ret;
            SlotsTable table = new SlotsTable();
            table.Slots = new UsrKeysInfo[16];
            uint count;
            if ((ret = Wbotho.GetDrvInfo(ref table, out count)) > 0)
            {
                Console.WriteLine("GetDrvInfo error : {0}", ret);
                return ret;
            }
            Console.WriteLine();
            Console.Write("DrvInfo:");
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine();
                Console.WriteLine("Slot : {0}", table.Slots[i].KeySlotNumber);
                Console.WriteLine("NUM  : {0}", table.Slots[i].Num);
                Console.WriteLine("NUMP : {0}", table.Slots[i].Nump);
            }
            return ret;
        }
    }

    /// <summary>
    /// Справочник кодов возврата
    /// </summary>
    public static class Result
    { 
        /// <summary>
        /// Возвращает текстовую строку с кодом и описанием ошибки по коду возврата функций библиотеки
        /// </summary>
        /// <param name="errСode">Код возврата (0 или код ошибки)</param>
        /// <returns>Строка сообщения об ошибке</returns>
        public static string Text(int errCode)
        {
            if (errCode == 0)
            {
                return "Нет ошибок.";
            }
            string s;
            switch (errCode)
            {
                case 1: s = "Недостаточно динамической памяти"; break;
                case 2: s = "Сбой криптографической функции или искажение тела библиотеки"; break;
                case 3: s = "Ошибка датчика случайных чисел"; break;
                case 4: s = "Не совпадает имитовставка - файл (блок памяти) искажён"; break;
                case 5: s = "Системная ошибка"; break;
                case 6: s = "Ключ не найден (или искажён)"; break;
                case 7: s = "Ошибка параметра обращения к функции"; break;
                case 8: s = "Ошибка инициализации"; break;

                case 10: s = "Неверная длина блока памяти"; break;
                case 11: s = "Попытка расшифровать незашифрованный блок памяти"; break;
                case 12: s = "Попытка проверить подпись неподписанного блока памяти"; break;

                case 21: s = "Ошибка открытия входного файла"; break;
                case 22: s = "Ошибка открытия выходного файла"; break;
                case 23: s = "Ошибка записи файла"; break;
                case 24: s = "Ошибка чтения файла"; break;
                case 25: s = "Ошибка переименования(перемещения) файла"; break;
                case 26: s = "Неверная (например, нулевая) длина файла"; break;
                case 27: s = "Несовпадение контрольной суммы зашифрованного файла"; break;

                case 29: s = "Попытка расшифрования незашифрованного файла"; break;
                case 30: s = "Попытка проверки подписи неподписанного файла"; break;
                case 31: s = "Ошибка перемещения указателя в файле"; break;
                case 32: s = "Ошибка закрытия файла"; break;
                case 33: s = "Ошибка удаления файла"; break;
                case 34: s = "Ошибка обращения к GK"; break;
                case 35: s = "Ошибка обращения к KS"; break;
                case 36: s = "Ошибка обращения к устройству"; break;
                case 37: s = "Повторная загрузка ключа"; break;
                case 38: s = "Ошибка нет свободных слотов"; break;
                case 39: s = "Ключ не установлен"; break;
                case 40: s = "Ошибка чтения GK"; break;
                case 41: s = "Ошибка записи GK"; break;
                case 42: s = "Неподдерживаемый формат"; break;

                case 101: s = "NUM(P) не соответствует считанному из ДСЧ"; break;
                case 102: s = "Значение хэш-функции не совпало"; break;
                case 103: s = "Ошибка открытия справочника"; break;
                case 104: s = "Ошибка открытия файла *.IMM"; break;
                case 105: s = "Ошибка чтения UZ"; break;
                case 106: s = "Ошибка чтения  CKD(I)"; break;
                case 107: s = "Длины файлов не соответствуют друг другу"; break;
                case 108: s = "Ошибка чтения справочника"; break;
                case 109: s = "Ошибка записи справочника"; break;
                case 110: s = "Ошибка чтения имитовставки"; break;
                case 111: s = "Неверная имитовставка"; break;
                case 112: s = "Открытый ключ скомпрометирован"; break;
                case 113: s = "Ошибка создания каталога"; break;
                case 114: s = "Ошибка при создании *.IMM(P) или *.SPR"; break;
                case 115: s = "В заданном каталоге уже есть файл *.SPR"; break;
                case 116: s = "Ошибка записи в файл *.IMM"; break;
                case 117: s = "В справочнике нет заданного открытого ключа"; break;
                case 118: s = "Неверная длина  файла  *.SPR или *.IMM(P)"; break;
                case 119: s = "Ошибка открытия временного файла"; break;
                case 120: s = "Справочник открытых ключей пуст"; break;
                case 121: s = "Искажен заголовок открытого ключа"; break;
                case 122: s = "Не найден справочник"; break;
                case 123: s = "Открытый ключ не является резервным"; break;
                case 124: s = "Искажен заголовок файла имитовставок"; break;
                case 125: s = "Нет имитовставки на открытый ключ"; break;
                case 126: s = "Нет имитовставки с указанным номером"; break;
                case 127: s = "Ошибка при обращении к гибкому диску"; break;
                case 128: s = "Не найден справочник открытых ключей"; break;
                case 129: s = "Неправильный ключ"; break;
                case 130: s = "Ошибка пакования буфера"; break;
                case 131: s = "Имитовставка выработана на ключе другой серии"; break;
                case 132: s = "Неверный тип ключа"; break;
                case 133: s = "Вставлен другой носитель"; break;

                default:
                    s = "Ошибки при операции. Смотрите протокол"; break;
            }
            return "Ошибка " + errCode + " : " + s + ".";
        }
    }

    /// <summary>
    /// Класс для тестирования полноты функционирования
    /// </summary>
    class CheckIntegrity
    {
        static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    switch (args[0].ToLower())
                    {
                        case "e": // file.txt file.cry c:\key\ c:\pub 0005999999 0006 ... //TODO +n
                            return Wftesto.Encrypt(args[1], args[2], args[3], args[4], args[5], args[6]);
                        case "d": // file.cry file.txt c:\key c:\pub 1810
                            return Wftesto.Decrypt(args[1], args[2], args[3], args[4], args[5]);
                        case "g":
                            return Wftesto.GetAbonents(args[1]);
                        case "s": // file.txt file.sig a: 000122222201
                            return Wftesto.Sign(args[1], args[2], args[3], args[4]);
                        case "v": // file.txt c:\pub
                            return Wftesto.Verify(args[1], args[2]);
                        case "u":
                            return Wftesto.Unsign(args[1]);
                        case "i": // [Key_ID] [Key_Dev]
                            switch (args.Length)
                            {
                                case 3: return Wftesto.InitKey(args[1], args[2]);
                                case 2: return Wftesto.InitKey(args[1], "");
                                case 1: return Wftesto.InitKey("", "");
                            }
                            break;
                        case "r":
                            switch (args.Length)
                            {
                                case 2: return Wftesto.ResetKey(args[1]);
                                case 1: return Wftesto.ResetKeys();
                            }
                            break;
                        case "l":
                            return Wftesto.ListKeys();

                        case "?": //
                            Console.WriteLine("Справка: " + Result.Text(int.Parse(args[1])));
                            return 0;

                        default:
                            Console.WriteLine("Непонятная команда: " + args[0]);
                            return 1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка в параметрах: " + e.Message);
                    return 1;
                }
            }
            Console.WriteLine(Result.Text(0)); //TODO Usage
            return 0;
        }
    }
}

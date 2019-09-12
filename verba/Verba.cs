// Copyright (c) 2018-2019 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/Verba-OW-Automation

using System;
using System.Collections.Generic;
using System.IO;
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        public string Num;         // char num[11];
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 13)]
        public string Name;        // char Name[NAME_LEN + 1];
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 121)]
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
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 304)]
        public byte[] Bytes;
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
        /// <param name="pub">Строка пути к каталогу c OPENKEY или FAXKEY (NULL, если ключи симметричные)</param>
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
        public static extern ushort EnCryptFileEx(string fileIn, string fileOut, string id, IntPtr[] keys, uint keysCount, ulong flags);

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
        public static extern ushort DeCryptFileEx(string fileIn, string fileOut, string id, byte[] key);
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
        /// 8.8.2 Подпись файла с сохранением ЭЦП в отдельном файле
        /// </summary>
        /// <param name="file">Исходный файл для подписывания</param>
        /// <param name="id">Идентификатор абонента (КА)</param>
        /// <param name="fileSig">Отдельный файл с подписями</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignFile (char* src_file_name, char* name, char* sign_file);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignFileSeparate", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignFileSeparate(string file,
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 13)] string id, string fileSig);

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
        public static extern ushort CheckFileSignEx(string file, IntPtr keys, uint keysCount, out byte count, out CheckList list);
        // FreeMemory(list) finally!

        /// <summary>
        /// 8.8.5 Проверка подписи, сохраненной в отдельном файле
        /// </summary>
        /// <param name="file">Полное имя файла</param>
        /// <param name="count">Число обнаруженных подписей</param>
        /// <param name="list">Массив результатов проверки каждой подписи</param>
        /// <param name="fileSig">Отдельный файл с подписями</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI CheckFileSeparate (char* file_name, P8bit count, Check_Status_Ptr* stat_array, char* sign_file);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "CheckFileSeparate", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CheckFileSeparate(string file, out byte count, out CheckList list, string fileSig);
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
        public static extern ushort ExtractKey(string pub, string id, byte[] key);
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
    /// Класс базовых методов для поточных PowerShell Process {}
    /// (Используются справочники на диске.)
    /// </summary>
    public static class Posh
    {
        /// <summary>
        /// Зашифрование файла
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Зашифрованный файл</param>
        /// <param name="id">Номер отправителя (серия опционально)</param>
        /// <param name="to">Номера получателей (той же серии) ...</param>
        /// <returns>0 или код ошибки</returns>
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
        /// Расшифрование файла
        /// </summary>
        /// <param name="fileIn">Исходный зашифрованный файл</param>
        /// <param name="fileOut">Расшифрованный файл</param>
        /// <param name="id">Номер получателя (XXXX)</param>
        /// <returns>0 или код ошибки</returns>
        public static int Decrypt(string fileIn, string fileOut, string id)
        {
            return Wbotho.DeCryptFile(fileIn, fileOut, ushort.Parse(id));
        }

        /// <summary>
        /// Подпись файла с добавлением подписи в конец подписываемого файла
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Подписанный файл</param>
        /// <param name="id">Код аутентификации (КА)</param>
        /// <returns>0 или код ошибки</returns>
        public static int Sign(string fileIn, string fileOut, string id)
        {
            return Wbotho.SignFile(fileIn, fileOut, id);
        }

        /// <summary>
        /// Подпись файла с сохранением ЭЦП в отдельном файле
        /// </summary>
        /// <param name="file">Исходный файл</param>
        /// <param name="id">Код аутентификации (КА)</param>
        /// <param name="fileSign">Отдельный файл с подписями</param>
        /// <returns>0 или код ошибки</returns>
        public static int SignSeparate(string file, string id, string fileSign)
        {
            return Wbotho.SignFileSeparate(file, id, fileSign);
        }

        /// <summary>
        /// Проверка подписи, добавленной в конец исходного файла
        /// </summary>
        /// <param name="file">Подписанный файл</param>
        public static int Verify(string file)
        {
            int ret;
            byte count;
            CheckList list;
            if ((ret = Wbotho.CheckFileSign(file, out count, out list)) > 0)
            {
                return ret;
            }
            for (int i = 0; i < (int)count; i++)
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
        /// Проверка подписи, сохраненной в отдельном файле
        /// </summary>
        /// <param name="file">Подписанный файл</param>
        /// <param name="fileSig">Файл с подписями</param>
        public static int VerifySeparate(string file, string fileSig)
        {
            int ret;
            byte count;
            CheckList list;
            if ((ret = Wbotho.CheckFileSeparate(file, out count, out list, fileSig)) > 0)
            {
                return ret;
            }
            for (int i = 0; i < (int)count; i++)
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
        /// Удаление подписи, добавленной в конец исходного файла
        /// </summary>
        /// <param name="file">Файл с подписями</param>
        /// <returns>0 или код ошибки</returns>
        public static int Unsign(string file)
        {
            return Wbotho.DelSign(file, -1);
        }
    }

    /// <summary>
    /// Класс расширенных методов для поточных PowerShell Process {}
    /// (Используются ключи с предварительной загрузкой в память.)
    /// </summary>
    public static class PoshEx
    {
        /// <summary>
        /// Зашифрование файла (расширенное)
        /// </summary>
        /// <param name="fileIn">Исходный файл</param>
        /// <param name="fileOut">Зашифрованный файл</param>
        /// <param name="pub">Строка пути к каталогу c OPENKEY</param>
        /// <param name="id">Номер отправителя (XXXXSSSSSS)</param>
        /// <param name="to">Номер получателя (XXXXSSSSSS)</param>
        /// <returns>0 или код ошибки</returns>
        public static int EncryptEx(string fileIn, string fileOut, string pub, string id, string to)
        {
            int ret;
            byte[] key = new byte[304];
            if ((ret = Wbotho.ExtractKey(pub, to, key)) > 0)
            {
                return ret;
            }

            IntPtr[] ptr = new IntPtr[] { Marshal.AllocHGlobal(304) };
            Marshal.Copy(key, 0, ptr[0], 304);

            ret = Wbotho.EnCryptFileEx(fileIn, fileOut, id, ptr, 1, 0);

            Marshal.FreeHGlobal(ptr[0]);
            return ret;
        }

        /// <summary>
        /// Расшифрование файла (расширенное)
        /// </summary>
        /// <param name="fileIn">Исходный зашифрованный файл</param>
        /// <param name="fileOut">Расшифрованный файл</param>
        /// <param name="pub">Строка пути к каталогу c OPENKEY</param>
        /// <param name="id">Номер отправителя (XXXXSSSSSS)</param>
        /// <param name="to">Номер получателя (XXXXSSSSSS)</param>
        public static int DecryptEx(string fileIn, string fileOut, string pub, string id, string to)
        {
            int ret;
            byte[] key = new byte[304];
            if ((ret = Wbotho.ExtractKey(pub, id, key)) > 0)
            {
                return ret;
            }

            return Wbotho.DeCryptFileEx(fileIn, fileOut, to, key);
        }

        /// <summary>
        /// Проверка подписи, добавленной в конец исходного файла (расширенная)
        /// </summary>
        /// <param name="file">Исходный файл с подписями</param>
        /// <param name="keys">Ключи проверки подписей отправителей</param>
        /// <param name="count">Число обнаруженных подписей</param>
        /// <param name="list">Массив результатов проверки каждой подписи</param>
        /// <returns>0 или код ошибки</returns>
        public static int VerifyEx(string file, OpenKey[] keys, out byte count, out CheckList list)
        {
            var allocated = new List<IntPtr>();

            int ptrSize = Marshal.SizeOf(typeof(IntPtr));
            IntPtr keysPtr = Marshal.AllocHGlobal(ptrSize * keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                IntPtr keyPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(OpenKey)));
                allocated.Add(keyPtr);
                Marshal.StructureToPtr(keys[i], keyPtr, false);

                Marshal.WriteIntPtr(keysPtr, i * ptrSize, keyPtr);
            }

            int ret = Wbotho.CheckFileSignEx(file, keysPtr, (uint)keys.Length, out count, out list);

            Marshal.FreeHGlobal(keysPtr);
            foreach (IntPtr ptr in allocated)
            {
                Marshal.FreeHGlobal(ptr);
            }
            return ret;
        }

        #region Bulk
        /// <summary>
        /// Пакетное зашифрование файлов
        /// </summary>
        /// <param name="pathIn">Папка с исходными файлами</param>
        /// <param name="mask">Маска файлов для обработки (*.*)</param>
        /// <param name="pathOut">Папка для зашифрованных файлов (e)</param>
        /// <param name="ext">Новое расширение для зашифрованных файлов (.*)</param>
        /// <param name="pub">Строка пути к каталогу c OPENKEY (\Pub)</param>
        /// <param name="id">Номер отправителя (XXXXSSSSSS)</param>
        /// <param name="to">Номер получателя (XXXXSSSSSS)</param>
        /// <param name="move">Удалять исходные файлы в случае успеха</param>
        /// <returns>0 или код последней ошибки</returns>
        public static int BulkEncrypt(string pathIn, string mask, string pathOut, string ext, string pub, string id, string to, bool move)
        {
            Wbotho.CryptoInit(pub, pub);

            int ret;
            byte[] key = new byte[304];
            if ((ret = Wbotho.ExtractKey(pub, to, key)) > 0)
            {
                return ret;
            }

            IntPtr[] ptr = new IntPtr[] { Marshal.AllocHGlobal(304) };
            Marshal.Copy(key, 0, ptr[0], 304);

            Directory.CreateDirectory(pathOut);
            bool changeExt = !ext.Equals(".*");

            foreach (string file in Directory.GetFiles(pathIn, mask))
            {
                string fileOut = Path.Combine(pathOut, changeExt ?
                    Path.GetFileNameWithoutExtension(file) + ext :
                    Path.GetFileName(file));
                int r = Wbotho.EnCryptFileEx(file, fileOut, id, ptr, 1, 0);
                if (r == 0 && move && File.Exists(fileOut))
                { 
                    File.Delete(file);
                }
                else
                {
                    ret = r;
                }
            }

            Marshal.FreeHGlobal(ptr[0]);

            Wbotho.CryptoDone();
            return ret;
        }

        /// <summary>
        /// Пакетное расшифрование файлов
        /// </summary>
        /// <param name="pathIn">Папка с исходными файлами</param>
        /// <param name="mask">Маска файлов для обработки (*.*)</param>
        /// <param name="pathOut">Папка для расшифрованных файлов (d)</param>
        /// <param name="ext">Новое расширение для расшифрованных файлов (.*)</param>
        /// <param name="pub">Строка пути к каталогу c OPENKEY (\Pub)</param>
        /// <param name="id">Номер отправителя (XXXXSSSSSS)</param>
        /// <param name="to">Номер получателя (XXXXSSSSSS)</param>
        /// <param name="move">Удалять исходные файлы в случае успеха</param>
        /// <returns>0 или код последней ошибки</returns>
        public static int BulkDecrypt(string pathIn, string mask, string pathOut, string ext, string pub, string id, string to, bool move)
        {
            Wbotho.CryptoInit(pub, pub);

            int ret;
            byte[] key = new byte[304];
            if ((ret = Wbotho.ExtractKey(pub, id, key)) > 0)
            {
                return ret;
            }

            Directory.CreateDirectory(pathOut);
            bool changeExt = !ext.Equals(".*");

            foreach (string file in Directory.GetFiles(pathIn, mask))
            {
                string fileOut = Path.Combine(pathOut, changeExt ?
                    Path.GetFileNameWithoutExtension(file) + ext :
                    Path.GetFileName(file));
                int r = Wbotho.DeCryptFileEx(file, fileOut, to, key);
                if (r == 0 && move && File.Exists(fileOut))
                {
                    File.Delete(file);
                }
                else
                {
                    ret = r;
                }
            }

            Wbotho.CryptoDone();
            return ret;
        }

        /// <summary>
        /// Пакетное подписывание файлов
        /// </summary>
        /// <param name="pathIn">Папка с исходными файлами</param>
        /// <param name="mask">Маска файлов для обработки (*.*)</param>
        /// <param name="pathOut">Папка для подписанных файлов (s)</param>
        /// <param name="pub">Строка пути к каталогу c OPENKEY (\Pub)</param>
        /// <param name="id">Код аутентификации - КА (XXXXSSSSSSYY)</param>
        /// <param name="move">Удалять исходные файлы в случае успеха</param>
        /// <returns>0 или код последней ошибки</returns>
        public static int BulkSign(string pathIn, string mask, string pathOut, string pub, string id, bool move)
        {
            Wbotho.SignInit(pub, pub);
            Wbotho.SignLogIn(pub);

            Directory.CreateDirectory(pathOut);

            int ret = 0;
            foreach (string file in Directory.GetFiles(pathIn, mask))
            {
                string fileOut = Path.Combine(pathOut, Path.GetFileName(file));
                int r = Wbotho.SignFile(file, fileOut, id);
                if (r == 0 && move && File.Exists(fileOut))
                {
                    File.Delete(file);
                }
                else
                {
                    ret = r;
                }
            }

            Wbotho.SignLogOut();
            Wbotho.SignDone();
            return ret;
        }

        /// <summary>
        /// Пакетная проверка подписей файлов
        /// </summary>
        /// <param name="pathIn">Папка с исходными файлами</param>
        /// <param name="mask">Маска файлов для обработки (*.*)</param>
        /// <param name="pathOut">Папка для проверенных файлов (v)</param>
        /// <param name="pub">Строка пути к каталогу c OPENKEY (\Pub)</param>
        /// <param name="move">Удалять исходные файлы в случае успеха</param>
        /// <returns>0 или код последней ошибки</returns>
        public static int BulkVerify(string pathIn, string mask, string pathOut, string pub, bool move)
        {
            Wbotho.SignInit(pub, pub);
            Wbotho.SignLogIn(pub);

            Directory.CreateDirectory(pathOut);

            int ret = 0;
            foreach (string file in Directory.GetFiles(pathIn, mask))
            {
                string fileOut = Path.Combine(pathOut, Path.GetFileName(file));
                File.Copy(file, fileOut, true);
                int r = Posh.Verify(fileOut); //TODO Wbotho.CheckFileSignEx()
                if (r == 0 && move && File.Exists(fileOut))
                {
                    File.Delete(file);
                }
                else
                {
                    File.Delete(fileOut);
                    ret = r;
                }
            }

            Wbotho.SignLogOut();
            Wbotho.SignDone();
            return ret;
        }

        /// <summary>
        /// Пакетное удаление подписей из файлов
        /// </summary>
        /// <param name="pathIn">Папка с исходными файлами</param>
        /// <param name="mask">Маска файлов для обработки (*.*)</param>
        /// <param name="pathOut">Папка для очищенных файлов (u)</param>
        /// <param name="move">Удалять исходные файлы в случае успеха</param>
        /// <returns>0 или код последней ошибки</returns>
        public static int BulkUnsign(string pathIn, string mask, string pathOut, bool move)
        {
            Directory.CreateDirectory(pathOut);

            int ret = 0;
            foreach (string file in Directory.GetFiles(pathIn, mask))
            {
                string fileOut = Path.Combine(pathOut, Path.GetFileName(file));
                File.Copy(file, fileOut, true);
                int r = Wbotho.DelSign(fileOut, -1);
                if (r == 0 && move && File.Exists(fileOut))
                {
                    File.Delete(file);
                }
                else
                {
                    File.Delete(fileOut);
                    ret = r;
                }
            }
            return ret;
        }
        #endregion Bulk
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
}

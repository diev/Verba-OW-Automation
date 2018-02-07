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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SlotsTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public UsrKeysInfo[] Slots;
    }

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
    #endregion Structures

    public static class Wbotho
    {
        #region Key
        /// <summary>
        /// Загрузить ключи в драйвер ASYNCR
        /// </summary>
        /// <param name="keyDev">Строка с именем ключевого устройства</param>
        /// <param name="keyId">Идентификатор ключа или пустая строка ("")</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI InitKey (char* key_dev, char* key_ID);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "InitKey", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort InitKey(string keyDev, string keyId);

        /// <summary>
        /// Выгрузить ключи из драйвера ASYNCR
        /// </summary>
        /// <param name="keyId">Идентификатор ключа</param>
        /// <param name="flag">Признак возможности выгрузки ключа из "слота" 0: FALSE-выгрузка запрещена, TRUE-разрешена</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI ResetKeyEx (char* key_ID, int flag);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "ResetKeyEx", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort ResetKeyEx(string keyId, bool flag);

        /// <summary>
        /// Получить список ключей, загруженных в драйвер ASYNCR
        /// </summary>
        /// <param name="keysList">Массив структур с информацией о прогруженных ключах</param>
        /// <param name="count">Количество загруженных ключевых слотов</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI GetDrvInfo(USR_KEYS_INFO* keys_info, P32bit nKeySlots);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "GetDrvInfo", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort GetDrvInfo(ref SlotsTable table, out uint count);
        #endregion Key

        #region Crypt
        /// <summary>
        /// Инициализация функций шифрования
        /// </summary>
        /// <param name="sec">Указатель на строку полного пути к секретным ключам</param>
        /// <param name="pub">Указатель на строку полного пути к открытым ключам NULL, если ключи симметричные</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI CryptoInit (char* path, char* base_path);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "CryptoInit", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CryptoInit(string sec, string pub);

        /// <summary>
        /// Завершение функций шифрования
        /// </summary>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI CryptoDone(void);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "CryptoDone", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CryptoDone();

        /// <summary>
        /// Зашифровывание файла
        /// </summary>
        /// <param name="fileIn">Исходный открытый файл</param>
        /// <param name="fileOut">Зашифрованный файл</param>
        /// <param name="id">Свой идентификатор (XXXX)</param>
        /// <param name="toList">Массив криптографических номеров получателей</param>
        /// <param name="ser">Номер подсети (серии) SSSSSS, куда направляется файл</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI EnCryptFile (char* file_in, char* file_out, T16bit node_From, P16bit node_To, char* ser);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "EnCryptFile", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort EnCryptFile(string fileIn, string fileOut, ushort id, ushort[] toList,
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 7)] string ser);

        /// <summary>
        /// Расшифровать файл
        /// </summary>
        /// <param name="fileIn">Исходный зашифрованный файл</param>
        /// <param name="fileOut">Расшифрованный файл</param>
        /// <param name="id">Номер получателя</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI DeCryptFile (char* file_in, char* file_out, T16bit abonent);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "DeCryptFile", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort DeCryptFile(string fileIn, string fileOut, ushort id);

        /// <summary>
        /// Получить список получателей зашифрованного файла
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
        #endregion Crypt

        #region Sign
        /// <summary>
        /// Инициализация функций подписи
        /// </summary>
        /// <param name="sec">Путь к файлу с секретным ключом подписи</param>
        /// <param name="pub">Путь к базе открытых ключей подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignInit (char* pathToSecret, char* pathToBase);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignInit", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignInit(string sec, string pub);

        /// <summary>
        /// Завершение работы с библиотеками подписи
        /// </summary>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignDone (void);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignDone", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignDone();

        /// <summary>
        /// Прочитать ключ подписи абонента в память
        /// </summary>
        /// <param name="sec">Путь к файлу с секретным ключом подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignLogIn (char* path);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignLogIn", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignLogIn(string sec);

        /// <summary>
        /// Удалить ключ подписи из памяти
        /// </summary>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI SignLogOut (void);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "SignLogOut", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort SignLogOut();

        /// <summary>
        /// Подпись файла
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
        /// Удаление подписей с конца файла
        /// </summary>
        /// <param name="file">Полное имя файла</param>
        /// <param name="count">Количество удаляемых подписей, (-1) - удалить все подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI DelSign (char* file_name, T8bit count);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "DelSign", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort DelSign(string file, sbyte count);

        /// <summary>
        /// Проверка подписей под файлом
        /// </summary>
        /// <param name="file">Полное имя файла</param>
        /// <param name="count">Число обнаруженных подписей</param>
        /// <param name="list">Массив результатов проверки каждой подписи</param>
        /// <returns>0 или код ошибки</returns>
        /// <remarks>extern T16bit WINAPI check_file_sign (char* file_name, P8bit count, Check_Status_Ptr* stat_array);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "check_file_sign", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern ushort CheckFileSign(string file, out byte count, out CheckStatus[] list);
        // FreeMemory(list) finally!
        #endregion Sign

        /// <summary>
        /// Освободить память, распределенную библиотекой
        /// </summary>
        /// <param name="lpMemory">Указатель, полученный в функциях библиотеки</param>
        /// <remarks>extern void WINAPI FreeMemory (void* lpMemory);</remarks>
        [DllImport("wbotho.dll", EntryPoint = "FreeMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void FreeMemory(IntPtr lpMemory);

        /// <summary>
        /// Освободить память, распределенную библиотекой
        /// </summary>
        /// <param name="list">Указатель, полученный в GetCryptKeysF()</param>
        [DllImport("wbotho.dll", EntryPoint = "FreeMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void FreeMemory(ushort[] list);

        /// <summary>
        /// Освободить память, распределенную библиотекой
        /// </summary>
        /// <param name="list">Указатель, полученный в CheckFileSign()</param>
        [DllImport("wbotho.dll", EntryPoint = "FreeMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern void FreeMemory(CheckStatus[] list);
    }

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
            ushort ret = Wbotho.CryptoInit(sec, pub);
            if (ret > 0) {
                Console.WriteLine("CryptoInit error : {0}", ret);
                return ret;
            }
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
            ret = Wbotho.EnCryptFile(fileIn, fileOut, ushort.Parse(id), nodes, ser);
            if (ret > 0)
            {
                Console.WriteLine("EnCryptFile error : {0}", ret);
                return ret;
            }
            Wbotho.CryptoDone();
            Console.WriteLine("File {0} encrypted to {1}", fileIn, fileOut);
            return 0;
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
            ushort ret = Wbotho.CryptoInit(sec, pub);
            if (ret > 0)
            {
                Console.WriteLine("CryptoInit error : {0}", ret);
                return ret;
            }
            ret = Wbotho.DeCryptFile(fileIn, fileOut, ushort.Parse(id));
            if (ret > 0)
            {
                Console.WriteLine("DeCryptFile error : {0}", ret);
                return ret;
            }
            Wbotho.CryptoDone();
            Console.WriteLine("File {0} decrypted to {1}", fileIn, fileOut);
            return 0;
        }

        /// <summary>
        /// Получить список получателей зашифрованного файла
        /// </summary>
        /// <param name="file">Зашифрованный файл</param>
        /// <remarks>wftesto.exe g file.cry</remarks>
        public static int GetAbonents(string file)
        {
            ushort ret = Wbotho.CryptoInit("", "");
            if (ret > 0)
            {
                Console.WriteLine("CryptoInit error : {0}", ret);
                return ret;
            }
            ushort count = 0;
            ushort[] toList;
            ret = Wbotho.GetCryptKeysF(file, out count, out toList, "");
            if (ret > 0)
            {
                Console.WriteLine("GetCryptKeysF error : {0}", ret);
                return ret;
            }
            Wbotho.CryptoDone();
            Console.WriteLine("File {0} is encrypted for these abonents :", file); //новое: исправлено слово
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine("ID{0} - {1}", i + 1, toList[i]);
            }
            Wbotho.FreeMemory(toList);
            return 0;
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
            ushort ret = Wbotho.SignInit(sec, "");
            if (ret > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            ret = Wbotho.SignFile(fileIn, fileOut, id);
            if (ret > 0)
            {
                Console.WriteLine("SignFile error : {0}", ret);
                return ret;
            }
            Wbotho.SignDone();
            Console.WriteLine("File {0} signed to {1}", fileIn, fileOut);
            return 0;
        }

        /// <summary>
        /// Проверить все подписи в конце файла (новое: они реально проверяются)
        /// </summary>
        /// <param name="file">Файл с подписями</param>
        /// <param name="pub">Путь к открытым ключам</param>
        /// <remarks>wftesto.exe v file.txt c:\pub</remarks>
        public static int Verify(string file, string pub)
        {
            ushort ret = Wbotho.SignInit("", pub);
            if (ret > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            byte count;
            CheckStatus[] signList;
            ret = Wbotho.CheckFileSign(file, out count, out signList);
            if (ret > 0)
            {
                Console.WriteLine("check_file_sign error : {0}", ret);
                return ret;
            }
            Wbotho.SignDone();
            ret = 0;
            for (int i = 0; i < count; i++)
            {
                Console.Write("{0} - ", signList[i].Alias);
                switch (signList[i].Status)
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
            Wbotho.FreeMemory(signList);
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
            ushort ret = Wbotho.SignInit("", "");
            if (ret > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            ret = Wbotho.DelSign(file, -1);
            if (ret > 0)
            {
                Console.WriteLine("DelSign error : {0}", ret);
                return ret;
            }
            Wbotho.SignDone();
            Console.WriteLine("Sign deleted in file {0}", file);
            return 0;
        }

        /// <summary>
        /// Загрузить в память драйвера ключ с внешнего ключевого устройства
        /// </summary>
        /// <param name="id">Номер ключа шифрования или КА</param>
        /// <param name="dev">Ключевое устройство (A: или другое)</param>
        /// <remarks>wftesto.exe i [000566666601] [ruToken]</remarks>
        public static int InitKey(string id, string dev)
        {
            ushort ret = Wbotho.InitKey(dev, id);
            if (ret > 0)
            {
                Console.WriteLine("InitKey error : {0}", ret);
                return ret;
            }
            Console.WriteLine("InitKey done : {0}", ret);
            return 0;
        }

        /// <summary>
        /// Выгрузить ключ из памяти драйвера (новое: слот 0 разрешен также)
        /// </summary>
        /// <param name="id">Номер ключа</param>
        /// <remarks>wftesto.exe r 000566666601</remarks>
        public static int ResetKey(string id)
        {
            ushort ret = Wbotho.ResetKeyEx(id, true);
            if (ret > 0)
            {
                Console.WriteLine("ResetKey error : {0}", ret);
                return ret;
            }
            Console.WriteLine("ResetKey done : {0}", ret);
            return 0;
        }

        /// <summary>
        /// Выгрузить все ключи из памяти драйвера (новое)
        /// </summary>
        /// <remarks>wftesto.exe r</remarks>
        public static int ResetKeys()
        {
            SlotsTable table = new SlotsTable();
            table.Slots = new UsrKeysInfo[16];
            uint count;
            ushort ret = Wbotho.GetDrvInfo(ref table, out count);
            if (ret > 0)
            {
                Console.WriteLine("GetDrvInfo error : {0}", ret);
                return ret;
            }
            int errs = 0;
            for (int i = 0; i < count; i++)
            {
                if (table.Slots[i].Num[0] != '\0') //crypto
                {
                    if (0 != ResetKey(table.Slots[i].Num)) errs++;
                }
                else if (table.Slots[i].Nump[0] != '\0') //sign
                {
                    if (0 != ResetKey(table.Slots[i].Nump)) errs++;
                }
            }
            return errs == 0 ? 0 : 1;
        }

        /// <summary>
        /// Получить перечень загруженных в память драйвера ключей
        /// </summary>
        /// <remarks>wftesto.exe l</remarks>
        public static int ListKeys()
        {
            SlotsTable table = new SlotsTable();
            table.Slots = new UsrKeysInfo[16];
            uint count;
            ushort ret = Wbotho.GetDrvInfo(ref table, out count);
            if (ret > 0)
            {
                Console.WriteLine("GetDrvInfo error : {0}", ret);
                return ret;
            }
            Console.Write("\nDrvInfo:");
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine("\nSlot : {0}", table.Slots[i].KeySlotNumber);
                Console.WriteLine("NUM  : {0}", table.Slots[i].Num);
                Console.WriteLine("NUMP : {0}", table.Slots[i].Nump);
            }
            return 0;
        }
    }

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

    class CheckIntegrity
    {
        static int Main(string[] args)
        {
            if (args.Length > 0)
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
                    default:
                        //Console.WriteLine(Result.Text(0));
                        break;
                }
            }
            return 1;
        }
    }
}

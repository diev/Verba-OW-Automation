// Copyright (c) 2018-2019 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/Verba-OW-Automation

// Используйте %WINDIR%\Microsoft.NET\Framework\v3.5\csc /?
// для Microsoft(R) .NET Framework версии 3.5
// или %WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc для версии .NET 4+

using System;

namespace Verba
{
    /// <summary>
    /// Тестовая консольная утилита проверки функционирования библиотеки
    /// </summary>
    class VerbaTest
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
                        case "s2": // file.txt file.sig a: 000122222201
                            return Wftesto.SignSeparate(args[1], args[3], args[4], args[2]);
                        case "v": // file.txt c:\pub
                            return Wftesto.Verify(args[1], args[2]);
                        case "v2": // file.txt file.sig c:\pub
                            return Wftesto.VerifySeparate(args[1], args[2], args[3]);
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
        /// Подписать файл с подписью в отдельном файле (новое)
        /// </summary>
        /// <param name="file">Исходный файл</param>
        /// <param name="fileSig">Файл с подписями</param>
        /// <param name="sec">Путь к секретному ключу</param>
        /// <param name="id">Код аутентификации (КА)</param>
        /// <remarks>wftesto.exe s2 file.txt file.sig a: 000122222201</remarks>
        public static int SignSeparate(string file, string fileSig, string sec, string id)
        {
            int ret;
            if ((ret = Wbotho.SignInit(sec, "")) > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            if ((ret = Posh.SignSeparate(file, id, fileSig)) > 0)
            {
                Console.WriteLine("SignFileSeparate error : {0}", ret);
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
        /// Проверить все подписи в конце файла (новое: они реально проверяются)
        /// </summary>
        /// <param name="file">Подписанный файл</param>
        /// <param name="fileSig">Файл с подписями</param>
        /// <param name="pub">Путь к открытым ключам</param>
        /// <remarks>wftesto.exe v2 file.txt file.sig c:\pub</remarks>
        public static int VerifySeparate(string file, string fileSig, string pub)
        {
            int ret;
            if ((ret = Wbotho.SignInit("", pub)) > 0)
            {
                Console.WriteLine("SignInit error : {0}", ret);
                return ret;
            }
            byte count;
            CheckList list;
            if ((ret = Wbotho.CheckFileSeparate(file, out count, out list, fileSig)) > 0)
            {
                Console.WriteLine("CheckFileSeparate error : {0}", ret);
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
}

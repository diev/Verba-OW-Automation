# Copyright (c) 2018-2019 Dmitrii Evdokimov. All rights reserved.
# Licensed under the Apache License, Version 2.0.
# Source https://github.com/diev/Verba-OW-Automation

# Пример вызова функций из любого скрипта PS:

Set-StrictMode -Version Latest

# Серия и номера ключей
$Sr = "941009"
$D3 = "3939"
$FS = "2010"

# Подключаем скрипты Вербы
. .\Verba.ps1

# Тестовые данные
$folder = ".\test"
$files = Get-ChildItem -Path $folder -Filter *.txt -File

# Перед первым запуском необходимо загрузить все необходимые
# ключевые носители штатной утилитой asrkeyw.exe

# Все открытые ключи должны быть в одном справочнике (в C:\Pub)

# Подписать (номер КА прописан в JSON)
$files | Sign-File
# Зашифровать от нас на получателя (и нас)
$files | Encrypt-File $D3$Sr $FS$Sr
# Расшифровать от отправителя (от нас) на нас
$files | Decrypt-File $FS$Sr $D3$Sr
# Проверить подписи (все известные) и отсутствие повреждений
$files | Verify-File
# Удалить подписи (важно для XML, например)
$files | Unsign-File

# Тестовые данные вернулись в исходное состояние

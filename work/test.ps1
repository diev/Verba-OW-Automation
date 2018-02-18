# Пример вызова функции из любого скрипта PS:
# . $localDir"verba.ps1" -logfile $log	# Подключаем модуль Верба
# $files = Get-ChildItem -Recurse $folder | Where {!$_.PSisContainer}

$folder = "c:\work\test"
$log = "c:\work\test.log"

. c:\work\verba.ps1 -logfile $log	# Подключаем модуль Верба
$files = Get-ChildItem $folder | Where {!$_.PSisContainer}

$files | Sign -KeyNumber "206594104001"
#$files | Encrypt -KeyFrom "1594942009" -KeyTo "0001"
$files | Encrypt -KeyFrom "1594942009" -KeyTo "7020"
$files | Decrypt -KeyTo "1594"
#$files | Decrypt -KeyTo "7007"
$files | Verify
$files | Unsign

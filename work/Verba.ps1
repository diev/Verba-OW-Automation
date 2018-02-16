# TODO

# Пример вызова функции из любого скрипта PS:
# . $localDir"verba.ps1" -logfile $log	# Подключаем модуль Верба
# $files = Get-ChildItem -Recurse $folder | Where {!$_.PSisContainer}
# $files | Sign    -KeyPath $keyCB.path  -KeyNumber "$($keyCB.me)$($keyCB.seria)01"
# $files | Encrypt -KeyPath $keyFNS.path -KeyFrom   $keyFNS.me -KeyTo    $KeyFNS.to    -KeySeria $KeyFNS.seria
# $files | Decrypt -KeyPath $keyFNS.path -KeyTo     $KeyFNS.me -KeySeria $KeyFNS.seria
# $files | Verify  -KeyPath $keyCB.path  -KeyNumber "$($keyCB.me)$($keyCB.seria)01"
# $files | Unsign

Param (
	$logFile
)

# Определяем рабочий каталог
$dir = (Split-Path -Parent $MyInvocation.MyCommand.Definition) + "\"
Add-Type -Path $dir"Verba.cs"	# Подключаем модуль библиотек

Function Log {
	Param (
		$event,
		$result
	)

	$error = [Verba.Result]::Text($result)
	"$event : $error" | Out-File $logFile -Append

	if ($result) {
		Write-Host -ForegroundColor Red "ОШИБКА. РАБОТА СКРИПТА ОСТАНОВЛЕНА. СООБЩИТЕ АДМИНИСТРАТОРУ"
		Write-Host -ForegroundColor Red "$event : $error"
		Exit
	}
}

Function Encrypt {
	Param (
		$KeyPath,	# Путь к каталогу с ключами
		$KeyFrom,	# Номер ключа отправителя
		$KeyTo,		# Номер ключа получателя
		$KeySeria	# Серия ключа
	)

	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::InitKey("A:", "")
		log -Event "InitKey" -Result $command

		$command = [Verba.Wbotho]::CryptoInit($KeyPath, $KeyPath)
		log -Event "CryptoInit" -Result $command
	}

	Process {
		$i++
		$command = [Verba.Posh]::Encrypt($_.FullName, $_.FullName, $KeyFrom, $KeyTo)
		log -Event "Encrypt $_" -Result $command

		Write-Progress -Activity "Шифруем" -Status "Зашифровано $i" 
	}

	End {
		$command = [Verba.Wbotho]::CryptoDone()
		log -Event "CryptoDone" -Result $command

		$command = [Verba.Wbotho]::ResetKeyEx("$($KeyFrom)$($KeySeria)", $true)
		log -Event "ResetKey" -Result $command

		Return $i
	}
}

Function Decrypt {
	Param (
		$KeyPath,	# Путь к каталогу с ключами
		$KeyTo,		# Номер ключа получателя
		$KeySeria	# Серия ключа
	)

	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::InitKey("A:", "")
		log -Event "InitKey" -Result $command

		$command = [Verba.Wbotho]::CryptoInit($KeyPath, $KeyPath)
		log -Event "CryptoInit" -Result $command
	}

	Process {
		$i++
		$command = [Verba.Posh]::Decrypt($_.FullName, $_.FullName, $KeyTo)
		log -Event "Decrypt $_" -Result $command

		Write-Progress -Activity "Расшифровываем" -Status "Расшифровано $i"
	}

	End {
		$command = [Verba.Wbotho]::CryptoDone()
		log -Event "CryptoDone" -Result $command

		$command = [Verba.Wbotho]::ResetKeyEx("$($KeyTo)$($KeySeria)", $true)
		log -Event "ResetKey" -Result $command

		Return $i
	}
}

Function Sign {
	Param (
		$KeyPath,	# Путь к каталогу с ключами
		$KeyNumber	# Номер ключа
	)

	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::InitKey("A:", "")
		log -Event "InitKey" -Result $command

		$command = [Verba.Wbotho]::SignInit("", $KeyPath)
		log -Event "SignInit" -Result $command

		$command = [Verba.Wbotho]::SignLogIn("A:")
		log -Event "SignLogIn" -Result $command
	}

	Process {
		$i++
		$command = [Verba.Posh]::Sign($_.FullName, $_.FullName, $KeyNumber)
		log -Event "Sign $_" -Result $command

		Write-Progress -Activity "Подписываем" -Status "Подписано $i" 
	}

	End {
		$command = [Verba.Wbotho]::SignLogOut()
		log -Event "SignLogOut" -Result $command

		$command = [Verba.Wbotho]::SignDone()
		log -Event "SignDone" -Result $command

		$command = [Verba.Wbotho]::ResetKeyEx($KeyNumber, $true)
		log -Event "ResetKey" -Result $command	

		Return $i
	}
}

Function Unsign {
	Process {
		$i++
		$command = [Verba.Posh]::Unsign($_.FullName)
		log -Event "Unsign $_" -Result $command

		Write-Progress -Activity "Отрезаем" -Status "Отрезано $i" 
	}

	End {
		Return $i
	}
}

Function Verify {
	Param (
		$KeyPath,	# Путь к каталогу с ключами
		$KeyNumber	# Номер ключа
	)

	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::InitKey("A:", "")
		log -Event "InitKey" -Result $command

		$command = [Verba.Wbotho]::SignInit("", $KeyPath)
		log -Event "SignInit" -Result $command

		$command = [Verba.Wbotho]::SignLogIn("A:")
		log -Event "SignLogIn" -Result $command
	}

	Process {
		$i++
		$command = [Verba.Posh]::Verify($_.FullName)
		log -Event "Verify $_" -Result $command

		Write-Progress -Activity "Проверяем" -Status "Проверено $i"
	}

	End {
		$command = [Verba.Wbotho]::SignLogOut()
		log -Event "SignLogOut" -Result $command

 		$command = [Verba.Wbotho]::SignDone()
		log -Event "SignDone" -Result $command

		$command = [Verba.Wbotho]::ResetKeyEx($KeyNumber, $true)
		log -Event "ResetKey" -Result $command

		Return $i
	}
}

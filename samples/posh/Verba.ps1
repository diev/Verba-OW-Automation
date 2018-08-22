# Copyright (c) 2018 Dmitrii Evdokimov. All rights reserved.
# Licensed under the Apache License, Version 2.0.
# Source https://github.com/diev/Verba-OW-Automation

Set-StrictMode -Version Latest

# Определяем рабочий каталог
$dir = (Split-Path -Parent $MyInvocation.MyCommand.Definition) + "\"
# Подключаем модуль библиотек
#Add-Type -Path C:\Verba\Verba.cs
#Add-Type -Path $dir"Verba.cs"
Add-Type -Path ..\..\Verba\Verba.cs

# Читаем конфиг
$config = Get-Content .\Verba.json | ConvertFrom-Json
$log = Get-Date -UFormat $config.path.log
if (!(Test-Path $log)) { New-Item $log -Force | Out-Null }
#$pub = "c:\pub"
$pub = $config.path.pub

Function Log {
	Param (
		$event,
		$result
	)

	$error = [Verba.Result]::Text($result)
	"$event : $error" | Out-File $log -Append

	if ($result) {
		Write-Host -ForegroundColor Red "ОШИБКА. РАБОТА СКРИПТА ОСТАНОВЛЕНА. СООБЩИТЕ АДМИНИСТРАТОРУ"
		Write-Host -ForegroundColor Red "$event : $error"
		Exit
	}
}

Function Encrypt-File {
	Param (
		[string]$KeyFrom,	# Номер ключа отправителя
		[string]$KeyTo		# Номер ключа получателя
	)

	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::CryptoInit($pub, $pub)
		log -Event "CryptoInit" -Result $command
	}

	Process {
		$i++
		$command = [Verba.PoshEx]::EncryptEx($_.FullName, $_.FullName, $KeyFrom, $KeyTo)
		log -Event "Encrypt $_" -Result $command
		Write-Progress -Activity "Шифруем" -Status "Зашифровано $i" 
	}

	End {
		$command = [Verba.Wbotho]::CryptoDone()
		log -Event "CryptoDone" -Result $command
		Return $i
	}
}

Function Decrypt-File {
	Param (
		[string]$KeyFrom,	# Номер ключа отправителя
		[string]$KeyTo		# Номер ключа получателя
	)

	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::CryptoInit($pub, $pub)
		log -Event "CryptoInit" -Result $command
	}

	Process {
		$i++
		$command = [Verba.PoshEx]::DecryptEx($_.FullName, $_.FullName, $KeyFrom, $KeyTo)
		log -Event "Decrypt $_" -Result $command
		Write-Progress -Activity "Расшифровываем" -Status "Расшифровано $i"
	}

	End {
		$command = [Verba.Wbotho]::CryptoDone()
		log -Event "CryptoDone" -Result $command
		Return $i
	}
}

Function Sign-File {
	Param (
#		[string]$KA	# Номер ключа подписи
	)

	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::SignInit($pub, $pub)
		log -Event "SignInit" -Result $command
		$command = [Verba.Wbotho]::SignLogIn($pub)
		log -Event "SignLogIn" -Result $command
	}

	Process {
		$i++
		$command = [Verba.Posh]::Sign($_.FullName, $_.FullName, $config.key.KA)
		log -Event "Sign $_" -Result $command
		Write-Progress -Activity "Подписываем" -Status "Подписано $i" 
	}

	End {
		$command = [Verba.Wbotho]::SignLogOut()
		log -Event "SignLogOut" -Result $command
		$command = [Verba.Wbotho]::SignDone()
		log -Event "SignDone" -Result $command
		Return $i
	}
}

Function Unsign-File {
	Begin {
		$i = 0;
	}

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

Function Verify-File {
	Begin {
		$i = 0;
		$command = [Verba.Wbotho]::SignInit($pub, $pub)
		log -Event "SignInit" -Result $command
		$command = [Verba.Wbotho]::SignLogIn($pub)
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
		Return $i
	}
}

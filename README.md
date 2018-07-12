# [Verba-OW-Automation](http://diev.github.io/Verba-OW-Automation)

[![Build status](https://ci.appveyor.com/api/projects/status/rhukdxrkjsw5dud7?svg=true)](https://ci.appveyor.com/project/diev/verba-ow-automation)
[![GitHub Release](https://img.shields.io/github/release/diev/Verba-OW-Automation.svg)](https://github.com/diev/Verba-OW-Automation/releases/latest)

Скрипты автоматизации ПО *Верба-OW*.  
Не встраивание, а именно скрипты - с открытым кодом, используя только 
штатные средства системы. Без какого-либо стороннего ПО. Это важно.

## Что требуется

* Установленное ПО **Верба-OW**. Банкам предоставляется Банком России. 
Можно купить отдельно, как и штатный модуль командной строки, 
у производителя (но не банкам). 
Бывает в том или ином виде в составе разных Интернет-Банков и клиентов 
бирж, где его можно скачать. Свободно не распространяется, так как для 
установки требует именную лицензию и учет распространения, а также ключи.
* Из этого ПО для взаимодействия требуется только библиотека **wbotho.dll**.
* Установленный **PowerShell** минимальной версии **2.0**, поскольку он 
для **Windows XP** годится и является штатным компонентом для систем выше.
* Установленный **.NET** минимальной версии **3.5**, поскольку он является 
штатным компонентом всех систем Windows и потому все еще поддерживается 
производителем. Именно на языке *C#* версии 3.5 написан код *Verba.cs*. 
Но можно использовать и дополнительно устанавливаемый .NET версии 4.0 
(последний для Windows XP) или любой другой выше.

## Что не требуется

* Никакое стороннее ПО, недоверенные бинарные модули, лицензии на разработку 
и встраивание.
* SDK и документы с грифом ДСП.
* **Wftesto** - скомпилированный EXE из состава SDK Верба-OW, приведенный 
там лишь в качестве примера вызова функций из библиотеки wbotho.dll, 
содержащий неточности и не предназначенный для промышленного применения.
* **Vb** (VbO) - исполняемый модуль командной строки от создателей Верба-W/OW, 
но против покупки и использования которого Банк России.
* **Visual Studio** - прилагаемый здесь в папке [vs](vs) проект служит лишь 
для удобства написания программных текстов в нем и проверки их запускаемости. 
Эти тексты можно написать в любом подручном редакторе и проверить с помощью 
системных компиляторов ```csc``` из состава установленных библиотек .NET.
Примеры их использования есть в папке [test](test):
  * ```csc.cmd``` - для .NET 3.5
  * ```csc4.cmd``` - для .NET 4+
* Знания C# свежее 2008 года - скрипт на PowerShell будет использовать 
тот .NET, который установлен в Вашей системе. Если он там версии 3.5, 
например, то и набранный в редакторе текст не должен содержать те 
нововведения языка C#, которые умеет применять Visual Studio наших дней.
* Знания, как запускать PowerShell - используйте ```run.cmd``` в 
папке [work](work).

## Как использовать

Написать какой-либо свой обработчик массы файлов, требующих обработки 
с помощью ПО Верба-OW, на любом скриптовом языке типа PowerShell, CMD, 
JavaScript или HTA и запускать функции из файла ```Verba.ps1``` (все 
требуемое для работы - собрано в папке [verba](verba)) или так, как 
показано в качестве примера в [work](work). 
Никакого постороннего ПО или недоверенных бинарных модулей не требуется.

В отличие от штатного АРМ РМП можно использовать несколько загруженных 
в память драйвера ключей одновременно, убрав при этом ключевые носители 
после загрузки с них. Для этого надо лишь сформировать штатными средствами 
общий справочник, как показано в папке [pub](pub).

Для проверки функционирования скриптов можно использовать как запускаемый 
PowerShell скриптовый код на языке C#, так и скомпилированный под Ваш 
.NET ```VerbaTest.exe``` ([пример](samples/VerbaTest)). Синтаксис команд у 
него почти такой же, как у общеизвестной программы из SDK ```Wftesto.exe```. 
Сделаны лишь несколько улучшений:

* возврат кода 0 (успешно) или 1+ (были ошибки);
* очистка всех слотов в драйвере разом (команда ```r``` без параметров);

Это готовый wrapper для запуска функций DLL из любого языка .NET.

## Благодарности

Спасибо всем, чьи идеи мне удалось встретить на форуме 
[BANKIR.RU](http://bankir.ru/dom/forum/) на стыке Автоматизации и ИБ.

## Лицензионное соглашение

Licensed under the [Apache License, 
Version 2.0](http://www.apache.org/licenses/LICENSE-2.0 "LICENSE").  
Вы можете использовать этот код совершенно свободно без всяких ограничений 
с моей стороны.

Если Вы захотите встроить что-то в свои приложения, то Вам необходимо 
будет проверить необходимость применения правил лицензирования по работе 
с таким ПО, принятых в Вашем государстве.

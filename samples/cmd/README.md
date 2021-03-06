# Пример генерации утилит с помощью одного только CMD

Этот скрипт генерирует набор исполняемых утилит в создаваемой
папке *bin* с помощью системного компилятора *csc* из состава
установленных библиотек .NET.
Примеры этого есть в папке [test]:

* ```csc.cmd``` - для .NET 3.5
* ```csc4.cmd``` - для .NET 4+

Для сборки не требуется ничего больше - даже не нужно полного
исходного кода [Verba.cs] - весь необходимый для генерации утилит
минимум кода содержится в тексте самого *make.cmd* как скрипта -
никаких подозрительных побочных файлов.

## Общий справочник

Для выполнения данных утилит нужен общий справочник, для которого
не требуется переставлять ключи для каждой операции.
Подробнее о таком справочнике в [pub].

## Настройки

Постоянные настройки удобно зашить в исходный текст, чтобы не передавать 
массу параметров в командной строке или в конфигах.
Например, путь к общему справочнику (*C:\Pub*).
Идентификатор своей подписи (ключ КА), но тогда при его смене надо не забыть
перегенерировать утилиту *sign*.
Можно указать до трех (это число можно расширить) файлов для записи логов,
указав формат даты-времени при подстановке как в именах файлов, так в их
строках.

Эти настройки можно указать в начале файла *make.cmd* в переменных
*set*, а затем использовать через подстановки в *%%* далее по тексту.

    set out=bin
    set pub=c:\pub
    set ka=206194104001
    set sig=sig

    set log1=logs\{0:yyyyMMdd}.log
    set log2=P:\PTK PSD\LOG\test_{0:yyyyMMdd}.log
    set log3=

    set msg=\r\n{0:HH:mm:ss} {1}
    set err=***FAILED!


## Использование

    encrypt in\[*] out\ XXXXSSSSSS XXXX[SSSSSS] [ext]
    decrypt in\[*] out\ XXXX[SSSSSS] XXXXSSSSSS [ext]

    sign    in\[*] out\ [XXXXSSSSSSYY]
    verify  in\[*] out\
    unsign  in\[*] out\

    sign2   in\[*] out\ [XXXXSSSSSSYY]
    verify2 in\[*] out\

Последняя пара функция для операций с подписями в отдельных файлах.

### Обязательные параметры:

* *in\\* - папка исходных файлов.
* *\** - фильтр исходных файлов (опционально).
* *out\\* - папка назначения для успешно обработанных файлов.
* *XXXSSSSSS* - полные идентификаторы ключей шифрования отправителя и
получателя (если серия совпадает, что практически всегда, то второй раз
ее можно не указывать). Если при шифровании указан полный идентификатор
КА, то будет использовано 10 символов ключа шифрования.

### Опциональные параметры:

* *XXXSSSSSSYY* - идентификатор ключа КА (по умолчанию - своей
подписи, указанной в *make.cmd* при генерации).
* *ext* - новое расширение обработанных файлов, если его надо
изменить - например, *VRB* - для зашифрованных файлов.

## Примечания

Для расшифровывания обратно своих отправленных файлов надо указать
первоначальный ключ получателя и свой ключ (в таком порядке).

Утилиты не возвращают никакого кода возврата во избежание необходимости
контроля встраивания. Весь результат - появление или отсутствие файла
в папке назначения, обработанного штатными функциями ядра.

[test]: ../../test
[pub]: ../../pub
[Verba.cs]: ../../verba/Verba.cs

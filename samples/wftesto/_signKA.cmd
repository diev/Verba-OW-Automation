@echo off

if "%1"=="" goto err
if not exist %1 goto err
if "%2"=="" goto err
if exist %2 goto err

set bin=%~dp0bin
set wftesto=%bin%\wftesto.exe
set asrkeyw=%bin%\asrkeyw.exe

%wftesto% l>nul
if errorlevel 1 %asrkeyw%

set cmd=%wftesto% s %1 %2 b:\ 209594104001

echo %cmd%
%cmd%

:err
echo ERROR!
goto :eof

@echo off

if "%1"=="" goto err
if not exist %1 goto err

set bin=%~dp0bin
set wftesto=%bin%\wftesto.exe
set asrkeyw=%bin%\asrkeyw.exe

%wftesto% l>nul
if errorlevel 1 %asrkeyw%

set cmd=%wftesto% u %1

echo %cmd%
%cmd%
goto :eof

:err
echo ERROR!
goto :eof

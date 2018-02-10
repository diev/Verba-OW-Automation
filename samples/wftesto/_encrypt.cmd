@echo off

if "%1"=="" goto err
if not exist %1 goto err
if "%2"=="" goto err
if exist %2 goto err
if "%3"=="" goto err

set bin=%~dp0bin
set wftesto=%bin%\wftesto.exe
set asrkeyw=%bin%\asrkeyw.exe

%wftesto% l>nul
if errorlevel 1 %asrkeyw%

rem KFM=2001
rem FNS=0020
rem FTS=0020

set cmd=%wftesto% e %1 %2 b:\ b:\Profile 3457 %3

echo %cmd%
%cmd%
goto :eof

:err
echo ERROR!
goto :eof

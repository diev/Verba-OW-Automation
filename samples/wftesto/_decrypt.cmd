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

for /f "tokens=3 skip=1" %%l in ('%wftesto% g %1') do set to=%%l

set to=000%to%
set to=%to:~-4%

set cmd=%wftesto% d %1 %2 b:\ b:\Profile %to%

echo %cmd%
%cmd%

:err
echo ERROR!
goto :eof

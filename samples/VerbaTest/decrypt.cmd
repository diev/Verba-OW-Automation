@echo off
rem decrypt in\* out\ XXXX [ext]

if not defined init call init

if "%4"=="" (
  for %%f in (%1) do call :decrypt_1 %%f %2%%~nxf %3
) else (
  for %%f in (%1) do call :decrypt_1 %%f %2%%~nf.%4 %3
)
goto :eof

:decrypt_1
set ok=%~dp1ok\%lymd%
set bad=%~dp1bad\%lymd%

set cmd=%VRB% d %1 %2 %Pub% %Pub% %3
echo %cmd%
%cmd%
if errorlevel 1 (
  call log D error %errorlevel% %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else if not exist %2 (
  call log D bad %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else (
  call log D %1
  if not exist %ok% md %ok%
  move %1 %ok%\
)
echo.
goto :eof

@echo off
rem encrypt in\* out\ XXXXSSSSSS XXXX [ext]

if not defined init call init

if "%5"=="" (
  for %%f in (%1) do call :encrypt_1 %%f %2%%~nxf   %3 %4
) else (
  for %%f in (%1) do call :encrypt_1 %%f %2%%~nf.%5 %3 %4
)
goto :eof

:encrypt_1
set ok=%~dp1ok\%lymd%
set bad=%~dp1bad\%lymd%

set cmd=%VRB% e %1 %2 %Pub% %Pub% %3 %4
echo %cmd%
%cmd%
if errorlevel 1 (
  call log E error %errorlevel% %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else if not exist %2 (
  call log E bad %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else (
  call log E %1
  if not exist %ok% md %ok%
  move %1 %ok%\
)
echo.
goto :eof

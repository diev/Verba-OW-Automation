@echo off
rem sign in\* out\ XXXXSSSSSSYY

if not defined init call init

for %%f in (%1) do call :sign_1 %%f %2%%~nxf %3
goto :eof

:sign_1
set ok=%~dp1ok\%lymd%
set bad=%~dp1bad\%lymd%

set cmd=%VRB% s %1 %2 %Pub% %3
echo %cmd%
%cmd%
if errorlevel 1 (
  call log S error %errorlevel% %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else if not exist %2 (
  call log S bad %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else (
  call log S %1
  if not exist %ok% md %ok%
  move %1 %ok%\
)
echo.
goto :eof

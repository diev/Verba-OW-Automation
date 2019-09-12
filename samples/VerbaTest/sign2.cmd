@echo off
rem sign2 in\* out\ XXXXSSSSSSYY sig

if not defined init call init

for %%f in (%1) do call :sign2_1 %%f %2%%~nf.%4 %3 %2
goto :eof

:sign2_1
set ok=%~dp1ok\%lymd%
set bad=%~dp1bad\%lymd%

set cmd=%VRB% s2 %1 %2 %Pub% %3
echo %cmd%
%cmd%
if errorlevel 1 (
  call log S2 error %errorlevel% %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else if not exist %2 (
  call log S2 bad %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else (
  call log S2 %1
  if not exist %ok% md %ok%
  copy %1 %4
  move %1 %ok%\
)
echo.
goto :eof

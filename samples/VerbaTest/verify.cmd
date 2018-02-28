@echo off
rem verify in\* out\

if not defined init call init

for %%f in (%1) do call :verify_1 %%f  %2%%~nxf
goto :eof

:verify_1
set ok=%~dp1ok\%lymd%
set bad=%~dp1bad\%lymd%

set cmd=%VRB% v %1 %Pub%
echo %cmd%
%cmd%
if errorlevel 1 (
  call log V error %errorlevel% %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else (
  copy %1 %2>nul
  call log V %1
  if not exist %ok% md %ok%
  move %1 %ok%\
)
echo.
goto :eof

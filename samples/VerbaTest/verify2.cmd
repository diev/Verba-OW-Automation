@echo off
rem verify2 in\* out\ sig

if not defined init call init

for %%f in (%1) do call :verify2_1 %%f %%f.%3 %2
goto :eof

:verify2_1
set ok=%~dp1ok\%lymd%
set bad=%~dp1bad\%lymd%

set cmd=%VRB% v2 %1 %2 %Pub%
echo %cmd%
%cmd%
if errorlevel 1 (
  call log V2 error %errorlevel% %1
  if not exist %bad% md %bad%
  move %1 %bad%\
) else (
  copy %1 %3>nul
  call log V2 %1
  if not exist %ok% md %ok%
  move %1 %ok%\
  copy %2 %ok%\
)
echo.
goto :eof

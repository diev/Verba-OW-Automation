@echo off
rem unsign in\* out\

if not defined init call init

for %%f in (%1) do call :unsign_1 %%f %2%%~nxf
goto :eof

:unsign_1
set ok=%~dp1ok\%lymd%
set bad=%~dp1bad\%lymd%

copy %1 %temp%\%~nx2>nul
set cmd=%VRB% u %temp%\%~nx2
echo %cmd%
%cmd%
if errorlevel 1 (
  call log U error %errorlevel% %1
  if not exist %bad% md %bad%
  move %1 %bad%\
  del %temp%\%~nx2
) else (
  call log U %1
  if not exist %ok% md %ok%
  move %1 %ok%\
  move %temp%\%~nx2 %2
)
echo.
goto :eof

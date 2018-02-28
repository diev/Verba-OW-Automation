@echo off
if not defined init call init

if "%2"=="alert" goto :loga %*
if "%2"=="error" goto :loge %*

:log
if defined fresh (
  set fresh=
  echo.>>%log%
  echo.>>%log2%
)
if "%1"=="" (
  echo %dt%
  echo %dt%>>%log%
  echo %dt%>>%log2%
) else (
  echo %dt% %*>>%log%
  echo %dt% %*>>%log2%
)
goto :eof

:loga
color 1f
call :log *** %*
goto :eof

:loge
color 4f
call :log ***** %*
goto :eof

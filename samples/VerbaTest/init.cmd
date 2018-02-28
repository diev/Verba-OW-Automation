@echo off
title %~n0 

set dd=%date:~-10,2%
set mm=%date:~-7,2%
set yy=%date:~-2%
set yyyy=%date:~-4%
set ymd=%yyyy%\%mm%\%dd%
set iymd=%yyyy%-%mm%-%dd%
set lymd=%yyyy%%mm%%dd%
set dmy=%dd%.%mm%.%yyyy%
set dt=%yyyy%-%mm%-%dd% %time:~0,8%

if not exist logs\nul md logs>nul
set log=logs\%lymd%.log
set log2=nul
set fresh=1

set COPYCMD=/Y
set arj32_sw=-y

set PATH=%~dp0bin;%PATH%
set CAB=cabarc
set ARJ=arj32
set VRB=VerbaTest

set Pub=c:\pub

rem -------------------------------------------------------------------------

set D1=1594942009
set D3=700294200901
set D5=206594104001
set D6=700794200901

rem if "%ymd%" geq "2017\08\11" set D5=206594104003

set TOKFM=0001
set TOFNS=7020
set TOFTS=7020
set TO550=7008
set TOVBK=7003

set init=1
echo Init done
goto :eof

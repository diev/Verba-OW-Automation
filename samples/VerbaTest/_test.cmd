@echo off
cls
if not defined init call init

if not exist test md test
echo TEST>test\test.txt
echo. >>test\test.txt

if not exist s md s
call sign test\* s\ %D5%

if not exist e md e
call encrypt s\* e\ %D1:~0,10% %TOFNS% vrb

if not exist d md d
call decrypt e\* d\ %D1:~0,4% txt

if not exist v md v
call verify d\* v\

if not exist u md u
call unsign v\* u\

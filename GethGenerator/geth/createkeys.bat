@echo off
for /F "tokens=*" %%A in (passwords.txt) do (
	mkdir %%A
   (echo %%A && echo %%A)|geth account new --keystore c:\test\%%A
echo %%A > c:\test\%%A\%%A.txt
 )
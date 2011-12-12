@echo off
echo Code signing certificate
echo ----------------------------
echo.
echo NOTES:
echo.
echo (1) Password not needed for the first step, but required in the wizard.
echo.
echo (2) Choose to export private key in the wizard, no strong protection, and save to the UI.Client\Certificates project folder. Double-click it and import, checking
echo "allow it to be exported"
echo.
echo (3) After that, export public key only using SHA1 option by going to MMC.exe Certificates snap-in, (Current User)\Personal\Certificates, right click export.
echo Save it as "Deployment\Certificates\Certificate-public.cer"
echo.
pause

set PATH="C:\Program Files\Microsoft SDKs\Windows\v7.0A\bin";%PATH%

rd temp /q /s
mkdir temp

rem // You can change the email or the expire date here
@echo on
makecert -r -n "CN=Edge.BI,E=support@edge.bi" -sv temp\EdgeBI.pvk -b 01/01/2010 -e 01/01/2020  temp\EdgeBI.cer

cert2spc temp\EdgeBI.cer temp\EdgeBI.spc

pvk2pfx /spc temp\EdgeBI.spc /pvk temp\EdgeBI.pvk

@echo off
rd temp /q /s

echo.
echo DONE!

pause
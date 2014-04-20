rmdir /S /Q bin
call release.bat
mkdir bin
cd bin
del *.pdb *.xml
cd ..
@IF %ERRORLEVEL% NEQ 0 PAUSE
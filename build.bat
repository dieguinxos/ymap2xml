@echo off
echo Building ymap2xml...
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained false -o dist
echo.
echo Done! ymap2xml.exe is in the dist\ folder.
pause

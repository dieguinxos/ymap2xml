Write-Host "Building ymap2xml..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) { Read-Host "Restore failed, press Enter"; exit 1 }
dotnet publish -c Release -r win-x64 --self-contained false -o dist
if ($LASTEXITCODE -ne 0) { Read-Host "Build failed, press Enter"; exit 1 }
Write-Host "Done! ymap2xml.exe is in the dist\ folder." -ForegroundColor Green

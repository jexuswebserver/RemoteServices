if ([System.IO.File]::Exists("RemoteServices.zip"))
{
    Write-Host "Delete existing package."
    Remove-Item RemoteServices.zip
}

$files = Get-Content list.txt
if (![System.IO.Directory]::Exists("bin"))
{
    Write-Host "Likely an error happened. Create bin folder."
    New-Item bin -ItemType Directory
}

Set-Location bin
Remove-Item *.pdb,*.xml
Compress-Archive $files -CompressionLevel Optimal -DestinationPath ..\RemoteServices.zip
Set-Location ..

Write-Host "Package is ready, RemoteServices.zip."

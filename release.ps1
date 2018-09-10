$msBuild = "msbuild"
& $msBuild /version
if ($LastExitCode -ne 0)
{
    Write-Host "MSBuild doesn't exist. Use VSSetup instead."

    Install-Module VSSetup -Scope CurrentUser -Force
    $instance = Get-VSSetupInstance -All | Select-VSSetupInstance -Require 'Microsoft.Component.MSBuild' -Latest
    $installDir = $instance.installationPath
    $msBuild = $installDir + '\MSBuild\15.0\Bin\MSBuild.exe'
    if (![System.IO.File]::Exists($msBuild))
    {
        Write-Host "MSBuild doesn't exist. Exit."
        exit 1
    }

    Write-Host "Likely on Windows."
}
else
{
    Write-Host "Likely on Linux/macOS."
}

Write-Host "MSBuild found. Compile the projects."
& $msBuild /p:Configuration=Release /t:Rebuild
if ($LastExitCode -ne 0)
{
    Write-Error "MSBuild failed."
    exit 1
}

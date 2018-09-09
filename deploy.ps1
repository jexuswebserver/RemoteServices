Invoke-Expression $PSScriptRoot\release.ps1
if ($LastExitCode -ne 0)
{
    Write-Error "Release script failed."
    exit 1
}

Invoke-Expression $PSScriptRoot\package.ps1
if ($LastExitCode -ne 0)
{
    Write-Error "Package script failed."
    exit 1
}

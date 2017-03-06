function NugetPack {
    param($basepath, $nuspec)
    if (Test-Path $nuspec)
    {
        & $nuget pack $nuspec -Version $version -BasePath $basepath
        ProcessLastExitCode $lastexitcode "nuget pack $nuspec error"
    }
}

function ProcessLastExitCode {
    param($exitCode, $msg)
    if ($exitCode -ne 0)
    {
        Write-Error "$msg, exit code: $exitCode"
        Pop-Location
        Exit 1
    }
}

$version = "1.0.0"
$currentDir = $($MyInvocation.MyCommand.Definition) | Split-Path
$nuget = "$env:LOCALAPPDATA\Nuget\Nuget.exe"
if (-not(Test-Path $nuget))
{
    Write-Host "Downloading NuGet.exe..."
    mkdir -Path "$env:LOCALAPPDATA\Nuget" -Force
    $ProgressPreference = 'SilentlyContinue'
    [Net.WebRequest]::DefaultWebProxy.Credentials = [Net.CredentialCache]::DefaultCredentials
    Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile $nuget
}

Copy-Item -Path (Join-Path $currentDir "run.ps1") -Destination (Join-Path $currentDir "..\bin\Release")
NugetPack (Join-Path $currentDir "..\bin\Release") (Join-Path $currentDir ".\nuspec\SplitToc.nuspec")

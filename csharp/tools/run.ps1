param (
    [parameter(mandatory=$true)]
    [hashtable]$ParameterDictionary
)

$errorActionPreference = 'Stop'

$currentDir = $($MyInvocation.MyCommand.Definition) | Split-Path
$repositoryRoot = $ParameterDictionary.environment.repositoryRoot
$tocPath = $ParameterDictionary.environment["toc_path_need_to_be_splitted"]
if ($tocPath)
{
	Write-Host "toc_path_need_to_be_splitted in openpublish.config.json can't be found, skip to split toc.yml into namespace level.";
	exit 1;
}

$tocPath = Join-Path $repositoryRoot $tocPath
$splitTocExeName = "SplitToc.exe"
$splitTocExePath = Join-Path $currentDir $splitTocExeName

Write-Host "Executing $splitTocExePath $tocPath" | timestamp
& "$splitTocExePath" $tocPath
if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}
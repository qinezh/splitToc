param (
    [parameter(mandatory = $true)]
    [hashtable]$ParameterDictionary
)

$errorActionPreference = 'Stop'

$currentDir = $($MyInvocation.MyCommand.Definition) | Split-Path
$repositoryRoot = $ParameterDictionary.environment.repositoryRoot
$splitTocExeName = "SplitToc.exe"
$splitTocExePath = Join-Path $currentDir $splitTocExeName
$tocPath = $ParameterDictionary.environment.publishConfigContent.toc_path_need_to_split

if (!$tocPath) {
    Write-Host "toc_path_need_to_split in openpublish.config.json can't be found, skip to split toc.yml into namespace level.";
    exit 1;
}

function SplitToc {
    param($tocPath)
    Write-Host "Executing $splitTocExePath $tocPath" | timestamp
    & "$splitTocExePath" $tocPath
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

if ($tocPath -is [System.Array]) {
    foreach ($singlePath in $tocPath) {
        $singlePath = Join-Path $repositoryRoot $singlePath
        SplitToc($singlePath)
    }
}
else {
    $tocPath = Join-Path $repositoryRoot $tocPath
    SplitToc($tocPath)
}

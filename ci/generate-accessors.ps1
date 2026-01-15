param (
    [Parameter(Mandatory)][string]$DataType,
    [Parameter(Mandatory)][string]$MetaDataPath,
    $RepoName # unused, here for compatibility
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Push-Location "$PSScriptRoot/../PropertyGenerator"
try {
    # TODO: remove -a after IpIntelligence properly fixes .targets file
    # otherwise without passing this argument it tries to infer win-x86
    # from the IpIntelligence package and fails
    # it is supposed to build on ubuntu-latest x64, so this should work
    dotnet run -c:Release -a:x64 -- "$DataType" .. "$MetaDataPath"
} finally {
    Pop-Location
}

param (
    [Parameter(Mandatory)][string]$RepoName,
    [Parameter(Mandatory)][string]$DataFile
)
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

Push-Location $RepoName/PropertyGenerator
try {
    # TODO: remove after IpIntelligence properly fixes .targets file 
    # otherwise without passing this argument it tries to infer win-x86 
    # from the IpIntelligence package and fails
    # it is supposed to build on ubuntu-latest x64, so this should work
    dotnet build -c:Release -p:Platform=x64
    dotnet "$(find ./bin -name PropertyGenerator.dll | head -n 1)" "$DataFile" ..
} finally {
    Pop-Location
}

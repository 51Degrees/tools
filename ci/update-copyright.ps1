param (
    [string]$RepoName,
    [string]$TargetRepo
)

$RepoPath = [IO.Path]::Combine($pwd, $RepoName)
$TargetRepoPath = [IO.Path]::Combine($pwd, $TargetRepo)
$UpdaterPath = [IO.Path]::Combine($RepoPath, "CopyrightUpdater")

Push-Location $UpdaterPath

try {

    dotnet build -c Release
    dotnet run -- -e $TargetRepoPath

}
finally {
    Pop-Location
}
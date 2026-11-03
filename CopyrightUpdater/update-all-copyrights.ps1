param($BaseUri = 'git@github.com:51Degrees')
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

$repos = @(
    '51degrees-cxx'
    '51degrees-dotnet'
    '51degrees-java'
    '51degrees-php'
    'device-detection-nginx'
    'device-detection-node'
    'device-detection-python'
    'internal-common-dotnet'
    'location-node'
    'location-python'
    'pipeline-node'
    'pipeline-python'
    # 'pipeline-wordpress' # disabled due to CopyrightUpdater requiring having the language in the repo name
)

foreach ($repo in $repos) {
    Write-Host "--- Cloning $repo ---"
    git clone --quiet --filter=tree:0 "$BaseUri/$repo.git"

    Write-Host "Updating copyright..."
    dotnet run --project "$PSScriptRoot/CopyrightUpdater.csproj" -- -e $repo

    Write-Host "Checking for changes..."
    $changes = git -C $repo status --porcelain --untracked-files=no
    if ($changes) {
        Write-Host "Changed:`n$changes"
        git -C $repo add -u
        git -C $repo -c 'user.name=github-actions[bot]' -c 'user.email=41898282+github-actions[bot]@users.noreply.github.com' commit -m 'Update copyright'

        Write-Host "Pushing to $repo/update-copyright"
        git -C $repo push --quiet --force origin HEAD:update-copyright

        $fullRepo = "$($BaseUri -replace '.*[:/]', '')/$repo"
        if ((gh pr list -R $fullRepo -H update-copyright -B main --json number --jq '.[].number') -gt 0) {
            Write-Host "PR already exists"
        } else {
            Write-Host "Creating PR..."
            gh pr create -R $fullRepo -H update-copyright -B main --title 'Update copyright' --body 'Update copyright by https://github.com/51Degrees/tools/tree/main/CopyrightUpdater'
        }
    }
    Remove-Item -Recurse -Force $repo
}

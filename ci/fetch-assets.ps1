param (
    [string]$DeviceDetectionKey,
    [string]$DeviceDetectionUrl
)
$ErrorActionPreference = 'Stop'

./steps/fetch-assets.ps1 -DeviceDetection:$DeviceDetection -DeviceDetectionUrl:$DeviceDetectionUrl -Assets 'TAC-HashV41.hash'
New-Item -ItemType SymbolicLink -Force -Target "$PWD/assets/TAC-HashV41.hash" -Path "$PSScriptRoot/../TAC-HashV41.hash"

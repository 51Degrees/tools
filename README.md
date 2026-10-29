# V4 Pipeline Tools

## Property Generator
The `PropertyGenerationTool` generates code for `DeviceDataBase` and `DeviceData` (`IDeviceData` in .NET) classes for
device detection Java and .NET using the V4 data file.

See: 
[DeviceDataBase.java](https://github.com/51Degrees/device-detection-java/blob/master/device-detection.shared/src/main/java/fiftyone/devicedetection/shared/DeviceDataBase.java)
[DeviceData.java](https://github.com/51Degrees/device-detection-java/blob/master/device-detection.shared/src/main/java/fiftyone/devicedetection/shared/DeviceData.java)

[IDeviceData.cs](https://github.com/51Degrees/device-detection-dotnet/blob/master/FiftyOne.DeviceDetection/FiftyOne.DeviceDetection.Data/Data/IDeviceData.cs)
[DeviceDataBase.cs](https://github.com/51Degrees/device-detection-dotnet/blob/master/FiftyOne.DeviceDetection/FiftyOne.DeviceDetection.Data/DeviceDataBase.cs)

The License text is kept in the PropertyGenerationTool folder and should contain only the license text.

This is program is run as a part of a daily workflow which rebuilds the device classes from the most recent V4 TAC data file downloaded from the distributor.

## Copyright Updater

This command line utility is intended for reporting on, updating and adding license/copyright headers in source code files.

### Arguments

The utility expect two arguments: [command] [path]

**command** can be either:
- -r - report mode - list the license/copyright notices present in the files under the path.
- -e - enforce mode - modify matching files to have a particular notice.

**path** is the complete path to the directory to check. For example, D:\Workspace\51degrees-cxx

### Configuration

Configuration options are available in the appsettings.json file.

TODO: Add descriptions of each item.

(Include fact that submodule directories are excluded automatically)

### Process

TODO: Description of process including:  
    - Behavior if 'copyright' text is in existing comment  
    - Behavior if 'patent' text is in existing comment
    - Behavior if 'swig' text is in existing comment


## DoxyGen

This repository contains a precompiled DoxyGen binary from the 51Degrees source version of DoxyGen.
This has a few modifications. See [source](https://github.com/51degrees/DoxyGen) for more.

## GitHubAutiomation

Contains scripts for automating administrative GitHub tasks.

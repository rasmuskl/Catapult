@echo off
"source\.nuget\nuget.exe" install tools\packages.config -OutputDirectory tools -ExcludeVersion
"tools\FAKE\tools\Fake.exe" build.fsx

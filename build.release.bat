@ ECHO off
cls
SET /p ApiKey=nuget api key:
"c:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" Build.proj /m /p:ConfigurationName=Release /p:ApiKey=%ApiKey% /p:VisualStudioVersion=14.0%*
pause
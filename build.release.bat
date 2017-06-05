@ ECHO off
cls
SET /p ApiKey=nuget api key:
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" Build.proj /m /p:ConfigurationName=Release /p:ApiKey=%ApiKey% /p:VisualStudioVersion=14.0%*
pause
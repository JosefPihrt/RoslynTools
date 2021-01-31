@echo off

set _programFiles=%ProgramFiles(x86)%
if not defined _programFiles set _programFiles=%ProgramFiles%

set _msbuildPath="%_programFiles%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"

%_msbuildPath%\msbuild "..\src\CommandLine.sln" /t:Build /p:Configuration=Debug /v:m /m

"..\src\CommandLine\bin\Debug\net48\roslynator" loc "..\src\Roslynator.sln" ^
 --msbuild-path %_msbuildPath% ^
 --ignore-block-boundary ^
 --verbosity d ^
 --file-log "roslynator.log" ^
 --file-log-verbosity diag

pause

@echo off

set _programFiles=%ProgramFiles(x86)%
if not defined _programFiles set _programFiles=%ProgramFiles%

set _roslynatorExe="..\src\CommandLine\bin\Debug\net48\roslynator"
set _msbuildPath="%_programFiles%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"
set _msbuildProperties="Configuration=Release"

%_msbuildPath%\msbuild "..\src\CommandLine.sln" /t:Clean,Build /p:Configuration=Debug /v:m /m

%_roslynatorExe% list-symbols "..\src\Tests\Testing.CSharp.sln" ^
 --msbuild-path %_msbuildPath% ^
 --properties %_msbuildProperties% ^
 --visibility public ^
 --depth member ^
 --ignored-projects Core ^
 --ignored-parts containing-namespace assembly-attributes ^
 --output "testing_csharp_api.txt"

pause

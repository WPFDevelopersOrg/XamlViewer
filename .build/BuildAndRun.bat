@echo off
  
set frameworkpath=%windir%\microsoft.net\framework\v4.0.30319
set solutionname=XamlViewer
set platform=Any CPU
set configuration=Debug

set runapp=..\outputs\Debug\Xaml Viewer.exe
 
cd %~dp0
  
%frameworkpath%\msbuild.exe /m ..\%solutionname%.sln /p:Configuration=%configuration% /p:Platform="%platform%"

@IF %ERRORLEVEL% EQU 0 GOTO run
@IF %ERRORLEVEL% NEQ 0 pause

@exit /B 0

:run 
start "" "%runapp%"


 

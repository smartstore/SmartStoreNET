for /f "usebackq tokens=1* delims=: " %%i in (`lib\vswhere\vswhere -latest -requires Microsoft.Component.MSBuild`) do (
	if /i "%%i"=="installationPath" set InstallDir=%%j
)

FOR %%b in (
       "%InstallDir%\Common7\Tools\VsMSBuildCmd.bat"
       "%VS140COMNTOOLS%\Common7\Tools\vsvars32.bat"
    ) do (
    if exist %%b ( 
       call %%b
       goto findmsbuild
    )
)

echo "Unable to detect suitable environment. Build may not succeed."

:findmsbuild

SETLOCAL ENABLEDELAYEDEXPANSION

if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
	if not defined MsBuildPath (	
		SET "MsBuildPath=%InstallDir%\MSBuild\15.0\Bin\MsBuild.exe"
		goto build	
	)	
)


echo "Unable to detect suitable MsBuild version (15.0). Build may not succeed."

:build
cd /d %~dp0

echo "Restoring NuGet packages"
lib\nuget\nuget.exe restore "src\SmartStoreNET.Full-sym.sln"

call "!MsBuildPath!" SmartStoreNET.proj /p:SlnName=SmartStoreNET /m /p:DebugSymbols=false /p:DebugType=None /maxcpucount %*

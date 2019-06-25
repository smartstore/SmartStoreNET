SETLOCAL ENABLEDELAYEDEXPANSION

for /f "usebackq tokens=1* delims=: " %%i in (`lib\vswhere\vswhere -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild`) do (
	if /i "%%i"=="installationPath" (
		set InstallDir=%%j
		echo !InstallDir!
		if exist "!InstallDir!\MSBuild\15.0\Bin\MSBuild.exe" (
			echo "Using MSBuild from Visual Studio 2017"
			set msbuild="!InstallDir!\MSBuild\15.0\Bin\MSBuild.exe"
			goto build
		)
	)
)

FOR %%b in (
       "%VS140COMNTOOLS%\vsvars32.bat"
       "%VS120COMNTOOLS%\vsvars32.bat"
       "%VS110COMNTOOLS%\vsvars32.bat"
    ) do (
    if exist %%b ( 
		echo "Using MSBuild from %%b"
		call %%b
		set msbuild="msbuild"
		goto build
    )
)

echo "Unable to detect suitable environment. Build may not succeed."

:build

cd /d %~dp0

echo "Restoring NuGet packages"
lib\nuget\nuget.exe restore "src\SmartStoreNET.sln"

%msbuild% SmartStoreNET.proj /p:SlnName=SmartStoreNET /m /p:DebugSymbols=false /p:DebugType=None /maxcpucount %*

:end

pause

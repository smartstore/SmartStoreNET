SETLOCAL ENABLEDELAYEDEXPANSION

for /f "usebackq tokens=1* delims=: " %%i in (`lib\vswhere\vswhere -latest -requires Microsoft.Component.MSBuild`) do (
	if /i "%%i"=="installationPath" (
		set InstallDir=%%j
		echo !InstallDir!
		if exist "!InstallDir!\MSBuild\15.0\Bin\MSBuild.exe" (
			echo "Using MSBuild from Visual Studio 2017"
			set msbuild="!InstallDir!\MSBuild\15.0\Bin\MSBuild.exe"
			goto build
		)
		if exist "!InstallDir!\MSBuild\Current\Bin\MSBuild.exe" (
			echo "Using MSBuild from Visual Studio 2019"
			set msbuild="!InstallDir!\MSBuild\Current\Bin\MSBuild.exe"
			goto build
		)
	)
)

FOR %%b in (
	   "%VS150COMNTOOLS%\vsvars32.bat"
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

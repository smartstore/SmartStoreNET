FOR %%b in (
		"%VS140COMNTOOLS%..\..\VC\vcvarsall.bat"
		"%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat"
		"%ProgramFiles%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" 

		"%VS120COMNTOOLS%..\..\VC\vcvarsall.bat"
		"%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat"
		"%ProgramFiles%\Microsoft Visual Studio 12.0\VC\vcvarsall.bat"

		"%VS110COMNTOOLS%..\..\VC\vcvarsall.bat"
		"%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat"
		"%ProgramFiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" 
	) do (
	if exist %%b ( 
		call %%b x86
		goto findmsbuild
	)
)
  
echo "Unable to detect suitable environment. Build may not succeed."

:findmsbuild

SETLOCAL ENABLEDELAYEDEXPANSION

FOR %%p in (
	   "%ProgramFiles(x86)%\MSBuild\14.0\Bin"
	   "%ProgramFiles%\MSBuild\14.0\Bin"
    ) do (
	if exist %%p (
		if not defined MsBuildPath (
			SET "MsBuildPath=%%~p"
			goto build	
		)
	)
)

echo "Unable to detect suitable MsBuild version (14.0). Build may not succeed."

:build

call "!MsBuildPath!\msbuild.exe" SmartStoreNET.proj /p:DebugSymbols=false /p:DebugType=None /P:SlnName=SmartStoreNET /maxcpucount %*
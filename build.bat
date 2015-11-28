set MSBuildPath=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe

@IF NOT EXIST %MSBuildPath% @ECHO COULDN'T FIND MSBUILD: %MSBuildPath% (Is .NET 4 installed?) 
ELSE GOTO END

:CheckOS
IF EXIST "%PROGRAMFILES(X86)%" (GOTO 64BIT) ELSE (GOTO 32BIT)

:64BIT
echo 64-bit...
set MSBuildPath="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
GOTO END

:32BIT
echo 32-bit...
set MSBuildPath="%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe"
GOTO END

:END

%MSBuildPath% SmartStoreNET.proj /p:DebugSymbols=false /p:DebugType=None /P:SlnName=SmartStoreNET /maxcpucount %*
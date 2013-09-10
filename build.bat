set MSBuildPath=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe

@IF NOT EXIST %MSBuildPath% @ECHO COULDN'T FIND MSBUILD: %MSBuildPath% (Is .NET 4 installed?)

%MSBuildPath% SmartStoreNET.proj /p:DebugSymbols=false /p:DebugType=None /P:SlnName=SmartStoreNET /maxcpucount %*
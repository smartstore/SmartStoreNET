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
       goto build
    )
)
  
echo "Unable to detect suitable environment. Build may not succeed."

:build

msbuild SmartStoreNET.proj /p:DebugSymbols=false /p:DebugType=None /P:SlnName=SmartStoreNET /maxcpucount %*
# Readme

This solution shows a crash that occurs running on the `browser-wasm` platform with a specific setup when calling `string GetProgramInfoLog(uint program)` from `Silk.NET.OpenGLES.GL`.
It depends on `Silk.Net.OpenGLES` 2.23.0 and requires the WebAssembly build tools for DotNet 10.

Please follow the following steps to reproduce the issue:
1. Ensure that WebAssembly build tools are installed for .Net 10:
```
dotnet workload install wasm-tools
dotnet workload install wasm-experimental
```
2. Open `SilkCrashRepro.slnx` with Visual Studio 2026 and press F5 to launch the application in Debug mode.

After entering Debug mode, a browser window should open. The solution runs as interpreted WASM code inside the browser.  
The `SilkProgramInfoLogRepro.Run` method is entered and sets up the crash by compiling a vertex+fragment shader pair that can't be successfully loaded as a shader program. The code throws an ArgumentOutOfRangeException while trying to trim the text returned to the user in the `Silk.NET.OpenGLES.GL` method `void GetProgramInfoLog(uint program, out string info)`.

Please note that it's still possible to use the span based overload instead. If you comment the call to `TryStringOverload` then `TrySpanOverload` will be able to successfully retrieve the program info log.
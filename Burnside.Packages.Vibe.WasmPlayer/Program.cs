/* SilkCrash Repro

Please read Readme.md to learn about the purpose of this software.
*/

using Silk.NET.OpenGLES;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

namespace Burnside.Packages.Vibe.WasmPlayer;

static class Program
{
    [UnmanagedCallersOnly]
    public static int Frame(double time, nint userData)
    {
        return 0;// stripped
    }

    public static async Task Main(string[] args)
    {
        try
        {
            await Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine("CRITICAL ERROR IN MAIN:");
            Console.WriteLine(ex.ToString());
            Interop.ShowError(ex.Message);
        }
    }

    static async Task Run()
    {
        var display = EGL.GetDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            throw new Exception("Display was null");

        if (!EGL.Initialize(display, out _, out _))
            throw new Exception("Initialize() returned false.");

        int[] attributeList =
        [
            EGL.EGL_RED_SIZE  , 8,
            EGL.EGL_GREEN_SIZE, 8,
            EGL.EGL_BLUE_SIZE , 8,
            EGL.EGL_DEPTH_SIZE, 24,
            EGL.EGL_STENCIL_SIZE, 8,
            EGL.EGL_SURFACE_TYPE, EGL.EGL_WINDOW_BIT,
            EGL.EGL_RENDERABLE_TYPE, EGL.EGL_OPENGL_ES3_BIT,
            EGL.EGL_SAMPLES, 0,
            EGL.EGL_NONE
        ];

        var config = IntPtr.Zero;
        var numConfig = IntPtr.Zero;
        if (!EGL.ChooseConfig(display, attributeList, ref config, 1, ref numConfig))
            throw new Exception("ChoseConfig() failed");

        if (!EGL.BindApi(EGL.EGL_OPENGL_ES_API))
            throw new Exception("BindApi() failed");

        int[] ctxAttribs = [EGL.EGL_CONTEXT_CLIENT_VERSION, 3, EGL.EGL_NONE];
        var context = EGL.CreateContext(display, config, EGL.EGL_NO_CONTEXT, ctxAttribs);
        if (context == IntPtr.Zero)
            throw new Exception("CreateContext() failed");

        var surface = EGL.CreateWindowSurface(display, config, IntPtr.Zero, IntPtr.Zero);
        if (surface == IntPtr.Zero)
            throw new Exception("CreateWindowSurface() failed");

        if (!EGL.MakeCurrent(display, surface, surface, context))
            throw new Exception("MakeCurrent() failed");

        TrampolineFuncs.ApplyWorkaroundFixingInvocations();

        var gl = GL.GetApi(EGL.GetProcAddress);
        SilkProgramInfoLogRepro.Run(gl);

        unsafe
        {
            Emscripten.RequestAnimationFrameLoop((delegate* unmanaged<double, nint, int>)&Frame, nint.Zero);
        }
    }
}

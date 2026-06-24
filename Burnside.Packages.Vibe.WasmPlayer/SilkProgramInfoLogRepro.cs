using System.Text;
using Silk.NET.OpenGLES;

namespace Burnside.Packages.Vibe.WasmPlayer;

static class SilkProgramInfoLogRepro
{
    public static void Run(GL gl)
    {
        Console.WriteLine("[SilkRepro] Starting program info log repro.");
        RunCase(gl, "type-mismatch", VertexTypeMismatch, FragmentTypeMismatch);
    }

    static void RunCase(GL gl, string name, string vertexSource, string fragmentSource)
    {
        var vertex = Compile(gl, ShaderType.VertexShader, vertexSource, $"{name}:vertex");
        var fragment = Compile(gl, ShaderType.FragmentShader, fragmentSource, $"{name}:fragment");
        if (vertex == 0 || fragment == 0)
        {
            if (vertex != 0)
            {
                gl.DeleteShader(vertex);
            }

            if (fragment != 0)
            {
                gl.DeleteShader(fragment);
            }

            return;
        }

        var program = gl.CreateProgram();
        gl.AttachShader(program, vertex);
        gl.AttachShader(program, fragment);
        gl.LinkProgram(program);
        gl.GetProgram(program, GLEnum.LinkStatus, out var linked);
        gl.GetProgram(program, GLEnum.InfoLogLength, out var infoLogLength);
        Console.WriteLine($"[SilkRepro] {name} linkStatus={linked} infoLogLength={infoLogLength}");

        if (linked == 0)
        {
            // gl.GetProgramInfoLog(program) is expected to crash in the next function call as a
            // result of GetProgramInfoLog trying to call `info = info.Substring(0, (int)length);`
            // with a desired string length exceeding the given text's length.
            //
            // Expected text: Types of varying 'v_value' differ between VERTEX and FRAGMENT shaders.\nFRAGMENT varying v_value does not match any VERTEX varying
            // Text length: 131
            // Length returned by GetProgramInfoLog: 132
            TryStringOverload(gl, program, name);

            // If you comment the above call you can verify that span based program info log
            // retrieval is not affected.
            TrySpanOverload(gl, program, name);
        }
        else
        {
            Console.WriteLine($"[SilkRepro] {name} unexpectedly linked successfully.");
        }

        gl.DeleteProgram(program);
        gl.DeleteShader(vertex);
        gl.DeleteShader(fragment);
    }

    static uint Compile(GL gl, ShaderType type, string source, string label)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out var compiled);
        gl.GetShader(shader, ShaderParameterName.InfoLogLength, out var infoLogLength);
        Console.WriteLine($"[SilkRepro] {label} compileStatus={compiled} infoLogLength={infoLogLength}");
        if (compiled != 0)
        {
            return shader;
        }

        TryShaderStringOverload(gl, shader, label);
        gl.DeleteShader(shader);
        return 0;
    }

    static void TryStringOverload(GL gl, uint program, string name)
    {
        Console.WriteLine($"[SilkRepro] {name} string overload begin");
        try
        {
            var info = gl.GetProgramInfoLog(program);
            Console.WriteLine($"[SilkRepro] {name} string overload ok len={info.Length}");
            Console.WriteLine($"[SilkRepro] {name} string overload log: {Escape(info)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SilkRepro] {name} string overload threw: {ex}");
        }
    }

    static void TrySpanOverload(GL gl, uint program, string name)
    {
        Console.WriteLine($"[SilkRepro] {name} span overload begin");
        try
        {
            gl.GetProgram(program, GLEnum.InfoLogLength, out var infoLogLength);
            Span<byte> buffer = infoLogLength > 0
                ? stackalloc byte[infoLogLength]
                : stackalloc byte[1];
            gl.GetProgramInfoLog(program, out uint written, buffer);
            var safeLength = (int)Math.Min(written, (uint)buffer.Length);
            var info = Encoding.UTF8.GetString(buffer[..safeLength]);
            Console.WriteLine($"[SilkRepro] {name} span overload ok len={info.Length} written={written} buffer={buffer.Length}");
            Console.WriteLine($"[SilkRepro] {name} span overload log: {Escape(info)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SilkRepro] {name} span overload threw: {ex}");
        }
    }

    static void TryShaderStringOverload(GL gl, uint shader, string label)
    {
        Console.WriteLine($"[SilkRepro] {label} shader string overload begin");
        try
        {
            var info = gl.GetShaderInfoLog(shader);
            Console.WriteLine($"[SilkRepro] {label} shader string overload ok len={info.Length}");
            Console.WriteLine($"[SilkRepro] {label} shader string overload log: {Escape(info)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SilkRepro] {label} shader string overload threw: {ex}");
        }
    }

    static string Escape(string value) =>
        value.Replace("\r", "\\r").Replace("\n", "\\n");

    const string VertexTypeMismatch = """
        #version 300 es
        precision mediump float;
        layout(location = 0) in vec2 a_position;
        out vec2 v_value;
        void main()
        {
            v_value = a_position;
            gl_Position = vec4(a_position, 0.0, 1.0);
        }
        """;

    const string FragmentTypeMismatch = """
        #version 300 es
        precision mediump float;
        in float v_value;
        out vec4 fragColor;
        void main()
        {
            fragColor = vec4(v_value, 0.0, 0.0, 1.0);
        }
        """;
}

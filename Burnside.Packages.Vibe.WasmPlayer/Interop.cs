using System.Runtime.InteropServices.JavaScript;

namespace Burnside.Packages.Vibe.WasmPlayer;

static partial class Interop
{
    [JSImport("initialize", "main.js")]
    public static partial void Initialize();

    [JSImport("getMsaaSamples", "main.js")]
    public static partial int GetMsaaSamples();

    [JSImport("updateLoading", "main.js")]
    public static partial void UpdateLoading(float ratio, string message);

    [JSImport("showReady", "main.js")]
    public static partial void ShowReady();

    [JSImport("showError", "main.js")]
    public static partial void ShowError(string message);

    [JSExport]
    public static void OnKeyDown(bool shift, bool ctrl, bool alt, bool repeat, int code)
    { }

    [JSExport]
    public static void OnKeyUp(bool shift, bool ctrl, bool alt, int code)
    { }

    [JSExport]
    public static void OnMouseMove(float x, float y)
    { }

    [JSExport]
    public static void OnMouseDown(bool shift, bool ctrl, bool alt, int button)
    { }

    [JSExport]
    public static void OnMouseUp(bool shift, bool ctrl, bool alt, int button)
    { }

    [JSExport]
    public static void OnCanvasResize(float width, float height, float devicePixelRatio)
    {

    }

    [JSExport]
    public static void SetRootUri(string uri)
    {
        //stripped
    }

    [JSExport]
    public static void AddLocale(string locale)
    {
    }
}

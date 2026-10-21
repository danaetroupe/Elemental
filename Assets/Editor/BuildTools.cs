using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildTools
{
    [MenuItem("Build/WebGL (itch.io)")]
    public static void BuildWebGL()
    {
        // itch.io serves files without Content-Encoding headers, so compressed builds need the JS decompression fallback.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        Build(BuildTarget.WebGL, "Builds/WebGL");
    }

    [MenuItem("Build/Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/Elemental.exe");
    }

    private static void Build(BuildTarget target, string path)
    {
        var options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = path,
            target = target,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"[BuildTools] {target} build {report.summary.result}: {report.summary.totalSize / (1024 * 1024)} MB at {path}");

        if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}

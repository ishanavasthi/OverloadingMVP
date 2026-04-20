using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Batch/menu WebGL build entry point for itch.io submission.
/// </summary>
public static class WebGLBuild
{
    private const string BuildPath = "Builds/WebGL";
    private static readonly string[] Scenes = { "Assets/Scenes/SampleScene.unity" };

    [MenuItem("Overloading/Build WebGL")]
    public static void Build()
    {
        Directory.CreateDirectory(BuildPath);

        BuildPlayerOptions buildOptions = new()
        {
            scenes = Scenes,
            locationPathName = BuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"WebGL build succeeded: {BuildPath} ({summary.totalSize} bytes)");
            return;
        }

        Debug.LogError($"WebGL build failed: {summary.result}");
        EditorApplication.Exit(1);
    }
}

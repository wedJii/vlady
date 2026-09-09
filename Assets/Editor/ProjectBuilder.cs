#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectBuilder
{
    private static readonly string TriggerPath = Path.Combine(Directory.GetCurrentDirectory(), "BuildTrigger.txt");
    private static readonly string StatusPath = Path.Combine(Directory.GetCurrentDirectory(), "BuildStatus.txt");

    static ProjectBuilder()
    {
        EditorApplication.delayCall += CheckTrigger;
    }

    [MenuItem("Tools/Build Windows Standalone")]
    public static void BuildWindowsManual()
    {
        PerformBuild();
    }

    private static void CheckTrigger()
    {
        if (File.Exists(TriggerPath))
        {
            try
            {
                File.Delete(TriggerPath);
            }
            catch { }
            PerformBuild();
        }
    }

    public static void PerformBuild()
    {
        try
        {
            Debug.Log("[ProjectBuilder] Starting StandaloneWindows64 build...");
            File.WriteAllText(StatusPath, "BUILDING");

            string rootDir = Directory.GetCurrentDirectory();
            string buildFolder = Path.Combine(rootDir, "Build", "vlady");
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string exePath = Path.Combine(buildFolder, "vlady.exe");

            string[] preferredScenes = new[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/viborDungeon.unity",
                "Assets/Scenes/Dungeon 1.unity",
                "Assets/Scenes/poshalko.unity",
                "Assets/Scenes/poshalochko2.unity"
            };

            var settingsScenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .Where(p => !preferredScenes.Contains(p));

            var allScenes = preferredScenes.Concat(settingsScenes).Where(File.Exists).ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = allScenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string successMsg = $"SUCCESS: Total size {summary.totalSize} bytes, warnings: {summary.totalWarnings}";
                Debug.Log($"[ProjectBuilder] {successMsg}");
                File.WriteAllText(StatusPath, successMsg);
            }
            else
            {
                string failMsg = $"FAILED: Errors: {summary.totalErrors}, result: {summary.result}";
                Debug.LogError($"[ProjectBuilder] {failMsg}");
                File.WriteAllText(StatusPath, failMsg);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            File.WriteAllText(StatusPath, $"EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
#endif

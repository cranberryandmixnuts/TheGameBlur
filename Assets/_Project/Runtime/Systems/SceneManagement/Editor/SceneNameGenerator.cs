using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SceneNameGenerator
{
    [InitializeOnLoadMethod]
    private static void OnLoad() =>
        EditorBuildSettings.sceneListChanged += Generate;

    public static void Generate()
    {
        var scenes = EditorBuildSettings.scenes;
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("// AUTO GENERATED FILE - DO NOT MODIFY");
        stringBuilder.AppendLine("using System;");
        stringBuilder.AppendLine("[Serializable]");
        stringBuilder.AppendLine("public enum SceneType {");

        foreach (var scene in scenes)
        {
            string path = scene.path;
            string name = Path.GetFileNameWithoutExtension(path);
            string safeName = ToSafeIdentifier(name);

            if (safeName != name)
            {
                Debug.LogError("Scene Name: " + name);
            }
            stringBuilder.AppendLine($"\t{name},");
        }

        stringBuilder.AppendLine("}");

        string savePath = "Assets/_Project/Runtime/Systems/SceneManagement/Generated/SceneType.cs";
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        File.WriteAllText(savePath, stringBuilder.ToString());

        AssetDatabase.Refresh();
    }

    private static string ToSafeIdentifier(string name)
    {
        if (char.IsDigit(name[0]))
            name = "_" + name;

        return System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
    }
}

internal class SceneAssetPostProcessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        bool sceneChanged = false;

        foreach (string path in importedAssets)
        {
            if (path.EndsWith(".unity"))
                sceneChanged = true;
        }

        foreach (string path in deletedAssets)
        {
            if (path.EndsWith(".unity"))
                sceneChanged = true;
        }

        for (int i = 0; i < movedAssets.Length; ++i)
        {
            if (movedAssets[i].EndsWith(".unity") || movedFromAssetPaths[i].EndsWith(".unity"))
                sceneChanged = true;
        }

        if (sceneChanged)
            SceneNameGenerator.Generate();
    }
}
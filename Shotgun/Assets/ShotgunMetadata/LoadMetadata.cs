using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;

[InitializeOnLoad]
public static class LoadMetadata
{
    static LoadMetadata()
    {
        EditorApplication.update += RunPythonScript;
    }

    private static void RunPythonScript()
    {
        EditorApplication.update -= RunPythonScript;
        PythonRunner.RunFile(System.IO.Path.Combine(Application.dataPath, "ShotgunMetadata/parseMetadata.py"));
    }
}

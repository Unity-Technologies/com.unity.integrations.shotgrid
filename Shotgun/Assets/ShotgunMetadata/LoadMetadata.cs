using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;

[InitializeOnLoad]
public static class LoadMetadata
{
    static LoadMetadata()
    {
        EditorApplication.delayCall += RunPythonScript;
    }

    private static void RunPythonScript()
    {
        PythonRunner.RunFileOnClient(System.IO.Path.Combine(Application.dataPath, "ShotgunMetadata/parseMetadata.py"));
    }

}
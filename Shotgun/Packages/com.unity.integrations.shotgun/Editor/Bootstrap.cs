//#define DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;
using System.IO;
using Python.Runtime;

public class Bootstrap
{
    private const string ImportServerString          = "from unity_rpyc import unity_server as unity_server\n";
    private const string ClientInitModuleFileName    = "sg_client_init.py";

    /// <summary>
    /// Runs Python code on the Python client
    /// </summary>
    /// <param name="pythonCodeToExecute">The code to execute.</param>
    public static void RunPythonCodeOnClient(string pythonCodeToExecute)
    {
        string serverCode = ImportServerString + string.Format(@"unity_server.run_python_code_on_client('{0}')",pythonCodeToExecute);
        PythonRunner.RunString(serverCode);
    }

    /// <summary>
    /// Runs a Python script on the Python client
    /// </summary>
    /// <param name="pythonFileToExecute">The script to execute.</param>
    public static void RunPythonFileOnClient(string pythonFileToExecute)
    {
        string serverCode = ImportServerString + string.Format(@"unity_server.run_python_file_on_client('{0}')",pythonFileToExecute);
        PythonRunner.RunString(serverCode);
    }

    /// <summary>
    /// Starts the Unity server (rpyc)
    /// </summary>
    /// <param name="clientInitModulePath">Path to the client init module that should be used when the Unity client starts.</param>
    public static void CallStartServer(string clientInitModulePath = null)
    {
        string clientInitPathString;
        if (clientInitModulePath != null)
        {
            clientInitModulePath = clientInitModulePath.Replace("\\","/");
            clientInitPathString = string.Format("'{0}'",clientInitModulePath);
        }
        else
        {
            clientInitPathString = "None";
        }

        string serverCode = ImportServerString + string.Format(@"unity_server.start({0})", clientInitPathString);
        PythonRunner.RunString(serverCode);

        // We need to stop the server on Python shutdown 
        // (which is triggered by domain unload)
        PythonEngine.AddShutdownHandler(OnPythonShutdown);
    }

    /// <summary>
    /// Stops the Unity server
    /// </summary>
    public static void CallStopServer()
    {
        string serverCode = ImportServerString + @"unity_server.stop()";
        PythonRunner.RunString(serverCode);
    }

    /// <summary>
    /// Starts the Unity server, then bootstraps Shotgun on the Unity client
    /// </summary>
    public static void CallBootstrap()
    {
        // Use the engine's rpyc client script
        string bootstrapScript = System.Environment.GetEnvironmentVariable("SHOTGUN_UNITY_BOOTSTRAP_LOCATION");
        bootstrapScript = bootstrapScript.Replace(@"\","/");

        string clientInitModulePath = Path.GetDirectoryName(bootstrapScript);
        clientInitModulePath = Path.Combine(clientInitModulePath,ClientInitModuleFileName);

        // First start the rpyc server
        CallStartServer(clientInitModulePath);

        // Then bootstrap Shotgun on the client
        string serverCode = ImportServerString + string.Format(@"unity_server.call_remote_service('bootstrap_shotgun','{0}')",bootstrapScript);
        PythonRunner.RunString(serverCode);
    }

    /// <summary>
    /// Bootstraps Shotgun when the domain is loaded
    /// </summary>
    [InitializeOnLoadMethod]
    public static void OnReload()
    {
        CallBootstrap();
    }

    /// <summary>
    /// Stops the Unity server on Python
    /// </summary>
    public static void OnPythonShutdown()
    {
        CallStopServer();
        PythonEngine.RemoveShutdownHandler(OnPythonShutdown);
    }

#if DEBUG
    [MenuItem("Shotgun/Debug/Bootstrap Engine")]
    public static void CallBootstrapEngine()
    {
        CallBootstrap();
    }

    [MenuItem("Shotgun/Debug/Print Engine Envs")]
    public static void CallPrintEnv()
    {
        string[] envs = { "SHOTGUN_UNITY_BOOTSTRAP_LOCATION","BOOTSTRAP_SG_ON_UNITY_STARTUP",};

        foreach(string env in envs)
        {
            UnityEngine.Debug.Log(env + ": " + System.Environment.GetEnvironmentVariable(env));
        }
    }
#endif
}

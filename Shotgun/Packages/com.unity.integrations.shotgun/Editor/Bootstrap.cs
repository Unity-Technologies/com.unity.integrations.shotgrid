//#define DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;
using System.IO;
using Python.Runtime;

namespace UnityEditor.Integrations.Shotgun
{
    public static class Bootstrap
    {
        private const string ClientInitModuleFileName = "sg_client_init.py";

        /// <summary>
        /// Starts the Unity server, then bootstraps Shotgun on the Unity client
        /// </summary>
        public static void CallBootstrap()
        {
            // Use the engine's rpyc client script
            string bootstrapScript = System.Environment.GetEnvironmentVariable("SHOTGUN_UNITY_BOOTSTRAP_LOCATION");
            
            if (bootstrapScript == null)
            {
                // Unity was not lauched from Shotgun. Exit early
                return;
            }

            string clientInitModulePath = Path.GetDirectoryName(bootstrapScript);
            clientInitModulePath = Path.Combine(clientInitModulePath,ClientInitModuleFileName);

            bootstrapScript      = bootstrapScript.Replace(@"\","/");
            clientInitModulePath = clientInitModulePath.Replace(@"\","/");

            // First start the rpyc server
            PythonRunner.StartServer(clientInitModulePath);

            // We need to stop the server on Python shutdown 
            // (which is triggered by domain unload)
            PythonEngine.AddShutdownHandler(OnPythonShutdown);

            // We need to clean-up on quit
            EditorApplication.quitting += DeleteShotgunAssetDir;

            // Then bootstrap Shotgun on the client
            PythonRunner.CallServiceOnClient("'bootstrap_shotgun'", string.Format("'{0}'", bootstrapScript));
        }

        /// <summary>
        /// Bootstraps Shotgun when the domain is loaded
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnReload()
        {
            CallBootstrap();
        }

        /// <summary>
        /// Stops the Unity server on Python
        /// </summary>
        private static void OnPythonShutdown()
        {
            PythonRunner.StopServer();
            PythonEngine.RemoveShutdownHandler(OnPythonShutdown);
        }

        /// <summary>
        /// Tries to remove Assets/Shotgun
        /// </summary>
        private static void DeleteShotgunAssetDir()
        {
            string shotgunAssetPath = UnityEngine.Application.dataPath + "/Shotgun";
            if (Directory.Exists(shotgunAssetPath))
            {
                try
                {
                    Directory.Delete(shotgunAssetPath, true);
                }
                catch (IOException ex)
                {
                    Debug.LogWarning(string.Format("Could not delete the Shotgun Asset Directory located at {}",shotgunAssetPath));
                }
            }
        }

#if DEBUG
        [MenuItem("Shotgun/Debug/Bootstrap Engine")]
        private static void CallBootstrapEngine()
        {
            // Stop the server (and client)
            PythonRunner.StopServer(true);

            CallBootstrap();
        }

        [MenuItem("Shotgun/Debug/Print Engine Envs")]
        private static void CallPrintEnv()
        {
            string[] envs = { "SHOTGUN_UNITY_BOOTSTRAP_LOCATION","BOOTSTRAP_SG_ON_UNITY_STARTUP",};

            foreach(string env in envs)
            {
                UnityEngine.Debug.Log(env + ": " + System.Environment.GetEnvironmentVariable(env));
            }
        }
#endif
    }
}
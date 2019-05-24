 #define DEBUG_SHOTGUN

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
            if (!EnsureShotgunIsPresent())
            {
                return;
            }

            // Use the engine's rpyc client script
            string bootstrapScript = System.Environment.GetEnvironmentVariable("SHOTGUN_UNITY_BOOTSTRAP_LOCATION");

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
        /// Checks if Unity was launched from Shotgun. If not, issues a 
        /// warning and removes the Assets/Shotgun directory
        /// 
        /// Returns true if Shotgun is present, false otherwise
        /// </summary>
        internal static bool EnsureShotgunIsPresent()
        {
            if (System.Environment.GetEnvironmentVariable("SHOTGUN_UNITY_BOOTSTRAP_LOCATION") == null)
            {
                // Unity was not lauched from Shotgun. Log warning and exit early
                Debug.LogWarning("The Shotgun package is present in the project but Unity was not launched from Shotgun. Shotgun features will not be available.");

                // Remove the Shotgun menu
                DeleteShotgunAssetDir();
                AssetDatabase.Refresh();
                return false;
            }
            return true;
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
                catch (IOException)
                {
                    Debug.LogWarning(string.Format("Could not delete the Shotgun Asset Directory located at {0}",shotgunAssetPath));
                }
            }
        }

#if DEBUG_SHOTGUN
        [MenuItem("Shotgun/Debug/Bootstrap Engine")]
        private static void CallBootstrapEngine()
        {
            // Stop the server (and client)
            PythonRunner.StopServer(true);
            CallBootstrap();
        }
#endif
    }
}
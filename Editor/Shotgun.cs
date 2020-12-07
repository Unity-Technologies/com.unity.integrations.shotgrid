﻿using Python.Runtime;
using System;
using System.IO;
using UnityEditor.Scripting.Python;

namespace UnityEditor.Integrations.Shotgun
{
    /// <summary>
    /// This class provides constants that are used throughout the Shotgun 
    /// integration.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The name of the client.
        /// </summary>
        public const string clientName = "com.unity.integrations.shotgun";

        /// <summary>
        /// The name of the package.
        /// </summary>
        public const string packageName = clientName;

        /// <summary>
        /// The shotgun client module filename.
        /// </summary>
        public const string shotgunClientModule = "sg_client.py";
    }

    /// <summary>
    /// Manages initialization and termination of the Shotgun integration.
    /// Also manages the Shotgun Python client lifecycle.
    /// </summary>
    public static class Bootstrap
    {
        // Will spawn the default client
        private static void SpawnClient()
        {
            if(!VerifyLaunchedFromShotgun())
            {
                return;
            }

            // Use the default client
            string bootstrapScript = System.Environment.GetEnvironmentVariable("SHOTGUN_UNITY_BOOTSTRAP_LOCATION");
            bootstrapScript      = bootstrapScript.Replace(@"\","/");

            string clientPath = Path.GetDirectoryName(bootstrapScript);

            // add path to 'client' to sys path
            PythonRunner.EnsureInProcessInitialized();
            using (Py.GIL())
            {
                // Get the builtin module, which is 'builtins' on Python3 and __builtin__ on Python2
                dynamic builtins = PythonEngine.ImportModule("builtins");
                // prepend to sys.path
                dynamic sys = PythonEngine.ImportModule("sys");
                dynamic syspath = sys.GetAttr("path");
                dynamic pySitePackages = builtins.list();
                pySitePackages.append(clientPath);
                //pySitePackages.append("C:\\Users\\Viktoria\\AppData\\Roaming\\Shotgun\\unity-dev\\p86c523.basic.desktop\\cfg\\install\\core\\python");
                pySitePackages += syspath;
                sys.SetAttr("path", pySitePackages);
            }

            /*PythonRunner.EnsureInProcessInitialized();
            using (Py.GIL())
            {
                dynamic sg_client = PythonEngine.ImportModule("sg_client");
                sg_client.bootstrap_shotgun();
            }*/

            clientPath = Path.Combine(clientPath, Constants.shotgunClientModule);
            PythonRunner.RunFile(clientPath, "__main__");
        }

        /*public static void CallQtLoop()
        {
            PythonRunner.EnsureInProcessInitialized();
            using (Py.GIL())
            {
                try
                {
                    dynamic sg_client = PythonEngine.ImportModule("sg_client");
                    sg_client.main_loop();
                }
                catch (PythonException e)
                {
                    UnityEngine.Debug.LogError("Got exception while invoking the Qt loop: " + e);
                }
            }
        }*/

        /// <summary>
        /// Called from the client.
        /// Tells Unity that the tk-unity engine has been successfully 
        /// initialized. Will initiate the post_init hook logic.
        /// </summary>
        static public void OnEngineInitialized()
        {
            // Install a delay call to return right away.
            // Otherwise calling AssetDatabase.Refresh() would break the 
            // connection and possibly lock the client
            EditorApplication.delayCall += DoOnEngineInitialized;
        }

        internal static void DoOnEngineInitialized()
        {
            CallPostInitHook();

            //UnityEditor.EditorApplication.update += CallQtLoop;

            // Now that toolkit has bootstrapped, we can validate that the 
            // package and engine versions are compatible (matching Major and 
            // Minor version numbers)
            string tkUnityVersionString = "";//PythonRunner.CallServiceOnClient(Constants.clientName, "tk_unity_version");
            string packageVersionString = PackageManager.PackageInfo.FindForAssetPath($"Packages/{Constants.packageName}/Editor/Shotgun.cs").version;

            PythonRunner.EnsureInProcessInitialized();
            using (Py.GIL())
            {
                dynamic sg_client = PythonEngine.ImportModule("sg_client");
                tkUnityVersionString = sg_client.test_tk_unity_version();
            }
            UnityEngine.Debug.Log("tk unity version: " + tkUnityVersionString);

            // Strip the leading "v" in the tk-unity version string. 
            // tk-unity version numbers have this form: "vX.Y". We want to 
            // extract "X.Y"
            var index = tkUnityVersionString.IndexOf("v");
            if (index != -1 && index < (tkUnityVersionString.Length-1))
            {
                tkUnityVersionString = tkUnityVersionString.Substring(index+1);
            }
            
            // Remove everything after "preview" in the package string
            // Version numbers have this form: "X.Y.Z[-preview][.W]", 
            // e.g "0.9.0-preview.1", "1.0.1-preview", "2.0.3".
            // We want to extract "X.Y.Z"
            index = packageVersionString.IndexOf("preview");
            if (index > 0)
            {
                packageVersionString = packageVersionString.Substring(0, index-1);
            }
    
            System.Version tkUnityVersion = null;
            System.Version packageVersion = null;

            try 
            {
                tkUnityVersion = new System.Version(tkUnityVersionString);
            } 
            catch (Exception)
            {
                UnityEngine.Debug.LogWarning($"Cannot determine the version number for tk-unity ({tkUnityVersionString}). Some Shotgun features might not function properly");
            }

            try 
            {
                packageVersion = new System.Version(packageVersionString);
            } 
            catch (Exception)
            {
                UnityEngine.Debug.LogWarning($"Cannot determine the version number for {Constants.packageName} ({packageVersionString}). Some Shotgun features might not function properly");
            }

            if (tkUnityVersion != null && packageVersion != null)
            { 
                // We were able to parse the version numbers. Now compare 
                // them to make sure they are compatible
                if (tkUnityVersion.Major != packageVersion.Major || 
                    tkUnityVersion.Minor != packageVersion.Minor)
                {
                    UnityEngine.Debug.LogWarning($"The tk-unity engine version ({tkUnityVersionString}) is not compatible with the Shotgun package version ({packageVersionString}). Some Shotgun features might not function properly");
                }
            }
        }

        internal static void CallPostInitHook()
        {
            // Start by refreshing the Asset Database so Unity catches the 
            // Shotgun menu items that were generated while bootstraping
            AssetDatabase.Refresh();

            PythonRunner.EnsureInProcessInitialized();
            using (Py.GIL())
            {
                dynamic sg_client = PythonEngine.ImportModule("sg_client");
                sg_client.test_invoke_post_init_hook();
            }
        }

        /// <summary>
        /// Checks if Unity was launched from Shotgun. If not, issues a 
        /// warning and removes the Assets/Shotgun directory
        /// 
        /// Returns true if Shotgun is present, false otherwise
        /// </summary>
        internal static bool VerifyLaunchedFromShotgun()
        {
            if (System.Environment.GetEnvironmentVariable("SHOTGUN_UNITY_BOOTSTRAP_LOCATION") == null)
            {
                // Unity was not lauched from Shotgun. Log warning and exit early
                UnityEngine.Debug.LogWarning("The Shotgun package is present in the project but Unity was not launched from Shotgun. Shotgun features will not be available.");

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
            // This prevents multiple attempts at spawning the client. There 
            // are several domain reloads on editor startup. Using delayCall 
            // will make sure we only spawn the client once all the domain 
            // reloads are completed.
            EditorApplication.delayCall += SpawnClient;
            
            // Install our clean-up callback
            EditorApplication.quitting += DeleteShotgunAssetDir;
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
                    UnityEngine.Debug.LogWarning(string.Format("Could not delete the Shotgun Asset Directory located at {0}",shotgunAssetPath));
                }
            }
        }
    }

    /// <summary>
    /// This class allows calling services on the Shotgun client.
    /// </summary>
    public static class Service
    {
        /// <summary>
        /// Calls a service on the Shotgun client.
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        /// <param name="args">Arguments to pass to the service</param>
        public static void Call(string serviceName, string menuItem)
        {
            PythonRunner.EnsureInProcessInitialized();
            using (Py.GIL())
            {
                dynamic sg_client = PythonEngine.ImportModule("sg_client");
                sg_client.test_execute_menu_item(menuItem);
            }

        }
    }
}
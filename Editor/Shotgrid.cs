using Python.Runtime;
using System;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.Scripting.Python;

namespace UnityEditor.Integrations.Shotgrid
{
    /// <summary>
    /// This class provides constants that are used throughout the Shotgrid
    /// integration.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The name of the package.
        /// </summary>
        public const string packageName = "com.unity.integrations.shotgrid";
        /// <summary>
        /// The shotgrid bootstrap module filename.
        /// </summary>
        public const string shotgridBootstrapModule = "sg_bootstrap.py";
    }

    /// <summary>
    /// Manages initialization and termination of the Shotgrid integration.
    /// Also manages the Shotgrid Python bootstrap lifecycle.
    /// </summary>
    public static class Bootstrap
    {
        // Will start the bootstrap process
        private static void DoBootstrap()
        {
            if(!VerifyLaunchedFromShotgrid())
            {
                return;
            }

            // Use the default bootstrap
            string bootstrapScript = System.Environment.GetEnvironmentVariable("SHOTGRID_UNITY_BOOTSTRAP_LOCATION");
            bootstrapScript      = bootstrapScript.Replace(@"\","/");
            
            string bootstrapPath = Path.GetDirectoryName(bootstrapScript);
            // Get PySide2 from the same place as Shotgrid Desktop.
            // If Python for Unity starts to ship with its own PySide2 then we should switch to using the built-in version.
            string pysideLocation = System.Environment.GetEnvironmentVariable("SHOTGRID_UNITY_PYSIDE_LOCATION");
            pysideLocation= pysideLocation.Replace(@"\","/");
            // add path to 'bootstrap' to sys path
            PythonRunner.EnsureInitialized();
            using (Py.GIL())
            {
                dynamic builtins = PythonEngine.ImportModule("builtins");
                // prepend to sys.path
                dynamic sys = PythonEngine.ImportModule("sys");
                dynamic syspath = sys.GetAttr("path");
                dynamic pySitePackages = builtins.list();
                pySitePackages.append(bootstrapPath);
                pySitePackages.append(pysideLocation);
                pySitePackages += syspath;
                sys.SetAttr("path", pySitePackages);
            }


            bootstrapPath = Path.Combine(bootstrapPath, Constants.shotgridBootstrapModule);
            PythonRunner.RunFile(bootstrapPath, "__main__");
            //Subscribe to Package Manager API and remove Shotgrid Asset when SG package is uninstalled
            //Packman API implemented for unity v2020.3 and higher
#if UNITY_2020_3_OR_NEWER 
            UnityEditor.PackageManager.Events.registeringPackages += (PackageRegistrationEventArgs args) => {
                foreach ( var info in args.removed )
                {
                    if(info.assetPath == "Packages/com.unity.integrations.shotgrid")
                    {
                        DeleteShotgridAssetDir();
                    }
                }
            };
#endif
        }


        /// <summary>
        /// Called from SG bootstrap.
        /// Tells Unity that the tk-unity engine has been successfully 
        /// initialized. Will initiate the post_init hook logic.
        /// </summary>
        static public void OnEngineInitialized()
        {
            // Install a delay call to return right away.
            // Otherwise calling AssetDatabase.Refresh() would break the 
            // connection and possibly lock bootstrapping
            EditorApplication.delayCall += DoOnEngineInitialized;
        }

        internal static void DoOnEngineInitialized()
        {
            CallPostInitHook();
            // Now that toolkit has bootstrapped, we can validate that the 
            // package and engine versions are compatible (matching Major and 
            // Minor version numbers)
            string tkUnityVersionString = "";
            string packageVersionString = PackageManager.PackageInfo.FindForAssetPath($"Packages/{Constants.packageName}/Editor/Shotgrid.cs").version;

            PythonRunner.EnsureInitialized();
            using (Py.GIL())
            {
                dynamic sg_bootstrap = PythonEngine.ImportModule("sg_bootstrap");
                tkUnityVersionString = sg_bootstrap.tk_unity_version();
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
            // Version numbers have this form: "X.Y.Z[-exp][.W]", 
            // e.g "0.9.0-exp.1", "1.0.1-exp", "2.0.3".
            // We want to extract "X.Y.Z"
            index = packageVersionString.IndexOf("exp");
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
                UnityEngine.Debug.LogWarning($"Cannot determine the version number for tk-unity ({tkUnityVersionString}). Some Shotgrid features might not function properly");
            }

            try 
            {
                packageVersion = new System.Version(packageVersionString);
            } 
            catch (Exception)
            {
                UnityEngine.Debug.LogWarning($"Cannot determine the version number for {Constants.packageName} ({packageVersionString}). Some Shotgrid features might not function properly");
            }

            if (tkUnityVersion != null && packageVersion != null)
            { 
                // We were able to parse the version numbers. Now compare 
                // them to make sure they are compatible
                if (tkUnityVersion.Major != packageVersion.Major || 
                    tkUnityVersion.Minor != packageVersion.Minor)
                {
                    UnityEngine.Debug.LogWarning($"The tk-unity engine version ({tkUnityVersionString}) is not compatible with the Shotgrid package version ({packageVersionString}). Some Shotgrid features might not function properly");
                }
            }
        }

        internal static void CallPostInitHook()
        {
            // Start by refreshing the Asset Database so Unity catches the 
            // Shotgrid menu items that were generated while bootstraping
            AssetDatabase.Refresh();

            PythonRunner.EnsureInitialized();
            using (Py.GIL())
            {
                dynamic sg_bootstrap = PythonEngine.ImportModule("sg_bootstrap");
                sg_bootstrap.invoke_post_init_hook();
            }
        }

        /// <summary>
        /// Checks if Unity was launched from Shotgrid. If not, issues a 
        /// warning and removes the Assets/Shotgrid directory
        /// 
        /// Returns true if Shotgrid is present, false otherwise
        /// </summary>
        internal static bool VerifyLaunchedFromShotgrid()
        {
            if (System.Environment.GetEnvironmentVariable("SHOTGRID_UNITY_BOOTSTRAP_LOCATION") == null)
            {
                // Unity was not lauched from Shotgrid. Log warning and exit early
                UnityEngine.Debug.LogWarning("The Shotgrid package is present in the project but Unity was not launched from Shotgrid. Shotgrid features will not be available.");

                // Remove the Shotgrid menu
                DeleteShotgridAssetDir();
                AssetDatabase.Refresh();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Bootstraps Shotgrid when the domain is loaded
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnReload()
        {
            // This prevents multiple attempts at bootstrapping. There 
            // are several domain reloads on editor startup. Using delayCall 
            // will make sure we only bootstrap once all the domain 
            // reloads are completed.
            EditorApplication.delayCall += DoBootstrap;
            
            // Install our clean-up callback
            EditorApplication.quitting += DeleteShotgridAssetDir;
        }

        /// <summary>
        /// Tries to remove Assets/Shotgrid
        /// </summary>
        private static void DeleteShotgridAssetDir()
        {
            string shotgridAssetPath = UnityEngine.Application.dataPath + "/Shotgrid";
            string shotgridAssetMetaPath = UnityEngine.Application.dataPath + "/Shotgrid.meta";
            if (Directory.Exists(shotgridAssetPath))
            {
                try
                {
                    Directory.Delete(shotgridAssetPath, true);
                    File.Delete(shotgridAssetMetaPath);
                }
                catch (IOException)
                {
                    UnityEngine.Debug.LogWarning(string.Format("Could not delete the Shotgrid Asset Directory located at {0}",shotgridAssetPath));
                }
            }
        }
    }

}
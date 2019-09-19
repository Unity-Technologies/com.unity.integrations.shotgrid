using Python.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
        /// The name of the client
        /// </summary>
        public const string clientName = "com.unity.integrations.shotgun";

        /// <summary>
        /// The name of the package
        /// </summary>
        public const string packageName = clientName;

        /// <summary>
        /// The shotgun client module filename
        /// </summary>
        public const string shotgunClientModule = "sg_client.py";
    }

    /// <summary>
    /// Manages initialization and termination of the Shotgun integration.
    /// Also manages the Shotgun Python client lifecycle.
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// Spawns a Shotgun client. This method needs to be called 
        /// from a Unity Editor which was launched from Shotgun.
        /// </summary>
        /// <param name="clientPath">The client Python module to use as the 
        /// Shotgun client</param>
        public static void SpawnClient(string clientPath = null)
        {
            if(!VerifyLaunchedFromShotgun())
                return;

            if (Client.IsAlive == true)
                return;

            string bootstrapScript = System.Environment.GetEnvironmentVariable("SHOTGUN_UNITY_BOOTSTRAP_LOCATION");
            bootstrapScript      = bootstrapScript.Replace(@"\","/");

            if (string.IsNullOrEmpty(clientPath))
            {
                // Use the default client
                clientPath = Path.GetDirectoryName(bootstrapScript);
                clientPath = Path.Combine(clientPath, Constants.shotgunClientModule);
            }

            // Spawn the client
            dynamic pOpenObject = PythonRunner.SpawnClient(clientPath);

            using (Py.GIL())
            {
                Client.PID = pOpenObject.pid;
            }

            // Remember we spawned a client
            Client.IsAlive = true;
        }

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
            EditorApplication.delayCall += CallPostInitHook;
        }
        
        internal static void CallPostInitHook()
        {
            // Start by refreshing the Asset Database so Unity catches the 
            // Shotgun menu items that were generated while bootstraping
            AssetDatabase.Refresh();

            // Call the hook
            Service.Call("invoke_post_init_hook");
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
            SpawnClient();
            
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
    
    
    internal static class Client
    {
        // We use the SessionState class to persist the Shotgun client PID
        private static string shotgunClientPIDStateKey = "shotgun_client_pid";
        
        // Returns the client PID, or -1 if there is no client
        internal static int PID
        {
            get
            {
                return SessionState.GetInt(shotgunClientPIDStateKey, -1);
            }
            set
            {
                SessionState.SetInt(shotgunClientPIDStateKey, value);
            }
        }

        // The IsAlive property tells whether the client process is 
        // still running
        internal static bool IsAlive
        {
            get
            {
                // Client does not exist if the session state variable does 
                // not exist
                if (PID == -1)
                {
                    UnityEngine.Debug.LogWarning("The Shotgun client is not running. Please reimport the Shotgun package");
                    return false;
                }
                
                // Check if the process exists
                try
                {
                    Process process = Process.GetProcessById(PID);

                    // Crashed/closed processes can still exist. Calling 
                    // WaitForExit will return true on such processes
                    try
                    {
                        if (process.WaitForExit(0) == false)
                        {
                            // The process is still running
                            return true;
                        }
                    }
                    catch (SystemException)
                    {
                        IsAlive = false;
                    }
                }
                catch (ArgumentException)
                {
                    // The process is not running
                    IsAlive = false;
                }

                // The process does not exist anymore
                UnityEngine.Debug.LogWarning("The Shotgun client is not running. Please reimport the Shotgun package");
                return false;
            }
            set
            {
                // The Client PID session state variable is set right after 
                // we spawn the client. The property setter only needs to 
                // erase the variable when the client disappears
                if (!value)
                {
                    Client.PID = -1;
                }
            }
        }

        // Returns whether the client is connected or not.
        // If the client is not connected but is alive (see IsAlive), we assume
        // it is reconnecting and the method will wait (blocking the main thread)
        // for a certain time until the client reconnects. 
        internal static bool EnsureConnection()
        {
            if (!IsAlive)
            {
                return false;
            }

            // The client is alive. Give it some time to reconnect if it 
            // is not
            var iter = PythonRunner.WaitForConnection(Constants.clientName, 5);

            bool moving = true;
            while(moving)
            {
                using (Py.GIL())
                {
                    moving = iter.MoveNext();
                }
                Thread.Sleep(20);
            }

            if (PythonRunner.IsClientConnected(Constants.clientName))
            {
                return true;
            }

            // The client never reconnected
            UnityEngine.Debug.LogWarning("The Shotgun client process is not connected. Please reimport the Shotgun package");

            // Client is not alive, update its property
            IsAlive = false;
            return false;
        }
    }

    /// <summary>
    /// This class allows calling services on the Shotgun client.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Calls a service on the Shotgun client.
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        /// <param name="args">Arguments to pass to the service</param>
        /// <returns>The awaited task</returns>
        public static void Call(string serviceName, params object[] args)
        {
            // Do our best to get a valid client
            if (!Client.EnsureConnection())
            {
                return;
            }

            // Then call the service
            _ = PythonRunner.CallAsyncServiceOnClient(Constants.clientName, serviceName, args);
        }
    }
}
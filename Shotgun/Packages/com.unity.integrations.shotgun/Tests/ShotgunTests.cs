using NUnit.Framework;
using System;
using System.IO;
using System.Collections;
using UnityEditor;
using UnityEditor.Integrations.Shotgun;
using UnityEditor.Scripting.Python;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    internal class ShotgunTests
    {
        private static string testsPath = Path.Combine(Path.GetFullPath($"Packages/{Constants.packageName}"), "Tests");
        private static string clientPath = Path.Combine(testsPath, "shotgun_test_client.py");

        [UnityTest]
        public IEnumerator StandalonePublish()
        {
            // Restart the server, making sure there is no Shotgun client
            PythonRunner.StopServer(false);

            // Bootstrap Shotgun with our test client
            Bootstrap.SpawnClient(clientPath);

            // Give some time for the client to connect
            yield return PythonRunner.WaitForConnection(Constants.clientName);

            // Wait until the client is fully bootstrapped
            double initTime = EditorApplication.timeSinceStartup;
            double timeout = 30;
            while (!PythonRunner.CallServiceOnClient(Constants.clientName, "bootstrapped"))
            {
                if (EditorApplication.timeSinceStartup - initTime > timeout)
                {
                    break;
                }
                yield return null;
            }

            // Fake a recording by copying a video file in the right location
            string videoFilePath = Path.Combine(testsPath, "standalone_publish.mp4");
            var destinationPath = Path.Combine(System.IO.Path.GetTempPath(), Application.productName);
            destinationPath += ".mp4";

            System.IO.File.Copy(videoFilePath, destinationPath, true);

            Service.Call("standalone_publish");

            // Wait until the client has finished publishing
            initTime = EditorApplication.timeSinceStartup;
            timeout = 30;
            while (!PythonRunner.CallServiceOnClient(Constants.clientName, "published"))
            {
                if (EditorApplication.timeSinceStartup - initTime > timeout)
                {
                    break;
                }
                yield return null;
            }

            // Upon success, the Python script creates a game object named 
            // after the product name
            GameObject go = GameObject.Find(Application.productName);
            Assert.IsNotNull(go);
        }
        private IEnumerator Wait(int milliseconds)
        {
            // Give some time for the client to process the remote call,
            // but also give back the control to Unity so it can invoke the
            // Python interpreter (Python threads do not run if the interpreter 
            // does not run periodically
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            TimeSpan duration = end-start;
            while (duration.TotalMilliseconds < milliseconds)
            {
                yield return null;
                end = DateTime.Now;
                duration = end-start;
            }
        }
    }
}

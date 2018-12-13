using NUnit.Framework;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Scripting.Python;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ShotgunTests
    {
        private static string TestsPath = Path.Combine(Path.GetFullPath("Packages/com.unity.integrations.shotgun"), "Tests");

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator StandalonePublish()
        {
            // Bootstrap Shotgun
            Bootstrap.CallBootstrap();

            // Fake a recording by copying a video file in the right location
            string videoFilePath = Path.Combine(TestsPath, "standalone_publish.mp4");
            var destinationPath = Path.Combine(System.IO.Path.GetTempPath(), Application.productName);
            destinationPath += ".mp4";

            System.IO.File.Copy(videoFilePath, destinationPath, true);

            // Run the test script on client
            string standaloneScript = Path.Combine(TestsPath, "standalone_publish.py");
            PythonRunner.RunFileOnClient(standaloneScript);

            // Give the script some time to publish
            yield return Wait(5000);

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

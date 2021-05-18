using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Integrations.Shotgun;
using UnityEditor.Scripting.Python;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    internal class ShotgunTests
    {
        private const string testsPath = "Packages/" + Constants.packageName + "/Tests";
        private const string clientPath = testsPath + "/shotgun_test_client.py";
        
        // In seconds
        private const double connectionTimeout = 20;
        private const double shutdownTimeout = 5;
        private const double bootstrapTimeout = 100;
        private const double publishTimeout = 100;

        [Test, Explicit]
        public void StandalonePublish()
        {
            

            // Fake a recording by copying a video file in the right location
            string videoFilePath = Path.GetFullPath($"{testsPath}/standalone_publish.mp4");
            var destinationPath = Path.Combine(System.IO.Path.GetTempPath(), Application.productName);
            destinationPath += ".mp4";

            System.IO.File.Copy(videoFilePath, destinationPath, true);
            string stand_alone_path = Path.GetFullPath($"{testsPath}/standalone_publish.py");
            Assert.True(File.Exists(stand_alone_path));
            PythonRunner.RunFile(stand_alone_path);
            // Upon success, the Python script creates a game object named 
            // after the product name
            GameObject go = GameObject.Find(Application.productName);
            Assert.IsNotNull(go);
        }

        [Test]
        public void QaReportTest ()
        {
            var qaReportFile = Path.GetFullPath("Packages/com.unity.integrations.shotgun/QAReport.md");
            var changelogFile = Path.GetFullPath("Packages/com.unity.integrations.shotgun/CHANGELOG.md");
            var versionRegex = @"\[\d+.\d+.\d+(\-preview(\.\d{1,3})?)?\]";
            Assert.True(File.Exists(qaReportFile));
            using (StreamReader qaReport = new StreamReader(qaReportFile), 
                                changelog = new StreamReader(changelogFile))
            {
                var qaContents = qaReport.ReadToEnd();
                var qaVersion = Regex.Match(qaContents, versionRegex).ToString();

                var changelogContents = changelog.ReadToEnd();
                var changelogVersion = Regex.Match(changelogContents, versionRegex).ToString();
                Assert.That(qaVersion, Is.EqualTo(changelogVersion));
                
            }
        }
    }
}

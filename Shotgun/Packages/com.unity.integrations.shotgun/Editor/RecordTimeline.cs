﻿using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using System.Reflection;

public class RecordTimeline
{
    private static string m_origFilePath = null;
    private static MovieRecorderSettings m_recorderSettings = null;

    /// <summary>
    /// We must install the delegate on each domain reload
    /// </summary>
    [InitializeOnLoadMethod]
    private static void OnReload()
    {
        if (IsRecording)
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }
    }

    [MenuItem("Shotgun/Record Timeline")]
    private static void Record()
    {
        IsRecording = true;

        m_origFilePath = RecorderPath;

        RecorderPath = GetTempFilePath();

        EditorApplication.playModeStateChanged += OnPlayModeStateChange;

        StartRecording();
    }

    private static string lockFilePath = GetTempFilePath() + ".RecordTimeline.lock";
    private static bool IsRecording
    {
        get 
        {
            return System.IO.File.Exists(lockFilePath);
        }
        set
        {
            if (value == true)
            {
                System.IO.File.Create(lockFilePath);
            }
            else
            {
                if (System.IO.File.Exists(lockFilePath))
                {
                    System.IO.File.Delete(lockFilePath);
                }
            }
        }
    }

    private static string GetTempFilePath()
    {
        // store to a temporary path, to delete after publish
        var tempPath = System.IO.Path.GetTempPath();

        // TODO: what should the name of the video file be?
        tempPath = System.IO.Path.Combine(tempPath, Application.productName);

        return tempPath;
    }

    private static void OnPlayModeStateChange(PlayModeStateChange state)
    {
        if (IsRecording)
        {
            // Domain reloads lose the overriden Recorder path. We know a 
            // domain reload occurred if m_origFilePath is not set (cleared 
            // by a domain reload)
            if (null == m_origFilePath)
            {
                m_origFilePath = RecorderPath;
                RecorderPath = GetTempFilePath();
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.ExecuteMenuItem("Shotgun/Publish...");

                EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
                RecorderPath = m_origFilePath;
                IsRecording = false;
            }
        }
    }

    private static object GetFieldValue(string fieldName, object from)
    {
        FieldInfo fi = from.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return fi.GetValue(from);
    }

    private static object GetPropertyValue(string propName, object from, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)
    {
        PropertyInfo propInfo = from.GetType().GetProperty(propName, bindingFlags);
        return propInfo.GetValue(from);
    }

    private static MovieRecorderSettings RecorderSettings
    {
        get
        {
            if (m_recorderSettings == null)
            {
                m_recorderSettings = GetRecorder();
                if (m_recorderSettings == null)
                {
                    Debug.LogError("Could not find a valid MovieRecorder");
                }
            }
            return m_recorderSettings;
        }
    }

    private static string RecorderPath {
        get { return RecorderSettings.outputFile; }
        set
        {
            var filenameGenerator = GetFieldValue("fileNameGenerator", RecorderSettings);
            MethodInfo mi = filenameGenerator.GetType().GetMethod("FromPath", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(filenameGenerator, new object[] { value });
        }
    }

    private static void StartRecording()
    {
        var recorderWindow = EditorWindow.GetWindow<RecorderWindow>();
        if (!recorderWindow)
        {
            return;
        }
        // start recording
        recorderWindow.StartRecording();
    }

    private static MovieRecorderSettings GetRecorder()
    {
        var recorderWindow = EditorWindow.GetWindow<RecorderWindow>();
        if (!recorderWindow)
        {
            return null;
        }

        // first try to get the selected item, if it's not a MovieRecorder,
        // then go through the list and try to find one that is called "Shotgun".
        // if there isn't one then just take one of the MovieRecorders.
        var selectedRecorder = GetFieldValue("m_SelectedRecorderItem", recorderWindow);
        if (selectedRecorder != null)
        {
            RecorderSettings recorderSettings = GetPropertyValue("settings", selectedRecorder, BindingFlags.Public | BindingFlags.Instance) as RecorderSettings;
            if (recorderSettings.GetType().Equals(typeof(MovieRecorderSettings)))
            {
                // found movie recorder settings
                return recorderSettings as MovieRecorderSettings;
            }
        }
        
        var recorderList = GetFieldValue("m_RecordingListItem", recorderWindow);
        var itemList = (IEnumerable)GetPropertyValue("items", recorderList, BindingFlags.Public | BindingFlags.Instance);
        MovieRecorderSettings movieRecorder = null;
        foreach (var item in itemList)
        {
            RecorderSettings settings = GetPropertyValue("settings", item, BindingFlags.Public | BindingFlags.Instance) as RecorderSettings;
            var recorder = settings as MovieRecorderSettings;
            if(recorder == null)
            {
                continue;
            }
            movieRecorder = recorder;

            var editableLabel = GetFieldValue("m_EditableLabel", item);
            var labelText = (string)GetPropertyValue("text", editableLabel);
            if (labelText.Equals("Shotgun"))
            {
                return movieRecorder;
            }
        }
        return movieRecorder;
    }
}
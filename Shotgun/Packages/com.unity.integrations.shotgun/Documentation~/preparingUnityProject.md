# Preparing a Unity project for Shotgun

You will need to add the Shotgun package to your Unity project’s Packages/manifest.json file, like so:
```
{
  "dependencies": {
    "com.unity.integrations.shotgun": "0.1.0-preview"
  },
  "registry": "https://staging-packages.unity.com"
}
```

## Compatibility Matrix

Please use the Shotgun package version corresponding to your configuration version

| tk-config-unity version | com.unity.integrations.shotgun version |
| :---------------------- | :------------------------------------- |
| 1.1.15.1                | 0.1.0-preview                          |
   
Shotgun should automatically initialize on Unity startup. You should see Shotgun-related entries in the console (e.g. “[1] - Engine launched.”). If not use the Shotgun/Debug/Bootstrap Engine menu.

You can now use the Shotgun menu applications. Note that windows tend to pop up behind the Unity window. Also, it is recommended to disable Debug Logging for better performance (right-click in a gray area of Shotgun Desktop / Advanced / Toggle Debug Logging):

![Toggle Debug Logging](images/toggle_debug.png)
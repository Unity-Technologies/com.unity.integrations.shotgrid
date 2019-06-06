# Preparing a Unity project for Shotgun

You will need to add the Shotgun package to your Unity project's Packages/manifest.json file, like so:
```
{
  "dependencies": {
    "com.unity.integrations.shotgun": "0.3.0-preview",
	[...]
  },
  "registry": "https://staging-packages.unity.com"
}
```

## Compatibility Matrix

Please use the Shotgun package version corresponding to your configuration version

| tk-config-unity version | com.unity.integrations.shotgun version |
| :---------------------- | :------------------------------------- |
| 1.2.1.1                 | 0.3.0-preview                          |
| 1.1.15.2                | 0.2.0-preview                          |
| 1.1.15.1                | 0.1.0-preview                          |
   
Shotgun should automatically initialize on Unity startup. You should see a 
progress bar reporting the Shotgun toolkit bootstrap progress:

![Toolkit Progress Bar](images/toolkit_progress_bar.png)

Once the progress bar disappears, you can use the Shotgun menu applications. 

## OSX
It is possible that your system Python (`/usr/bin/python`) is not using a secure
version of TLS (lower than v1.2), which makes it incompatible with Shotgun, as described 
[here](https://support.shotgunsoftware.com/hc/en-us/articles/360009371913-Insecure-HTTPS-and-Old-Toolkit-Core-Deprecation-May-15th-2019).

It is recommended to set your Out-of-process Python to the one that ships with 
Shotgun Desktop. In order to do so, Open the Python Settings (Edit/Projects Settings/Python):

![Python Settings](images/python_settings.png)

Set the Out-of-process Python to the Shotgun Desktop Python, typically 
`/Applications/Shotgun.app/Contents/Frameworks/Python/bin/python`

You might need to kill Unity and the Out-of-process Python from a terminal:
```
killall Unity
killall python
```

Killing Python will require you to restart Shotgun Desktop

Restart Unity from Shotgun Desktop. Shotgun should bootstrap

Other Python installation might work, but could potentially:
* crash the Python client
* report a `ResponseNotReady` Python exception Error stack trace similar to this:

```
Traceback (most recent call last):
  File "/Users/david/Library/Caches/Shotgun/bundle_cache/sg/unity-dev/v1977/tk-unity/plugins/basic/bootstrap.py", line 40, in plugin_startup
    tk_unity_basic.plugin_bootstrap(plugin_root_path)
  File "/Users/david/Library/Caches/Shotgun/bundle_cache/sg/unity-dev/v1977/tk-unity/plugins/basic/tk_unity_basic/plugin_bootstrap.py", line 70, in plugin_bootstrap
    __launch_sgtk(base_config, plugin_id, bundle_cache)
  File "/Users/david/Library/Caches/Shotgun/bundle_cache/sg/unity-dev/v1977/tk-unity/plugins/basic/tk_unity_basic/plugin_bootstrap.py", line 139, in __launch_sgtk
    entity

  [...]

  File "/Users/david/Library/Caches/Shotgun/unity-dev/p86c79.basic.desktop/cfg/install/core/python/tank_vendor/shotgun_api3/lib/httplib2/__init__.py", line 1350, in _request
    (response, content) = self._conn_request(conn, request_uri, method, body, headers)
  File "/Users/david/Library/Caches/Shotgun/unity-dev/p86c79.basic.desktop/cfg/install/core/python/tank_vendor/shotgun_api3/lib/httplib2/__init__.py", line 1306, in _conn_request
    response = conn.getresponse()
  File "/System/Library/Frameworks/Python.framework/Versions/2.7/lib/python2.7/httplib.py", line 1119, in getresponse
    raise ResponseNotReady()
ResponseNotReady
```

# Unity 2019.1 and more recent versions
Starting with Unity 2019.1, launching Unity from Shotgun will bring up the Unity 
Hub (instead of the project selector). 

Version 2.0.x of the Unity Hub will keep running after Unity is launched, and 
will be brought back every time you launch Unity from Shotgun. This is 
problematic if you need to launch Unity from a different Shotgun context, as 
the Unity Hub will retain the first environment variables it was launched with
(the first Shotgun context).

It is important to quit the Unity Hub between launches of Unity.

**On Windows and CentOS, use the system tray icon:**

![quit_hub_win](images/quit_hub_win.png)
![quit_hub_lnx](images/quit_hub_lnx.png)

**On Mac, use the menu bar:**

![quit_hub_mac](images/quit_hub_mac.png)

This is a known problem and Unity will release a fix for it in an upcoming version
of the Unity Hub.

# Notes
A Shotgun folder will be created under your Unity project's 
Assets folder. This Shotgun folder will automatically be deleted when Unity 
exits. Do not use the `Assets/Shotgun` folder to store your files, and do not 
modify its content.

It is recommended to disable Debug Logging in order to get better performance 
(right-click in a 
gray area of Shotgun Desktop / Advanced / Toggle Debug Logging):

![Toggle Debug Logging](images/toggle_debug.png)


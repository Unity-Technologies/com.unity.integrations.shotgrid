# Preparing a Unity project for Shotgun
Using the Package Manager window, add the Shotgun package which matches your
pipeline configuration in the table below:

| tk-config-unity version | com.unity.integrations.shotgun version |
| :---------------------- | :------------------------------------- |
| 1.2.9.1                 | 0.9.0                          |
   
Shotgun should automatically initialize on Unity startup if you launched Unity 
from Shotgun. You should see a progress bar reporting the Shotgun toolkit 
bootstrap progress:

![Toolkit Progress Bar](images/toolkit_progress_bar.png)

Once the progress bar disappears, you can use the Shotgun menu items. 

## Unity 2019.1 and more recent versions
See the [Troubleshooting](troubleshooting.md#unity-20191-and-more-recent-versions) 
section on how to workaround Shotgun problems caused by the Unity Hub 2.0.

## Notes
A Shotgun folder will be created under your Unity project's 
Assets folder. This Shotgun folder will automatically be deleted when Unity 
exits. Do not use the `Assets/Shotgun` folder to store your files, and do not 
modify its content.

It is recommended to disable Debug Logging in order to get better performance 
(right-click in a 
gray area of Shotgun Desktop / Advanced / Toggle Debug Logging):

![Toggle Debug Logging](images/toggle_debug.png)


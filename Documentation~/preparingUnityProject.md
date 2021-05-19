# Preparing a Unity project for Shotgun
Using the Package Manager window, add the Shotgun package which matches your
pipeline configuration in the table below:

| tk-config-unity version | tk-unity version | com.unity.integrations.shotgun version |
| :---------------------- | :--------------- | :------------------------------------- |
| 1.2.9.1                 | 0.9              | 0.9.0                                  |
| 1.2.9.2                 | 0.10             | 0.10.0                                 |
   
Shotgun should automatically initialize on Unity startup if you launched Unity 
from Shotgun. You should see a progress bar reporting the Shotgun toolkit 
bootstrap progress:

![Toolkit Progress Bar](images/toolkit_progress_bar.png)

Once the progress bar disappears, you can use the Shotgun menu items. 

## Notes

### Launching A Project
Shotgun replaces the need to launch a project through the Unity hub.
Simply launch Shotgun desktop and click on your unity version. A file Selection
Directory should pop up. Navigate to your Unity project folder and select it. 
This should launch the project directly into your project Unity Editor. 

### The Shotgun folder 
A Shotgun folder will be created under your Unity project's 
Assets folder. This Shotgun folder will automatically be deleted when Unity 
exits. Do not use the `Assets/Shotgun` folder to store your files, and do not 
modify its content.

### Disable Debug Logging
It is recommended to disable Debug Logging in order to get better performance 
(right-click in a 
gray area of Shotgun Desktop / Advanced / Toggle Debug Logging):

![Toggle Debug Logging](images/toggle_debug.png)


# Preparing a Unity project for ShotGrid
Using the Package Manager window, add the ShotGrid package which matches your
pipeline configuration in the table below:

| tk-config-unity version | tk-unity version | com.unity.integrations.shotgrid version |
| :---------------------- | :--------------- | :------------------------------------- |
| 1.3.19.1                | 1.1              | 1.1.0
   
ShotGrid should automatically initialize on Unity startup if you launched Unity 
from ShotGrid. You should see a progress bar reporting the ShotGrid toolkit 
bootstrap progress:

![Toolkit Progress Bar](images/toolkit_progress_bar.png)

Once the progress bar disappears, you can use the ShotGrid menu items. 

## Notes

### Launching A Project
ShotGrid replaces the need to launch a project through the Unity hub.
Simply launch ShotGrid desktop and click on your unity version. A file Selection
Directory should pop up. Navigate to your Unity project folder and select it. 
This should launch the project directly into your project Unity Editor. 

### The ShotGrid folder 
A ShotGrid folder will be created under your Unity project's 
Assets folder. This ShotGrid folder will automatically be deleted when Unity 
exits. Do not use the `Assets/ShotGrid` folder to store your files, and do not 
modify its content.

### Disable Debug Logging
It is recommended to disable Debug Logging in order to get better performance 
(right-click in a 
gray area of ShotGrid Desktop / Advanced / Toggle Debug Logging):

![Toggle Debug Logging](images/toggle_debug.png)


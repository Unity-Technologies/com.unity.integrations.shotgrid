# Troubleshooting

We are actively working on problems with the Shotgun integration and are releasing new configs/engines/Python packages regularly. Sometimes it is still required to quit Unity when Shotgun does not respond anymore. You might also need to kill the client “python.exe” process that is launched when operating Shotgun before starting Unity from Shotgun Desktop again.

In case you are blocked, please send the following data to us (#devs-shotgun):
* Your Unity Editor log
* The contents of %APPDATA%/Shotgun/Logs
* The content of the Python console running python.exe (client)
* * Option 1: paste the content of the console if available
* * Option 2: set UNITY_PYTHON_CLIENT_LOGFILE in the environment prior to launching Shotgun Desktop then send the file contents. E.g.:
```
set UNITY_PYTHON_CLIENT_LOGFILE=d:\temp\mylog.txt
```

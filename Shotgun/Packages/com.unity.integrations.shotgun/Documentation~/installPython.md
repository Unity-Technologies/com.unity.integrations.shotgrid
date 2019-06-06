# Bring your own Python

The Shotgun integration for Unity requires that you have Python installed with specific packages available.

## Windows
* Install Python 2.7 x64 (2.7.15 recommended, preferably under c:\Python27)
https://www.python.org/ftp/python/2.7.15/python-2.7.15.amd64.msi

The Python interpreter should be in a location listed in the PATH environment variable.
* Install PySide and RPyC
    * In a command prompt, go to c:\Python27\Scripts
    * pip.exe install PySide
    * pip.exe install rpyc

## OSX

On macOS Sierra, no extra setup step is required since the system Python (`/usr/bin/python`)
is compatible with the Unity Python server.

The Unity Project's Python Settings will need to tuned for the Shotgun integration
to use a different Python. See [Preparing a Unity project for Shotgun](preparingUnityProject.md)
for details.

## CentOS7
**Caveats** :
The Shotgun integration doesn't work with the Python package `python27-python-2.7.13-5` provided from the software collections
  
* Install PySide and RPyC
    * pip install rpyc
    * yum install python2-pyside
        *  Also installable from pip, but it requires the `qt-devel` package

## Validation
To verify that your Python interpreter is properly configured, follow these steps:
* Launch a command prompt/shell
* Type "python"

The Python interpreter should launch. It should report a 64-bit version of Python 2.7

Copy the following code in a Python script (e.g. validate_python.py) and run it, by either:
* using "execfile" in the Python interpreter to execute the script (e.g. `execfile('validate_python.py')`)
* passing the script to the interpreter in a command prompt/shell (e.g. `python.exe validate_python.py`)


*validate_python.py*
```
import sys
# Python version (expected: 2.7 64 bit)
try:
    import platform
    version = platform.python_version()
    tokens = version.split('.')

    if len(tokens) < 2 or int(tokens[0]) != 2 or int(tokens[1]) != 7:
        print('ERROR: invalid Python version. Expected 2.7.x, got %s'%version)
        sys.exit(1)
    
    (bits,_) = platform.architecture()
    if bits != '64bit':
        print('ERROR: invalid architecture. Expected "64bit", got "%s"'%bits)
        sys.exit(1)
    
except:
    print('ERROR: could not determine the Python version')
    sys.exit(1)

# PySide 
try:
    import PySide
except:
    print('ERROR: could not import PySide')
    sys.exit(1)

# rpyc
try:
    import rpyc
except:
    print('ERROR: could not import rpyc')
    sys.exit(1)
    
print('SUCCESS: The Python interpreter is properly configured for Shotgun in Unity')
```

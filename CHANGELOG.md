# Changes in Shotgrid for Unity

RELEASE NOTES

## [1.2.0-exp.1] - 2021-07-07
This version requires:
* tk-unity v1.2
* tk-config-unity v1.3.19.2

NEW FEATURES
* Rebrand from Shotgun to Shotgrid

## [1.1.0-exp.1] - 2021-06-22
INCOMPATIBLE CHANGES:
* Rebrand to Shotgrid
* Updated to Python 3.7. Your scripts will need to be ported.
* Updated to Python for Unity 4.0. The out of process API no longer exists, instead we can safely run Python within the Unity main thread. This should simplify new development but will require adapting scripts written for the prior version.

FIXES and IMPROVEMENTS:
* it is no longer necessary to kill the Unity Hub when launching from Shotgrid Desktop.
* There are no longer any additional client installation steps beyond adding the com.unity.integrations.shotgrid package within Unity.
* Uninstalling the package removes Shotgrid assets added to the project.

## [0.10.0-preview.1] - 2020-07-03
FIXES
* Bump the recorder dependency to 2.2.0 or later, fixing a compile error caused by an API change.
* Minor changes to begin to prepare for Python 3.

## [0.9.0-preview.4] - 2020-01-08
This version requires:
* tk-unity v0.9
* Python for Unity 2.0.0

We recommended to use tk-config-unity v1.2.9.1 as the Shotgun pipeline 
configuration.

NEW FEATURES
* New "Jump to Frame" action for Notes in the Shotgun Panel

KNOWN ISSUES
* Sometimes, there is a 30-seconds delay in the communication between Unity and
the Shotgun client for Python. The most common symptoms are:
    * Shotgun takes 30 seconds before bootstrapping after Unity is launched
    * A toolkit dialog shows up 30 seconds after being invoked from the Shotgun  
    menu in Unity

## [0.4.0-preview] - 2019-06-10
NEW FEATURES
* When recordings get published from Unity, users can:
  * Launch Unity from related Version/Note entities (Web Shotgun). Unity will 
  open in the associated project and scene
* The `Shotgun/Record Timeline...` menu item has been renamed to 
`Shotgun/Publish Recording...`

## [0.3.0-preview] - 2019-03-14
NEW FEATURES
* Addition of a progress bar to the bootstrapping process.
* A warning is now issued upon launch outside of the expected Shotgun environment.
FIXES
* Improved handling of the Shotgun asset folder following an unexpected termination of Unity.
* Improved handling of the Publish window.

## [0.2.0-preview] - 2018-12-14
NEW FEATURES
* Added documentation on the Shotgun menu (user manual)
* Publishing playblasts is now achieved via the Shotgun/Record Timeline menu

## [0.1.0-preview] - 2018-12-07
NEW FEATURES
* added Unity Shotgun Integration

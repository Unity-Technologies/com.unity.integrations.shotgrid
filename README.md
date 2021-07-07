# Shotgrid for Unity: the Unity package

This repository is part of the Shotgrid for Unity system. This is the Unity
Package Manager package that implements the C# code to integrate Shotgrid into
Unity itself.

The full documentation is maintained in the [com.unity.integrations.shotgrid](https://docs.unity3d.com/Packages/com.unity.integrations.shotgrid@latest) package which is derived from the `Documentation~` directory.

Related repositories include:
* [`com.unity.integrations.shotgrid`](https://github.com/Unity-Technologies/com.unity.integrations.shotgrid) lets Unity communicate with Shotgrid
* [`tk-unity`](https://github.com/Unity-Technologies/tk-unity) provides the engine description for Shotgrid
* [`tk-config-unity`](https://github.com/Unity-Technologies/tk-config-unity) is a sample pipeline config that includes Unity support

Please be aware that the use of this package requires a [Shotgrid](https://www.shotgridsoftware.com/) account and
acceptance of Autodesk's terms and conditions.

[Contributions](CONTRIBUTING.md) are welcome.

## Organization

Since Shotgrid is an editor-only package, all the code is in the `Editor`
directory, except for the unit tests in `Tests`.

Documentation is in the `Documentation~` directory.

There is no build process to convert this repository into a package; it can be
used as a package directly, which makes development easy.

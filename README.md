# CementSource

This is the source code for the Cement Mod Loader for Gang Beasts. **To install the latest release of Cement, go to [the Cement website](https://cementgb.github.io)** and download through the installer.

## Outline

### cement

The assetbundle for Cement.

### Cement.cs

This C# file contains the Cement class, which is the actual BepInEx plugin. It manages all the other classes as well as the interactions with Gang Beasts itself. If you want to dig around start here, in the Awake method.

### Helpers

The Helpers directory just contains a bunch of helper classes, not anything interesing.

### ModDownload

This directory contains the classes which all help downloading mods in some way.

### ModMenu

I mean it's pretty obvious this directory just contains the classes which handle the mod menu. Although I should mention that the mod menu is instantiated in the Cement.cs file.

### Mods

This directory contains all the classes which deal with mods directly, for instance, the ModFile class.

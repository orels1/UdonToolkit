---
description: Purpose-built Udon Behaviours and tools to make your own
---

# UdonToolkit

![](.gitbook/assets/68747470733a2f2f63646e2e7672636861742e73682f75742f70726f6d6f2f6769746875625f62616e6e65722e706e67.png)

Udon Toolkit is a project aimed at simplifying usage of Udon by providing a set of easy to use purpose-built behaviours, from generic triggers and actions, to more complete systems like Key Items, Cabinet Drawers, etc.

This also includes a system of attributes that allow you to utilize the same UI elements when building your own behaviours!

If you find that something is broken or works not as you would expect - please ping me in the [discord server](https://discord.com/invite/fR869XP), or better - create a [new issue](https://github.com/orels1/UdonToolkit/issues/new).

### Requirements

* [VRC SDK3 with Udon](https://vrchat.com/home/download) (v 2021.06.03.14.57+)
* [Udon Sharp Compiler](https://github.com/Merlin-san/UdonSharp) (v 0.19.12+)
* \[Optional] Post Processing from Unity Package Manager **(required for the Camera System)**

### Installation

* Grab the latest package [from releases](https://github.com/orels1/UdonToolkit/releases)
* Import it into the project

{% hint style="danger" %}
**If you are upgrading from a version prior to 1.x.x** [**you'll need to follow these upgrade steps to upgrade**](extras/migration-to-v1.0.0.md)****
{% /callout %}

* When importing you can select what to import:
  * **Internal: REQUIRED**. Contains all of the editor functionality
  * **Systems**: Various systems listed in the **SYSTEMS** section of this documentation
  * **Behaviours**: A collection of scripts that perform actions in the world. You can find the full list in the [**BEHAVIOURS** ](behaviours/overview.md)section of this documentation
  * **Demo**: assets and examples scenes for all the systems and behaviours. _You should import this only if you are importing the full package_
* Open your `Project Settings` -> `UdonSharp` and in the `Default Behaviour Editor` select `UdonToolkit Editor`

### How to use

* Browse the list of available behaviours in [the behaviours section](behaviours/overview.md)
* Select a Game Object you want to use (or make a new one)
* Click `Add Component` and type the name of a Behaviour
* A new component with all the corresponding UI and parameters should appear
* If it is your first time using Udon Toolkit, you might want to click "Compile All Behaviours"&#x20;

{% callout type="note" %}
Tip: You can click on the `?` icon in the Behaviour header to open the docs for that particular script
{% /callout %}

### Known Issues

* Prefabs are fundamentally broken in Udon as of June 2021. Toolkit will show a warning if you will try to use a prefab, it is **highly recommended** to unpack the prefab before editing anything, as values can just reset to whatever they were before when entering play mode

{% callout type="note" %}
An alternative to that might be to utilize [Unity Presets](https://docs.unity3d.com/2018.4/Documentation/Manual/Presets.html) which allow you to save the values of the current behaviour into a file
{% /callout %}

### Advanced

#### UI System

UdonToolkit is not just a set of prebuilt behaviours, but also a UI system built using Unity's Custom Property Drawers and Attributes that allows you to quickly create user-friendly UI. You can [read more here](attributes/attributes-overview.md).

### Thanks ❤

* To VRC Team for making Udon which actually made me learn C# and Unity Editor Tools development
* To [Merlin ](https://github.com/MerlinVR)for making [Udon Sharp](https://github.com/Merlin-san/UdonSharp) without which none of this would've been possible

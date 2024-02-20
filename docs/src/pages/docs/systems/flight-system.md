---
title: Flight System
description: Add the power of flight to your world
---

![](</img/docs/Flight System Banner Wide.png>)

{% video url="https://www.youtube.com/embed/b4IVc7wMA6g?t=60" title="Flight System preview" /%}

{% callout type="note" %}
Udon Toolkit's Flight System allows your players to fly like a superman, glide across the ground and hover mid air on both Desktop and VR platforms!
{% /callout %}

**Check it out in my** [**Catch A Flight world**](https://vrchat.com/home/launch?worldId=wrld\_65fd7c51-89f7-4c7b-8a4f-2620a16f0a75)**!**

A more developer-oriented test world with more info is [also available here](https://vrchat.com/home/launch?worldId=wrld\_842e079c-8ad3-4c99-afa2-ac6afaf78176)

## Features

* Cross-platform (VR/Desktop) flight system with Superman-like controls (point toward direction)
* Ability to glide across the ground maintaining consistent height
* Ability to hover mid-air
* One Handed mode support
* Speed Boost and Flight Restriction mechanics
* Hover and Gliding mechanics extendable with your own logic via provided callback events

## Installation

1. Drag the Flight System prefab from the `UdonToolkit/Flight System` folder into your scene
2. Build a test version
3. Jump to start flying!

{% callout type="warning" %}
Testing in editor requires [CyanEmu](https://github.com/CyanLaser/CyanEmu)
{% /callout %}

## Usage

* **VR**
  * Point your arms slightly forward and up, jump to start the flight. If glide is enabled (it is by default) - point your arms slightly above your head to stop the glide and takeoff
  * Spread your arms to slow down, bring them together to speed up
  * Point where you want to fly. Think about it as if you were a Superman
  * Point back to freeze mid-air
  * When gliding - point upwards, above your head to take off
  * When gliding - spread your arms to slow down and point downward to land
* **VR One Handed**
  * Point the selected one handed tracking source where you want to fly (Head, Left Hand or Right Hand)
  * Press Left Trigger or Right trigger to slow down
  * When gliding - look up to take off
  * When gliding - press down the trigger and look towards the ground to land

{% callout type="note" %}
During glide when going full speed (arms close together, pointing in same direction) you will not be able to land by pointing your arms towards the ground. That is done to help with arm fatigue allowing for extended periods of gliding without getting tired. To land - slow down by spreading your arms and point them towards the ground
{% /callout %}

* **Desktop**
  * Jump to start flying, if glide is enabled (it is by default), press and hold `T` to takeoff
  * Use your mouse to control the direction
  * **There are currently no speed controls on desktop**
  * If hover is enabled (it is by default), press `F` to enter hover mode, you'll freeze in place allowing you to look around and chat with people. Press `F` again to exit hover
  * Press `P` to respawn to the **Reset Position**

## Configuration

{% callout type="note" %}
UdonToolkit's FlightSystem provides many configuration options for you to adjust. I recommend still using the prefab as a starting point for easier configuration though.
{% /callout %}

![](</img/docs/image (12).png>)

### Parameters

* Tracking References
  * **Right Hand**: Right hand tracking object
  * **Left Hand**: Left hand tracking object
  * **Head**: Head tracking object
* General Settings
  * **Max Speed**: Maximum flight speed
  * **Allow Boosting**: Specifies whether the `Boost` event will do anything
  * **Boost Length**: Determines the time of the boost in seconds
  * **Boost Speed**: Maximum speed during boost
  * **Allow Gliding**: Specifies whether gliding along the surface is allowed
  * **Ground Layers**: Layers used to determine the ground altitude used for landing/takeoff speed scaling and gliding
  * **Allow Hover**: Specifies whether hovering is allowed using the hover key

{% callout type="warning" %}
If **Allow Gliding** is checked - Desktop players will need to press the takeoff/landing key in order to enter flight or exit glide
{% /callout %}

* Comfort Settings
  * **Acceleration Smooth Factor**: The lerping amount applied to the acceleration (lower is smoother)
  * **Deceleration Smooth Factor**: The lerping amount applied to deceleration (lower is smoother)
  * **Ground Slow Max Height**: The maximum height at which the takeoff/landing assist will take place. FlightSystem will scale the maximum speed based on the distance from the ground to aid with precise landing and smoother takeoff
  * **Min Ground Speed Modifier**: Specifies the speed threshold at which the system will automatically slow the player down during takeoff/landing. The value of `0.5` will mean that the player will be never be slowed down to less than `50%` of max speed during takeoff/landing

{% callout type="note" %}
It is recommended to always have a relatively high **Deceleration Smooth Factor** as sluggish deceleration can cause the feeling if sluggishness and cause nausea
{% /callout %}

* Desktop Keybinds
  * **Glide Take Off Key**: The takeoff/landing key allowing desktop players to perform those actions
  * **Freeze In Place Key:** The hover key switching between hovering and gliding/flying states if hovering is allowed
  * **Force Respawn Key**: The respawn key which will stop the flight and teleport the player to the **Reset Position**&#x20;
* Initial State
  * **Flight Restricted**: Specifies whether the flight system is turned off by default as if the player was in a restricted zone. You can send an `AllowFlight` event to the system to enable it
  * **Reset Position**: Determines the target to which the player will be teleported when pressing the **Force Respawn Key**
* Callbacks
  * **On Flight Start**: Sends the specified event to the provided UdonBehaviour when the player engages the flight system
  * **On Flight End**: Sends the specified event to the provided UdonBehaviour when the player lands
  * **On Hover Start**: Sends the specified event to the provided UdonBehaviour when desktop hover is started by pressing the **Freeze In Place Key** or when the **** player spreads their arms to stop in VR
  * **On Hover End**: Sends the specified event to the provided UdonBehaviour when desktop hover is ended by pressing the **Freeze In Place Key** again or when the player brings their arms closer together
  * **On Glide Start**: Sends the specified event to the provided UdonBehaviour when the player enters glide
  * **On Glide End**: Sends a the specified event to the provided UdonBehaviour when the player exists glide (either by taking off or landing)

{% callout type="note" %}
There is an included `FlightSystemDebugger` behaviour with the logging for all the Flight System Callbacks enabled so you can quickly test your environment and when each system engages/disengages
{% /callout %}

{% callout type="warning" %}
**VR currently has a very rudimentary hover mechanic.** I would encourage you to create some extra system for VR specific hover toggle, like maybe a couple interacts that appear if the player slows down, which they can click to switch into a hover mode. That will make the hover / flight switch more intentional and less spontaneous. I might add an example of this at some point in the future
{% /callout %}

### Events

* **Boost**: Raises the **Max Speed** to the **Boost Speed** for **Boost Length** seconds
* **RestrictFlight**: Prohibits players from flying, even if the player is already mid-flight (will make them fall)
* **AllowFlight**: Allows players to fly
* **DisableBoosting**: Disables the boosting behaviour, calling `Boost` after boosting has been disabled will not do anything. This does not affect players that are already boosting. They will stop boosting as their boost timer runs out.
* **EnableBoosting**: Enables the boosting behaviour
* **Stop Boost**: Stops currently active boost
* **DisableGliding**: Disables gliding, players will simply land and take off while close to the ground. The takeoff/landing speed helpers will still work as usual. This will interrupt current glide
* **EnableGliding**: Enables gliding
* **StopGliding:** Stops current glide and grounds the player
* **EnableOneHandedMode**: Enables One Handed Mode for flight controls
* **SetOneHandedTargetHead**: Sets One Handed Mode tracking target to player's Head
* **SetOneHandedTargetLeftHand**: Sets One Handed Mode tracking target to player's Left Hand
* **SetOneHandedTargetRightHand**: Sets One Handed Mode tracking target to player's Right Hand
* **DisableOneHandedMode**: Disables One Handed Mode for flight controls and returns to the default two-handed flight

{% callout type="note" %}
If you are planning to build upon the logic exposed by the Flight System, I would also encourage you to check out the code! Almost every single line is commented, with motivation / explanation which should help you better understand how it all comes together
{% /callout %}

## Demo

**Check it out in my** [**Catch A Flight world**](https://vrchat.com/home/launch?worldId=wrld\_65fd7c51-89f7-4c7b-8a4f-2620a16f0a75)**!**

{% callout type="note" %}
A more developer-oriented test world with more info is[ also available here](https://vrchat.com/home/launch?worldId=wrld\_842e079c-8ad3-4c99-afa2-ac6afaf78176). The same world is also provided as a demo scene in the `UdonToolkit/Demo/Flight System Demo` scene. Don't hesitate to take a look!
{% /callout %}

## Feedback

{% callout type="note" %}
If you have any suggestions for the FlightSystem or encounter any issues, please do not hesitate to [file an issue on Github](https://github.com/orels1/UdonToolkit/issues/new) or reach out[ in my Discord](https://discord.com/invite/fR869XP), as well as [support the Toolkit on Patreon](https://www.patreon.com/orels1)
{% /callout %}

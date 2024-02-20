---
description: A collection of small, general / single purpose built behaviours
---

# Misc Behaviours

## Movement / Tracking

### Universal Tracker

_Tracks the local player's Tracking Data source, or a Bone transform and copies it over to the target transform_

![](</img/docs/image (34).png>)

**Parameters**

* **Track Bone**: Switches between tracking a Tracking Data source (Head/Hands) and tracking Bones
* **Bone**: _Only visible when Track Bone is checked_. Specifies which Bone to track
* **Track Player Base**: Switches to player base tracking mode where the Target Transform will follow the player's ground position
* **Track Playspace**: Switches to playspace tracking mode. Allows Target Transform to persist in players playspace, only moving when the player moves with the stick
* **Tracking Target**: _Only visible when Track Bone is unchecked_. Specifies which Tracking Data target to use
* **Target Transform**: Transform to copy the tracked Bone / Tracking Data position and rotation to
* **Track Position**: Specifies whether the position should be copied
* **Track Rotation**: Specifies whether the rotation should be copied
* **Tracking Correction**: Allows to apply an extra X/Y/Z rotation to the transform after copying it over
* **Follow Main Camera**: _For editor use only_. Makes the object follow the main camera, helpful for editor testing

**Events**

* **Reset Offsets**: Resets the player offsets from their player base tracker. Call this to calibrate player offsets at least once, otherwise the tracking will be wrong

**Usage / Examples**

The basic usage of this component is intended to be things like attaching objects to player's hands, head or both.

### Velocity Tracker

_Tracks an object or the local player's Tracking Data source using unity physics systems to provide proper collision and velocity_

![](</img/docs/image (10).png>)

**Parameters**

* **Track Transform**: Track an arbitrary object
* **Track Player**: Track player's Tracking Data source
* **Source Transform**: The Transform to follow (only visible when Track Transform is selected)
* **Source on Player**: The Tracking Data Type to follow (only visible when Track Player is selected)
* **Target**: The Transform to move
* **Track Position**: Specifies whether to follow Source's position
* **Track Rotation**: Specifies whether to follow Source's rotation

{% callout type="warning" %}
The `Target` object **MUST** have a Rigidbody for this to work
{% /callout %}

**Usage / Examples**

This behaviour is aimed to be used in cases where you want to interact with physical objects at high speeds. Good example being: adding a sphere collider to the player's hand by selecting `Track Player` and setting Right Hand or Left Hand as the `Source On Player`

That way you'll have a proper physics-driven collider following the player which can nicely interact with the world.

Another example might be - following a pickup using `Track Transform` option. Allowing you to easily implement something like a golf club.

### Lerped Follower

_Makes an object follow another object with smooth interpolation_

![](</img/docs/image (42).png>)

**Parameters**

* **Source Transform**: The source of position and rotation to copy from
* **Target Transform**: The target of position and rotation to copy to
* **Lerp Position**: Specifies whether the position should be interpolated (smoothed) or instant
* **Lerp Rotation**: Specifies whether the rotation should be interpolated (smoothed) or instant
* **Lerp Speed**: A speed with which Target will achieve Source's position and rotation
* **Ignore Rotation**: Specifies whether to copy rotation or completely ignore it

**Usage / Examples**

Generally good feeling lerp value for scene objects seems to be at around 5-15, for the player attached object (like with Universal Tracker) higher values are recommended.

Ignore rotation can be used in cases where you want to have the object look at a particular spot or generally not rotate with the Source object. Thins like Rotation / LookAt or Aim constraints come to mind as good combinations.

### Teleporter

_Teleports an object or the local player to a desired position_

![](</img/docs/image (36).png>)

**Parameters**

* **Teleport Object**: Switches the Teleporter to an object teleport mode
  * **Object To Teleport**: The object to be teleported to the Teleport Target's location
  * **Networked**: Teleports the object for all players
* **Teleport Player**: Switches the Teleporter to the player teleport mode
  * **Keep Player Rotation**: Maintains player view rotation after teleporting. Otherwise - the player will be aligned with the Z axis of Teleport Target
* **Teleport Target**: The target position / rotation to teleport objects and players to

{% callout type="warning" %}
Make sure to only use `Networked` option when calling Teleporter locally. Otherwise you will have a double-teleport
{% /callout %}

**Methods**

* **Trigger**: Executes the teleport

### Player Movement Modifier

_Adjusts different player movement parameters like speed and gravity_

![](</img/docs/image (47).png>)

**Parameters**

* **Jump Impulse**
  * **Jump Impulse Change Type**: Determines how the jump impulse will be adjusted
  * **Jump Impulse**: The value to use for jump impulse adjustments
  * **Set Jump Impulse**: Specifies if the jump impulse needs to be adjusted
* **Walk Speed**
  * **Walk Speed Change Type**: Determines how the walk speed will be adjusted
  * **Walk Speed**: The value to use for walk speed adjustments
  * **Set Walk Speed**: Specifies if the walk speed needs to be adjusted
* **Run Speed**
  * **Run Speed Change Type**: Determines how the run speed will be adjusted
  * **Run Speed**: The value to use for run speed adjustments
  * **Set Run Speed**: Specifies if the run speed needs to be adjusted
* **Strafe Speed**
  * **Strafe Speed Change Type**: Determines how the strafe speed will be adjusted
  * **Strafe Speed**: The value to use for strafe speed adjustments
  * **Set Strafe Speed**: Specifies if the strafe speed needs to be adjusted
* **Gravity Strength**
  * **Gravity Strength Change Type**: Determines how the gravity strength will be adjusted
  * **Gravity Strength**: The value to use for gravity strength adjustments
  * **Set Gravity Strength**: Specifies if the gravity strength needs to be adjusted

**Methods**

* **Trigger**: Executes the selected adjustments

**Usage / Examples**

This behaviour allows you to adjust different player parameters, like run or strafe speed. You can either just set a specific value, or use a relative adjustment by specifying a change type other than `SET`.&#x20;

Only the values that have checked checkboxes will actually be adjusted.

Example: Adding 2 to player's run speed

1. In the Run Speed section - select `ADD` in the change type dropdown
2. Set the value to `2`
3. Check the checkbox on the far right to tell the behaviour to actually change the value
4. Send a Trigger event to the Behaviour (via any of the triggers, e.g, an [Interact Trigger](misc-behaviours#interact-trigger)) to perform the adjustments

## Triggers / Actions

### Universal Action

_The bread and butter of any world reactivity, allows you to perform various actions in response to an incoming `Trigger` event. Think about it as a VRC SDK2 Triggers Lite_

![](</img/docs/image (58).png>)

**Parameters**

* **Active**: Determines if the behaviour is running, since toggling game objects on and off only means they won't run `Update`
* **One Shot**: Specifies if this trigger can only be activated once
* **Fire After Delay**: _Object must be enabled at all times for this to work_. Specifies whether the triggers should be fired after a delay
* **Delay Length**: _Only visible when Fire After Delay is checked_. Sets the delay length in seconds
* **Animations**
  * **Fire Animation Triggers**: Specifies whether anything in this section should be executed
  * **Animator Triggers List**: A list of Animators and Triggers to fire, you can specify the same Animator multiple times to fire multiple triggers
  * **Animator Bools List**: A list of Animators and Booleans to be set to a provided value
  * **Animator Floats List**: A list of Animators and Floats to be set to a provided value
  * **Animator Ints List**: A list of Animators and Ints to be set to a provided value
* **Udon Events**
  * **Fire Udon Events**: Specifies whether anything in this section should be executed
  * **Networked**: When checked - Udon Events will be sent to targets set in Network Targets
  * **Network Target**: Specifies who to send the network events to
  * **Udon Events List**: A list of Udon Behaviours and Events to call
* **Game Object Toggles**
  * **Fire Object Toggles**: Specifies whether anything in this section should be executed
  * **Game Objects List**: A list of Game Objects to Enable, Disable or Toggle
* **Collider Toggles**
  * **Fire Collider Toggles**: Specifies whether anything in this section should be executed
  * **Colliders List**: A list of Colliders to Enable, Disable or Toggle
* **Audio**
  * **Fire Audio Events**: Specifies whether anything in this section should be executed
  * **Audio List**: A list of Audio Sources and Audio Clips to play through them

{% callout type="warning" %}
Make sure to only use Networked option when executing this trigger locally, e.g. via a UI Button, otherwise you might cause oversync
{% /callout %}

**Events**

* **Trigger**: Triggers this Universal Action
* **Activate**: Activates the behaviour, so it will react to Trigger events
* **Deactivate**: Deactivates the behaviour, so it will no longer react
* **Toggle**: Toggles the active state, provided for convenience
* **ResetOneShot**: Allows this trigger to be activated again

**Usage / Examples**

Universal Action will probably be the most used behaviour in your world, as its basically a simplified trigger from SDK2. It is not as deep as the original triggers, though, so you are expected to combine it with more specific behaviours from Udon Toolkit or other places to extend its functionality.

This component makes it pretty trivial to do things via UI Buttons simply by calling a `SendCustomEvent` on it with `Trigger` as the event name.

You can check out a guide on setting up a basic mirror [right here](../guides/mirror-toggle) which should provide a good example of Universal Action usage.

The major thing to note here is to make sure that the component, and the game object its on is always enabled if you're using the `Delay` parameter, otherwise it will never fire as the Update method will never be called.

### Secret Actions

_Sends events to udon behaviours based on the player's name_

![](</img/docs/image (1).png>)

**Parameters**

* **Active**: Determines if the actions will be executed on Start
* **Actions List**: A list of Player Names and Udon Behaviours to send the provided Events to

**Events**

* **Trigger**: Executes the logic again, you will probably want this to be called from the network to run for everyone again

**Usage / Examples**

Ever wanted to have something special activate only for particular users? This behaviour does exactly that! Combine this with Universal Action to do something special for that user, or specify behaviours of your own.

If sending events once isn't enough - you can use the `Trigger` event to fire it again.

### Networked Trigger

_A "proxy" of sorts that expects a "Trigger" event and sends Networked events to specified behaviours_

![](</img/docs/image (22).png>)

**Parameters**

* **Event Target**: Specifies who to send the events to
* **One Shot**: Specifies if this trigger can only be activated once
* **Master Only**: Limits the trigger execution to Master only (everyone will get the trigger effects)
* **Owner Only**: Limits the trigger execution to Owner only (everyone will get the trigger effects)
* **Allowed Users**: Limits the trigger execution to the provided list of users (everyone will get the trigger effects)
* **Udon Events List**: A list of Udon Behaviours and Events to send to them

**Methods**

* **Trigger**: Fires the events on the provided behaviour over the network
* **Activate**: Activates the behaviour, so it will react to objects
* **Deactivate**: Deactivates the behaviour, so it will no longer react
* **Toggle**: Toggles the active state, provided for convenience
* **ResetOneShot**: Allows this trigger to be activated again
* **TakeOwnership**: Makes local player the owner of the object

**Usage / Examples**

Think about this as a middle-man between local only things and the network. While its not that useful on its own - it might serve as a nice layer between UI Buttons, and some other behaviours, that you want to call for everyone in the room, or their owners.

{% callout type="note" %}
You can achieve the same results by using Universal Action and checking the Networked checkbox, but this is lighter on performance, so this separation is just a way to simplify things.
{% /callout %}

### Area Trigger

_Sends events to other behaviours when an object enters and exits a trigger collider_

![](</img/docs/image (26).png>)

**Parameters**

* **Active**: Specifies whether the triggers will fire, as just disabling the object doesn't stop it from reacting to enter/exit events
* **One Shot**: Specifies if this trigger can only be activated once
* **Collide Target**: Specifies what this trigger will collide with: Objects or Players
* **Collide With**: Specifies the list of layers to filter out the objects you don't want to react to (only visible when Collide Target is set to Objects)
* **Collide With Local Players**: Specifies whether the event would fire on local player collisions&#x20;
  * (only visible when Collide Target is set to Players)
* **Collide With Remote Players**: Specifies whether the event would fire on remote player collisions&#x20;
  * (only visible when Collide Target is set to Players)
* **Master Only**: Limits the trigger execution to Master only (everyone will get the trigger effects)
* **Owner Only**: Limits the trigger execution to Owner only (everyone will get the trigger effects)
* **Allowed Users**: Limits the trigger execution to the provided list of users (everyone will get the trigger effects)
* **Networked**: Specifies whether the events should be sent over the network or only fired locally
* **Network Target**: Specifies who to send the network events to
* **Enter Events List**: A list of Udon Behaviours and Events to send when the correct object enters the trigger collider
* **Exit Events List**: A list of Udon Behaviours and Events to send when the correct object exits the trigger collider

**Events**

* **Activate**: Activates the behaviour, so it will react to objects
* **Deactivate**: Deactivates the behaviour, so it will no longer react
* **Toggle**: Toggles the active state, provided for convenience
* **ResetOneShot**: Allows this trigger to be activated again
* **TakeOwnership**: Makes local player the owner of the object

**Usage / Examples**

This is an equivalent of VRC SDK2's `Trigger Enter` and `Trigger Exit` triggers

Some things to note when using Area Triggers

* Do not use the `LocalPlayer` or `Player` layers in the `Collide With` option, use the `Collide With Local Players` and `Collide With Remote Players` checkboxes instead
* Generally speaking you would want the `Area Trigger` to be on the `MirrorReflection` layer to avoid some weird vrchat issues, unless you have a case where you need a custom layer
* Do not use the `Networked` option if you are using `Collide With Remote Players`, as this will make everyone send events to everyone all the time, causing oversync and possibly desyncing people in the instance.

{% callout type="note" %}
A good rule of thumb when it comes to things like collisions: "if everyone can see object X enter trigger Y - you can keep everything local"
{% /callout %}

### Interact Trigger

_Makes something react to player interaction, VRC SDK2-style_

![](</img/docs/image (46).png>)

**Parameters**

* **Active**: Specifies whether the object can be interacted with
* **One Shot**: Specifies if this trigger can only be activated once
* **Master Only**: Limits the trigger execution to Master only (everyone will get the trigger effects)
* **Owner Only**: Limits the trigger execution to Owner only (everyone will get the trigger effects)
* **Allowed Users**: Limits the trigger execution to the provided list of users (everyone will get the trigger effects)
* **Networked**: Specifies whether the Udon Events should be sent to other players
* **Network Target**: Specifies who to send the Udon Events to
* **Udon Events**: A list of Udon Behaviours and Events to send

{% callout type="note" %}
To set interaction text or activation distance - click the `Udon Settings` button to unfold the Udon Behaviour menu
{% /callout %}

**Methods**

* **Interact**: Sends events to provided Udon Behaviours
* **Activate**: Activates the behaviour, so the object will become interactive
* **Deactivate**: Deactivates the behaviour, so the object will no longer be interactive
* **Toggle**: Toggles the active state, provided for convenience
* **ResetOneShot**: Allows this trigger to be activated again
* **TakeOwnership**: Makes local player the owner of the object

**Usage / Examples**

This is an almost one-to-one equivalent of the VRC SDK2 `Interact` trigger. Use in combination with `Universal Action` to do something when user clicks on an object.

`Activate`/`Deactivate`/`Toggle` events will also allow you to disable the highlight on the object, if you don't want it to be interactive anymore.

### Synced Trigger

Provides basic functionality for syncing an action for late-joiners

![](</img/docs/image (7).png>)

**Parameters**

* **Synced Value**: Specifies the initial state of the trigger (based on which the ON/OFF events are fired)
* **Use Interact**: Specifies whether the trigger is placed on an interactable object
* **Master Only**: Limits the trigger execution to Master only (everyone will get the trigger effects)
* **Owner Only**: Limits the trigger execution to Owner only (everyone will get the trigger effects)
* **Allowed Users**: Limits the trigger execution to the provided list of users (everyone will get the trigger effects)
* **ON Events**: List of events to send when the Synced Value will be set to True
* **OFF Events**: List of events to send when the Synced Value will be set to False

**Methods**

* **Toggle**: Switches the current synced value to the opposite
* **TurnOn**: Sets the current synced value to True
* **TurnOff**: Sets the current synced value to False
* **TakeOwnership**: Changes the owner of the object, useful for `OwnerOnly` synced triggers

**Usage / Examples**

This behaviour is aimed at creating an equivalent of a buffered trigger from SDK2 (`AlwaysBufferOne`). Late joiners will execute the methods for the current value of the `Synced Value` unless it is in the default state.

You can put this behaviour on some sort of a button or a switch in the world together with a `Use Interact` option - so that players will be able to trigger it by clicking on the switch.

**Logic Overview**

1. Player triggers the Synced Trigger (either by clicking on the object, or otherwise)
2. Synced Trigger checks if they have access based on `MasterOnly`, `OwnerOnly` and `Allowed Users`
3. If player has access - the Synced Value is changed, the player triggers ON or OFF events respectively
4. All remote players trigger ON or OFF events respectively
5. When a player joins into the world - they receive the current value of Synced Value. If it is the same as the initial value set in Unity - nothing will happen. If the value is different - they will trigger ON or OFF events respectively

### Object Boundary Trigger

_Sends Events to Udon Behaviours if a target object crosses a particular threshold_

![](</img/docs/image (28).png>)

**Parameters**

* **Active**: Toggles the threshold checks on and off
* **Target**: The target object to observe
* **Cross Mode**: Makes this trigger run every time the Target crosses the threshold (either way), also makes it, so the trigger doesn't disable itself after the threshold is crossed
* **Test Mode**: Specifies whether the trigger will fire if ALL or ANY of the specifies boundaries are crossed
* **Boundary List**: A list of conditions and coordinates to check against, e.g. `Above Y: 10` will make the trigger fire events if the Target gets above 10 units on Y coordinate
* **Networked**: Specifies whether the Udon Events should be sent to other players
* **Network Target**: Specifies who to send the Udon Events to
* **Udon Events**: A list of Udon Behaviours and Events to send

**Events**

* **Trigger**: Fires the events specified in the Udon Events list
* **Activate**: Activates the behaviour, so it will start checking the boundaries
* **Deactivate**: Deactivates the behaviour, so it will no perform any checks
* **Toggle**: Toggles the active state, provided for convenience

**Usage / Examples**

While this might seem confusing at first, the gist of this one is to trigger something when object's position exceeds one of the limits. A good example of it would be using a player head tracker and making it so if the tracker's Y position is below your water level in the map - you enable special fog settings or something like that.

By default, the trigger will only run once and disable itself, otherwise it will start triggering things every frame which is not perfect. You can enable it again by sending an `Activate` even to it.

Although often you might want to trigger things when the object crosses the limit in either direction. Like turning the fog on and off in the example above. In that case you would want to use the `Cross Mode` option that will trigger once every time the limits are crossed.

### Platform Trigger

_Sends Events to Udon Behaviours based on the User's Platform_

![](</img/docs/image (35).png>)

**Parameters**

* **Fire On Start**: Specifies whether the events should be sent on start
* **Desktop Events**: A list of Udon Behaviours and Events to send if the Player is on Desktop
* **VR Events**: A list of Udon Behaviours and Events to send if the Player is in VR

**Events**

* **Trigger**: Fires the events specified in the Desktop/VR Events Lists

**Usage / Examples**

By default this trigger will fire on the world load, unless the `Fire On Start` is unchecked.

Generally speaking this behaviours is aimed at providing better cross-platform support. Its pretty common to have separate versions of game mechanics, pickups, etc for different platforms that are enabled and disabled on Start.

You can also send the `Trigger` event to `Platform Trigger` to execute it again, e.g. if something happens in your world and you want Desktop / VR react to it differently.

### Start Trigger

_Sends Events to Udon Behaviours on scene start_

![](</img/docs/image (59).png>)

**Parameters**

* **Active**: Specifies whether it will fire the triggers or not
* **Networked**: Specifies whether the events should be sent over the network or executed locally (check Usage section below to learn when you might want to use this)
* **Network Target**: Specifies who the network event should be sent to
* **Udon Events List**: A list of Udon Behaviours and Events to send

**Methods**

* **Trigger**: Allows to trigger the events again even after the scene already started

**Usage / Examples**

This is as basic as it gets. This behaviour will send some events when the world loads and nothing else.

However, there is an interesting use-case for `Networked` option. If it is checked, and `Network Target` is set to `Owner` - by default the world Master will receive an event every time a player joins. If set to `All` - every player will receive an event when another player joins, which might be useful in some cases.

### Respawn Trigger

_Sends Events to Udon Behaviours on player respawn_

![](</img/docs/image (15).png>)

**Parameters**

* **Active**: Specifies whether it will fire the triggers or not
* **One Shot**: Specifies if the trigger can only be activated once
* **Local Only**: Sets the trigger to only react to local player respawns
* **Master Only**: Limits the trigger execution to Master only (everyone will get the trigger effects)
* **Owner Only**: Limits the trigger execution to Owner only (everyone will get the trigger effects)
* **Allowed Users**: Limits the trigger execution to the provided list of users (everyone will get the trigger effects)
* **Networked**: Specifies whether the events should be sent over the network or executed locally (check Usage section below to learn when you might want to use this)
* **Network Target**: Specifies who the network event should be sent to
* **Udon Events List**: A list of Udon Behaviours and Events to send

**Methods**

* **Activate**: Activates the behaviour, so it will start checking the boundaries
* **Deactivate**: Deactivates the behaviour, so it will no perform any checks
* **Toggle**: Toggles the active state, provided for convenience
* **ResetOneShot**: Resets the one shot state allowing the trigger to be activated again
* **TakeOwnership**: Makes local player the owner of the object

## Visuals

### Skybox Adjustment

_Switches skybox materials or smoothly adjusts values of a single skybox material_

![](</img/docs/image (38).png>)

**Parameters**

* **Active Skybox**: The skybox material to transition to
* **Active Floats List**: A list of numeric parameters to transition to
* **Active Color List**: A list of color parameters to transition to
* **Active Vector3 List**: A list of Vector3 parameters to transition to
* **Instant Transition**: Makes the skyboxes switch instantly
* **Transition Time**: Defines the time, in seconds, over which the Float/Color/Vector3 values will be transitioned from default values to active and vice-versa

**Events**

* **Trigger**: Performs a transition from default to active. Send it again to transition back

**Usage / Examples**

Main thing to decide on when using this is whether you need to transition instantly or slowly over time. If you want to transition instantly - its very simple

* Select a material you want to swap the skybox to and put it into the `Active Skybox` field
* Send a `Trigger` event from one of the Udon Toolkit's many triggers when you want to switch
* Done

If you want to slowly transition between different settings, e.g. between high exposure and low exposure to imitate the day/night cycle - its a bit of a different setup

* Uncheck the `Instant Transition` checkbox and specify the `Lerp Time` in seconds
* Expand the `Active Floats List` and click `Add Element`
* Choose `_Exposure` from the dropdown and set the value you want to transition to (e.g. 0.2)
* Send a `Trigger` event from one of the Udon Toolkit's triggers when you want to start transitioning
* Done

{% callout type="warning" %}
The Skybox Adjustment behaviour will use currently active values as a starting point (as of version v0.4.5)
{% /callout %}

### Fog Adjustment

_Switches between two different fog settings - instantly, or over time_

![](</img/docs/image (49).png>)

**Parameters**

* **Default Fog Color**: First fog color to transition from
* **Default Fog Density**: _Only visible if the fog mode is not Linear_. First fog density to transition from
* **Default Fog Start**: _Only visible if the fog mode is Linear_. First fog start range to transition from
* **Default Fog End**: _Only visible if the fog mode is Linear_. First fog end range to transition from
* **Active Fog Color**: Second fog color to transition to
* **Active Fog Density**: _Only visible if the fog mode is not Linear_. Second fog density to transition to
* **Active Fog Start**: _Only visible if the fog mode is Linear_. Second fog start range to transition to
* **Active Fog End**: _Only visible if the fog mode is Linear_. Second fog end range to transition to
* **Fog Transition Time**: Specifies the duration of the transition, 0 to make it instant
* **Start Active**: Makes it, so the fog will start from the active state, so the first transition will be Active to Default, instead of the other way around
* **Set Initial State**: Applies the Default or Active fog settings, based on whether the `Start Active` checkbox is set

**Events**

* **ActivateFog**: Explicitly transitions from Default to Active state
* **DeactivateFog**: Explicitly transitions from Active to Default state
* **Trigger**: Switches between the two states or starts a smooth transition

**Usage / Examples**

Very similar to [`Skybox Adjustment`](https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours/#skybox-adjustment) in terms of functionality.\
Set the Default and Active setting and then use something like an `Area Trigger` to switch between them when a player enters or exits a particular area.

If you want to just slowly adjust from current values to the Active values - you don't have to set the `Default` settings at all. Those are only needed if you want to use 2 specific states and toggle between them.

## Advanced

{% callout type="note" %}
This area intended for advanced content creators and people developing their own Udon stuff just to make their life easier and more organized, be advised
{% /callout %}

### UI Readout

_Takes compatible variable values from one Udon Behaviour and displays them in UI Text or UI Slider elements, with formatting support_

![](</img/docs/image (11).png>)

**Parameters**

* **Active**: Turns the value update on and off
* **Source**: The source of the values
* **Text Readouts List**: A list of variables that can be converted to text and UI Text components to display them in_._ Formats allow you to format the text before displaying. See Usage below
* **Text Readout Formats**: A list of formatting rules applied to each readout value
* **Slider Readouts List**: A list of numeric variables that can be assigned to a value of a UI Slider. Multipliers allow you to adjust the value before assigning it to a slider. See Usage below

**Events**

* **Activate**: Activates the behaviour, so it will start reading fresh values
* **Deactivate**: Deactivates the behaviour, so it stops reading values
* **Toggle**: Toggles the active state, provided for convenience

**Usage / Examples**

> The values are grabbed and assigned in `LateUpdate` to have the most up to date values possible

The formatter works by replacing `{0}` in whatever text you provide to each formatter with the value of the variable. So something like `Speed: {0} u/s` for a variable `speed` in the `Target` that has value of `10` will result in `Speed: 10 u/s` being displayed in the text element.

Multipliers work in a similar fashion - by multiplying a corresponding variable value by this multiplier before passing it to a slider.

`UI Readout` is built to provide a quick way of setting up something like a HUD you can attach to player's head via `Unversal Tracker` that would print out all the needed information from something like a flight system, or a combat system of some sorts. It will pre-populate lists of compatible variables for you, so you wouldn't need to dig into the code to figure out what you can display.

Note that this process can be performance heavy, so you don't want many of these behaviours running at all times, use the `Toggle` event to only read values when needed.

### Shader Feeder

_Passes various shader properties to a provided Mesh renderer_

![](</img/docs/image (57).png>)

**Parameters**

* **Active**: Turns the shader property value updates on and off
* **Source**: The shader to get the property names from
* **Targets**: The list of MeshRenderers to pass the property values to
* **Custom Update Rate**: Specifies whether a custom Update Rate should be used, if unchecked - the values will be passed every frame
* **Update Rate**: (Only visible if Custom Update Rate is checked) Specifies how often the values should be set, Per Second or Per Minute (think of it as BPM mode)
* **Set Scene Start Time**: Specifies whether scene start time should be passed on world load
* **Start Time Target**: Name of the property to pass the start time to
* **Set Cycle Length**: (Only visible if Custom Update Rate is checked) Specifies whether the current cycle time (based on a Custom Update Rate) should be passed on Start
* **Cycle Length Target**: Name of the property to pass the cycle length to
* **Set Cycle Start Time**: Specifies whether the current time (which is also the time of the current update cycle start) should be passed
* **Cycle Time Target**: Name of the property to pass the cycle start time to
* **Slider Sources**: List of Slider components and corresponding property names to pass the Slider values to
* **Transform Sources**: List of Transforms and corresponding property names to pass the Transform positions to
* **Text Sources:** List of UI Input Fields and corresponding property names to pass the input fields values to. Values will be cast to `float` so make sure to only enter numbers into them
* **Udon Variables**: List of Udon Behaviours, their variables and corresponding property names to pass their values to. Values will be cast to the provided type. Allowed types: `float`, `int`, `Color`, `Vector2/3/4`

**Usage / Examples**

One of the most common examples would be to use `ShaderFeeder` as a way to pass some object's position to a shader. Player position being one of those, if used in combination with [UniversalTracker in a Player Base Tracking mode](misc-behaviours#universal-tracker).

To do that you will want to add a Transform that follows the Player to the Transform Sources list and choose a property to pass the value too and that's it!

Another popular choice would be to make an object do something on beat, in that case all you need is the Cycle Start Time and Cycle Length to be passed every Update Cycle. Check the Custom Update Rate and select Per Minute as the mode with your target BPM, and you're done!

The `ShaderFeeder` demo scene includes 2 basic shaders for things mentioned above: a Light Bridge of sorts that tracks a transform, and a BPM synced pulse with adjustable hue.

You can also read any public variable of an Udon Behaviour and pass it to the material using the Udon Variables list. Its extremely useful when building game logic driven effects, and I highly encourage you to play around with it!

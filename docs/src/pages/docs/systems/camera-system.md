---
description: >-
  A Depth of Field camera system with VR Friendly physical controls and a
  desktop mode
---

# Camera System

![](/img/docs/camera_system_header.png)

## Quick Setup Guide

* Follow the general [installation guide](../#requirements)
* Import the PostProcessing package by clicking `Window` -> `Package Manager` in Unity and searching for `PostProcessing`
* Create an empty object in your scene where you want your camera to be spawned
* Click on `Window` -> `UdonToolkit` -> `Camera System Setup` in the unity top bar
* Drag and drop the created empty into the Camera Position field
* Provide a watermark overlay if you want to draw a watermark on top of the pictures people take, e.g. the world logo

{% callout type="warning" %}
The watermark should be a transparent 16:9 image (1920x1080)
{% /callout %}

* You can also add one of the default guide panels near the camera by checking "Add Camera Guide". This option is recommended, so your players know how to use the system
* Click `Setup Layers`
* Click `Create Camera System`
* Click on the created `Camera Lens` object and there `Compile All Udon Sharp Programs` at the bottom of the Udon Behaviour component
* You're done!
* If the positioning isn't exactly how you like it: `Ctrl+Z`, move the empty object and click `Create Camera System` again

Now you can build your project, and the camera should be working!

![](</img/docs/image (37).png>)

## Full Documentation

![](</img/docs/image (2).png>)

### **Customization tips**

* You can change the watermark after the creation of Camera System via the setup script by adjusting the material on `Camera Tracker` -> `Sphere` object
*   You can change Focus range by adjusting `Focus Distance` in the `Camera Lens Profiles` -> `Camera Lens PP Far` object's PostProcessing settings

    > When adjusting Focus range, don't forget to change the `Auto Focus Distance` on the `Camera Lens` object
* You can adjust minimum focal length by changing the Focal Length in the `Camera Lens Profiles` -> `Camera Lens PP Focal` object's PostProcessing settings
* You can adjust maximum focal length by changing the Focal Length in the `Camera Lens Profiles` -> `Camera Lens PP` object's PostProcessing settings
* You can adjust finger sphere's size and look by changing the `Camera Left Finger` -> `Sphere` object

### **Parameters**

* **Active**: Specifies whether the camera is actively responding to inputs
* **Lens Animator**: An animator that controls Camera PostProcessing profiles
* **Camera Animator**: The main animator that controls Camera UI
* **Pickup**: The camera pickup component
* **Auto Focus**: Specifies whether the auto focus is currently enabled
* **Auto Focus Distance**: Specifies the auto focus max distance (should correspond to the `Camera Lens PP Far` focus distance setting)
* **User Interface**
  * **Desktop UI**: The UI Canvas for Desktop players
  * **VR UI**: The UI Canvas for VR Players
  * **Focus/Focal/Zoom Sliders**: The Focus/Focal/Zoom UI Sliders for Desktop
  * **VR Focus/Focal/Zoom Sliders**: The Focus/Focal/Zoom UI Sliders for VR
  * **Focus/Focal/Zoom Text**: The Focus/Focal/Zoom UI Text that is adjusted to display the currently active control
  * **Focus/Focal/Zoom BG**: The Focus/Focal/Zoom UI Text that is adjusted to display the currently active control
  * **Position Text**: UI Text that displays if the camera exists in World or Player relative space
* **VR Controls**
  * **Vr Finger Reference**: Reference to the finger tracking object to be used for VR interactions
  * **Sphere Radius**: The size of the sphere triggers
  * **Tutorial Sphere**: The position of the tutorial pass sphere trigger
  * **Flip Sphere**: The position of the camera flip sphere trigger
  * **Always On Sphere**: The position of the always on sphere trigger
  * **Focus Sphere**: The position of the autofocus sphere trigger
  * **World Space Sphere**: The position of the world space / local space switch sphere trigger
* **View Sphere**: The Camera View sphere which override's player's desktop view or camera view (based on type of player)
* **Camera Object**: Object that has a Camera component on it
* **Visuals**: The camera visual mesh
* **Finger Sphere**: The Mesh Renderer of the finger collider (`_Enabled` property will be set depending on whether the camera is held)
* **Auto Focus Icon**: Auto Focus Frame in Desktop UI that will be colored `Active Control Color` when enabled
* **Always On Icon**: Always On Frame in Desktop UO that will be colored `Active Control Color` when enabled
* **Active Control Color**: The color used in Desktop UI to signify that control is enabled
* **Drop Target**: The Transform camera will be reparented to when Player relative space lock is enabled
* **Playspace Tracker**: The Universal Tracker instance with `Track Playspace` enabled (camera will automatically call `ResetOffsets` on it whenever it is picked up and dropped)

### **Events**

* **SwitchPosition**: Switches between World and Player relative positioning
* **FlipCamera**: Flips the camera direction and screen
* **SwitchAF**: Toggles Auto Focus on and off
* **SwitchControl**: Switches between currently active controls (Focus/Focal/Zoom)
* **ToggleAlwaysOn**: Toggles the Always On mode (camera stays active on drop when Always On is enabled)
* **ToggleWatermark**: Toggles final image watermark on and off

### **Usage and Examples**

{% callout type="note" %}
You are expected to use `Camera System Setup` UI from `Window` -> `UdonToolkit` -> `Camera System Setup` in the unity top bar to set everything up, consult the top of this page for the required steps. But here is a guide on the manual setup
{% /callout %}

* Drag and drop `UT Camera System` from `UdonToolkit/Camera System` folder into the scene
* Right click and click `Unpack Prefab Completely` (that is very important)
* Move out all the objects inside `UT Camera System` into the root of the scene (also important)
* By default camera is set up to use layer 27 for PostProcessing, and layer 28 for the VR Controls
* VR Controls layer is expected to be set to only collide with itself, otherwise you might get unwanted vr control interactions even if you did not actually press anything with your finger
* Make sure those exist or change them
  * You can set objects inside `Camera Lens Profiles` to any layer that is not `PostProcessing` or `Default`
  * Don't forget to update the target layer of the `PostProcessing Layer` component on the `Camera Lens` -> `Lens Camera` object
  * When using a custom VR Controls layer, make sure to change tha layer of `Camera Left Finger`, as well as the `VR Controls` and `Start Sphere` objects
  * If you changed the VR Controls layer - you'll also need to update it in each `Sphere` object's Area Trigger `Collide With` parameter
* Click `Compile All UdonSharp Programs`
* Check that every UdonBehaviour in the system shows set public variables: check `Camera Lens`, any of the `VR Controls` -> `Sphere` objects, `Camera Tracker` object, any of the `Camera Flip`, `Always On`, `Focus` and `Switch Position` objects
  * If any of them show `No Public Variables` - click `Force Compile Script`
* Click on the `Camera Lens` object and uncheck `Is Active` on the `Parent Constraint`
* Move the Camera Lens wherever you want
* Click `Activate` on the `Parent Constraint`
* Uncheck `Is Active`, expand `Constraint Settings`
* Uncheck `Lock` and set all the `Position Offset` and `Rotation Offset` values to 0, check `Lock` again
* Check `Is Active`
* You should now be ready to use the camera in your world!

{% callout type="note" %}
This is a bit of an involved process, hence why the automated setup was created, the manual guide is provided for reference of what UdonToolkit is actually doing when you click **Create Camera System**
{% /callout %}

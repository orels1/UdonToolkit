---
title: Teleport Players
description: This guide describes 2 of the most common ways to teleport players
---

## Interact Teleport

_Teleport the player when they click on an object, e.g. a door knob_

* Add Sphere to your scene, this will act as your door knob
* Check the `Is Trigger` box on the Sphere Collider
* Add an Interact Trigger component to it and click "Convert to UdonBehaviour"

![](/img/docs/cJpmoaJDJM.gif)

* Create a new Empty GameObject and put it where you want your player to be teleported to
* Name it something nice, like "Room Teleport"
* Make sure you are in the Local view and rotate the object so the blue (Z) arrow is pointing where you want the player to look after the teleport

* Add a Teleporter component to this new Game Object and click "Convert to Udon Behaviour"
* Click "Teleport Player" and drag and drop the GameObject itself into the Teleport Target field

* Select the Sphere you added originally
* Click on the "Udon Events List" foldout in the Interact Trigger
* Drag and Drop the "Room Teleport" object onto the "Udon Events List" foldout header&#x20;
* It should now say "Udon Events List \[1]" and you should have your Room Teleport there with a `Trigger` event selected to the right

{% callout type="note" %}
That's it! Now when the player clicks on your sphere - they will get teleported!

Check out the [full documentation of the Teleporter](../behaviours/misc-behaviours#teleporter) to learn more about it
{% /callout %}

## Area Trigger Teleport

_Teleports the player when they enter a trigger_

* Add a new Empty Game object to the scene and position it roughly where you want your trigger to go, e.g. a portal entrance
* Name it something nice like "Portal Trigger"
* Click on the Layer dropdown on the top of the inspector and set it to MirrorReflection layer (to avoid issues with player's interaction laser)
* Add a Box Collider component to the Portal Trigger and check "Is Trigger" checkbox

![](</img/docs/image (25).png>)

* Set the collider size the way you need it
* Add an Area Trigger component and click "Convert To Udon Behaviour"
* Click "Collide with Players" and check "Collide with Local Players" checkbox

Now for the teleport exit

* Add a new Empty GameObject and call it something nice, for example "Portal Exit"
* Add a Teleporter component to it and click Teleport Players
* Drag and drop the Portal Exit object itself into the Teleport Target field

Now to connect it all

* Select your Portal Trigger and expand the Enter Events List foldout by clicking on it
* Drag and drop your Portal Exit object onto the Enter Events List foldout header
* It should appear in the list with `Trigger` selected as an event to send

{% callout type="note" %}
That's it! Now when the player enters the trigger - they will get teleported!

Check out the [full documentation of the Teleporter](../behaviours/misc-behaviours#teleporter) to learn more about it
{% /callout %}

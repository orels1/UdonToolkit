---
description: An example of setting up a mirror toggle via UI Button
---

# Mirror Toggle

* Add a VRC Mirror prefab, disable it to save performance by default
* Add a `Universal Action` to the Mirror object
* Make sure `Active` is checked, check `Fire Object Toggles`, click on the `Game Objects List` and click `Add Element`
* Drag your Mirror object into the empty field, select `Toggle` in the dropdown on the right

![](</img/docs/image (13).png>)

* Add a `UI -> Button` to your scene, set the `Render Mode` to `World Space` and scale it way down and place it where you want
* Add a `Ui Shape` component to the Canvas
* Change canvas layer to `Default`
* Click on your Button inside the Canvas and add a new `On Click()` event by clicking the small `+` icon in the bottom right
* Drag and Drop your Mirror object into the empty field that appeared
* In the dropdown next to `Runtime Only` select `UdonBehaviour -> SendCustomEvent` (its all the way at the bottom)
* Type `Trigger` in the text field

![](</img/docs/image (6).png>)

You're done! Now you can toggle your mirror via UI button in Udon.

{% callout type="warning" %}
The major thing to note here is to make sure that the component, and the game object its on, is always enabled if you're using the `Delay` parameter, otherwise it will never fire as the Update method will never be called.
{% /callout %}

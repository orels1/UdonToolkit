---
description: General info on UdonToolkit behaviours
---

# Behaviours Overview

{% callout type="note" %}
Udon Toolkit provides a selection of Udon Behaviours for you to utilize as building blocks in the world
{% /callout %}


Select a category of behaviours in the list on the left

![Sidebar Navigation](/img/docs/behaviours/overview/overview-image.png)

You'll see a list of all available behaviours in that category on the right

![In-Page Navigation](/img/docs/behaviours/overview/overview-image-1.png)

## How to read the docs

* **Parameters** list describes all the public variables and settings exposed to you, some of them are UI only, so if you're planning to modify them via other udon behaviours - make sure to check the actual `UdonBehaviour` component on the same object.
* **Events** list describes all the exposed custom events the component expects to receive. Most things that are expected to be "Triggered" in some way usually have a `Trigger` event exposed. The full list of events can also be seen at the bottom of the behaviour UI with a convenience button for you to use while testing in editor.
* **Usage / Examples** is a free-form section with some personal usage advice and other notes

## Overall approach

You can generally split the things below into `Triggers` and `Actions` (plus some extras), one group captures player interactions in some way - the other performs some actions in response.

Most of the time you would add a bunch of `Universal Action` behaviours to the scene and then trigger them from all the different trigger behaviours listed on this page. But this system can also easily interact with other things in your world outside of Udon Toolkit! So don't feel limited by the `Universal Action` and use the `Udon Events` list with other behaviours to mix and match stuff.

## Terms

* **Event** means Udon Custom Event
* **To Trigger** means to send a Custom Event or specifically send a `Trigger` custom event
* **Action** means some kind of reaction to your triggers, generally refers to using Universal Action behaviour

{% callout type="note" %}
Check out the included Demo scene for reference usage!
{% /callout %}


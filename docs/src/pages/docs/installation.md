---
title: Installation
description: Step-by-step guides to setting up UdonToolkit in your project.
---

There are a couple of ways to install UdonToolkit depending on the type of your project. This page will guide you through the process

---

## Using the VRChat Creator Companion

Use the button below

{% add-to-vcc /%}

Or add it manually by using the URL below:

```
https://orels1.github.io/UdonToolkit/index.json
```

If you're not familiar with how to add new repositories to the VCC - [check out the VCC docs](https://vcc.docs.vrchat.com/guides/community-repositories).

Open the Project Management screen for your project and add the "UdonToolkit" package.

That should all the necessary things to your project!

Check out these docs to learn more about the Behaviours and Attributes available! [The guides section](/docs/guides/guides-overview) might be a good place to start.

## For any other Unity project

- Download and import the latest release from the [releases page](https://github.com/orels1/UdonToolkit/releases) (the first .unitypackage file)
- Enjoy!

## Using the Unity Package Manager via git

{% callout type="warning" title="Advanced use-case" %}
This method is only recommended for advanced users due to fairly poor implementation of the Unity Package Manager.
{% /callout %}

You can also install UdonToolkit via the Unity Package Manager. You can use the following git URLs to install all the necessary packages package (in the order of dependencies):

### Required

```text
https://github.com/orels1/UdonToolkit.git?path=Packages/sh.orels.udontoolkit.inspector
```

```text
https://github.com/orels1/UdonToolkit.git?path=Packages/sh.orels.udontoolkit
```

### Optional

```text
https://github.com/orels1/UdonToolkit.git?path=Packages/sh.orels.udontoolkit.systems.presentation
```

```text
https://github.com/orels1/UdonToolkit.git?path=Packages/sh.orels.udontoolkit.systems.flight
```

## Learning about Attributes and Behaviours

You can find documentation for all the attributes [here](/docs/attributes/attributes-list). And all the Behaviours [here](/docs/behaviours/overview).

## Getting Updates

If you're updating from an old version, most of the time you can just get the latest release the same way you got the original package and that's it

---

## Getting help

If you're having trouble with the oacjage, you can ask for help in my [discord server](https://discord.gg/orels1).

If you are a tool developer yourself and want to file a code-specific issue with a bit more detail, drop by the github repo and [submit an issue there](https://github.com/orels1/UdonToolkit/issues/new)!

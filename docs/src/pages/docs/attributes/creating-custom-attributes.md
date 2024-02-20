---
description: >-
  Udon Toolkit can be extended with your own custom attributes! All you need is
  inherit from one of the classes provided below
---

# Creating Custom Attributes

### Example

Here is a sample of an attribute that achieves several goals

* Changes the background of the element to a yellow color
* Adds a header on top
* Adds a reset button on the bottom

The final look we want to achieve

![](</img/docs/image (60).png>)

Based on that - we need to do a couple of things

1. Create a new file for your attributes, let's say `MyCustomAttributes.cs` anywhere in your project
2. Add an `#if UNITY_EDITOR` at the beginning, followed by an empty line and an `#endif`
3. Between those you will create your new class Inheriting from the `UTPropertyAttribute`

```csharp
using UnityEditor;
using UnityEngine;
using System; // This in particular is very important
// As it will be used down th eline

#if UNITY_EDITOR
public class MyAttribute : UTPropertyAttribute {
}
#endif
```

Another  very important thing is that we need a separate version of the attribute for the VRChat's build system (otherwise we'll get build errors, and those aren't fun!)

1. Change an `#endif` to `#else`
2. Add a separate version of `MyAttribute` that inherits from `Attribute` (that is why we needed `using System;` earlier)

```csharp
using UnityEditor;
using UnityEngine;
using System; // This in particular is very important
// As it will be used down th eline

#if UNITY_EDITOR
public class MyAttribute : UTPropertyAttribute {
}
#else
// this just tells unity to use this attribute on fields
[AttributeUsage(AttributeTargets.Field)]
public class MyAttribute : Attribute {
  public MyAttribute() { // since we do not have any parameters
  // we create an empty constructor
  }
}
#endif
```

{% callout type="note" %}
The above process is required, because VRChat will need some version of the attribute to build the code. In this case we are using a dummy empty attribute as nothing we're doing here is important for the final build.

You can check the last third of the `UTAttributes` file to see how to handle attributes with parameters in a similar fashion
{% /callout %}

With that - let's add it to our behaviour code

1. Create a new UdonBehaviour and make a script
2. Inside of the script - make a new public variable, for example `public float someVariable`
3. Add the `MyAttribute` attribute to it

```csharp
public class CustomAttributeTest : UdonSharpBehaviour {
  [MyAttribute] public float someVariable;
  private void Start() {
  }
}
```

If you check the Behaviour now - nothing would change! It will just be a normal field.

Now we can override some methods to make it look different! Let's start with the things **above** or **before** the field. For this we'll use the `BeforeGUI` method.

1. Add a `public override void BeforeGUI(SerializedProperty property)` method
2. Inside of it - add our header label
3. Follow that with a new `GUI.color` assignment. Which will color everything that is rendered below it - with that color

```csharp
#if UNITY_EDITOR
public class MyAttribute : UTPropertyAttribute {
  public override void BeforeGUI(SerializedProperty property) {
    EditorGUILayout.LabelField("HEADER TEXT", EditorStyles.largeLabel);
    GUI.color = new Color(0.98f, 0.92f, 0.35f);
  }
}
#endif
```

You should now see the field with a header and colored with a shade of yellow!

![](</img/docs/image (23).png>)

The only problem is - if you have any other fields bellow the `someVariable` they will also be colored in this yellow color, as it will override everything it encounters. Try it out: let's add another variable below the `someVaribale`.

```csharp
public class CustomAttributeTest : UdonSharpBehaviour {
  [MyAttribute] public float someVariable;
  
  public bool anotherVariable;
  
  private void Start() {
  }
}
```

![](</img/docs/image (19).png>)

Now that's not really what we want. Let's fix that by adding another method override that would be called **below** or **after** the field. This one is called `AfterGUI`!

1. Add a `public override void AfterGUI(SerializedProperty property)` method
2. Reset the color back to white by assigning to the `GUI.color` again

```csharp
#if UNITY_EDITOR
public class MyAttribute : UTPropertyAttribute {
  public override void BeforeGUI(SerializedProperty property) {
    EditorGUILayout.LabelField("HEADER TEXT", EditorStyles.largeLabel);
    GUI.color = new Color(0.98f, 0.92f, 0.35f);
  }

  public override void AfterGUI(SerializedProperty property) {
    GUI.color = Color.white;
  }
}
#endif
```

Now its fixed and the 2nd property doesn't get colored.

![](</img/docs/image (3).png>)

The only thing left to do now is to add a button!

1. Add a `GUILayout.Button("Reset Value")` wrapped in an if
2. Reset the value of our property to `0` inside of that if

```csharp
#if UNITY_EDITOR
public class MyAttribute : UTPropertyAttribute {
  public override void BeforeGUI(SerializedProperty property) {
    EditorGUILayout.LabelField("HEADER TEXT", EditorStyles.largeLabel);
    GUI.color = new Color(0.98f, 0.92f, 0.35f);
  }

  public override void AfterGUI(SerializedProperty property) {
    GUI.color = Color.white;
    if (GUILayout.Button("Reset Value")) {
      property.floatValue = 0;
    }
  }
}
#endif
```

![](/img/docs/lIUNh1579P.gif)

And we're done! Read below to see what else you can do by overriding methods like `OnGUI` and `GetVisible`

{% callout type="warning" %}
One important thing to note here! You might've noticed that we used a `property.floatValue` to reset our value. That will only work for... well... variables that are of `float` type. If you'll want to work with a `bool` or an `int` or some other property type - you'll need to check the documentation for `serializedProperty` [to learn how to access and change those!](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/SerializedProperty.html)

For those who know about serialized properties - do not worry, you do not need to call `ApplyModifiedProperties` yourself - UdonToolkit's editor will do it for you!
{% /callout %}

{% callout type="note" %}
The final code is provided in the `Demo/CustomAttributeSample` folder, so feel free to use it as a starting point!
{% /callout %}

### UTPropertyAttribute

The base class for an attribute that would modify the display logic for a particular property.

It has a couple methods that you can override

#### GetVisible

`public virtual bool GetVisible(SerializedProperty property)`

This method determines if the property will be visible in the inspector. The attributes will be called from top to bottom, from left to right. As soon as at least one of the attributes returns `false` in the `GetVisible` method - the property won't be drawn and the code will proceed to the next one.

```csharp
// HideIf attribute implementation
public override bool GetVisible(SerializedProperty property) {
  isVisible = UTUtils.GetVisibleThroughAttribute(property, methodName, true);
  return isVisible;
}
```

#### BeforeGUI

`public virtual void BeforeGUI(SerializedProperty property)`

This method will be called right before drawing the field. It is useful for adding headers and other special info right above the actual field.

```csharp
// SectionHeader attribute implementation
public override void BeforeGUI(SerializedProperty property) {
  UTStyles.RenderSectionHeader(text);
}
```

#### OnGUI

`public virtual void OnGUI(SerializedProperty property)`

This method **replaces** the drawing of the serialized property with the logic provided within it.

{% callout type="note" %}
UT will always use the first provided OnGUI method. So if the property has multiple attributes that override OnGUI - only the first one will be called
{% /callout %}

```csharp
// Toggle attribute implementation
public override void OnGUI(SerializedProperty property) {
  var text = String.IsNullOrWhiteSpace(label) ? property.displayName : label;      
  property.boolValue = GUILayout.Toggle(property.boolValue, text, "Button");
}
```

#### AfterGUI

public virtual void AfterGUI(SerializedProperty property)

This method will be called right after drawing the field. You can add extra information, buttons or anything else that is related to the field above in here.

```csharp
// HelpBox attribute implementation
public override void AfterGUI(SerializedProperty property) {
  UTStyles.RenderNote(text);
}
```

### UTVisualAttribute

The base class for purely data-storing attributes that are generally used to alter general looks of the editor UI and not just the single field it is attached too. Things like `ListView` or `TabGroup` are inherited from `UTVisualAttribute`

While inheriting from this attribute does not do anything on its own - it is useful if you are planning to extend the `UTEditor` class itself.

Internally its just an empty C# attribute

```csharp
  /// <summary>
  /// These attributes are used to pass dat to custom logic in the UTEditor
  /// </summary>
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
  public class UTVisualAttribute : Attribute {
  }
```

---
title: 'Attributes List'
description: A collection of attributes to create custom UIs for your own behaviours
---


## Class Attributes

The following attributes are only available to use with your behaviour classes.

### CustomName

`[CustomName(string name)]`

Sets a custom name shown in the header bar, otherwise the header bar is hidden.

```csharp
[CustomName("My Fancy Controller")]
public class MyUdonSharpBehaviour : UdonSharpBehaviour {}
```

![](</img/docs/Custom Name.png>)

### HelpMessage

`[HelpMessage(string message)]`

Displays a box with some text below the controller's header bar and above the rest of the UI.\
Usually used to describe the purpose of the behaviour if it is not obvious, or to warn user about anything in particular.

```csharp
[HelpMessage("This behaviour should be enabled at all times")]
public class MyUdonSharpBehaviour : UdonSharpBehaviour {}
```

![](</img/docs/Help Message.png>)

### OnBeforeEditor

`[OnBeforeEditor(string methodName)]`

Calls the specified method every editor update loop before all the editor code was executed. The `SerializedObject` is passed to your method as a parameter which you can use to perform any necessary modifications.

```csharp
[OnBeforeEditor("BeforeEditor")]
public class MyUdonSharpBehaviour : UdonSharpBehaviour {
  public void BeforeEditor(SerializedObject obj) {
    // do stuff
  }
}
```

### OnAfterEditor

`[OnAfterEditor(string methodName)]`

Calls the specified method every editor update loop after all the editor code was executed. This will contain all the latest values before they are saved into the object. The `SerializedObject` is passed to your method as a parameter which you can use to perform any necessary modifications.

```csharp
[OnAfterEditor("AfterEditor")]
public class MyUdonSharpBehaviour : UdonSharpBehaviour {
  public void AfterEditor(SerializedObject obj) {
    // do stuff
  }
}
```

### OnValuesChanged

`[OnValuesChanged(string methodName)]`

Calls the specified method whenever any value has been changed by the user in the inspector. The `SerializedObject` is passed to your method as a parameter which you can use to perform any necessary modifications.

> If you need to react to a specific value change - use a field-level [`OnValueChanged` attribute](attributes-list#on-value-changed)

```csharp
[OnValuesChanged("ValuesChangeHandler")]
public class MyUdonSharpBehaviour : UdonSharpBehaviour {
  public void ValuesChangeHandler(SerializedObject obj) {
    // do stuff
  }
}
```

## Field Attributes

These attributes are meant to go on public fields of your Controller. They serve as UI building blocks and provide an ability to react to value changes, for example if you would like to do something specific in the scene when user checks a checkbox.

### **Attribute Order**

Some attributes affect whether the field is displayed or not, for those - order of the attributes is important. As soon as the editor encounters an attribute that hides a field - the field will be hidden no matter what the consecutive attributes return. The order is from the topmost attribute to the one right before the `public` keyword.

```csharp
// this will hide the whole field based on the value of `someBool`
[SectionHeader("Other Test")]
[HideIf("@someBool")]
public float var;

public bool someBool;
```

Many of these attributes can be combined to form more elaborate UI systems, so something like this is completely normal

```csharp
[OnValueChanged("HandleChange"]
[HelpBox("Value cannot be negative", "CheckValueValidity")]
[HideIf("@!transition")]
[HideLabel]
public float duration;
```

### Common Parameters

Some attributes share parameter names. One of such parameters is `methodName`. In most cases you will be able to pass either a method, or a variable name to it (where it makes sense, there are exceptions, and they are mentioned separately).

```csharp
// Will execute `ShouldShowBox` to determine whether the box should be visible
[HelpBox("", "ShouldShowBox")];
public bool active;

// Must return `bool`
public bool ShouldShowBox() {
  return active;
}
```

To use a variable instead of a method - prefix the variable name with `@`, you can also invert the variable value by adding `!` after that.

```csharp
// Will show if active is `true`
[HelpBox("", "@active")];
public bool active;

// Will show if active is `false`
[HelpBox("", "@!active")];
public bool active;
```

This can be any public variable of a class, doesn't have to be specifically the same variable. You can use something like `[HideInInspector]` for storing the state of the box without displaying it in the editor.

### SectionHeader

`[SectionHeader(string title)]`

Displays a visual separator box with some text

![](</img/docs/image (32).png>)

```csharp
[SectionHeader("General")]
public Transform sourceTransform;
```

### HelpBox

`[HelpBox(string message, [string methodName])]`

Displays a box with a help message. You can conditionally hide and show the box based on a method or variable. See [Common Parameters](attributes-list#common-parameters) for more info.

![](</img/docs/image (39).png>)

```csharp
// Always visible
[HelpBox("This option disables rotation transfer")]
public bool ignoreRotation;

// Only visible if provided variable is set to `true`
[HelpBox("This option disables rotation transfer", "@ignoreRotation")]
public bool ignoreRotation;

// Only visible if provided variable is set to `false`
[HelpBox("This option disables rotation transfer", "@!ignoreRotation")]
public bool ignoreRotation;

// Only visible if the provided method returns `true`
[HelpBox("This option disables rotation transfer", "ShowIgnoreHelp")]
public bool ignoreRotation;

public bool ShowIgnoreHelp() {
  return ignoreRotation;
}
```

### HideIf

`[HideIf(string methodName)]`

Hides the field based on the provided method or variable. See [Common Parameters](attributes-list#common-parameters) for more info.

![](/img/docs/VTKZHYxbDV.gif)

```csharp
public bool enableEffects = true;

// Hides the field if `enableEffects` is unchecked
[HideIf("@!enableEffects")]
public float effectsDuration;

public bool skipTransition;

// Hides the field if `skipTransition` is checked
[HideIf("This option disables rotation transfer", "@skipTransition")]
public float transitionDuration;

// Hides the fields if the provided method returns true
public bool enableEffects;
public float effectsDuration;

[HideIf("HideExtras")]
public bool extraOption1;

[HideIf("HideExtras")]
public bool extraOption2;

public bool HideExtras() {
  return enableEffects && effectsDuration > 0;
}
```

### HideLabel

`[HideLabel]`

Simply hides the field label and draws the value field only. Helpful in combinations with things like `Horizontal` attribute

![](</img/docs/image (33).png>)

```csharp
// Will show the field only
[HideLabel]
public GameObject active;
```

### Tab Group

`[TabGroup(string name, [string variableName])]`

Creates a tab system which only displays properties assigned to a particular tab.

![](</img/docs/image (41).png>)

{% callout type="warning" %}
All tabs will be displayed where the `[TabGroup]` will be first encountered. So if your first `[TabGroup]` is added to the 5th property of the object - the whole tab system will show up right above the 5th property. That is the current limitation of the system that might be adjusted at some point
{% /callout %}

```csharp
[TabGroup("Basics")]
public bool basicSettingA;

[TabGroup("Basics")]
public bool basicSettingB;

[TabGroup("Advanced")]
public float advancedSettingA;

[TabGroup("Advanced")]
public float advancedSettingB;

// you can have any amount of properties inside a tab
[TabGroup("Advanced")]
public Vector3 advancedSettingC;
```

Tabs support any kind of attributes within them, e.g. you can use a `[FoldoutGroup]` inside a tab

```csharp
[TabGroup("TabA")]
[FoldoutGroup("Stuff in Tab A")]
public bool vA;

[TabGroup("TabA")]
[FoldoutGroup("Stuff in Tab A")]
public bool vB;


[TabGroup("TabB")]
public float vC;
[TabGroup("TabB")]
public float vD;
```

You can also pass a `variableName` to the `[TabGroup]` to save the currently selected tab index to that variable. You only need to provide that option to the first instance of `TabGroup` in the file.

This is often used to create different "modes" for a behaviour which you can then use in the code.

![](/img/docs/YdvlaHKFo1.gif)

```csharp
[TabGroup("Collide With Objects", "collisionType")]
public LayerMask collideWith;

[TabGroup("Collide With Players")]
public bool collideWithLocalPlayers;
[TabGroup("Collide With Players")]
public bool collideWithRemotePlaers;

// We save the selected type into this variable to use in Udon code later
[HideInInspector]
public int collisionType;
```

### Foldout Group

`[FoldoutGroup(string name)]`

A general purpose foldout allowing you to nest properties inside of it

![](</img/docs/image (30).png>)

```csharp
[FoldoutGroup("Foldout")]
public float floatInFoldout;

[FoldoutGroup("Foldout")]
public bool boolInFoldout;

// You can nest other layout elements inside the FoldoutGroup
[FoldoutGroup("Foldout")]
[Horizontal("Horizontal In Foldout", true)]
public GameObject objInFoldout;

[FoldoutGroup("Foldout")]
[Horizontal("Horizontal In Foldout", true)]
public float floatInHorizontalInFoldout;

[FoldoutGroup("Foldout")]
[ListView("ListViewInFodlout")]
public UdonSharpBehaviour[] foldoutTargets;

[FoldoutGroup("Foldout")]
[ListView("ListViewInFodlout")]
[Popup("behaviour", "@foldoutTargets")]
public string[] foldoutEvents;
```

### OnValueChanged

`[OnValueChanged(string methodName)]`

Calls provided method when the value of the variable changes, so you can react to it in the UI.\
Has some special behaviour when used together with [ListView](attributes-list#list-view).

{% callout type="warning" %}
Due to limitations of Udon - the use of **#if !UDONSHARP\_COMPILER && UNITY\_EDITOR** is required for the handler methods
{% /callout %}

For regular fields the method signature should look like `public void MethodName(SerializedProperty value)`

```csharp
// Will call the provided method with the fresh value every time it changes
[OnValueChanged("ToggleLights")]

// Incoming value will be the new value, while the current variable will not be updated yet.
// This allows you to compare old and new values.
#if !COMPILER_UDONSHARP && UNITY_EDITOR
public void ToggleLights(SerializedProperty value) {
  // cast to the expected type first
  var val = value?.boolValue;
  if (value) {
    // do something here
  } else {
    // do something here
  }
}
#endif
```

For array type fields the method signature is `public void MethodName(SerializedProperty value, int index)`

```csharp
[OnValueChanged("HandleArrayChange")]
public string[] namesList;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
public void HandleArrayChange(SerializedProperty value, int index) {
  // if value is `null` - the value at `index` was removed
  if (value == null) {
    // handle removal
    return;
  }
  // if `index` is out of range of the current array - a new value was added
  if (index == namesList.length) {
    // handle addition of new value
    return;
  }
  // handle change of an existing value
}
#endif
```

When used with `ListView` the method signature should be different: `public void MethodName(object lValue, object rValue, int index)`, where `lValue` and `rValue` represent the left and right variable of the list view at the changed index.

You only need to attach `[OnValueChanged]` to the first instance of a particular `ListView`.

```csharp
[OnValueChanged("HandleChange")
[ListView("Events List")]
public UdonBehaviour[] udonTargets;

[ListView("Events List")]
public string[] udonEvents;

// Handle a change of row values
#if !COMPILER_UDONSHARP && UNITY_EDITOR
public void HandleChange(SerializedProperty lValue, SerializedProperty rValue, int index) {
  // when the value is removed - both will be null
  if (lValue == null && rValue == null) {
    // handle the removal of a row at `index` here
    return;
  }
  // when the value is added - `index` will be out of range of the current list
  if (index == udonTargets.Length) {
    // handle addition of new row
    return;
  }
  var lCasted = (UdonBehaviour) lValue?.objectReferenceValue;
  var rCasted = rValue?.stringValue;
  // handle change of an existing values
}
#endif
```

If you wish to get a full value instead of just the changed elements in both Array types and the `ListView` powered blocks, you will need to use a different signature.

* For regular arrays: `public void MethodName(SerializedProperty[] value)`
* For `ListView` blocks: `public void MethodName(SerializedProperty[] lValue, SerializedProperty[] rValue)`

If you also want to update some other value on the same object inside the change handler function - you need to accept an incoming `SerializedObject` as well

* For regular arrays: `public void MethodName(SerializedObject obj, SerializedProperty[] value)`
* For `ListView` blocks: `public void MethodName(SeriazliedObject obj, SerializedProperty[] lValue, SerializedProperty[] rValue)`

You can then use it to update properties like `obj.FindProperty("someProp").floatValue = 0.01f`

```csharp
[OnValueChanged("HandleArrayChange")]
public string[] namesList;

// Log current array value as `Values: array[0], array[1]...`
#if !COMPILER_UDONSHARP && UNITY_EDITOR
public void HandleArrayChange(SerializedProperty[] value) {
  var casted = value.ToList();
  // don't forget to grab the value of needed type or cast it from i.objectReferenceType as <yourTargetType>
  Debug.LogFormat("Values: {0}", string.Join(", ", casted.Select(i => i.floatValue).ToArray()));
}
#endif

// Works for `ListView too
[OnValueChanged("Hand`leChange")
[ListView("Events List")]
public UdonBehaviour[] udonTargets;

[ListView("Events List")]
public string[] udonEvents;

// Log current `ListView` value as `Event: udonEvents[i], Target udonTargets[i].name`
#if !COMPILER_UDONSHARP && UNITY_EDITOR
public void HandleChange(SerializedProperty[] lValue, SerializedProperty[] rValue) {
  var cLeft = leftVal.ToList();
  var cRight = rightVal.ToList();
  var eventNames = new List<string>();
  for (int i = 0; i < cLeft.Count; i++) {
    var behName = cLeft[i].objectReferenceValue != null
      ? (cRight[i].objectReferenceValue as UdonBehaviour).name
      : "null";
    eventNames.Add($"Event: {cRight[i].stringValue}, Target: {behName}");
  }
  Debug.Log(string.Join("\n", eventNames));
}
#endif

[OnValueChanged("HandleSliderChange")] [RangeSlider(0, 2)]
public float someFloat = 0.2f;

public bool someBoolProp;

// Modify another property in response to a value change
#if !COMPILER_UDONSHARP && UNITY_EDITOR
public void HandleSliderChange(SerializedObject obj, SerializedProperty value) {
  obj.FindProperty("someBoolProp").boolValue = value.floatValue > 1;
}
#endif
```

{% callout type="note" %}
Check CustomUISample.cs for live examples!
{% /callout %}

### Horizontal

`[Horizontal(string groupName, [bool showHeader])]`

Combines multiple fields into a horizontal group based on the provided name.\
You can define variables in any order, they can even have other variables between them. The fetching is done purely by the group name.

![](</img/docs/image (45).png>)

You can also pass a `showHeader` bool to display the group name in the UI.

![](</img/docs/image (20).png>)

```csharp
// When using object types its pretty common to use a `[HideLabel]` attribute to make the UI cleaner
[Horizontal("Group")]
public GameObject varA;

[Horizontal("Group")]
public string varB;

// You can have many groups
[Horizontal("OtherGroup")]
public int varC;

[Horizontal("OtherGroup")]
public int varD;

// Display the group name in the UI
[Horizontal("I'm a group!", true)]
public int varE;

[Horizontal("I'm a group!")]
public int varF;

// You can have more than 2 elements in a group!
[Horizontal("I'm a group!")]
public int varG;

```

### ListView

`[ListView(string name, [string addMethodName], [string addButtonText])]`

Combines arrays into a connected list of elements.

This is a cornerstone attribute of UdonToolkit. Due to dictionaries not being exposed in Udon we have to split things into separate arrays which makes navigating logically connected pieces of data very annoying, as well as forcing you to manually keep track of having enough elements in both arrays.

ListView covers that use case and provides some extras when combined with the [Popup attribute](attributes-list#popup).

![](/img/docs/image.png)

```csharp
[ListView("Udon Events List")]
public UdonBehaviour[] udonTargets;

[ListView("Udon Events List")]
public string[] udonEvents;
```

You can provide a custom add method to have full control on how the arrays are populated. It is also a good way to create something in the scene, like an instance of a prefab or a basic GameObject and set it as the list value at the same time.

![](</img/docs/image (4).png>)

You only need to configure the extra parameters in the first instance of `ListView` for a particular group.

```csharp
// You can also customize the add button text via an extra argument
[ListView("Udon Events List", "AddEvent", "Add new Event")]
public UdonBehaviour[] udonTargets;

// No need to define extra configuration here, the first instance of `ListView` for that group name is going to be used
[ListView("Udon Events List")]
public string[] udonEvents;

// The addition is fully relegated to your custom method, you can do whatevery you find suitable in here
public void AddEvent() {
  var newTargets = udonTargets.ToList();
  newTargets.Add(null);
  udonTargets = newTargets.ToArray();

  var newEvents = events.ToList();
  newEvents.Add($"NewEvent_{events.Length}");
  events = newEvents.ToArray();
}
```

Combining it with a `[Popup]` attribute is the way this is used the most across Udon Toolkit's behaviours. Creating list of Udon events and Animator triggers becomes a matter of a single line of code that handles everything behind the scenes.

![](</img/docs/image (17).png>)

```csharp
[ListView("Udon Events List")]
public UdonBehaviour[] udonTargets;

// You can combine many attributes together, here we use `Popup` to automatically populate the events list for us
[ListView("Udon Events List")]
[Popup("behaviour", "@udonTargets", true)]
public string[] udonEvents;
```

You can read more about `[Popup]` and how it can be populated [right here](attributes-list#popup)

### RangeSlider

`[RangeSlider(float min, float max)]`

Provides a slider to control the value in a predefined range, supports both floats and ints.

![](</img/docs/image (27).png>)

```csharp
[RangeSlider(1, 10)]
public float floatValue;

// If the value is int - slider will auto snap to only int values
[RangeSlider(3, 20)
public int intValue;
```

### Popup

`[Popup(string methodName)]`

Shows a popup with options to choose from and support for multiple sources of data. Only supported on `string` and `int` variable types, as well as their array variants at this time.

![](</img/docs/image (50).png>)

If used on an `int` or `int[]` it will save the index of the selected option in the list - into your target variable. So on the image above - selecting `fizz` will save `2` into `popupVar`

You can provide a method, or a variable, to populate the popup options list. See [Common Parameters](attributes-list#common-parameters) for more info.

```csharp
// Use a variable to populate the popup options
[SectionHeader("Popups")] [Popup("@popupOptions")]
public string popupVar;

[NonSerialized] public string[] popupOptions = {"foo", "bar", "fizz", "buzz"};

// You can also use a method to calculate options dynamically
[SectionHeader("Popups")] [Popup("GetOptions")]
public string popupVar;

public string[] GetOptions() {
  return new [] { "foo", "bar", "fizz", "buzz" };
}

// The method can accept a serialized property if you want to dynamically generate the options
#if !COMPILER_UDONSHARP && UNITY_EDITOR
public string[] GetOptions(SerializedProperty prop) {
  return new [] { $"options for {prop.name}" };
}
#endif
```

By providing an explicit `sourceType` other than `method` you can use the built-in Toolkit's ability to fetch a list of Animator Triggers, Udon Behaviour Custom Events or Shader Properties of a particular type. If there are no triggers or events available - the UI will inform you about it.

There are plans to add more sources in the future, like Material properties and other native elements.

You can combine this with [`[ListView]` attribute](attributes-list#list-view) to achieve dictionary-style results for Udon Behaviours. When used inside `ListView` - the popup method will be called with the `Serialized Property` of the current element on the opposing array (see example below).

![](</img/docs/image (29).png>)

```csharp
// a namespace to work with Lists
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
#endif
// We use `[Horizontal]` to visually connect the source and the popup
[Horizontal("AnimationTrigger")] [HideLabel]
public Animator animator;

// Using an Animator source type to auto fetch Triggers from the `animator` variable
[Horizontal("AnimationTrigger")] [Popup("animator, "@animator", true)]
public string somePopupVar;

[Horizontal("BehaviourTrigger")] [HideLabel]
public UdonBehaviour behaviour;

// Using an UdonBehaviour source type to auto fetch Custom Events from the `behaviour` variable
[Horizontal("BehaviourTrigger")] [Popup("behaviour", "@behaviour", true)]
public string behaviourPopupVar;

[Horizontal("ShaderProperty")] [HideLabel]
public Shader shader;

// Using a Shader source type to auto fetch `float` Shader Properties from the `shader` variable
[Horizontal("ShaderProperty")] [Popup("shader", "@shader", true)]
public string shaderPopupVar;

// You can also specify which type of shader proeprty to grab
[Popup("shader", "@shader","vector", false)]
public string shaderPopupVectorVar;

[ListView("Test List")]
public Transform[] transforms;

[ListView("Test List")]
[Popup("GetChildrenOptions")]
public string[] selectedChildren;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
// prop in this case is the element of `transforms` array on the current index
public string[] GetChildrenOptions(SerializedProperty prop) {
  var transObj = (Transform) prop.objectReferenceValue;
  if (transObj == null) {
    return new[] { "-- no transform set --"};
  }
  var childList = new List<string>();
  for (int i = 0; i < transObj.childCount; i++) {
    var child = transObj.GetChild(i);
    childList.Add(child.name);
  }

  return childList.ToArray();
}
#endif
```

### Toggle

`[Toggle([string label])]`

Toggle-type button for boolean fields.

![](/img/docs/HF2QkJzD5O.gif)

Combines well with things like [`[HideIf]` attribute](attributes-list#hideif) to toggle parts of the UI on and off to hide things that are unused in a particular toggle state.

```csharp
// Use the variable name as the toggle label
[Toggle]
public bool addSFX;

// Combine with [HideIf] to hide a variable that is irrelevant if the toggle is `false`
[HideIf("@!addSFX")]
public float someThirdVar;

// Use a custom toggle label
[Toggle("Custom Label")]
public bool extraToggle;
```

### Disabled

`[Disabled([string methodName])]`

Disables the editing of the provided field completely, or based on the provided `methodName`. Useful when manually populating fields via editor scripts (like with `OnValueChanged`) or in runtime.

When used with `ListView` it should be put on the first field in the list view.

```csharp
// Make a field read only
[Disabled]
public bool toggleThings;

// Make a field read only based on a method return value
[Disabled("GetDisabled")]
public float disabledFloat;

public bool GetDisabled() {
  return gameObject.activeSelf;
}

public bool allowEditing;

// Make a list view read only
[ListView("Stuff")][Disabled("@!allowEditing")]
public Transform[] objects;

[ListView("Stuff")]
public string[] objectDescriptions;
```

## Method Attributes

These attributes are meant to be added to your public methods and are handled separately. The UI for these is rendered after the main inspector is done, so they will always be at the bottom.

### Button

`[Button(string text)]`

Displays a button that calls the specified method in editor only. For example if you want to write some setup logic - you can use this attribute to provide a handy button for it.

![](</img/docs/image (31).png>)

```csharp
[Button("Helper Button")]
public void CustomButton() {
  Debug.Log("Pressed the Helper Button");
}
```

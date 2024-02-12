using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CustomUISample : UdonSharpBehaviour {
  [TabGroup("Basics")]
  [SectionHeader("Section Header")] public string someVal;
  
  [TabGroup("Basics")]
  [SectionHeader("Range Slider")] [RangeSlider(10, 20)]
  public float floatValue;

  [TabGroup("Basics")]
  [RangeSlider(1, 10)] public int intValue;

  [TabGroup("Basics")]
  [SectionHeader("Hide Label")] [HideLabel]
  public GameObject fieldWithNoLabel;

  [TabGroup("Basics")]
  [SectionHeader("Toggles")] [Toggle("Add SFX")]
  public bool hasSFX;

  [TabGroup("Basics")]
  [OnValueChanged("TestChange")]
  [HelpBox("Some helpful text", "TestBoxCondition")]
  [HideIf("@!hasSFX")]
  [RangeSlider(0, 10)]
  public float someThirdVar;

  [TabGroup("Basics")]
  [Toggle("Custom Label")] public bool extraToggle;

  [TabGroup("Basics")]
  [SectionHeader("Help Box")] [HelpBox("This checkbox shows extra options via [HideIf] attribute")]
  public bool helpBoxCheck;

  [TabGroup("Basics")]
  [HelpBox("This is only visible when option is checked", "@helpBoxExtra")] [HideIf("@!helpBoxCheck")]
  public bool helpBoxExtra;

  [TabGroup("Basics")]
  [SectionHeader("On Value Changed")]
  [OnValueChanged("UpdateDiffValue")]
  [HelpBox("This slider will update an editor only checkbox below")]
  [RangeSlider(0, 10)]
  public float valueChangeSlider;

  [TabGroup("Basics")]
  public bool isAbove5;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void UpdateDiffValue(SerializedProperty value) {
    var casted = value.floatValue;
    value.serializedObject.FindProperty("isAbove5").boolValue = casted > 5;
  }
#endif

  [TabGroup("Advanced")]
  [OnValueChanged("LogArrayValues")]
  [HelpBox("OnValueChanged has a separate signature for ListView and Array types you can use")]
  public float[] valueChangeArray;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void LogArrayValues(SerializedProperty[] value) {
    var casted = value.ToList();
    Debug.LogFormat("Values: {0}", string.Join(", ", casted.Select(i => i.floatValue).ToArray()));
  }
#endif

  [TabGroup("Advanced")]
  [Horizontal("Group", true)] public GameObject varA;
  
  [TabGroup("Advanced")]
  [Horizontal("Group")] public string varB;

  [TabGroup("Advanced")]
  [Horizontal("Group")] public int varC;

  [TabGroup("Advanced")]
  [Horizontal("Group")] public int varD;

  [TabGroup("Advanced")]
  [SectionHeader("Popup")] [Horizontal("AnimationTrigger")]
  public Animator animator;

  [TabGroup("Advanced")]
  [Horizontal("AnimationTrigger")] [Popup("animator", "@animator", true)]
  public string somePopupVar;

  [TabGroup("Advanced")]
  [Horizontal("BehaviourTrigger")] public UdonSharpBehaviour behaviour;

  [TabGroup("Advanced")]
  [Horizontal("BehaviourTrigger")] [Popup("behaviour", "@behaviour", true)]
  public string behaviourPopupVar;

  [TabGroup("Advanced")]
  [SectionHeader("List View")] [OnValueChanged("EventAdded")] [ListView("EventsList", "AddEvent", "Add Event")]
  public string[] events;

  [TabGroup("Advanced")]
  [ListView("EventsList")] public UdonSharpBehaviour[] targets;

  [TabGroup("Advanced")] [ListView("EventsList")]
  public float[] probabilities;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void EventAdded(SerializedProperty[] leftVal, SerializedProperty[] rightVal, SerializedProperty[] lastVal) {
    var cLeft = leftVal.ToList();
    var cRight = rightVal.ToList();
    var cLast = lastVal.ToList();
    var eventNames = new List<string>();
    for (int i = 0; i < cLeft.Count; i++) {
      var behName = cRight[i].objectReferenceValue != null
        ? (cRight[i].objectReferenceValue as UdonSharpBehaviour).name
        : "null";
      eventNames.Add($"Event: {cLeft[i].stringValue}, Target: {behName}, Float: {cLast[i].floatValue}");
    }

    Debug.Log(string.Join("\n", eventNames));
  }
#endif

  [TabGroup("Advanced")]
  [ListView("Udon Events List")] public UdonSharpBehaviour[] udonTargets;

  [TabGroup("Advanced")]
  [ListView("Udon Events List")] [Popup("behaviour", "@udonTargets", true)]
  public string[] udonEvents;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void AddEvent() {
    var newEvents = events.ToList();
    newEvents.Add($"NewEvent_{events.Length}");
    events = newEvents.ToArray();

    var newTargets = targets.ToList();
    newTargets.Add(null);
    targets = newTargets.ToArray();
  }
#endif

  [TabGroup("Advanced")]
  [SectionHeader("Regular Arrays")]
  [HelpBox("UTController also automatically updates array editors with reordering and per item removal UI")]
  [OnValueChanged("BasicChanged")]
  public string[] basicArray;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void BasicChanged(SerializedProperty value, int index) {
    Debug.Log($"{value?.stringValue} at {index}");
  }
#endif

  [TabGroup("Advanced")]
  [HelpBox("Hide If also works on array types")]
  public bool showRegularArray;

  [TabGroup("Advanced")]
  [HideIf("@!showRegularArray")] public string[] regularArray;

  public bool HideThirdVar() {
    return true;
  }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void TestChange(SerializedProperty value) {
    var casted = value.floatValue;
  }
#endif

  public bool TestBoxCondition() {
    return someThirdVar > 2;
  }

  [TabGroup("Advanced")]
  [SectionHeader("Popups")] [Popup("GetOptions")]
  public string popupVar;

  public string[] GetOptions() {
    return new[] {"foo", "bar", "fizz", "buzz"};
  }

  [TabGroup("Advanced")]
  [SectionHeader("Disabled")] [Disabled] public float disabledField;

  [TabGroup("Advanced")]
  [FoldoutGroup("Foldout")] public float floatInFoldout;
  [TabGroup("Advanced")][FoldoutGroup("Foldout")] public bool boolInFoldout;

  [TabGroup("Advanced")]
  [FoldoutGroup("Foldout")]
  [Horizontal("Horizontal In Foldout", true)]
  public GameObject objInFoldout;

  [TabGroup("Advanced")]
  [FoldoutGroup("Foldout")]
  [Horizontal("Horizontal In Foldout", true)]
  public float floatInHorizontalInFoldout;

  [TabGroup("Advanced")]
  [FoldoutGroup("Foldout")]
  [ListView("ListViewInFodlout")]
  public UdonSharpBehaviour[] foldoutTargets;

  [TabGroup("Advanced")]
  [FoldoutGroup("Foldout")]
  [ListView("ListViewInFodlout")]
  [Popup("behaviour", "@foldoutTargets")]
  public string[] foldoutEvents;

  [SectionHeader("Non-Tab Members")] public float varOutsideOfTabs;

  [Button("Helper Button")]
  public void CustomButton() {
    Debug.Log("Pressed the Helper Button");
  }

  void Start() {
  }
}

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
  [SectionHeader("Section Header")] [UTEditor]
  public string someVal;

  [SectionHeader("Range Slider")] [RangeSlider(10, 20)] [UTEditor]
  public float floatValue;

  [RangeSlider(1, 10)] [UTEditor] public int intValue;

  [SectionHeader("Toggles")] [Toggle("Add SFX")]
  public bool hasSFX;

  [OnValueChanged("TestChange")]
  [HelpBox("Some helpful text", "TestBoxCondition")]
  [HideIf("@!hasSFX")]
  [RangeSlider(0, 10)]
  public float someThirdVar;

  [Toggle("Custom Label")] [UTEditor] public bool extraToggle;

  [SectionHeader("Help Box")] [HelpBox("This checkbox shows extra options via [HideIf] attribute")] [UTEditor]
  public bool helpBoxCheck;

  [HelpBox("This is only visible when option is checked", "@helpBoxExtra")] [HideIf("@!helpBoxCheck")] [UTEditor]
  public bool helpBoxExtra;

  [SectionHeader("On Value Changed")]
  [OnValueChanged("UpdateDiffValue")]
  [HelpBox("This slider will update an editor only checkbox below")]
  [RangeSlider(0, 10)]
  [UTEditor]
  public float valueChangeSlider;

  [UTEditor] public bool isAbove5;

  #if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void UpdateDiffValue(SerializedProperty value) {
    var casted = value.floatValue;
    value.serializedObject.FindProperty("isAbove5").boolValue = casted > 5;
  }
  #endif

  [OnValueChanged("LogArrayValues")]
  [HelpBox("OnValueChanged has a separate signature for ListView and Array types you can use")]
  [UTEditor]
  public float[] valueChangeArray;

  #if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void LogArrayValues(SerializedProperty[] value) {
    var casted = value.ToList();
    Debug.LogFormat("Values: {0}", string.Join(", ", casted.Select(i => i.floatValue).ToArray()));
  }
  #endif

  [SectionHeader("Horizontal")] [Horizontal("Group")] [HideLabel]
  public GameObject varA;

  [Horizontal("Group")] [HideLabel] public string varB;

  [Horizontal("OtherGroup")] [UTEditor] public int varC;

  [Horizontal("OtherGroup")] [UTEditor] public int varD;

  [SectionHeader("Popup")] [Horizontal("AnimationTrigger")] [HideLabel] [UTEditor]
  public Animator animator;

  [Horizontal("AnimationTrigger")] [Popup("animator", "@animator", true)]
  public string somePopupVar;

  [Horizontal("BehaviourTrigger")] [HideLabel] [UTEditor]
  public UdonSharpBehaviour behaviour;

  [Horizontal("BehaviourTrigger")] [Popup("behaviour", "@behaviour", true)]
  public string behaviourPopupVar;

  [SectionHeader("List View")]
  [OnValueChanged("EventAdded")]
  [ListView("EventsList", "AddEvent", "Add Event")]
  public string[] events;

  [ListView("EventsList")] public UdonSharpBehaviour[] targets;

  #if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void EventAdded(SerializedProperty[] leftVal, SerializedProperty[] rightVal) {
    var cLeft = leftVal.ToList();
    var cRight = rightVal.ToList();
    var eventNames = new List<string>();
    for (int i = 0; i < cLeft.Count; i++) {
      var behName = cRight[i].objectReferenceValue != null
        ? (cRight[i].objectReferenceValue as UdonSharpBehaviour).name
        : "null";
      eventNames.Add($"Event: {cLeft[i].stringValue}, Target: {behName}");
    }

    Debug.Log(string.Join("\n", eventNames));
  }
  #endif

  [ListView("Udon Events List")] public UdonSharpBehaviour[] udonTargets;

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

  [SectionHeader("Regular Arrays")]
  [HelpBox("UTController also automatically updates array editors with reordering and per item removal UI")]
  [OnValueChanged("BasicChanged")]
  public string[] basicArray;

  #if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void BasicChanged(object value, int index) {
    Debug.Log(index);
    Debug.Log(((SerializedProperty) value)?.stringValue);
  }
  #endif

  [HelpBox("Hide If also works on array types")] [UTEditor]
  public bool showRegularArray;

  [HideIf("@!showRegularArray")] [UTEditor]
  public string[] regularArray;

  public bool HideThirdVar() {
    return true;
  }

  #if !COMPILER_UDONSHARP && UNITY_EDITOR
  public void TestChange(object value) {
    var casted = ((SerializedProperty) value)?.floatValue;
    var actualVal = Convert.ToSingle(casted);
  }
  #endif

  public bool TestBoxCondition() {
    return someThirdVar > 2;
  }

  [SectionHeader("Popups")] [Popup("GetOptions")] [UTEditor]
  public string popupVar;

  public string[] GetOptions() {
    return new[] {"foo", "bar", "fizz", "buzz"};
  }

  [Button("Helper Button")]
  public void CustomButton() {
    Debug.Log("Pressed the Helper Button");
  }

  void Start() {
  }
}
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

// This is a UTController based example that showcases all of the current attributes in action, as well as combinations of them
// Use this as a reference for the documentation available on github: https://github.com/orels1/UdonToolkit/wiki/Attributes

namespace UdonToolkit {
  [CustomName("Custom UI Sample")]
  public class UTTestController : UTController {
    [SectionHeader("Section Header")] [UdonPublic]
    public string someVal;

    [UdonPublic("someCustomUdonVar")]
    public int someOtherVal;

    [SectionHeader("Range Slider")]
    [RangeSlider(10, 20)] [UdonPublic]
    public float floatValue;

    [RangeSlider(1, 10)] [UdonPublic]
    public int intValue;

    [SectionHeader("Toggles")] [Toggle("Add SFX")]
    public bool hasSFX;
    
    [OnValueChanged("TestChange")]
    [HelpBox("Some helpful text", "TestBoxCondition")]
    [HideIf("@!hasSFX")]
    [RangeSlider(0, 10)]
    public float someThirdVar;

    [Toggle("Custom Label")] [UdonPublic]
    public bool extraToggle;

    [SectionHeader("Help Box")]
    [HelpBox("This checkbox shows extra options via [HideIf] attribute")]
    [UdonPublic]
    public bool helpBoxCheck;

    [HelpBox("This is only visible when option is checked", "@helpBoxExtra")]
    [HideIf("@!helpBoxCheck")]
    [UdonPublic]
    public bool helpBoxExtra;

    [SectionHeader("On Value Changed")]
    [OnValueChanged("UpdateDiffValue")]
    [HelpBox("This slider will update an editor only checkbox below")]
    [RangeSlider(0, 10)]
    [UdonPublic]
    public float valueChangeSlider;

    [UTEditor]
    public bool isAbove5;

    public void UpdateDiffValue(object value) {
      var casted = ((SerializedProperty) value)?.floatValue;
      if (casted == null) return;
      isAbove5 = casted > 5;
    }

    [OnValueChanged("LogArrayValues")]
    [HelpBox("OnValueChanged has a separate signature for ListView and Array types you can use")]
    [UdonPublic]
    public float[] valueChangeArray;

    public void LogArrayValues(object value) {
      var casted = (value as SerializedProperty[]).ToList();
      Debug.LogFormat("Values: {0}", string.Join(", ", casted.Select(i => i.floatValue).ToArray()));
    }

    [SectionHeader("Horizontal")] [Horizontal("Group")] [HideLabel]
    public GameObject varA;

    [Horizontal("Group")] [HideLabel] public string varB;

    [Horizontal("OtherGroup")] [UTEditor]
    public int varC;

    [Horizontal("OtherGroup")] [UTEditor] public int varD;

    [SectionHeader("Popup")] [Horizontal("AnimationTrigger")] [HideLabel] [UdonPublic]
    public Animator animator;

    [Horizontal("AnimationTrigger")] [Popup(PopupAttribute.PopupSource.Animator, "@animator", true)]
    public string somePopupVar;

    [Horizontal("BehaviourTrigger")] [HideLabel] [UdonPublic]
    public UdonBehaviour behaviour;

    [Horizontal("BehaviourTrigger")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@behaviour", true)]
    public string behaviourPopupVar;

    [SectionHeader("List View")]
    [OnValueChanged("EventAdded")]
    [ListView("EventsList", "AddEvent", addButtonText = "Add Event")]
    public string[] events;

    [ListView("EventsList")] public UdonBehaviour[] targets;

    public void EventAdded(object leftVal, object rightVal) {
      var cLeft = (leftVal as SerializedProperty[]).ToList();
      var cRight = (rightVal as SerializedProperty[]).ToList();
      var eventNames = new List<string>();
      for (int i = 0; i < cLeft.Count; i++) {
        var behName = cRight[i].objectReferenceValue != null
          ? (cRight[i].objectReferenceValue as UdonBehaviour).name
          : "null";
        eventNames.Add($"Event: {cLeft[i].stringValue}, Target: {behName}");
      }
      Debug.Log(string.Join("\n", eventNames));
    }

    [ListView("Udon Events List")] public UdonBehaviour[] udonTargets;

    [ListView("Udon Events List")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@udonTargets", true)]
    public string[] udonEvents;

    public void AddEvent() {
      var newEvents = events.ToList();
      newEvents.Add($"NewEvent_{events.Length}");
      events = newEvents.ToArray();

      var newTargets = targets.ToList();
      newTargets.Add(null);
      targets = newTargets.ToArray();
    }

    [SectionHeader("Regular Arrays")]
    [HelpBox("UTController also automatically updates array editors with reordering and per item removal UI")]
    [OnValueChanged("BasicChanged")] public string[] basicArray;

    public void BasicChanged(object value, int index) {
      Debug.Log(index);
      Debug.Log(((SerializedProperty) value)?.stringValue);
    }

    [HelpBox("Hide If also works on array types")] [UdonPublic]
    public bool showRegularArray;

    [HideIf("@!showRegularArray")] [UdonPublic] 
    public string[] regularArray;

    public bool HideThirdVar() {
      return true;
    }

    public void TestChange(object value) {
      var casted = ((SerializedProperty) value)?.floatValue;
      var actualVal = Convert.ToSingle(casted);
    }

    public bool TestBoxCondition() {
      return someThirdVar > 2;
    }

    public string[] GetPopupOptions() {
      if (animator == null) return new[] {"no triggers found"};
      if (animator.runtimeAnimatorController != null) {
        if (animator.GetCurrentAnimatorStateInfo(0).length == 0) {
          animator.enabled = false;
          animator.enabled = true;
          animator.gameObject.SetActive(true);
        }

        var found = animator.parameters.Where(p => p.type == AnimatorControllerParameterType.Trigger)
          .Select(x => x.name).ToArray();
        if (found.Length > 0) {
          return found;
        }
      }

      return new[] {"no triggers found"};
    }

    [SectionHeader("Popups")] [Popup("GetOptions")] [UdonPublic]
    public string popupVar;

    public string[] GetOptions() {
      return new [] { "foo", "bar", "fizz", "buzz" };
    }

    [Button("Helper Button")]
    public void CustomButton() {
      Debug.Log("Pressed the Helper Button");
    }
    
  }
}
#endif
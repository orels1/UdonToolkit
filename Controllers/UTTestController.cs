#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

namespace UdonToolkit {
  public class UTTestController : UTController {
    [SectionHeader("Something")] [UdonPublic]
    public string someVal;

    [SectionHeader("Some other thing")] [UdonPublic("someCustomUdonVar")]
    public int someOtherVal;

    [SectionHeader("Range Slider")]
    [RangeSlider(10, 20)] [UdonPublic]
    public float floatValue;

    [RangeSlider(1, 10)] [UdonPublic]
    public int intValue;

    [SectionHeader("SFX Controls")] [Toggle("Add SFX")]
    public bool hasSFX;
    
    [OnValueChanged("TestChange")]
    [HelpBox("Some helpful text", "TestBoxCondition")]
    [HideIf("@!hasSFX")]
    [RangeSlider(0, 10)]
    public float someThirdVar;

    [Toggle("Custom Label")] [UdonPublic] public bool extraToggle;

    [SectionHeader("Horizontal Group")] [Horizontal("Group")] [HideLabel]
    public GameObject varA;

    [Horizontal("Group")] [HideLabel] public string varB;

    [SectionHeader("Other Horizontal Group")] [Horizontal("OtherGroup")] [UTEditor]
    public int varC;

    [Horizontal("OtherGroup")] [UTEditor] public int varD;

    [SectionHeader("Animation Triggers")] [Horizontal("AnimationTrigger")] [HideLabel] [UdonPublic]
    public Animator animator;

    [Horizontal("AnimationTrigger")] [Popup(PopupAttribute.PopupSource.Animator, "@animator", true)]
    public string somePopupVar;

    [SectionHeader("Behaviour Triggers")] [Horizontal("BehaviourTrigger")] [HideLabel] [UdonPublic]
    public UdonBehaviour behaviour;

    [Horizontal("BehaviourTrigger")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@behaviour", true)]
    public string behaviourPopupVar;

    [SectionHeader("List View")]
    [OnValueChanged("EventAdded")]
    [ListView("EventsList", "AddEvent", addButtonText = "Add Event")]
    public string[] events;

    [ListView("EventsList")] public UdonBehaviour[] targets;

    public void EventAdded(object leftVal, object rightVal, int index) {
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

    [OnValueChanged("BasicChanged")] public string[] basicArray;

    public void BasicChanged(object value, int index) {
      Debug.Log(index);
      Debug.Log(((SerializedProperty) value)?.stringValue);
    }

    public bool HideThirdVar() {
      return true;
    }

    public void TestChange(object value) {
      var actualVal = Convert.ToSingle(value);
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
  }
}
#endif
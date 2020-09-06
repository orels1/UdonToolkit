#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Interact Trigger")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#interact-trigger")]
  public class InteractTriggerController : UTController {
    [SectionHeader("General")] [UdonPublic]
    public bool active = true;

    [OnValueChanged("SetInteract")] [UTEditor]
    public string interactionText = "Use";

    public void SetInteract(object value) {
      var val = ((SerializedProperty) value).stringValue;
      if (uB == null) return;
      uB.interactText = val;
    }

    [OnValueChanged("SetDistance")] [RangeSlider(0, 100)] [UTEditor]
    public float proximity = 2f;

    public void SetDistance(object value) {
      var val = ((SerializedProperty) value).floatValue;
      if (uB == null) return;
      uB.proximity = val;
    }

    [SectionHeader("Udon Events")] [UdonPublic]
    public bool networked;

    [UdonPublic] public NetworkEventTarget networkTarget;

    [ListView("Udon Events List")] [UdonPublic]
    public UdonBehaviour[] udonTargets;

    [ListView("Udon Events List")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@udonTargets", true)] [UdonPublic]
    public string[] udonEvents;

    [Button("Interact")]
    public void Interact() {
      if (uB == null) return;
      uB.Interact();
    }

    [Button("Activate")]
    public void Activate() {
      if (uB == null) return;
      uB.SendCustomEvent("Activate");
    }
    
    [Button("Deactivate")]
    public void Deactivate() {
      if (uB == null) return;
      uB.SendCustomEvent("Deactivate");
    }
    
    [Button("Toggle")]
    public void Toggle() {
      if (uB == null) return;
      uB.SendCustomEvent("Toggle");
    }
  }
}

#endif
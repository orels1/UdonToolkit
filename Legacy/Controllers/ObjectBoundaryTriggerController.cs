#if UNITY_EDITOR

using System;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Object Boundary Trigger")]
  [HelpMessage("This component tracks if objects cross ALL or ANY of the boundaries specified, " +
               "useful for checking if something is above / below a global threshold.\n" +
               "This will fire once and disable itself, send an \"Enable\" event to this component to re-enable the check")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#object-boundary-trigger")]
  public class ObjectBoundaryTriggerController: UTController {
    [SectionHeader("General")] [UdonPublic]
    public bool active = true;

    [UdonPublic] public Transform target;

    [HelpBox("Enabling cross mode will make the trigger run every time the setup conditions are met or not met anymore.\n" +
             "This is useful if you want to toggle something when the target object goes below some Y position and agan - when it comes back")]
    [UdonPublic] public bool crossMode;

    [Popup(PopupAttribute.PopupSource.Method, "@testOptions")] [UdonPublic]
    public string testMode;
    
    [NonSerialized] public string[] testOptions = {
      "ALL",
      "ANY"
    };

    [ListView("Boundary List")] [Popup(PopupAttribute.PopupSource.Method, "@compareOptions", true)] [UdonPublic]
    public string[] boundaryCompares;

    [NonSerialized] public string[] compareOptions = {
      "Above X",
      "Above Y",
      "Above Z",
      "Below X",
      "Below Y",
      "Below Z"
    };

    [ListView("Boundary List")] [UdonPublic]
    public float[] boundaryCoords;
    
    [SectionHeader("Udon Events")]
    [UdonPublic]
    public bool networked;

    [UdonPublic] public NetworkEventTarget networkTarget;

    [ListView("Udon Events List")] [UdonPublic]
    public UdonBehaviour[] udonTargets;

    [ListView("Udon Events List")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@udonTargets", true)] [UdonPublic]
    public string[] udonEvents;
    
    [Button("Trigger")]
    public void Trigger() {
      if (uB == null) return;
      uB.SendCustomEvent("Trigger");
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
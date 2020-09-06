using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Object Boundary Trigger")]
  [HelpMessage("This component tracks if objects cross ALL or ANY of the boundaries specified, " +
               "useful for checking if something is above / below a global threshold.\n" +
               "This will fire once and disable itself, send an \"Enable\" event to this component to re-enable the check")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#object-boundary-trigger")]
  public class ObjectBoundaryTrigger : UdonSharpBehaviour {
    [SectionHeader("General")] [UTEditor] public bool active = true;
    public Transform target;

    [HelpBox(
      "Enabling cross mode will make the trigger run every time the setup conditions are met or not met anymore.\nThis is useful if you want to toggle something when the target object goes below some Y position and agan - when it comes back")]
    [UTEditor]
    public bool crossMode;
    
    private string[] testOptions = {
      "ALL",
      "ANY"
    };

    [Popup("@testOptions")]
    [UTEditor]
    public string testMode = "ALL";

    [ListView("Boundary List")]
    [Popup("method", "@compareOptions", true)]
    [UTEditor]
    public string[] boundaryCompares;
    
    private string[] compareOptions = {
      "Above X",
      "Above Y",
      "Above Z",
      "Below X",
      "Below Y",
      "Below Z"
    };

    [ListView("Boundary List")] [UTEditor] public float[] boundaryCoords;

    public bool networked;
    public NetworkEventTarget networkTarget;

    [ListView("Udon Events List")] public UdonSharpBehaviour[] udonTargets;

    [ListView("Udon Events List")]
    [Popup("behaviour", "@udonTargets", true)]
    [UTEditor]
    public string[] udonEvents;

    private bool oldResult;

    private void Update() {
      if (!active) return;
      bool result = false;
      for (int i = 0; i < boundaryCompares.Length; i++) {
        if (i == 0) {
          result = CheckBoundary(boundaryCoords[i], boundaryCompares[i], target.position);
          continue;
        }

        if (testMode == "ALL") {
          result &= CheckBoundary(boundaryCoords[i], boundaryCompares[i], target.position);
        }
        else {
          result |= CheckBoundary(boundaryCoords[i], boundaryCompares[i], target.position);
        }
      }

      if (crossMode) {
        if (oldResult == result) return;
        FireTriggers();
        oldResult = result;
        return;
      }

      if (!result) return;
      FireTriggers();
      Deactivate();
    }

    [Button("Activate")]
    public void Activate() {
      active = true;
    }

    [Button("Deactivate")]
    public void Deactivate() {
      active = false;
    }

    [Button("Toggle")]
    public void Toggle() {
      active = !active;
    }

    private bool CheckBoundary(float boundary, string compare, Vector3 targetPosition) {
      switch (compare) {
        case "Above X":
          return targetPosition.x > boundary;
        case "Above Y":
          return targetPosition.y > boundary;
        case "Above Z":
          return targetPosition.z > boundary;
        case "Below X":
          return targetPosition.x < boundary;
        case "Below Y":
          return targetPosition.y < boundary;
        case "Below Z":
          return targetPosition.z < boundary;
        default:
          return false;
      }
    }

    [Button("Trigger")]
    public void Trigger() {
      FireTriggers();
    }

    private void FireTriggers() {
      for (int i = 0; i < udonTargets.Length; i++) {
        var uB = udonTargets[i];
        if (!networked) {
          uB.SendCustomEvent(udonEvents[i]);
          continue;
        }

        uB.SendCustomNetworkEvent(networkTarget, udonEvents[i]);
      }
    }
  }
}
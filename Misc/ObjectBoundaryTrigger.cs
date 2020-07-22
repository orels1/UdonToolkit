using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
    public class ObjectBoundaryTrigger : UdonSharpBehaviour {
      public bool active = true;
      public Transform target;
      public bool crossMode;
      public string testMode = "ALL";
      public string[] boundaryCompares;
      public float[] boundaryCoords;

      public bool networked;
      public NetworkEventTarget networkTarget;
      public Component[] udonTargets;
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

      public void Activate() {
        active = true;
      }

      public void Deactivate() {
        active = false;
      }

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

      public void Trigger() {
        FireTriggers();
      }
      
      private void FireTriggers() {
        for (int i = 0; i < udonTargets.Length; i++) {
          var uB = (UdonBehaviour) udonTargets[i];
          if (!networked) {
            uB.SendCustomEvent(udonEvents[i]);
            continue;
          }
          uB.SendCustomNetworkEvent(networkTarget, udonEvents[i]);
        }
      }
    }
}
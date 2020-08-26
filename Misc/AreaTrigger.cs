using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
    public class AreaTrigger : UdonSharpBehaviour {
      public bool active;
      public LayerMask collideWith;
      public bool networked;
      public NetworkEventTarget networkTarget;
      public Component[] enterTargets;
      public string[] enterEvents;
      public Component[] exitTargets;
      public string[] exitEvents;

      private int playerLayer = 9;
      private int playerLocalLayer = 10;
      private int collidersIn;
      
      public void Activate() {
        active = true;
      }

      public void Deactivate() {
        active = false;
      }

      public void Toggle() {
        active = !active;
      }
      
      private void OnTriggerEnter(Collider other) {
        if (!active) return;
        if (other == null) {
          if (collideWith == (collideWith | (1 << playerLayer)) ||
              collideWith == (collideWith | (1 << playerLocalLayer))) {
            if (collidersIn == 0) {
              FireTriggers("enter");
            }
            collidersIn++;
            return;
          }

          return;
        }
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (collidersIn == 0) {
            FireTriggers("enter");
          }
          collidersIn++;
        }
      }

      private void OnTriggerExit(Collider other) {
        if (!active) return;
        if (other == null) {
          if (collideWith == (collideWith | (1 << playerLayer)) ||
              collideWith == (collideWith | (1 << playerLocalLayer))) {
            if (collidersIn == 1) {
              FireTriggers("exit");
            }
            collidersIn--;
            return;
          }

          return;
        }
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (collidersIn == 1) {
            FireTriggers("exit");
          }
          collidersIn--;
        }
      }

      private void FireTriggers(string type) {
        var udonTargets = type == "enter" ? enterTargets : exitTargets;
        var udonEvents = type == "enter" ? enterEvents : exitEvents;
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
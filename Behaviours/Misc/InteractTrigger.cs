using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Interact Trigger")]
  [HelpURL("https://ut.orels.sh/behaviours/misc-behaviours#interact-trigger")]
    public class InteractTrigger : UdonSharpBehaviour {
      [SectionHeader("General")]
      public bool active = true;
      [SectionHeader("Udon Events")]
      public bool networked;
      public NetworkEventTarget networkTarget;
      [ListView("Udon Events List")]
      public UdonSharpBehaviour[] udonTargets;
      
      [ListView("Udon Events List")]
      [Popup("behaviour", "@udonTargets", true)]
      public string[] udonEvents;

      private Collider col;
      private void Start() {
        col = gameObject.GetComponent<Collider>();
      }

      public override void Interact() {
        if (!active) return;
        FireTriggers();
      }
      
      public void Activate() {
        if ((object) col != null) {
          col.enabled = true;
        }
        active = true;
      }
      
      public void Deactivate() {
        if ((object) col != null) {
          col.enabled = false;
        }
        active = false;
      }
      
      public void Toggle() {
        if ((object) col != null) {
          col.enabled = !col.enabled;
        }
        active = !active;
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

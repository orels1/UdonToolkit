using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Networked Trigger")]
  [HelpMessage(
    "This component waits for a \"Trigger\" custom event, e.g. from a UI Button, and calls a network event on all the provided behaviours.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#networked-trigger")]
    public class NetworkedTrigger : UdonSharpBehaviour {
      [SectionHeader("General")] [UTEditor]
      public NetworkEventTarget eventTarget;
      [ListView("Udon Events List")] [UTEditor]
      public UdonSharpBehaviour[] udonTargets;
      [ListView("Udon Events List")]
      [Popup("behaviour", "@udonTargets", true)]
      [UTEditor]
      public string[] udonEvents;

      [Button("Trigger")]
      public void Trigger() {
        for (int i = 0; i < udonTargets.Length; i++) {
          var uB = udonTargets[i];
          uB.SendCustomNetworkEvent(eventTarget, udonEvents[i]);
        }
      }
    }
}
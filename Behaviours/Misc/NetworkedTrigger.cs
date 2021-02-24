using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Networked Trigger")]
  [HelpMessage(
    "This component waits for a \"Trigger\" custom event, e.g. from a UI Button, and calls a network event on all the provided behaviours.")]
  [HelpURL("https://ut.orels.sh/behaviours/misc-behaviours#networked-trigger")]
    public class NetworkedTrigger : UdonSharpBehaviour {
      [SectionHeader("General")] 
      public NetworkEventTarget eventTarget;
      [ListView("Udon Events List")] 
      public UdonSharpBehaviour[] udonTargets;
      [ListView("Udon Events List")]
      [Popup("behaviour", "@udonTargets", true)]
      
      public string[] udonEvents;
      
      public void Trigger() {
        for (int i = 0; i < udonTargets.Length; i++) {
          var uB = udonTargets[i];
          uB.SendCustomNetworkEvent(eventTarget, udonEvents[i]);
        }
      }
    }
}

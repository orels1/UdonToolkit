using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
    public class NetworkedTrigger : UdonSharpBehaviour {
      public NetworkEventTarget eventTarget;
      public Component[] udonTargets;
      public string[] udonEvents;

      public void Trigger() {
        for (int i = 0; i < udonTargets.Length; i++) {
          var uB = (UdonBehaviour) udonTargets[i];
          uB.SendCustomNetworkEvent(eventTarget, udonEvents[i]);
        }
      }
    }
}
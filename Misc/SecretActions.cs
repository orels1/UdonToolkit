using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class SecretActions : UdonSharpBehaviour {
      public bool active = true;
      public string[] playerNames;
      public Component[] actions;

      private VRCPlayerApi player;

      private void Start() {
        if (!active) return;
        player = Networking.LocalPlayer;
        if (player == null) {
          active = false;
          return;
        }
        FireEvents();
      }

      private void FireEvents() {
        for (int i = 0; i < playerNames.Length; i++) {
          if (playerNames[i] != player.displayName) continue;
          var uB = (UdonBehaviour) actions[i];
          if (uB != null) {
            uB.SendCustomEvent("Trigger");
          }
        }
      }

      public void Trigger() {
        if (!active) return;
        FireEvents();
      }
    }
}
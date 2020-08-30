using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Secret Actions")]
  [HelpMessage("This component will send a \"Trigger\" event to specified behaviours based on player's display name.\n" +
               "This will only happen on Start, if you want it to fire again - send a \"Trigger\" event to this behaviour.\n" +
               "All events are sent locally, use Networked Trigger to make it global, e.g. for enabling something for everyone only if a particular player is present.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#secret-actions")]
    public class SecretActions : UdonSharpBehaviour {
      [SectionHeader("General")] [UTEditor]
      public bool active = true;
      
      [ListView("Actions List")] [UTEditor]
      public string[] playerNames;
      [ListView("Actions List")] [UTEditor]
      public UdonSharpBehaviour[] actions;

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
          var uB = actions[i];
          if (uB != null) {
            uB.SendCustomEvent("Trigger");
          }
        }
      }

      [Button("Trigger")]
      public void Trigger() {
        if (!active) return;
        FireEvents();
      }
    }
}
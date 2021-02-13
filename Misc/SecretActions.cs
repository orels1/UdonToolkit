using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Secret Actions")]
  [HelpMessage("This component will send a \"Trigger\" event to specified behaviours based on player's display name.\n" +
               "This will only happen on Start, if you want it to fire again - send a \"Trigger\" event to this behaviour.\n" +
               "All events are sent locally, use Networked Trigger to make it global, e.g. for enabling something for everyone only if a particular player is present.")]
  [HelpURL("https://ut.orels.sh/behaviours/misc-behaviours#secret-actions")]
    public class SecretActions : UdonSharpBehaviour {
      [SectionHeader("General")]
      public bool active = true;
      
      [ListView("Actions List")]
      public string[] playerNames;
      [ListView("Actions List")]
      public UdonSharpBehaviour[] targets;

      [ListView("Actions List")][Popup("behaviour", "@targets")] public string[] events;

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
          var uB = targets[i];
          if (uB != null) {
            uB.SendCustomEvent(events[i]);
          }
        }
      }
      
      public void Trigger() {
        if (!active) return;
        FireEvents();
      }
    }
}

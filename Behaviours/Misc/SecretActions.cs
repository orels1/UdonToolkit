using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Secret Actions")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#secret-actions")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class SecretActions : UdonSharpBehaviour {
    [SectionHeader("General")]
    public bool active = true;

    [ListView("Actions List")]
    public string[] playerNames;

    [ListView("Actions List")]
    public UdonSharpBehaviour[] targets;

    [ListView("Actions List")] [Popup("behaviour", "@targets")]
    public string[] events;

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

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlatformTrigger : UdonSharpBehaviour {
  public bool fireOnStart = true;
  public Component[] desktopTargets;
  public string[] desktopEvents;
  public Component[] vrTargets;
  public string[] vrEvents;

  private VRCPlayerApi player;

  private void Start() {
    player = Networking.LocalPlayer;
    if (!fireOnStart) return;
    FireTriggers();
  }
  
  public void Trigger() {
    FireTriggers();
  }

  private void FireTriggers() {
    if (player == null) return;
    if (!player.IsUserInVR()) {
      for (int i = 0; i < desktopTargets.Length; i++) {
        Debug.LogFormat("Sending {0} to {1}", desktopEvents[i], desktopTargets[i].gameObject.name);
        var uB = (UdonBehaviour) desktopTargets[i];
        uB.SendCustomEvent(desktopEvents[i]);
      }
    }
    else {
      for (int i = 0; i < vrTargets.Length; i++) {
        var uB = (UdonBehaviour) vrTargets[i];
        uB.SendCustomEvent(vrEvents[i]);
      }
    }
  }
}
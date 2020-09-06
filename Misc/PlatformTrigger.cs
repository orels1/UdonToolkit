using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace  UdonToolkit {
  [CustomName("Platform Trigger")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#platform-trigger")]
  [HelpMessage("This behaviour will send specified events on the Start of the world based on the current user platform")]
  public class PlatformTrigger : UdonSharpBehaviour {
    public bool fireOnStart = true;
    [ListView("Desktop Events")][UTEditor]
    public UdonSharpBehaviour[] desktopTargets;
    
    [ListView("Desktop Events")][UTEditor]
    [Popup("behaviour", "@desktopTargets", true)]
    [UTEditor]
    public string[] desktopEvents;
    
    [ListView("VR Events")][UTEditor]
    public UdonSharpBehaviour[] vrTargets;
    
    [ListView("VR Events")]
    [Popup("behaviour", "@vrTargets", true)]
    [UTEditor]
    public string[] vrEvents;

    private VRCPlayerApi player;

    private void Start() {
      player = Networking.LocalPlayer;
      if (!fireOnStart) return;
      FireTriggers();
    }
    
    [Button("Trigger")]
    public void Trigger() {
      FireTriggers();
    }

    private void FireTriggers() {
      if (player == null) return;
      if (!player.IsUserInVR()) {
        for (int i = 0; i < desktopTargets.Length; i++) {
          Debug.LogFormat("Sending {0} to {1}", desktopEvents[i], desktopTargets[i].gameObject.name);
          var uB = desktopTargets[i];
          uB.SendCustomEvent(desktopEvents[i]);
        }
      }
      else {
        for (int i = 0; i < vrTargets.Length; i++) {
          var uB = vrTargets[i];
          uB.SendCustomEvent(vrEvents[i]);
        }
      }
    }
  }
}

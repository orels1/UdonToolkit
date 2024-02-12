using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Platform Trigger")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#platform-trigger")]
  [HelpMessage(
    "This behaviour will send specified events on the Start of the world based on the current user platform")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class PlatformTrigger : UdonSharpBehaviour {
    public bool fireOnStart = true;
    [ListView("Desktop Events")] public UdonSharpBehaviour[] desktopTargets;

    [ListView("Desktop Events")] [Popup("behaviour", "@desktopTargets", true)]
    public string[] desktopEvents;

    [ListView("VR Events")] public UdonSharpBehaviour[] vrTargets;

    [ListView("VR Events")] [Popup("behaviour", "@vrTargets", true)]
    public string[] vrEvents;

    private VRCPlayerApi player;
    private bool starTriggersFired;

    private void Start() {
      player = Networking.LocalPlayer;
    }
    
    public void Trigger() {
      FireTriggers();
    }

    // We have to use the Update loop because VRC does not report the VR/Desktop state correctly in Start anymore
    private void Update() {
      if (!fireOnStart) return;
      if (Time.timeSinceLevelLoad > 3 && !starTriggersFired) {
        starTriggersFired = true;
        FireTriggers();
      }
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


using UdonSharp;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[CustomName("Start Trigger")]
[HelpMessage("You can also send a \"Trigger\" event to this behaviour to fire all the triggers again")]
[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class StartTrigger : UdonSharpBehaviour {
  [SectionHeader("General")]
  public bool active = true;
  [SectionHeader("Udon Events")]
  public bool networked;
  public NetworkEventTarget networkTarget;
  [ListView("Udon Events List")]
  public UdonSharpBehaviour[] udonTargets;
  
  [ListView("Udon Events List")]
  [Popup("behaviour", "@udonTargets", true)]
  public string[] udonEvents;

  void Start() {
    FireTriggers();
  }

  public void Trigger() {
    FireTriggers();
  }

  private void FireTriggers() {
    for (int i = 0; i < udonTargets.Length; i++) {
      var uB = udonTargets[i];
      if (!networked) {
        uB.SendCustomEvent(udonEvents[i]);
        continue;
      }
      uB.SendCustomNetworkEvent(networkTarget, udonEvents[i]);
    }
  }
}

#if UNITY_EDITOR
using System;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Networked Trigger")]
  [HelpMessage(
    "This component waits for a \"Trigger\" custom event, e.g. from a UI Button, and calls a network event on all the provided behaviours. " +
    "You can disable this object safely to save a bit of performance - it will still work as expected.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#networked-trigger")]
  public class NetworkedTriggerController : UTController {
    [SectionHeader("General")] [UdonPublic]
    public NetworkEventTarget eventTarget;
    
    [ListView("Udon Events List")] [UdonPublic]
    public UdonBehaviour[] udonTargets;

    [ListView("Udon Events List")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@udonTargets", true)] [UdonPublic]
    public string[] udonEvents;

    [Button("Trigger")]
    public void Trigger() {
      if (uB == null) return;
      uB.SendCustomEvent("Trigger");
    }

  }
}
#endif
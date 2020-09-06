#if UNITY_EDITOR

using System;
using UnityEngine;
using VRC.Udon;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Secret Actions")]
  [HelpMessage("This component will send a \"Trigger\" event to specified behaviours based on player's display name.\n" +
               "This will only happen on Start, if you want it to fire again - send a \"Trigger\" event to this behaviour.\n" +
               "All events are sent locally, use Networked Trigger to make it global, e.g. for enabling something for everyone only if a particular player is present.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#secret-actions")]
  public class SecretActionsController : UTController {
    [SectionHeader("General")] [UdonPublic]
    public bool active = true;

    [ListView("Actions List")] [UdonPublic] public string[] playerNames;

    [ListView("Actions List")] [UdonPublic]
    public UdonBehaviour[] actions;

    [Button("Trigger")]
    public void Trigger() {
      if (uB == null) return;
      uB.SendCustomEvent("Trigger");
    }
  }
}

#endif
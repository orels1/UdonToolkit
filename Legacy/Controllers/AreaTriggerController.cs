#if UNITY_EDITOR

using System;
using System.Security.Permissions;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Area Trigger")]
  [HelpMessage("It is recommended to put Area Triggers on a MirrorReflection layer unless they need a custom layer.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#area-trigger")]
  public class AreaTriggerController : UTController {
    [SectionHeader("General")]
    [HelpBox("This behaviour requires a trigger collider to be attached to the object", "CheckCollider")]
    [UdonPublic]
    public bool active = true;

    public bool CheckCollider() {
      var col = gameObject.GetComponent<Collider>();
      return col == null || !col.isTrigger;
    }

    [HelpBox(
      "It is impossible to distinguish between Local and Remote Players without putting the Area Trigger on a special layer that collides with either of those, keep that in mind when planning your triggers.",
      "PlayerLayerWarnings")]
    [HelpBox("It is not recommended to collide with Everything or the Default layer.", "CheckCollisionLayers")]
    [UdonPublic]
    public LayerMask collideWith;

    public bool CheckCollisionLayers() {
      var check = LayerMask.NameToLayer("Default");
      return collideWith == (collideWith | (1 << check)) || collideWith.value == ~0;
    }

    public bool PlayerLayerWarnings() {
      var player = LayerMask.NameToLayer("Player");
      var playerLocal = LayerMask.NameToLayer("PlayerLocal");
      var currL = gameObject.layer;
      return (collideWith == (collideWith | (1 << player)) || collideWith == (collideWith | (1 << playerLocal))) &&
             currL <= 22 && !(collideWith == (collideWith | (1 << player)) && collideWith == (collideWith | (1 << playerLocal)));
    }

    [SectionHeader("Udon Events")]
    [HelpBox("Do not use Networked option with target All when colliding with Player layer, as it will cause oversync.",
      "CheckNetworkedValidity")]
    [UdonPublic]
    public bool networked;

    [UdonPublic] public NetworkEventTarget networkTarget;

    public bool CheckNetworkedValidity() {
      var player = LayerMask.NameToLayer("Player");
      return collideWith == (collideWith | (1 << player)) && networked && networkTarget == NetworkEventTarget.All;
    }

    [ListView("Enter Events List")] [UdonPublic]
    public UdonBehaviour[] enterTargets;

    [ListView("Enter Events List")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@enterTargets", true)] [UdonPublic]
    public string[] enterEvents;
    
    [ListView("Exit Events List")] [UdonPublic]
    public UdonBehaviour[] exitTargets;

    [ListView("Exit Events List")] [Popup(PopupAttribute.PopupSource.UdonBehaviour, "@exitTargets", true)] [UdonPublic]
    public string[] exitEvents;

    [Button("Activate")]
    public void Activate() {
      if (uB == null) return;
      uB.SendCustomEvent("Activate");
    }
    
    [Button("Deactivate")]
    public void Deactivate() {
      if (uB == null) return;
      uB.SendCustomEvent("Deactivate");
    }
    
    [Button("Toggle")]
    public void Toggle() {
      if (uB == null) return;
      uB.SendCustomEvent("Toggle");
    }
  }
}

#endif
using UdonSharp;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
    [CustomName("Area Trigger")]
    [HelpMessage("It is recommended to put Area Triggers on a MirrorReflection layer unless they need a custom layer.")]
    [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#area-trigger")]
    public class AreaTrigger : UdonSharpBehaviour {
      [SectionHeader("General")]
      [HelpBox("This behaviour requires a trigger collider to be attached to the object", "CheckCollider")]
      [UTEditor]
      public bool active = true;
      
      private bool CheckCollider() {
        var col = gameObject.GetComponent<Collider>();
        return col == null || !col.isTrigger;
      }

      [HelpBox(
        "It is impossible to distinguish between Local and Remote Players without putting the Area Trigger on a special layer that collides with either of those, keep that in mind when planning your triggers.",
        "PlayerLayerWarnings")]
      [HelpBox("It is not recommended to collide with Everything or the Default layer.", "CheckCollisionLayers")]
      [UTEditor]
      public LayerMask collideWith;
      
      #if !COMPILER_UDONSHARP && UNITY_EDITOR
      private bool CheckCollisionLayers() {
        var check = LayerMask.NameToLayer("Default");
        return collideWith == (collideWith | (1 << check)) || collideWith.value == ~0;
      }

      private bool PlayerLayerWarnings() {
        var player = LayerMask.NameToLayer("Player");
        var playerLocal = LayerMask.NameToLayer("PlayerLocal");
        var currL = gameObject.layer;
        return (collideWith == (collideWith | (1 << player)) || collideWith == (collideWith | (1 << playerLocal))) &&
               currL <= 22 && !(collideWith == (collideWith | (1 << player)) && collideWith == (collideWith | (1 << playerLocal)));
      }
      #endif

      [SectionHeader("Udon Events")]
      [HelpBox("Do not use Networked option with target All when colliding with Player layer, as it will cause oversync.",
        "CheckNetworkedValidity")]
      [UTEditor]
      public bool networked;
      public NetworkEventTarget networkTarget;
      
      private bool CheckNetworkedValidity() {
        var player = LayerMask.NameToLayer("Player");
        return collideWith == (collideWith | (1 << player)) && networked && networkTarget == NetworkEventTarget.All;
      }

      [ListView("Enter Events List")] [UTEditor]
      public UdonSharpBehaviour[] enterTargets;
      
      [ListView("Enter Events List")]
      [Popup("behaviour", "@enterTargets", true)]
      [UTEditor]
      public string[] enterEvents;

      [ListView("Exit Events List")] [UTEditor]
      public UdonSharpBehaviour[] exitTargets;
      
      [ListView("Exit Events List")]
      [Popup("behaviour", "@exitTargets", true)]
      [UTEditor]
      public string[] exitEvents;

      private int playerLayer = 9;
      private int playerLocalLayer = 10;
      private int collidersIn;
      
      [Button("Activate")]
      public void Activate() {
        active = true;
      }

      [Button("Deactivate")]
      public void Deactivate() {
        active = false;
      }
      
      [Button("Toggle")]
      public void Toggle() {
        active = !active;
      }
      
      private void OnTriggerEnter(Collider other) {
        if (!active) return;
        if (other == null) {
          if (collideWith == (collideWith | (1 << playerLayer)) ||
              collideWith == (collideWith | (1 << playerLocalLayer))) {
            if (collidersIn == 0) {
              FireTriggers("enter");
            }
            collidersIn++;
            return;
          }

          return;
        }
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (collidersIn == 0) {
            FireTriggers("enter");
          }
          collidersIn++;
        }
      }

      private void OnTriggerExit(Collider other) {
        if (!active) return;
        if (other == null) {
          if (collideWith == (collideWith | (1 << playerLayer)) ||
              collideWith == (collideWith | (1 << playerLocalLayer))) {
            if (collidersIn == 1) {
              FireTriggers("exit");
            }
            collidersIn--;
            return;
          }

          return;
        }
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (collidersIn == 1) {
            FireTriggers("exit");
          }
          collidersIn--;
        }
      }

      private void FireTriggers(string type) {
        var udonTargets = type == "enter" ? enterTargets : exitTargets;
        var udonEvents = type == "enter" ? enterEvents : exitEvents;
        for (int i = 0; i < udonTargets.Length; i++) {
          var uB = (UdonSharpBehaviour) udonTargets[i];
          if (!networked) {
            uB.SendCustomEvent(udonEvents[i]);
            continue;
          }
          uB.SendCustomNetworkEvent(networkTarget, udonEvents[i]);
        }
      }
    }
}
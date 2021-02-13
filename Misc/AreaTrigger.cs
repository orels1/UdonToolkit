using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
    [CustomName("Area Trigger")]
    [HelpMessage("It is recommended to put Area Triggers on a MirrorReflection layer unless they need a custom layer.")]
    [HelpURL("https://ut.orels.sh/behaviours/misc-behaviours#area-trigger")]
    public class AreaTrigger : UdonSharpBehaviour {
      private bool CheckCollider() {
        var col = gameObject.GetComponent<Collider>();
        return col == null || !col.isTrigger;
      }
      
      [TabGroup("Collide With Objects")]
      [HelpBox(
        "Please use Collide With Local Players and Collide With Remote Players options instead of Player and PlayerLocal layers.",
        "PlayerLayerWarnings")]
      [HelpBox("It is not recommended to collide with Everything or the Default layer.", "CheckCollisionLayers")]
      [HideIf("HideLayerList")]
      public LayerMask collideWith;

      [TabGroup("Collide With Players")]
      public bool collideWithLocalPlayers;
      [TabGroup("Collide With Players")]
      public bool collideWithRemotePlayers;

      [SectionHeader("General")]
      [HelpBox("This behaviour requires a trigger collider to be attached to the object", "CheckCollider")]
      public bool active = true;
      
      private bool HideLayerList() {
        return collideWithLocalPlayers || collideWithRemotePlayers;
      }
      
      #if !COMPILER_UDONSHARP && UNITY_EDITOR
      private bool CheckCollisionLayers() {
        var check = LayerMask.NameToLayer("Default");
        return collideWith == (collideWith | (1 << check)) || collideWith.value == ~0;
      }

      private bool PlayerLayerWarnings() {
        var player = LayerMask.NameToLayer("Player");
        var playerLocal = LayerMask.NameToLayer("PlayerLocal");
        return (collideWith == (collideWith | (1 << player)) || collideWith == (collideWith | (1 << playerLocal)));
      }
      #endif

      [SectionHeader("Udon Events")]
      [HelpBox("Do not use Networked option with target All when colliding with Player layer, as it will cause oversync.",
        "CheckNetworkedValidity")]
      public bool networked;
      public NetworkEventTarget networkTarget;
      
      private bool CheckNetworkedValidity() {
        var player = LayerMask.NameToLayer("Player");
        return collideWith == (collideWith | (1 << player)) && networked && networkTarget == NetworkEventTarget.All;
      }

      [ListView("Enter Events List")]
      public UdonSharpBehaviour[] enterTargets;
      
      [ListView("Enter Events List")]
      [Popup("behaviour", "@enterTargets", true)]
      public string[] enterEvents;

      [ListView("Exit Events List")]
      public UdonSharpBehaviour[] exitTargets;
      
      [ListView("Exit Events List")]
      [Popup("behaviour", "@exitTargets", true)]
      public string[] exitEvents;

      private int playerLayer = 9;
      private int playerLocalLayer = 10;
      private int collidersIn;
      
      public void Activate() {
        active = true;
      }
      
      public void Deactivate() {
        active = false;
      }
      
      public void Toggle() {
        active = !active;
      }

      private bool shouldCollideWithPlayers;
      private bool shouldCollideWithLocals;
      private bool shouldCollideWithRemote;

      private void Start() {
        if (collideWith == (collideWith | (1 << playerLayer)) ||
            collideWith == (collideWith | (1 << playerLocalLayer))) {
          shouldCollideWithPlayers = true;
        }

        if (collideWith == (collideWith | (1 << playerLayer))) {
          shouldCollideWithRemote = true;
        }

        if (collideWith == (collideWith | (1 << playerLocalLayer))) {
          shouldCollideWithLocals = true;
        }
      }

      private void OnTriggerEnter(Collider other) {
        if (!active) return;
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (collidersIn == 0) {
            FireTriggers("enter");
          }
          collidersIn++;
        }
      }

      public override void OnPlayerTriggerEnter(VRCPlayerApi player) {
        if (!collideWithLocalPlayers && !collideWithRemotePlayers && !shouldCollideWithPlayers) {
          return;
        }
        var isLocal = player.isLocal;
        if (isLocal && (collideWithLocalPlayers || shouldCollideWithLocals)) {
          if (collidersIn == 0) {
            FireTriggers("enter");
            collidersIn++;
            return;
          }
        }

        if (!isLocal && (collideWithRemotePlayers || shouldCollideWithRemote)) {
          if (collidersIn == 0) {
            FireTriggers("enter");
            collidersIn++;
            return;
          }
        }
      }

      private void OnTriggerExit(Collider other) {
        if (!active) return;
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (collidersIn == 1) {
            FireTriggers("exit");
          }
          collidersIn--;
        }
      }

      public override void OnPlayerTriggerExit(VRCPlayerApi player) {
        if (!collideWithLocalPlayers && !collideWithRemotePlayers && !shouldCollideWithPlayers) {
          return;
        }
        var isLocal = player.isLocal;
        if (isLocal && (collideWithLocalPlayers || shouldCollideWithLocals)) {
          if (collidersIn == 1) {
            FireTriggers("exit");
            collidersIn--;
            return;
          }
        }

        if (!isLocal && (collideWithRemotePlayers || shouldCollideWithRemote)) {
          if (collidersIn == 1) {
            FireTriggers("exit");
            collidersIn--;
            return;
          }
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

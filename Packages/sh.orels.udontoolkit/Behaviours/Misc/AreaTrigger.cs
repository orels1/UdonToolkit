using System;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
    [OnBeforeEditor("CheckCollisionTab")]
    [CustomName("Area Trigger")]
    [HelpMessage("It is recommended to put Area Triggers on a MirrorReflection layer unless they need a custom layer.")]
    [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#area-trigger")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class AreaTrigger : UdonSharpBehaviour {
      [SectionHeader("General")]
      [HelpBox("This behaviour requires a trigger collider to be attached to the object", "CheckCollider")]
      public bool active = true;
      public bool oneShot;
      
      private bool CheckCollider() {
        var col = gameObject.GetComponent<Collider>();
        return col == null || !col.isTrigger;
      }
      
      [SectionHeader("Collision Settings")]
      [Popup("@collideTargetOptions")]
      [OnValueChanged("HandleCollisionTypeChange")]
      public int collideTarget;

      [NonSerialized] public string[] collideTargetOptions = new[] {
        "Objects", "Players"
      };

      [HelpBox(
        "Please use Collide With Local Players and Collide With Remote Players options instead of Player and PlayerLocal layers.",
        "PlayerLayerWarnings")]
      [HelpBox("It is not recommended to collide with Everything or the Default layer.", "CheckCollisionLayers")]
      [HideIf("HideCollisionLayers")]
      public LayerMask collideWith;
      
      [HideIf("HideCollisionPlayerTargets")]
      public bool collideWithLocalPlayers;
      [HideIf("HideCollisionPlayerTargets")]
      public bool collideWithRemotePlayers;

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

      public void CheckCollisionTab(SerializedObject obj) {
        var collideTargetProp = obj.FindProperty("collideTarget");
        var collideWithLocalsProp = obj.FindProperty("collideWithLocalPlayers");
        var collideWithRemotesProp = obj.FindProperty("collideWithRemotePlayers");
        if (collideWithLocalsProp.boolValue || collideWithRemotesProp.boolValue) {
          collideTargetProp.intValue = 1;
        }
      }

      private bool HideCollisionLayers() {
        return collideTarget == 1;
      }
      
      private bool HideCollisionPlayerTargets() {
        return collideTarget == 0;
      }

      public void HandleCollisionTypeChange(SerializedProperty prop) {
        var obj = prop.serializedObject;
        var collideWithLocalsProp = obj.FindProperty("collideWithLocalPlayers");
        var collideWithRemotesProp = obj.FindProperty("collideWithRemotePlayers");
        if (prop.intValue == 0) {
          collideWithLocalsProp.boolValue = false;
          collideWithRemotesProp.boolValue = false;
        }
      }

      public void SelectAccessLevel(SerializedProperty value) {
        if (!value.boolValue) return;
        switch (value.name) {
          case "masterOnly": {
            value.serializedObject.FindProperty("ownerOnly").boolValue = false;
            break;
          }
          case "ownerOnly": {
            value.serializedObject.FindProperty("masterOnly").boolValue = false;
            break;
          }
        }
      }
      #endif
      
      [SectionHeader("Access")]
      [Horizontal("Access")]
      [Toggle]
      [OnValueChanged("SelectAccessLevel")]
      public bool masterOnly;
      [Toggle]
      [Horizontal("Access")]
      [OnValueChanged("SelectAccessLevel")]
      public bool ownerOnly;

      [HelpBox("If any names are listed - only those users will be allowed to fire events, as well as the Master/Owner based on the options above")]
      public string[] allowedUsers;

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
      
      private bool isOwner;
      private bool used;
      private bool isComposite;
      
      public void Activate() {
        active = true;
        collidersIn = 0;
      }
      
      public void Deactivate() {
        active = false;
        collidersIn = 0;
      }
      
      public void Toggle() {
        active = !active;
        collidersIn = 0;
      }

      private void OnDisable() {
        collidersIn = 0;
      }

      private bool shouldCollideWithPlayers;
      private bool shouldCollideWithLocals;
      private bool shouldCollideWithRemote;

      private void Start() {
        if (collideWith == (collideWith | (1 << playerLayer)) ||
            collideWith == (collideWith | (1 << playerLocalLayer)) || collideTarget == 1) {
          shouldCollideWithPlayers = true;
        }

        if (collideWith == (collideWith | (1 << playerLayer))) {
          shouldCollideWithRemote = true;
        }

        if (collideWith == (collideWith | (1 << playerLocalLayer))) {
          shouldCollideWithLocals = true;
        }

        var colliders = GetComponents<Collider>();
        if (colliders != null && colliders.Length > 1) {
          isComposite = true;
        }
      }

      private void OnTriggerEnter(Collider other) {
        if (!active) return;
        if (other == null) return;
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (!isComposite) {
            FireTriggers("enter");
            return;
          }
          if (collidersIn == 0) {
            FireTriggers("enter");
          }
          collidersIn++;
        }
      }

      public override void OnPlayerTriggerEnter(VRCPlayerApi player) {
        if (!active) return;
        if (!collideWithLocalPlayers && !collideWithRemotePlayers && !shouldCollideWithPlayers) {
          return;
        }
        if (player == null) return;
        if (!Utilities.IsValid(player)) return;
        var isLocal = player.isLocal;
        if (isLocal && (collideWithLocalPlayers || shouldCollideWithLocals)) {
          if (!isComposite) {
            FireTriggers("enter");
            return;
          }
          if (collidersIn == 0) {
            FireTriggers("enter");
            collidersIn++;
            return;
          }
        }

        if (!isLocal && (collideWithRemotePlayers || shouldCollideWithRemote)) {
          if (!isComposite) {
            FireTriggers("enter");
            return;
          }
          if (collidersIn == 0) {
            FireTriggers("enter");
            collidersIn++;
          }
        }
      }

      private void OnTriggerExit(Collider other) {
        if (!active) return;
        if (other == null) return;
        if (collideWith == (collideWith | (1 << other.gameObject.layer))) {
          if (!isComposite) {
            FireTriggers("exit");
            return;
          }
          if (collidersIn == 1) {
            FireTriggers("exit");
          }
          collidersIn = Mathf.Max(0, collidersIn - 1);
        }
      }

      public override void OnPlayerTriggerExit(VRCPlayerApi player) {
        if (!active) return;
        if (!collideWithLocalPlayers && !collideWithRemotePlayers && !shouldCollideWithPlayers) {
          return;
        }
        if (player == null) return;
        if (!Utilities.IsValid(player)) return;
        var isLocal = player.isLocal;
        if (isLocal && (collideWithLocalPlayers || shouldCollideWithLocals)) {
          if (!isComposite) {
            FireTriggers("exit");
            return;
          }
          if (collidersIn == 1) {
            FireTriggers("exit");
            collidersIn = Mathf.Max(0, collidersIn - 1);
            return;
          }
        }

        if (!isLocal && (collideWithRemotePlayers || shouldCollideWithRemote)) {
          if (!isComposite) {
            FireTriggers("exit");
            return;
          }
          if (collidersIn == 1) {
            FireTriggers("exit");
            collidersIn = Mathf.Max(0, collidersIn - 1);
          }
        }
      }

      private bool CheckAccess() {
        var whitelist = allowedUsers.Length > 0;
        if (masterOnly && !Networking.IsMaster && !whitelist) return false;
        if (ownerOnly && !isOwner && !whitelist) return false;
        if (whitelist) {
          var allowed = false;
          var playerName = Networking.LocalPlayer.displayName;
          foreach (var user in allowedUsers) {
            if (user == playerName) {
              allowed = true;
              break;
            }
          }

          if (!allowed && ownerOnly && isOwner) {
            allowed = true;
          }

          if (!allowed && masterOnly && Networking.IsMaster) {
            allowed = true;
          }

          if (!allowed) return false;
        }

        return true;
      }
      
      public void TakeOwnership() {
        if (isOwner) return;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        isOwner = true;
      }
      
      public override void OnOwnershipTransferred(VRCPlayerApi player) {
        isOwner = Networking.IsOwner(gameObject);
      }

      public override void OnPlayerLeft(VRCPlayerApi player) {
        isOwner = Networking.IsOwner(gameObject);
      }
      
      public void ResetOneShot() {
        used = false;
        active = true;
        collidersIn = 0;
      }
      
      private void FireTriggers(string type) {
        if (!CheckAccess()) return;
        if (oneShot) {
          if (used) {
            return;
          }

          active = false;
          used = true;
          enabled = false;
        }

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

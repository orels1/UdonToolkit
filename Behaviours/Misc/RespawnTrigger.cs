using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Respawn Trigger")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#respawn-trigger")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class RespawnTrigger : UdonSharpBehaviour {
    [SectionHeader("General")]
    public bool active = true;
    public bool oneShot;
    [HelpBox("This option will only send the events when a local player respawns")]
    public bool localOnly;
      
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
      
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
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
      
    [SectionHeader("Udon Events")]
    public bool networked;
    public NetworkEventTarget networkTarget;
    [ListView("Udon Events List")]
    public UdonSharpBehaviour[] udonTargets;
      
    [ListView("Udon Events List")]
    [Popup("behaviour", "@udonTargets", true)]
    public string[] udonEvents;

    private bool used;
    private bool isOwner;
    public override void OnPlayerRespawn(VRCPlayerApi player) {
      if (localOnly && !player.isLocal) return;
      FireTriggers();
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
    }
    
    public void Activate() {
      active = true;
    }
      
    public void Deactivate() {
      active = false;
    }
      
    public void Toggle() {
      active = !active;
    }
      
    private void FireTriggers() {
      if (!CheckAccess()) return;
      if (oneShot) {
        if (used) {
          return;
        }

        active = false;
        used = true;
        enabled = false;
      }
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
}

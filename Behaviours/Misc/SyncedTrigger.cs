
using System;
using UdonSharp;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Synced Trigger")]
  [HelpMessage("This behaviour will fire ON triggers or OFF triggers when the synced value changes. Meaning it will be synced for late joiners")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#synced-trigger")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  public class SyncedTrigger : UdonSharpBehaviour {
    [SectionHeader("General")] [HelpBox("You can set the initial state here")] [UdonSynced]
    public bool syncedValue;

    [HelpBox("This option requires a collider on the object", "CheckCollider")]
    [HelpBox("When Use Interact is checked, clicking on this object will toggle the value, firing ON or OFF events respectively")]
    public bool useInteract;
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public bool CheckCollider() {
      return useInteract && GetComponent<Collider>() == null;
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

    [HelpBox("If any names are listed - only those users will be allowed to change the synced value, as well as the Master/Owner based on the options above")]
    public string[] allowedUsers;

    [ListView("ON Events")] public UdonSharpBehaviour[] onTargets;

    [ListView("ON Events")] [Popup("behaviour", "@onTargets")]
    public string[] onEvents;

    [ListView("OFF Events")] public UdonSharpBehaviour[] offTargets;

    [ListView("OFF Events")] [Popup("behaviour", "@onTargets")]
    public string[] offEvents;

    private bool localSyncedValue;
    private bool isOwner;
    private Collider col;

    private void Start() {
      isOwner = Networking.IsOwner(gameObject);
      col = GetComponent<Collider>();
      if (col == null) {
        useInteract = false;
        return;
      }

      if (!useInteract) {
        col.enabled = false;
      }
    }

    public override void Interact() {
      if (!useInteract) return;
      ProcessEvent(nameof(Toggle));
    }

    private void ProcessEvent(string eventName) {
      var whitelist = allowedUsers.Length > 0;
      if (masterOnly && !Networking.IsMaster && !whitelist) return;
      if (ownerOnly && !isOwner && !whitelist) return;
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

        if (!allowed) return;
      }
      if (!isOwner) {
        SendCustomNetworkEvent(NetworkEventTarget.Owner, eventName);
      }

      switch (eventName) {
        case nameof(Toggle): {
          syncedValue = !syncedValue;
          break;
        }
        case nameof(TurnOn): {
          syncedValue = true;
          break;
        }
        case nameof(TurnOff): {
          syncedValue = false;
          break;
        }
      }

      if (localSyncedValue == syncedValue) {
        return;
      }

      localSyncedValue = syncedValue;
      if (isOwner) {
        RequestSerialization();
      }
      if (syncedValue) {
        RunOnEvents();
        return;
      }

      RunOffEvents();
    }

    public void Toggle() {
      ProcessEvent(nameof(Toggle));
    }

    public void TurnOn() {
      ProcessEvent(nameof(TurnOn));
    }
    
    public void TurnOff() {
      ProcessEvent(nameof(TurnOff));
    }

    public void TakeOwnership() {
      if (isOwner) return;
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
      isOwner = true;
    }

    private void RunOnEvents() {
      for (int i = 0; i < onTargets.Length; i++) {
        onTargets[i].SendCustomEvent(onEvents[i]);
      }
    }

    private void RunOffEvents() {
      for (int i = 0; i < onTargets.Length; i++) {
        offTargets[i].SendCustomEvent(offEvents[i]);
      }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player) {
      isOwner = Networking.IsOwner(gameObject);
    }

    public override void OnPlayerLeft(VRCPlayerApi player) {
      isOwner = Networking.IsOwner(gameObject);
    }

    public override void OnDeserialization() {
      if (isOwner) return;
      if (localSyncedValue == syncedValue) return;
      localSyncedValue = syncedValue;
      if (syncedValue) {
        RunOnEvents();
        return;
      }

      RunOffEvents();
    }
  }
}

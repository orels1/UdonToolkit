using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Teleporter")]
  [HelpMessage("This behaviour will teleport the player or an object when a Trigger event is received")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#teleporter")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class Teleporter : UdonSharpBehaviour {
    [HideInInspector] public int teleportType;

    [TabGroup("Teleport Object", "teleportType")]
    public Transform objectToTeleport;
    [TabGroup("Teleport Object")]
    [HelpBox("Checking this will teleport object for all players")]
    public bool networked;
    
    [TabGroup("Teleport Player", "teleportType")]
    public bool keepPlayerRotation;
    
    public Transform teleportTarget;

    private bool isObjTeleporter;
    private bool isPlayerTeleporter;
    
    private void Start() {
      switch (teleportType) {
        case 0:
          isObjTeleporter = true;
          break;
        case 1:
          isPlayerTeleporter = true;
          break;
      }
    }

    public void Trigger() {
      if (isObjTeleporter) {
        if (networked) {
          SendCustomNetworkEvent(NetworkEventTarget.All, nameof(HandleObjectTeleport));
          return;
        }
        HandleObjectTeleport();
        return;
      }

      if (!isPlayerTeleporter) return;
      HandlePlayerTeleport();
    }

    private void HandleObjectTeleport() {
      objectToTeleport.SetPositionAndRotation(teleportTarget.position, teleportTarget.rotation);
    }

    private void HandlePlayerTeleport() {
      var player = Networking.LocalPlayer;
      if (keepPlayerRotation) {
        player.TeleportTo(teleportTarget.position, player.GetRotation());
        return;
      }
      player.TeleportTo(teleportTarget.position, teleportTarget.rotation);
    }
  }
}

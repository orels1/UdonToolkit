using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Camera Target")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  public class PresentationCameraTarget : UdonSharpBehaviour {
    public VRCPlayerApi.TrackingDataType source;
    private VRCPlayerApi target;

    private bool active;
    
    private void Start() {
      target = Networking.GetOwner(gameObject);
      active = !target.isLocal;
    }

    public override void OnPlayerLeft(VRCPlayerApi player) {
      if (player == target) {
        active = false;
      }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player) {
      target = Networking.GetOwner(gameObject);
    }

    public void TakeOwnership() {
      Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public void HandleTalkStart() {
      target = Networking.GetOwner(gameObject);
      active = true;
    }

    public void HandleTalkEnd() {
      active = false;
    }

    private void Update() {
      if (!active) return;
      if (target == null) return;
      var tData = target.GetTrackingData(source);
      var tPos = tData.position;
      var tRot = tData.rotation;
      transform.SetPositionAndRotation(tPos, tRot);
    }
  }
}

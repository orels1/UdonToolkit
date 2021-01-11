using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UdonToolkit {
  [CustomName("Universal Tracker")]
  [HelpMessage(
    "This component will take the specified Bone or Tracking Target and copy its position/rotation to the specified Target Transform. " +
    "You can attach all sorts of objects to the player in that way.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#universal-tracker")]
  public class UniversalTracker : UdonSharpBehaviour {
    [SectionHeader("Tracking Target")] [UTEditor]
    public Transform targetTransform;
    public bool trackBone;
    public bool trackPlayerBase;
    public bool trackPlayspace;
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public bool HideTrackDataDropdown() {
      return trackBone || trackPlayerBase || trackPlayspace;
    }

    public bool HideTrackBoneDropdown() {
      return !trackBone || trackPlayerBase || trackPlayspace;
    }
    #endif

    [HideIf("HideTrackDataDropdown")] [UTEditor]
    public VRCPlayerApi.TrackingDataType trackingTarget;
    
    [HideIf("HideTrackBoneDropdown")] [UTEditor]
    public HumanBodyBones bone = HumanBodyBones.Hips;

    [HideIf("@trackPlayspace")][UTEditor]
    public bool trackPosition = true;
    [HideIf("@trackPlayspace")][UTEditor]
    public bool trackRotation = true;

    [SectionHeader("Tracking Correction")]
    [HelpBox("The target transform will be rotated by this angles after copying source transforms.")]
    [UTEditor]
    public Vector3 rotateBy = new Vector3(0, 0, 0);

    [HideIf("ShowControllerCorrection")]
    [HelpBox("This option will align the trackers with the player's hand laser direction", "ShowControllerCorrectionInverted")][UTEditor]
    public bool correctForControllers;
    
    public bool ShowControllerCorrection() {
      return trackingTarget == VRCPlayerApi.TrackingDataType.Head || trackBone || trackPlayspace || trackPlayerBase || !trackRotation;
    }
    
    public bool ShowControllerCorrectionInverted() {
      return !ShowControllerCorrection();
    }

    private VRCPlayerApi player;
    private bool isEditor = true;
    private VRCPlayerApi.TrackingData trackingData;
    private Vector3 offsetPos;
    private float oldRot;
    private float offsetRot;

    private void Start() {
      player = Networking.LocalPlayer;
      if (player == null) {
        return;
      }
      if (trackPlayspace) {
        var targetPos = player.GetPosition();
        var targetRot = player.GetRotation();
        targetTransform.SetPositionAndRotation(targetPos, targetRot);
      }
      isEditor = false;
    }

    public void ResetOffsets() {
      offsetPos = Vector3.zero;
      oldRot = 0;
      offsetRot = 0;
      var targetPos = player.GetPosition();
      var targetRot = player.GetRotation();
      targetTransform.SetPositionAndRotation(targetPos, targetRot);
    }

    private void LateUpdate() {
      Vector3 targetPos;
      Quaternion targetRot;

      // we're tracking bones in regular Update, otherwise it doesnt sync with IK
      if (trackBone) {
        return;
      }
      if (trackPlayerBase) {
        targetPos = player.GetPosition();
        targetRot = player.GetRotation();
      }
      else if (trackPlayspace) {
        var vertical = Input.GetAxisRaw("Vertical");
        var horizontal = Input.GetAxisRaw("Horizontal");
        var rotation = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickHorizontal");
        targetPos = player.GetPosition();
        targetRot = player.GetRotation();
        offsetRot = oldRot - targetRot.eulerAngles.y;
        oldRot = targetRot.eulerAngles.y;
        if (vertical > 0 || vertical < 0 || horizontal > 0 || horizontal < 0) {
          targetPos += offsetPos;
          targetTransform.position = targetPos;
          if (rotation > 0 || rotation < 0) {
            targetTransform.RotateAround(player.GetPosition(), Vector3.up, -offsetRot);
          }
          return;
        }
        if (rotation > 0 || rotation < 0) {
          targetTransform.RotateAround(player.GetPosition(), Vector3.up, -offsetRot);
        }
        offsetPos = targetTransform.position - targetPos;
        return;
      }
      else {
        trackingData = player.GetTrackingData(trackingTarget);
        targetPos = trackingData.position;
        targetRot = trackingData.rotation;
      }

      if (trackPosition && trackRotation) {
        targetTransform.SetPositionAndRotation(targetPos, targetRot);
      } else if (trackPosition) {
        targetTransform.position = targetPos;
      }
      else {
        targetTransform.rotation = targetRot;
      }

      if (correctForControllers && !trackPlayspace && !trackPlayerBase) {
        // Thx to Phasedragon for testing and finding out the exact formula
        targetTransform.rotation = targetRot * Quaternion.Euler(-41, 0, 0);
      }
      if (rotateBy.magnitude > 0) targetTransform.Rotate(rotateBy);
    }

    private void Update() {
      if (isEditor || !trackBone) return;

      Vector3 targetPos;
      Quaternion targetRot;

      targetPos = player.GetBonePosition(bone);
      targetRot = player.GetBoneRotation(bone);

      if (trackPosition && trackRotation) {
        targetTransform.SetPositionAndRotation(targetPos, targetRot);
      } else if (trackPosition) {
        targetTransform.position = targetPos;
      }
      else {
        targetTransform.rotation = targetRot;
      }
      if (rotateBy.magnitude > 0) targetTransform.Rotate(rotateBy);
    }
  }
}
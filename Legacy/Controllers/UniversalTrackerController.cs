#if UNITY_EDITOR

using System;
using UnityEngine;
using VRC.SDKBase;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Universal Tracker")]
  [HelpMessage(
    "This component will take the specified Bone or Tracking Target and copy its position/rotation to the specified Target Transform. " +
    "You can attach all sorts of objects to the player in that way.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#universal-tracker")]
  public class UniversalTrackerController : UTController {
    [SectionHeader("Tracking Target")] [UdonPublic]
    public bool trackBone;
    [UdonPublic]
    public bool trackPlayerBase;
    [UdonPublic] public bool trackPlayspace;

    public bool HideTrackDataDropdown() {
      return trackBone || trackPlayerBase || trackPlayspace;
    }

    public bool HideTrackBoneDropdown() {
      return !trackBone || trackPlayerBase || trackPlayspace;
    }

    [HideIf("HideTrackDataDropdown")] [UdonPublic]
    public VRCPlayerApi.TrackingDataType trackingTarget = VRCPlayerApi.TrackingDataType.Head;

    [HideIf("HideTrackBoneDropdown")] [UdonPublic] public HumanBodyBones bone = HumanBodyBones.Hips;
    [UdonPublic] public Transform targetTransform;
    [HideIf("@trackPlayspace")]
    [UdonPublic] public bool trackPosition = true;
    [HideIf("@trackPlayspace")]
    [UdonPublic] public bool trackRotation = true;

    [SectionHeader("Tracking Correction")]
    [Tooltip("The target transform will be rotated by this angles after copying source transforms.")]
    [UdonPublic]
    public Vector3 rotateBy = new Vector3(0, 45f, 0);

    [SectionHeader("Editor Testing Helpers")]
    [Tooltip(
      "This will make the script follow the main camera instead of a tracking target, which doesnt work in editor.")]
    [UdonPublic]
    public bool followMainCamera;
  }
}

#endif
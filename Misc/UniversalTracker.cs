using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UdonToolkit {
  [AddComponentMenu("")]
  public class UniversalTracker : UdonSharpBehaviour {
    [Header("Tracking Data")] public Transform targetTransform;
    public VRCPlayerApi.TrackingDataType trackingTarget;

    [Space(10f)] [Header("Bone Tracking")] public bool trackBone;
    public HumanBodyBones bone = HumanBodyBones.Hips;

    [Space(10f)] public bool trackPosition = true;
    public bool trackRotation = true;

    public Vector3 rotateBy = new Vector3(0, 45f, 0);

    [Space(15f)] [Header("Editor Hacks")] public bool followMainCamera;

    private VRCPlayerApi player;
    private bool isEditor = true;
    private VRCPlayerApi.TrackingData trackingData;
    private GameObject mainCamera;

    private void Start() {
      player = Networking.LocalPlayer;
      if (player == null) {
        if (!followMainCamera) return;
        mainCamera = GameObject.Find("Main Camera");
      }

      isEditor = false;
    }

    private void Update() {
      if (isEditor) {
        if (!followMainCamera) return;
        var cameraPos = mainCamera.transform.position;
        var cameraRot = mainCamera.transform.rotation;
        targetTransform.SetPositionAndRotation(cameraPos, cameraRot);
        return;
      }

      Vector3 targetPos;
      Quaternion targetRot;

      if (trackBone) {
        targetPos = player.GetBonePosition(bone);
        targetRot = player.GetBoneRotation(bone);
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
      if (rotateBy.magnitude > 0) targetTransform.Rotate(rotateBy);
    }
  }
}
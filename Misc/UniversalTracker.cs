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

    public bool trackPlayerBase;

    [Space(10f)] public bool trackPosition = true;
    public bool trackRotation = true;
    public bool trackPlayspace;

    public Vector3 rotateBy = new Vector3(0, 45f, 0);

    [Space(15f)] [Header("Editor Hacks")] public bool followMainCamera;

    private VRCPlayerApi player;
    private bool isEditor = true;
    private VRCPlayerApi.TrackingData trackingData;
    private GameObject mainCamera;
    private Vector3 offsetPos;
    private float oldRot;
    private float offsetRot;

    private void Start() {
      player = Networking.LocalPlayer;
      if (player == null) {
        if (!followMainCamera) return;
        mainCamera = GameObject.Find("Main Camera");
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
      else if (trackPlayerBase) {
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
      if (rotateBy.magnitude > 0) targetTransform.Rotate(rotateBy);
    }
  }
}
using System;
using UdonSharp;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Velocity Tracker")]
  [HelpMessage("Uses physics-based tracking to allow for proper collision and acceleration between bodies. For best results - use with Continuous Speculative collision.")]
  [HelpURL("https://ut.orels.sh/v/v1.x/behaviours/misc-behaviours#velocity-tracker")]
  [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
  public class VelocityTracker : UdonSharpBehaviour {
    [HideInInspector]
    public int trackingType;

    [TabGroup("Track Transform", "trackingType")]
    public Transform sourceTransform;
    [TabGroup("Track Player")]
    public VRCPlayerApi.TrackingDataType sourceOnPlayer;

    [HelpBox("This behaviour requires an attached rigidbody to work", "GetRBState")]
    public GameObject target;
    public bool trackPosition;
    public bool trackRotation;

    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public bool GetRBState() {
      if (!target) return false;
      return target.GetComponent<Rigidbody>() == null;
    }
    #endif

    private Rigidbody rb;
    private VRCPlayerApi player;

    private bool trackTransform;

    private void Start() {
      player = Networking.LocalPlayer;
      rb = target.GetComponent<Rigidbody>();
      switch (trackingType) {
        case 0:
          trackTransform = true;
          break;
        default:
          trackTransform = true;
          break;
      }
      if (sourceTransform != null) {
        trackTransform = true;
      }
    }

    private void FixedUpdate() {
      var tData = player.GetTrackingData(sourceOnPlayer);
      var tPos = trackTransform ? sourceTransform.position : tData.position;
      var tRot = trackTransform ? sourceTransform.rotation : tData.rotation;
      
      if (trackPosition) {
        rb.velocity *= 0.65f;
        var posDelta = tPos - rb.worldCenterOfMass;
        var velocity = posDelta / Time.fixedDeltaTime;
        if (!float.IsNaN(velocity.x)) {
          rb.velocity += velocity;
        }
      }

      if (trackRotation) {
        rb.angularVelocity *= 0.65f;
        var rotationDelta = tRot * Quaternion.Inverse(rb.rotation);
        float angleInDegrees;
        Vector3 rotationAxis;
        rotationDelta.ToAngleAxis(out angleInDegrees, out rotationAxis);
        if (angleInDegrees > 180) {
          angleInDegrees -= 360;
        }

        if (Mathf.Abs(angleInDegrees) > Mathf.Epsilon) {
          var angularVelocity = rotationAxis * (angleInDegrees * Mathf.Deg2Rad) / Time.fixedDeltaTime;
          if (!float.IsNaN(angularVelocity.x)) {
            rb.angularVelocity += angularVelocity * rb.angularDrag;
          }
        }
      }
    }
  }
}

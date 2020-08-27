using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class VelocityTracker : UdonSharpBehaviour {
      public VRCPlayerApi.TrackingDataType source;
      public GameObject target;
      public bool trackPosition;
      public bool trackRotation;

      private Rigidbody rb;
      private VRCPlayerApi player;
      private bool isDesktop;
      private bool isEditor = true;

      private void Start() {
        player = Networking.LocalPlayer;
        if (player == null) {
          return;
        }

        isEditor = false;
        isDesktop = !player.IsUserInVR();
        rb = target.GetComponent<Rigidbody>();
      }

      private void FixedUpdate() {
        if (isEditor || isDesktop) return;
        var tData = player.GetTrackingData(source);
        if (trackPosition) {
          var tPos = tData.position;
          rb.velocity *= 0.65f;
          var posDelta = tPos - rb.worldCenterOfMass;
          var velocity = posDelta / Time.fixedDeltaTime;
          if (!float.IsNaN(velocity.x)) {
            rb.velocity += velocity;
          }
        }

        if (trackRotation) {
          var tRot = tData.rotation;
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
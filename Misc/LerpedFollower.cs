using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  public class LerpedFollower : UdonSharpBehaviour {
    public Transform sourceTransform;
    public Transform targetTransform;
    public bool lerpPosition = true;
    public bool lerpRotation = true;
    public float lerpSpeed = 10f;
    public bool ignoreRotation;

    [HideInInspector] public bool active = true;

    private void Start() {
      if (sourceTransform == null || targetTransform == null) {
        active = false;
      }
    }

    private void LateUpdate() {
      if (!active) return;
      var sourcePos = sourceTransform.position;
      var sourceRot = sourceTransform.rotation;
      var targetPos = targetTransform.position;
      var targetRot = targetTransform.rotation;
      targetTransform.position = lerpPosition ? Vector3.Lerp(targetPos, sourcePos, lerpSpeed * Time.deltaTime) : sourcePos;
      if (ignoreRotation) return;
      targetTransform.rotation = lerpRotation ? Quaternion.Lerp(targetRot, sourceRot, lerpSpeed * Time.deltaTime) : sourceRot;
    }
  }
}
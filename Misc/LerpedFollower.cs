using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Lerped Follower")]
  [HelpMessage("This component makes the Target Transform follow the Source Transform with linear interpolation. " +
               "Use this to make an object smoothly follow your target.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#lerped-follower")]
  public class LerpedFollower : UdonSharpBehaviour {
    [SectionHeader("General")] [UTEditor]
    public Transform sourceTransform;
    public Transform targetTransform;
    [HelpBox("Target will instantly match the Source position", "@!lerpPosition")] [UTEditor]
    public bool lerpPosition = true;
    [HelpBox("Target will instantly match the Source rotation", "@!lerpRotation")][UTEditor]
    public bool lerpRotation = true;
    public float lerpSpeed = 10f;
    [HelpBox("This option disables rotation transfer.")][UTEditor]
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
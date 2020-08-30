#if UNITY_EDITOR

using UnityEngine;

namespace UdonToolkit {
  [CustomName("Lerped Follower")]
  [HelpMessage("This component makes the Target Transform follow the Source Transform with linear interpolation. " +
               "Use this to make an object smoothly follow your target.")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#lerped-follower")]
  public class LerpedFollowerController : UTController {
    [SectionHeader("General")] [UdonPublic]
    public Transform sourceTransform;
    [UdonPublic] public Transform targetTransform;
    [HelpBox("Target will instantly match the Source position", "@!lerpPosition")]
    [UdonPublic] public bool lerpPosition = true;
    [HelpBox("Target will instantly match the Source rotation", "@!lerpRotation")]
    [UdonPublic] public bool lerpRotation = true;
    [UdonPublic] public float lerpSpeed  = 10f;
    [HelpBox("This option disables rotation transfer.")]
    [UdonPublic] public bool ignoreRotation;
  }
}

#endif
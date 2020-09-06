#if UNITY_EDITOR

using System;
using UnityEngine;

namespace UdonToolkit.Legacy{
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
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
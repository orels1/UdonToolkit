#if UNITY_EDITOR
using System;
using UdonSharp;
using UnityEngine;


namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Fog Adjustment")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#fog-adjustment")]
  public class FogAdjustmentController : UTController {
    [HideInInspector] public bool isLinear;
    [SectionHeader("Defaults")] [UdonPublic]
    public Color defaultFogColor;

    [HideIf("@isLinear")][UdonPublic] public float defaultFogDensity;
    [HideIf("@!isLinear")][UdonPublic]
    public float defaultFogStart;
    [HideIf("@!isLinear")][UdonPublic]
    public float defaultFogEnd;

    [SectionHeader("Active State")] [UdonPublic]
    public Color activeFogColor;

    [HideIf("@isLinear")][UdonPublic] public float activeFogDensity;
    [HideIf("@!isLinear")][UdonPublic]
    public float activeFogStart;
    [HideIf("@!isLinear")][UdonPublic]
    public float activeFogEnd;
    [HelpBox("Transition time cannot be negative", "CheckValidTransition")]
    [UdonPublic] [Tooltip("0 - Instant")] public float fogTransitionTime;
    
    public bool CheckValidTransition() {
      return fogTransitionTime < 0;
    }

    [SectionHeader("Extras")] [UdonPublic] public bool startActive;

    [HelpBox("Will set the fog color and density from the Default or Active state on start", "@setInitialState")]
    [UdonPublic]
    public bool setInitialState;

    public override void SetupController() {
      isLinear = RenderSettings.fogMode == FogMode.Linear;
    }

    [Button("ActivateFog")]
    public void ActivateFog() {
      if (uB != null) {
        uB.SendCustomEvent("ActivateFog");
      }
    }
    
    [Button("DeactivateFog")]
    public void DeactivateFog() {
      if (uB != null) {
        uB.SendCustomEvent("DeactivateFog");
      }
    }
    
    [Button("Trigger")]
    public void Trigger() {
      if (uB != null) {
        uB.SendCustomEvent("Trigger");
      }
    }
  }
}

#endif
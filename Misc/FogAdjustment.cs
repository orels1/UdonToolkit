using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Fog Adjustment")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#fog-adjustment")]
  public class FogAdjustment : UdonSharpBehaviour {
    public bool isLinearFog;
    [SectionHeader("Default State")][UTEditor]
    public Color defaultFogColor;
    [HideIf("@isLinearFog")][UTEditor]
    public float defaultFogDensity;
    [HideIf("@!isLinearFog")][UTEditor]
    public float defaultFogStart;
    [HideIf("@!isLinearFog")][UTEditor]
    public float defaultFogEnd;
    [SectionHeader("Active State")][UTEditor]
    public Color activeFogColor;
    [HideIf("@isLinearFog")][UTEditor]
    public float activeFogDensity;
    [HideIf("@!isLinearFog")][UTEditor]
    public float activeFogStart;
    [HideIf("@!isLinearFog")][UTEditor]
    public float activeFogEnd;
    
    [HelpBox("Transition time cannot be negative", "CheckValidTransition")]
    [Tooltip("0 - Instant")]
    [UTEditor]
    public float fogTransitionTime;

    private bool CheckValidTransition() {
      return fogTransitionTime < 0;
    }
    
    [SectionHeader("Extras")][UTEditor]
    public bool startActive;
    
    [HelpBox("Will set the fog color and density from the Default or Active state on start", "@setInitialState")]
    [UTEditor]
    public bool setInitialState;

    private FogMode mode = FogMode.ExponentialSquared;
    private bool active;
    private bool initial;
    private bool lerping;
    private float lerpEnd;
    private VRCPlayerApi player;

    void Start() {
      mode = RenderSettings.fogMode;
      if (startActive || startActive && setInitialState) {
        initial = true;
        ActivateFog();
      }
      else if (setInitialState) {
        initial = true;
        DeactivateFog();
      }


      player = Networking.LocalPlayer;
    }

    private void StartLerping() {
      lerping = true;
      lerpEnd = Time.time + fogTransitionTime;
      Debug.LogFormat("lerping {0} until {1}, current {2}", lerping, lerpEnd, Time.time);
    }

    private void SetFog(Color color, float density) {
      RenderSettings.fogColor = color;
      RenderSettings.fogDensity = density;
    }

    private void SetFogLinear(Color color, float start, float end) {
      RenderSettings.fogColor = color;
      RenderSettings.fogStartDistance = start;
      RenderSettings.fogEndDistance = end;
    }

    private void LerpFog(Color sColor, Color eColor, float sDensity, float eDensity, float alpha) {
      RenderSettings.fogColor = Color.Lerp(sColor, eColor, alpha);
      RenderSettings.fogDensity = Mathf.Lerp(sDensity, eDensity, alpha);
    }

    private void LerpFogLinear(Color sColor, Color eColor, float sStart, float eStart, float sEnd, float eEnd,
      float alpha) {
      RenderSettings.fogColor = Color.Lerp(sColor, eColor, alpha);
      RenderSettings.fogStartDistance = Mathf.Lerp(sStart, eStart, alpha);
      RenderSettings.fogEndDistance = Mathf.Lerp(sEnd, eEnd, alpha);
    }

    [Button("ActivateFog")]
    public void ActivateFog() {
      active = false;
      if (fogTransitionTime > 0 && !initial) {
        StartLerping();
        return;
      }

      if (mode == FogMode.Linear) {
        SetFogLinear(activeFogColor, activeFogStart, activeFogEnd);
      }
      else {
        SetFog(activeFogColor, activeFogDensity);
      }

      if (initial) {
        initial = false;
      }

      active = true;
    }

    [Button("DeactivateFog")]
    public void DeactivateFog() {
      active = true;
      if (fogTransitionTime > 0 && !initial) {
        StartLerping();
        return;
      }

      if (mode == FogMode.Linear) {
        SetFogLinear(defaultFogColor, defaultFogStart, defaultFogEnd);
      }
      else {
        SetFog(defaultFogColor, defaultFogDensity);
      }

      if (initial) {
        initial = false;
      }

      active = false;
    }

    [Button("Trigger")]
    public void Trigger() {
      if (fogTransitionTime > 0) {
        StartLerping();
        return;
      }

      if (active) {
        DeactivateFog();
        return;
      }

      ActivateFog();
    }

    private void Update() {
      Debug.LogFormat("updating, {0}", lerping);
      if (!lerping) return;
      var alpha = (Time.time - (lerpEnd - fogTransitionTime)) / fogTransitionTime;
      Debug.LogFormat("Lerping in Update, {0}", alpha);
      // if currently active - lerp down;
      if (active) {
        Debug.Log("lerping down");
        if (mode == FogMode.Linear) {
          LerpFogLinear(activeFogColor, defaultFogColor, activeFogStart, defaultFogStart, activeFogEnd, defaultFogEnd,
            alpha);
        }
        else {
          LerpFog(activeFogColor, defaultFogColor, activeFogDensity, defaultFogDensity, alpha);
        }
      } // otherwise - lerp up;
      else {
        Debug.Log("lerping up");
        if (mode == FogMode.Linear) {
          LerpFogLinear(defaultFogColor, activeFogColor, defaultFogStart, activeFogStart, defaultFogEnd, activeFogEnd,
            alpha);
        }
        else {
          LerpFog(defaultFogColor, activeFogColor, defaultFogDensity, activeFogDensity, alpha);
        }
      }

      if (!(Time.time > lerpEnd)) return;
      Debug.LogFormat("Reached lerp end, resetting {0}, current {1}", lerpEnd, Time.time);
      lerping = false;
      active = !active;
    }
  }
  
}

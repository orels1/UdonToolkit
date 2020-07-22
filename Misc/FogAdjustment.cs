using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FogAdjustment : UdonSharpBehaviour {
  public Color defaultFogColor;
  public float defaultFogDensity;
  public float defaultFogStart;
  public float defaultFogEnd;
  public Color activeFogColor;
  public float activeFogDensity;
  public float activeFogStart;
  public float activeFogEnd;
  public float fogFadeTime;
  public bool startActive;
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
    lerpEnd = Time.time + fogFadeTime;
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

  public void ActivateFog() {
    active = false;
    if (fogFadeTime > 0 && !initial) {
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

  public void DeactivateFog() {
    active = true;
    if (fogFadeTime > 0 && !initial) {
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

  public void Trigger() {
    if (fogFadeTime > 0) {
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
    if (!lerping) return;
    var alpha = (Time.time - (lerpEnd - fogFadeTime)) / fogFadeTime;
    // if currently active - lerp down;
    if (active) {
      if (mode == FogMode.Linear) {
        LerpFogLinear(activeFogColor, defaultFogColor, activeFogStart, defaultFogStart, activeFogEnd, defaultFogEnd,
          alpha);
      }
      else {
        LerpFog(activeFogColor, defaultFogColor, activeFogDensity, defaultFogDensity, alpha);
      }
    } // otherwise - lerp up;
    else {
      if (mode == FogMode.Linear) {
        LerpFogLinear(defaultFogColor, activeFogColor, defaultFogStart, activeFogStart, defaultFogEnd, activeFogEnd,
          alpha);
      }
      else {
        LerpFog(defaultFogColor, activeFogColor, defaultFogDensity, activeFogDensity, alpha);
      }
    }

    if (!(Time.time > lerpEnd)) return;
    lerping = false;
    active = !active;
  }
}
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class SkyboxAdjustment : UdonSharpBehaviour {
      public Material defaultSkybox;
      public string[] defaultFloatNames;
      public float[] defaultFloatValues;
      public string[] defaultColorNames;
      public Color[] defaultColorValues;
      public string[] defaultVector3Names;
      public Vector3[] defaultVector3Values;

      public Material activeSkybox;
      public string[] activeFloatNames;
      public float[] activeFloatValues;
      public string[] activeColorNames;
      public Color[] activeColorValues;
      public string[] activeVector3Names;
      public Vector3[] activeVector3Values;

      public bool instantTransition = true;
      public float transitionTime;

      private bool active;
      private bool lerping;
      private float lerpEnd;

      public void Trigger() {
        if (instantTransition || transitionTime <= 0) {
          RenderSettings.skybox = active ? defaultSkybox : activeSkybox;
          SetFinalValues();
          active = !active;
          return;
        }

        lerping = true;
        lerpEnd = Time.time + transitionTime;
      }

      private void LerpValues(float alpha) {
        var mat = defaultSkybox;
        var sFloatNames = !active ? defaultFloatNames : activeFloatNames;
        var sFloatVals = !active ? defaultFloatValues : activeFloatValues;
        var sColorNames = !active ? defaultColorNames : activeColorNames;
        var sColorVals = !active ? defaultColorValues : activeColorValues;
        var sVectorNames = !active ? defaultVector3Names : activeVector3Names;
        var sVectorVals = !active ? defaultVector3Values : activeVector3Values;
        
        var eFloatVals = active ? defaultFloatValues : activeFloatValues;
        var eColorVals = active ? defaultColorValues : activeColorValues;
        var eVectorVals = active ? defaultVector3Values : activeVector3Values;
        
        for (int i = 0; i < sFloatNames.Length; i++) {
          mat.SetFloat(sFloatNames[i], Mathf.Lerp(sFloatVals[i], eFloatVals[i], alpha));
        }
        for (int i = 0; i < sColorNames.Length; i++) {
          mat.SetColor(sColorNames[i], Color.Lerp(sColorVals[i], eColorVals[i], alpha));
        }
        for (int i = 0; i < sVectorNames.Length; i++) {
          mat.SetVector(sVectorNames[i], Vector3.Lerp(sVectorVals[i], eVectorVals[i], alpha));
        }
      }

      private void SetFinalValues() {
        var floatNames = active ? defaultFloatNames : activeFloatNames;
        var floatVals = active ? defaultFloatValues : activeFloatValues;
        var colorNames = active ? defaultColorNames : activeColorNames;
        var colorVals = active ? defaultColorValues : activeColorValues;
        var vectorNames = active ? defaultVector3Names : activeVector3Names;
        var vectorVals = active ? defaultVector3Values : activeVector3Values;
        var mat = active ? defaultSkybox : activeSkybox;
        for (int i = 0; i < floatNames.Length; i++) {
          mat.SetFloat(floatNames[i], floatVals[i]);
        }
        for (int i = 0; i < colorNames.Length; i++) {
          mat.SetColor(colorNames[i], colorVals[i]);
        }
        for (int i = 0; i < vectorNames.Length; i++) {
          mat.SetVector(vectorNames[i], vectorVals[i]);
        }
      }

      private void Update() {
        if (!lerping) return;
        if (Time.time >= lerpEnd) {
          SetFinalValues();
          lerping = false;
          active = !active;
          return;
        }
        var lerpAlpha = (Time.time - (lerpEnd - transitionTime)) / transitionTime;
        LerpValues(lerpAlpha);
      }
    }
}
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class UIReadout : UdonSharpBehaviour {
      public bool active;
      public UdonBehaviour source;
      public string[] textVariableNames;
      public Text[] textVariableTargets;
      public string[] textReadoutFormats;
      public string[] sliderVariableNames;
      public Slider[] sliderVariableTargets;
      public float[] sliderReadoutMultipliers;

      private void LateUpdate() {
        if (!active) return;
        for (int i = 0; i < textVariableNames.Length; i++) {
          var val = source.GetProgramVariable(textVariableNames[i]).ToString();
          textVariableTargets[i].text = textReadoutFormats[i].Replace("{0}", val);
        }

        for (int i = 0; i < sliderVariableNames.Length; i++) {
          var val = source.GetProgramVariable(sliderVariableNames[i]);
          sliderVariableTargets[i].value = Convert.ToSingle(val) * sliderReadoutMultipliers[i];
        }
      }
      
      public void Activate() {
        active = true;
      }

      public void Deactivate() {
        active = false;
      }

      public void Toggle() {
        active = !active;
      }
    }
}
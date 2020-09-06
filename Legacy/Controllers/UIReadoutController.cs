#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("UI Readout")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#ui-readout")]
  public class UIReadoutController: UTController {
    [SectionHeader("General")] [UdonPublic]
    public bool active;

    [UdonPublic] public UdonBehaviour source;

    [SectionHeader("Text Readouts")]
    [ListView("Text Readouts List", "AddTextVar")] [Popup(PopupAttribute.PopupSource.Method, "GetTextVariableOptions", true)] [UdonPublic]
    public string[] textVariableNames;

    [ListView("Text Readouts List")] [UdonPublic]
    public Text[] textVariableTargets;

    public void AddTextVar() {
      var newNames = textVariableNames.ToList();
      newNames.Add(GetTextVariableOptions()[0]);
      textVariableNames = newNames.ToArray();

      var newTargets = textVariableTargets.ToList();
      newTargets.Add(null);
      textVariableTargets = newTargets.ToArray();

      if (textReadoutFormats == null) {
        textReadoutFormats = new[] {"{0}"};
        return;
      }
      var newFormats = textReadoutFormats.ToList();
      newFormats.Add("{0}");
      textReadoutFormats = newFormats.ToArray();
    }

    [HelpBox("Each element of Text Readout Format corresponds to a Text Readout from the list above, make sure both lists are the same length or this will crash." +
             "\n{0} will be replaced by the value, e.g. \"Speed: {0} u/s\" will become \"Speed: 10 u/s\" if variable value is 10.")]
    [UdonPublic] public string[] textReadoutFormats;
    
    [SectionHeader("Slider Readouts")]
    [ListView("Slider Readouts List", "AddSliderVar")] [Popup(PopupAttribute.PopupSource.Method, "GetSliderVariableOptions", true)] [UdonPublic]
    public string[] sliderVariableNames;

    [ListView("Slider Readouts List")] [UdonPublic]
    public Slider[] sliderVariableTargets;

    public void AddSliderVar() {
      var newNames = sliderVariableNames.ToList();
      newNames.Add(GetSliderVariableOptions()[0]);
      sliderVariableNames = newNames.ToArray();

      var newTargets = sliderVariableTargets.ToList();
      newTargets.Add(null);
      sliderVariableTargets = newTargets.ToArray();

      if (sliderReadoutMultipliers == null) {
        sliderReadoutMultipliers = new[] {1f};
        return;
      }
      var newFormats = sliderReadoutMultipliers.ToList();
      newFormats.Add(1);
      sliderReadoutMultipliers = newFormats.ToArray();
    }

    [HelpBox("Each element of Slider Readout Multiplier corresponds to a Slider Readout from the list above, make sure both lists are the same length or this will crash.")]
    [UdonPublic] public float[] sliderReadoutMultipliers;

    public string[] GetTextVariableOptions() {
      if (source == null) return new[] {"no source set"};
      var vars = source.publicVariables.VariableSymbols.ToArray();
      var filtered = new List<string>();
      foreach (var v in vars) {
        if (!source.publicVariables.TryGetVariableType(v, out var type)) continue;
        if (!type.IsArray && type.GetMembers().Count(i => i.Name == "ToString") != 0) {
          filtered.Add(v);
        }
      }
      return filtered.Any() ? filtered.ToArray() : new [] {"no compatible variables"};
    }
    
    public string[] GetSliderVariableOptions() {
      if (source == null) return new[] {"no source set"};
      var vars = source.publicVariables.VariableSymbols.ToArray();
      var filtered = new List<string>();
      foreach (var v in vars) {
        if (!source.publicVariables.TryGetVariableType(v, out var type)) continue;
        if (type.IsArray || type == typeof (bool) || type.IsEnum) continue;
        if (!source.publicVariables.TryGetVariableValue(v, out var val)) continue;
        try {
          Convert.ToSingle(val);
          filtered.Add(v);
        }
        catch {
          // ignored
        }
      }
      return filtered.Any() ? filtered.ToArray() : new [] {"no compatible variables"};
    }
    
    [Button("Activate")]
    public void Activate() {
      if (uB == null) return;
      uB.SendCustomEvent("Activate");
    }
    
    [Button("Deactivate")]
    public void Deactivate() {
      if (uB == null) return;
      uB.SendCustomEvent("Deactivate");
    }
    
    [Button("Toggle")]
    public void Toggle() {
      if (uB == null) return;
      uB.SendCustomEvent("Toggle");
    }
  }
}

#endif
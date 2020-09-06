#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.UI;

namespace UdonToolkit.Legacy {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  [CustomName("Shader Feeder")]
  [HelpMessage("This is an advanced behaviour, its highly recommended to check out the docs by clicking the blue manual icon in the corner above. This behaviour might not play nicely with animations that touch materials")]
  [HelpURL("https://github.com/orels1/UdonToolkit/wiki/Misc-Behaviours#shader-feeder")]
  public class ShaderFeederController : UTController {
    [SectionHeader("General")] [UdonPublic]
    public bool active = true;

    public Shader source;
    [UdonPublic] public MeshRenderer[] targets;

    [UdonPublic] public bool customUpdateRate;

    [Horizontal("Update Rate")]
    [HideIf("@!customUpdateRate")] [UdonPublic]
    public float updateRate = 1;

    [Horizontal("Update Rate")]
    [Popup(PopupAttribute.PopupSource.Method,"@updateTypes", true)]
    [HideIf("@!customUpdateRate")]
    [UdonPublic]
    public string updateType;
    [NonSerialized] public string[] updateTypes = {"Per Second", "Per Minute"};

    [SectionHeader("Shader Properties")]
    [Toggle]
    [UdonPublic] public bool setSceneStartTime = true;
    [HideIf("@!setSceneStartTime")]
    [Popup(PopupAttribute.PopupSource.Shader, "@source")]
    [HelpBox("Scene start time will be saved to this float", "@setSceneStartTime")]
    [UdonPublic]
    public string startTimeTarget;
    
    [Toggle]
    [HideIf("@!customUpdateRate")]
    [UdonPublic] public bool setCycleLength = true;
    [HideIf("HideCycleLengthTarget")] [Popup(PopupAttribute.PopupSource.Shader, "@source")] [UdonPublic]
    public string cycleLengthTarget;

    public bool HideCycleLengthTarget() {
      return !customUpdateRate || !setCycleLength;
    }
    
    [Toggle]
    [UdonPublic] public bool setCycleStartTime = true;
    [HideIf("@!setCycleStartTime")]
    [Popup(PopupAttribute.PopupSource.Shader, "@source")] [HelpBox("Start time of each cycle will be saved to this float", "@setCycleStartTime")] [UdonPublic]
    public string cycleTimeTarget;
    
    [ListView("Slider Sources")] [HideLabel] [UdonPublic]
    public Slider[] sliderSources;

    [ListView("Slider Sources")]
    [Popup(PopupAttribute.PopupSource.Shader, "@source", PopupAttribute.ShaderPropType.Float, true)]
    [UdonPublic]
    public string[] sliderTargets;

    [ListView("Transform Sources")] [HideLabel] [UdonPublic]
    public Transform[] transformSources;

    [ListView("Transform Sources")]
    [Popup(PopupAttribute.PopupSource.Shader, "@source", PopupAttribute.ShaderPropType.Vector, true)]
    [UdonPublic]
    public string[] transformTargets;
  }
}
#endif
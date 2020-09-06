#if UNITY_EDITOR
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.Udon.Editor.ProgramSources;

namespace UdonToolkit.Legacy {
  [Obsolete("This attribute is not needed anymore as it is only used with now deprecated Controllers. Learn more: https://l.vrchat.sh/utV4Migrate")]
  [AttributeUsage(AttributeTargets.Class)]
  public class ControlledBehaviourAttribute : Attribute {
    public UdonProgramAsset uB;

    public ControlledBehaviourAttribute(Type T) {
      var assets = Resources.FindObjectsOfTypeAll(typeof(UdonSharpProgramAsset))
        .Select(a => a as UdonSharpProgramAsset).ToArray();
      foreach (var asset in assets) {
        try {
          if (asset != null && asset.sourceCsScript.GetClass() == T) {
            uB = asset;
          }
        }
        catch {
          // ignored
        }
      }
    }
  }

}
#endif
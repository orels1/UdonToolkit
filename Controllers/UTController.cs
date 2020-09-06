#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonToolkit {
  [Obsolete("Controllers are Deprecated since v0.4.0, you can use the attributes directly in U# code now! Learn more: https://l.vrchat.sh/utV4Migrate")]
  public class UTController : MonoBehaviour {
    [HideInInspector] public UdonBehaviour uB;
    
    public virtual void SetupController() {
    }
    
    public void SyncValues() {
      if (uB == null) {
        uB = GetComponent<UdonBehaviour>();
        if (uB == null) return;
      }

      var fields = GetType().GetFields().Where(f => f.GetAttribute<UdonPublicAttribute>() != null);
      foreach (var field in fields) {
        var uAttr = field.GetAttribute<UdonPublicAttribute>();
        var fieldType = field.GetReturnType();
        var fieldValue = field.GetValue(this);
        var fieldName = uAttr.varName.IsNullOrWhitespace() ? field.Name : uAttr.varName;
        var hiddenTypes = new[] {
          typeof(UdonBehaviour),
          typeof(GameObject),
          typeof(Transform),
          typeof(UdonBehaviour[])
        };
        if (hiddenTypes.Contains(fieldType)) {
          if (fieldValue != null) {
            IUdonVariable var;

            if (fieldType == typeof(UdonBehaviour[])) {
              var converted = (fieldValue as UdonBehaviour[]).Select(i => i as Component).ToArray();
              if (Application.isPlaying) {
                uB.SetProgramVariable(fieldName, converted);
                continue;
              }
              uB.publicVariables.TrySetVariableValue(fieldName, converted);
              continue;
            }
            if (fieldType == typeof(UdonBehaviour)) {
              var = new UdonVariable<UdonBehaviour>(fieldName, (UdonBehaviour) fieldValue);
            } else if (fieldType == typeof(GameObject)) {
              var = new UdonVariable<GameObject>(fieldName, (GameObject) fieldValue);
            } else if (fieldType == typeof(Transform)) {
              var = new UdonVariable<Transform>(fieldName, (Transform) fieldValue);
            }
            else {
              continue;
            }

            if (Application.isPlaying) {
              uB.SetProgramVariable(fieldName, var);
              continue;
            }
            uB.publicVariables.RemoveVariable(fieldName);
            uB.publicVariables.TryAddVariable(var);
            uB.publicVariables.TrySetVariableValue(fieldName, var);
          }

          continue;
        }

        if (Application.isPlaying) {
          uB.SetProgramVariable(fieldName, fieldValue);
          continue;
        }
        uB.publicVariables.TrySetVariableValue(fieldName, fieldValue);
      }
    }

    public void SyncBack() {
      if (uB == null) {
        uB = GetComponent<UdonBehaviour>();
        if (uB == null) return;
      }
      var fields = GetType().GetFields().Where(f => f.GetAttribute<UdonPublicAttribute>() != null);
      var t = new SerializedObject(this);
      foreach (var field in fields) {
        var uAttr = field.GetAttribute<UdonPublicAttribute>();
        var fieldType = field.GetReturnType();
        var fieldValue = field.GetValue(this);
        var fieldName = uAttr.varName.IsNullOrWhitespace() ? field.Name : uAttr.varName;
        if (uB.publicVariables.TryGetVariableValue(fieldName, out var uBFieldValue)) {
          // skip the value if its null and the default isn't null
          if (uBFieldValue == null && fieldValue != null) continue;
          if (uB.publicVariables.TryGetVariableType(fieldName, out var uBFieldType)) {
            // handle cases where types are changed between U# and UT
            if (uBFieldType == typeof(Component) && fieldType.InheritsFrom(typeof(UdonBehaviour))) {
              GetType().GetField(fieldName).SetValue(this, uBFieldValue as UdonBehaviour);
              continue;
            }
            if (uBFieldType == typeof(Component[]) && fieldType.IsArray &&
                fieldType.GetElementType().InheritsFrom(typeof(UdonBehaviour))) {
              var converted = (uBFieldValue as Component[]).ToList().Select(i => i as UdonBehaviour).ToArray();
              GetType().GetField(fieldName).SetValue(this, converted);
              continue;
            }
            GetType().GetField(fieldName).SetValue(this, uBFieldValue);
          }
        }
      }
    }
  }
}
#endif
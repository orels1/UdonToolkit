#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace UdonToolkit {
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

            uB.publicVariables.RemoveVariable(fieldName);
            uB.publicVariables.TryAddVariable(var);
            uB.publicVariables.TrySetVariableValue(fieldName, var);
          }

          continue;
        }
        
        uB.publicVariables.TrySetVariableValue(fieldName, fieldValue);
      }
    }
  }
}
#endif
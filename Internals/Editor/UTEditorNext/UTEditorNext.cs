using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UdonSharpEditor;
using UdonToolkit;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using Object = UnityEngine.Object;

[assembly: DefaultUdonSharpBehaviourEditor(typeof(UTEditorNext), "UdonToolkit Editor Next")]

namespace UdonToolkit {
  [CustomEditor(typeof(UdonSharpBehaviour), true), CanEditMultipleObjects]
  public class UTEditorNext : Editor {
    private UdonSharpBehaviour t;
    private Type tT;
    private UdonSharpProgramAsset programAsset;

    private VisualElement rootElement;
    private VisualTreeAsset visualTree;

    #region Editor State

    private UTEditor.UTBehaviourInfo behInfo;
    private Dictionary<string, UTEditor.UTField> fieldCache = new Dictionary<string, UTEditor.UTField>();
    private Dictionary<string, List<UTEditor.UTField>> listViews = new Dictionary<string, List<UTEditor.UTField>>();

    private Dictionary<string, List<UTEditor.UTField>> horizontalViews =
      new Dictionary<string, List<UTEditor.UTField>>();

    private Dictionary<string, Dictionary<string, UTEditor.UTFieldType>> tabs =
      new Dictionary<string, Dictionary<string, UTEditor.UTFieldType>>();

    private Dictionary<string, Dictionary<string, UTEditor.UTFieldType>> foldouts =
      new Dictionary<string, Dictionary<string, UTEditor.UTFieldType>>();

    private Dictionary<string, UTEditor.UTFieldType> fieldOrder = new Dictionary<string, UTEditor.UTFieldType>();

    private Dictionary<string, int> listPaginations = new Dictionary<string, int>();
    private Dictionary<string, bool> utilsShown = new Dictionary<string, bool>();

    private Dictionary<string, List<UTPropertyAttribute>> watchHideMethods =
      new Dictionary<string, List<UTPropertyAttribute>>();
    // source: [fields to update]
    private Dictionary<string, List<string>> boundHideVars = new Dictionary<string, List<string>>();

    #endregion

    private void OnEnable() {
      t = (UdonSharpBehaviour)target;
      rootElement = new VisualElement();
      visualTree =
        AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
          "Assets/UdonToolkit/Internals/Editor/UTEditorNext/UTEditorNextTemplate.uxml");
      rootElement.styleSheets.Add(
        AssetDatabase.LoadAssetAtPath<StyleSheet>(
          "Assets/UdonToolkit/Internals/Editor/UTEditorNext/UTEditorNextStyles.uss"));
    }

    public override VisualElement CreateInspectorGUI() {
      rootElement.Clear();
      visualTree.CloneTree(rootElement);
      var c = rootElement.Q("fields");
      HandleUSharpHeader();
      BuildCache();
      c.Bind(serializedObject);

      var toolkitClass = rootElement.Q<Label>("toolkitClassName");
      if (behInfo.customName != null || behInfo.helpUrl != null) {
        toolkitClass.text = behInfo.customName ?? behInfo.name;
        var helpButton = rootElement.Q("toolkitHelpButton");
        if (behInfo.helpUrl == null) {
          helpButton.AddToClassList("hidden");
        }
        else {
          helpButton.Q<Button>().clicked += () => {
            Application.OpenURL(behInfo.helpUrl);
          };
        }
      }
      else {
        rootElement.Q("toolkitHeader").AddToClassList("hidden");
      }
        
      HandleFields(fieldOrder, ref c);
      RegisterVisibilityCallbacks();
      RegisterVisibilityWatchers();
      return rootElement;
    }

    private void HandleUSharpHeader() {
      var uSharpHeader = rootElement.Q<IMGUIContainer>("uSharpHeader");
      var uSharpHeaderButton = rootElement.Q<Button>("uSharpHeaderToggle");
      uSharpHeader.onGUIHandler = () => { UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(t, true); };
      var showUdonSettings = (bool)(UTUtils.GetUTSetting("showUdonSettings", UTUtils.UTSettingType.Bool) ?? false);
      if (!showUdonSettings) {
        uSharpHeader.AddToClassList("hidden");
      }
      else {
        uSharpHeaderButton.AddToClassList("active");
      }

      uSharpHeaderButton.clicked += () => {
        showUdonSettings = !showUdonSettings;
        if (showUdonSettings) {
          uSharpHeader.RemoveFromClassList("hidden");
          uSharpHeaderButton.AddToClassList("active");
        }
        else {
          uSharpHeader.AddToClassList("hidden");
          uSharpHeaderButton.RemoveFromClassList("active");
        }

        UTUtils.SetUTSetting("showUdonSettings", UTUtils.UTSettingType.Bool, showUdonSettings);
      };
    }

    private void BuildCache() {
      tT = t.GetType();
      programAsset = UdonSharpEditorUtility.GetUdonSharpProgramAsset(t);
      behInfo = new UTEditor.UTBehaviourInfo(t);

      var prop = serializedObject.GetIterator();
      var next = prop.NextVisible(true);
      var tabsExist = false;

      if (next) {
        do {
          if (prop.name == "m_Script") {
            continue;
          }

          if (!fieldCache.ContainsKey(prop.name)) {
            var newField = new UTEditor.UTField(prop);
            if (newField.visibilityAttrs.Any()) {
              foreach (var attr in newField.visibilityAttrs) {
                // for UT's HideIf we can use the fact that we can be variable-bound and do some perf savings there
                if (attr is HideIfAttribute attribute) {
                  // the method-based visibility gets saved into the watch list
                  if (!attribute.methodName.StartsWith("@")) {
                    if (watchHideMethods.ContainsKey(prop.name)) {
                      watchHideMethods[prop.name].Add(attribute);
                    }
                    else {
                      watchHideMethods[prop.name] = new List<UTPropertyAttribute> { attribute };
                    }
                  }
                  else { // the variable-based visibility gets bound to specific var change to save on performance
                    var varName = attribute.methodName.Replace("@", "").Replace("!", "");
                    if (boundHideVars.ContainsKey(varName)) {
                      boundHideVars[varName].Add(prop.name);
                    }
                    else {
                      boundHideVars[varName] = new List<string> { prop.name };
                    }
                  }
                }
                else { // non-HideIf attributes are always pushed to the watcher list
                  if (watchHideMethods.ContainsKey(prop.name)) {
                    watchHideMethods[prop.name].Add(attr);
                  }
                  else {
                    watchHideMethods[prop.name] = new List<UTPropertyAttribute> { attr };
                  }
                }
              }
            }
            fieldCache.Add(prop.name, newField);
            if (newField.isInTabGroup) {
              if (!tabs.ContainsKey(newField.tabGroupName)) {
                tabs.Add(newField.tabGroupName, new Dictionary<string, UTEditor.UTFieldType>());
                // we only support one tab group per behaviour right now
                if (!tabsExist) {
                  fieldOrder.Add(newField.tabGroupName, UTEditor.UTFieldType.Tab);
                  tabsExist = true;
                }
              }

              // since tabs can host foldouts - we need to explicitly retarget them
              // this logic could be generalized if i ever want to make all of this recursive
              if (newField.isInFoldout) {
                if (!foldouts.ContainsKey(newField.foldoutName)) {
                  foldouts.Add(newField.foldoutName, new Dictionary<string, UTEditor.UTFieldType>());
                  tabs[newField.tabGroupName].Add(newField.foldoutName, UTEditor.UTFieldType.Foldout);
                }

                AddToFieldOrder(newField, prop, true);
                continue;
              }

              AddToFieldOrder(newField, prop, addToTabGroup: true);
              continue;
            }

            if (newField.isInFoldout) {
              if (!foldouts.ContainsKey(newField.foldoutName)) {
                foldouts.Add(newField.foldoutName, new Dictionary<string, UTEditor.UTFieldType>());
                fieldOrder.Add(newField.foldoutName, UTEditor.UTFieldType.Foldout);
              }

              AddToFieldOrder(newField, prop, true);
              continue;
            }

            AddToFieldOrder(newField, prop);
          }
        } while (prop.NextVisible(false));
      }
    }

    private void AddToFieldOrder(UTEditor.UTField field, SerializedProperty prop, bool addToFoldout = false,
      bool addToTabGroup = false) {
      if (!addToFoldout && !addToTabGroup) {
        AddToFieldOrder(field, prop, ref fieldOrder);
        return;
      }

      if (addToFoldout) {
        var foldoutTarget = foldouts[field.foldoutName];
        AddToFieldOrder(field, prop, ref foldoutTarget);
        foldouts[field.foldoutName] = foldoutTarget;
        return;
      }

      var tabGroupTarget = tabs[field.tabGroupName];
      AddToFieldOrder(field, prop, ref tabGroupTarget);
      tabs[field.tabGroupName] = tabGroupTarget;
      return;
    }

    private void AddToFieldOrder(UTEditor.UTField field, SerializedProperty prop,
      ref Dictionary<string, UTEditor.UTFieldType> targetObj) {
      // we reuse ListView rendering for the arrays
      if (field.isArray && !field.isInListView) {
        listPaginations.Add(prop.displayName, 0);
        utilsShown.Add(prop.displayName, false);
        listViews.Add(prop.displayName, new List<UTEditor.UTField> { field });
        targetObj.Add(prop.displayName, UTEditor.UTFieldType.ListView);
        return;
      }

      if (field.isInListView) {
        if (!listViews.ContainsKey(field.listViewName)) {
          utilsShown.Add(field.listViewName, false);
          listPaginations.Add(field.listViewName, 0);
          listViews.Add(field.listViewName, new List<UTEditor.UTField> { field });
          targetObj.Add(field.listViewName, UTEditor.UTFieldType.ListView);
        }
        else {
          listViews[field.listViewName].Add(field);
        }

        return;
      }

      if (field.isInHorizontal) {
        if (!horizontalViews.ContainsKey(field.horizontalName)) {
          horizontalViews.Add(field.horizontalName, new List<UTEditor.UTField> { field });
          targetObj.Add(field.horizontalName, UTEditor.UTFieldType.Horizontal);
        }
        else {
          horizontalViews[field.horizontalName].Add(field);
        }

        return;
      }
      else {
        targetObj.Add(field.name, UTEditor.UTFieldType.Regular);
      }
    }

    private void HandleFields(Dictionary<string, UTEditor.UTFieldType> fields, ref VisualElement c) {
      foreach (var fieldEntry in fields) {
        switch (fieldEntry.Value) {
          case UTEditor.UTFieldType.Regular: {
            DrawField(fieldEntry.Key, ref c);
            break;
          }
          case UTEditor.UTFieldType.Tab: {
            break;
          }
          case UTEditor.UTFieldType.Foldout: {
            break;
          }
          case UTEditor.UTFieldType.ListView: {
            break;
          }
          case UTEditor.UTFieldType.Horizontal: {
            break;
          }
        }
      }
    }

    private void DrawField(string propName, ref VisualElement c) {
      var prop = serializedObject.FindProperty(propName);
      var field = fieldCache[propName];
      var propType = prop.propertyType;
      var fieldEl = new VisualElement {
        name = $"utFieldContainer_{propName}"
      };
      var el = new PropertyField();
      el.BindProperty(prop);
      el.name = $"utField_{propName}";
      
      // GetVisible
      var isVisible = true;
      foreach (var attr in field.visibilityAttrs) {
        if (!attr.GetVisible(prop)) {
          isVisible = false;
          break;
        }
      }

      if (!isVisible) {
        fieldEl.AddToClassList("hidden");
      }

      // BeforeGUI
      foreach (var uiAttr in field.uiAttrs) {
        if (uiAttr.GetType().GetMethod("BeforeGUI")?.DeclaringType == uiAttr.GetType()) {
          var imgC = new IMGUIContainer();
          imgC.onGUIHandler = () => {uiAttr.BeforeGUI(prop);};
          fieldEl.Add(imgC);
        }
        if (uiAttr.GetType().GetMethod("CreateBeforeGUI")?.DeclaringType == uiAttr.GetType()) {
          fieldEl.Add(uiAttr.CreateBeforeGUI(prop));
        }
      }
      
      switch (propType) {
        case SerializedPropertyType.Boolean: {
          el.RegisterCallback<ChangeEvent<bool>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Bounds: {
          el.RegisterCallback<ChangeEvent<Bounds>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Character: {
          el.RegisterCallback<ChangeEvent<char>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Color: {
          el.RegisterCallback<ChangeEvent<Color>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Enum: {
          el.RegisterCallback<ChangeEvent<string>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Float: {
          el.RegisterCallback<ChangeEvent<float>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Gradient: {
          el.RegisterCallback<ChangeEvent<Gradient>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Integer: {
          el.RegisterCallback<ChangeEvent<int>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Quaternion: {
          el.RegisterCallback<ChangeEvent<Quaternion>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Rect: {
          el.RegisterCallback<ChangeEvent<Rect>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.String: {
          el.RegisterCallback<ChangeEvent<string>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Vector2: {
          el.RegisterCallback<ChangeEvent<Vector2>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Vector3: {
          el.RegisterCallback<ChangeEvent<Vector3>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.Vector4: {
          el.RegisterCallback<ChangeEvent<Vector4>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.AnimationCurve: {
          el.RegisterCallback<ChangeEvent<AnimationCurve>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.BoundsInt: {
          el.RegisterCallback<ChangeEvent<BoundsInt>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.LayerMask: {
          el.RegisterCallback<ChangeEvent<LayerMask>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        case SerializedPropertyType.RectInt: {
          el.RegisterCallback<ChangeEvent<RectInt>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
        default: {
          el.RegisterCallback<ChangeEvent<Object>>(evt => {
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          });
          break;
        }
      }
      fieldEl.Add(el);
      
      // AfterGUI
      foreach (var uiAttr in field.uiAttrs) {
        if (uiAttr.GetType().GetMethod("AfterGUI")?.DeclaringType == uiAttr.GetType()) {
          var imgC = new IMGUIContainer();
          imgC.onGUIHandler = () => {uiAttr.AfterGUI(prop);};
          fieldEl.Add(imgC);
        }
        if (uiAttr.GetType().GetMethod("CreateAfterGUI")?.DeclaringType == uiAttr.GetType()) {
          fieldEl.Add(uiAttr.CreateAfterGUI(prop));
        }
      }
      
      c.Add(fieldEl);
    }

    private void RegisterVisibilityCallbacks() {
      foreach (var hideVar in boundHideVars) {
        var field = rootElement.Q<PropertyField>($"utField_{hideVar.Key}");
        field?.RegisterCallback<ChangeEvent<bool>>(evt => {
          foreach (var targetField in hideVar.Value) {
            var isVisible = fieldCache[targetField].visibilityAttrs.First()
              .GetVisible(serializedObject.FindProperty(targetField));
            var fieldEl = rootElement.Q($"utFieldContainer_{targetField}");
            fieldEl?.EnableInClassList("hidden", !isVisible);
          }
        });
      }
    }

    private void RegisterVisibilityWatchers() {
      var watcher = rootElement.schedule.Execute(() => {
        foreach (var hideMethods in watchHideMethods) {
          var field = rootElement.Q($"utFieldContainer_{hideMethods.Key}");
          var isVisible = true;
          foreach (var attr in hideMethods.Value) {
            isVisible = attr.GetVisible(serializedObject.FindProperty(hideMethods.Key));
            if (!isVisible) {
              break;
            }
          }
          field?.EnableInClassList("hidden", !isVisible);
        }
      });
      watcher.Every(250);
    }
  }
}

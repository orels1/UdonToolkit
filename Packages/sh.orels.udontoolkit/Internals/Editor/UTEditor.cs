using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UdonSharpEditor;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

[assembly: DefaultUdonSharpBehaviourEditor(typeof(UTEditor), "UdonToolkit Editor")]

namespace UdonToolkit {
  [CustomEditor(typeof(UdonSharpBehaviour), true), CanEditMultipleObjects]
  public partial class UTEditor : Editor {
    private UdonSharpBehaviour t;
    private Type tT;
    private UdonSharpProgramAsset programAsset;
    private GUIContent undoArrow;
    private bool nonUBChecked;
    private bool nonUBMode;
    
    #region Editor State
    
    private UTBehaviourInfo behInfo;
    private Dictionary<string, UTField> fieldCache = new Dictionary<string, UTField>();
    private Dictionary<string, List<UTField>> listViews = new Dictionary<string, List<UTField>>();
    private Dictionary<string, List<UTField>> horizontalViews = new Dictionary<string, List<UTField>>();

    private Dictionary<string, Dictionary<string, UTFieldType>> tabs =
      new Dictionary<string, Dictionary<string, UTFieldType>>();

    private Dictionary<string, Dictionary<string, UTFieldType>> foldouts =
      new Dictionary<string, Dictionary<string, UTFieldType>>();

    private Dictionary<string, UTFieldType> fieldOrder = new Dictionary<string, UTFieldType>();

    private Dictionary<string, int> listPaginations = new Dictionary<string, int>();
    private Dictionary<string, bool> utilsShown = new Dictionary<string, bool>();

    private bool cacheBuilt;
    private bool firstRepaint = true;
    private bool droppedObjects;
    private bool shouldReserialize;
    private bool showUdonSettings;
    private bool tabsExist;
    private int tabOpen;
    private bool methodsExpanded;
    private bool buttonsExpanded = true;
    
    #endregion

    public override void OnInspectorGUI() {
      t = (UdonSharpBehaviour) target;

      showUdonSettings = (bool) (UTUtils.GetUTSetting("showUdonSettings", UTUtils.UTSettingType.Bool) ?? false);

      // we force-disable collision transfer for toolkit driven behaviours as it is not applicable
      if (!nonUBChecked) {
        nonUBMode = t.gameObject.GetComponent<UdonBehaviour>() == null;
        nonUBChecked = true;
      }

      if (!nonUBMode) {
        if (UdonSharpEditorUtility.GetBackingUdonBehaviour(t).AllowCollisionOwnershipTransfer) {
          UdonSharpEditorUtility.GetBackingUdonBehaviour(t).AllowCollisionOwnershipTransfer = false;
        }
      }

      var headerExited = false;
      EditorGUI.BeginChangeCheck();
      showUdonSettings = GUILayout.Toggle(showUdonSettings, "Udon Settings", UTStyles.smallButton);
      if (EditorGUI.EndChangeCheck()) {
        UTUtils.SetUTSetting("showUdonSettings", UTUtils.UTSettingType.Bool, showUdonSettings);
      }

      if (showUdonSettings) {
        headerExited = UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(t, true);
      }

      if (headerExited) return;

      #region Caching

      if (!cacheBuilt) {
        tT = t.GetType();
        programAsset = UdonSharpEditorUtility.GetUdonSharpProgramAsset(t);
        behInfo = new UTBehaviourInfo(t);

        var prop = serializedObject.GetIterator();
        var next = prop.NextVisible(true);
        
        if (next) {
          do {
            if (prop.name == "m_Script") {
              continue;
            }
            
            if (!fieldCache.ContainsKey(prop.name)) {
              var newField = new UTField(prop);
              fieldCache.Add(prop.name, newField);
              if (newField.isInTabGroup) {
                if (!tabs.ContainsKey(newField.tabGroupName)) {
                  tabs.Add(newField.tabGroupName, new Dictionary<string, UTFieldType>());
                  // we only support one tab group per behaviour right now
                  if (!tabsExist) {
                    fieldOrder.Add(newField.tabGroupName, UTFieldType.Tab);
                    tabsExist = true;
                  }
                }

                // since tabs can host foldouts - we need to explicitly retarget them
                // this logic could be generalized if i ever want to make all of this recursive
                if (newField.isInFoldout) {
                  if (!foldouts.ContainsKey(newField.foldoutName)) {
                    foldouts.Add(newField.foldoutName, new Dictionary<string, UTFieldType>());
                    tabs[newField.tabGroupName].Add(newField.foldoutName, UTFieldType.Foldout);
                  }

                  AddToFieldOrder(newField, prop, true);
                  continue;
                }

                AddToFieldOrder(newField, prop, addToTabGroup: true);
                continue;
              }

              if (newField.isInFoldout) {
                if (!foldouts.ContainsKey(newField.foldoutName)) {
                  foldouts.Add(newField.foldoutName, new Dictionary<string, UTFieldType>());
                  fieldOrder.Add(newField.foldoutName, UTFieldType.Foldout);
                }

                AddToFieldOrder(newField, prop, true);
                continue;
              }

              AddToFieldOrder(newField, prop);
            }
          } while (prop.NextVisible(false));
        }

        cacheBuilt = true;
        return;
      }
      
      var e = Event.current;
      if (e.type == EventType.Repaint) {
        if (firstRepaint) {
          firstRepaint = false;
          return;
        }
      }

      #endregion

      if (behInfo.customName != null || behInfo.helpUrl != null) {
        EditorGUILayout.BeginHorizontal();
        UTStyles.RenderHeader(behInfo.customName != null ? behInfo.customName : behInfo.name);
        if (behInfo.helpUrl != null) {
          if (GUILayout.Button("?", GUILayout.Height(26), GUILayout.Width(26))) {
            Application.OpenURL(behInfo.helpUrl);
          }
        }
        EditorGUILayout.EndHorizontal();
      }

      if (behInfo.helpMsg != null) {
        UTStyles.RenderNote(behInfo.helpMsg);
      }

      if (behInfo.onBeforeEditor != null) {
        behInfo.onBeforeEditor.Invoke(t, new object[] {serializedObject});
      }

      // handle jagged arrays
      Undo.RecordObject(t, "Change Properties");
      
      EditorGUI.BeginChangeCheck();
      droppedObjects = false;

      // this method is overrideable by custom code
      DrawGUI();

      if (EditorGUI.EndChangeCheck() || droppedObjects || shouldReserialize) {
        if (behInfo.onValuesChanged != null) {
          behInfo.onValuesChanged.Invoke(t, new object[] {serializedObject});
        }
        serializedObject.ApplyModifiedProperties();
      }

      if (droppedObjects) {
        return;
      }

      if (behInfo.onAfterEditor != null) {
        behInfo.onAfterEditor.Invoke(t, new object[] {serializedObject});
      }
      
      #region Buttons
      if (behInfo.buttons != null && !Application.isPlaying && behInfo.buttons.Length > 0) {
        buttonsExpanded = UTStyles.FoldoutHeader("Editor Methods", buttonsExpanded);
        if (buttonsExpanded) {
          EditorGUILayout.BeginVertical(new GUIStyle("helpBox"));
          foreach (var button in behInfo.buttons) {
            if (GUILayout.Button(button.Name)) {
              button.Invoke(t, new object[]{});
            }
          }
          EditorGUILayout.EndVertical();
        }
      }
      if (Application.isPlaying && behInfo.udonCustomEvents.Length > 0 && !nonUBMode) {
        methodsExpanded = UTStyles.FoldoutHeader("Udon Events", methodsExpanded);
        if (methodsExpanded) {
          EditorGUILayout.BeginVertical(new GUIStyle("helpBox"));
          var rowBreak = Mathf.Max(1, Mathf.Min(3, behInfo.udonCustomEvents.Length - 1));
          var rowEndI = -100;
          foreach (var (button, i) in behInfo.udonCustomEvents.WithIndex()) {
            if (i == rowEndI && i != behInfo.udonCustomEvents.Length - 1) {
              EditorGUILayout.EndHorizontal();
            }
            if (i % rowBreak == 0 && i != behInfo.udonCustomEvents.Length - 1) {
              EditorGUILayout.BeginHorizontal();
              rowEndI = Math.Min(i + rowBreak, behInfo.udonCustomEvents.Length - 1);
            }
            if (GUILayout.Button(button)) {
              UdonSharpEditorUtility.GetBackingUdonBehaviour(t).SendCustomEvent(button);
              UdonSharpEditorUtility.CopyUdonToProxy(t);
            }
            if (i == behInfo.udonCustomEvents.Length - 1 && rowEndI != -100) {
              EditorGUILayout.EndHorizontal();
            }
          }
          EditorGUILayout.EndVertical();
        }
      }
      #endregion
    }

    public virtual void DrawGUI() {
      HandleFields(fieldOrder);
    }

    #region Behaviour Handling
    
    private struct UTBehaviourInfo {
      public string name;
      public string customName;
      public string helpUrl;
      public string helpMsg;
      public MethodInfo onValuesChanged;
      public MethodInfo onBeforeEditor;
      public MethodInfo onAfterEditor;
      public string[] udonCustomEvents;
      public MethodInfo[] buttons;

      public UTBehaviourInfo(UdonSharpBehaviour targetBeh) {
        var targetType = targetBeh.GetType();
        name = targetBeh.name;
        var cNameAttr = targetType.GetCustomAttributes(typeof(CustomNameAttribute))
          .Select(i => i as CustomNameAttribute).ToArray();
        customName = cNameAttr.Any() ? cNameAttr.First().name : null;
        var helpUrlAttr = targetType.GetCustomAttributes(typeof(HelpURLAttribute))
          .Select(i => i as HelpURLAttribute).ToArray();
        helpUrl = helpUrlAttr.Any() ? helpUrlAttr.First().URL : null;
        var helpMsgAttr = targetType.GetCustomAttributes(typeof(HelpMessageAttribute))
          .Select(i => i as HelpMessageAttribute).ToArray();
        helpMsg = helpMsgAttr.Any() ? helpMsgAttr.First().helpMessage : null;
        var onValChangedAttr = targetType.GetCustomAttributes(typeof(OnValuesChangedAttribute))
          .Select(i => i as OnValuesChangedAttribute).ToArray();
        onValuesChanged = onValChangedAttr.Any() ? targetType.GetMethod(onValChangedAttr.First().methodName) : null;
        var onBeforeEditorAttr = targetType.GetCustomAttributes(typeof(OnBeforeEditorAttribute))
          .Select(i => i as OnBeforeEditorAttribute).ToArray();
        onBeforeEditor = onBeforeEditorAttr.Any() ? targetType.GetMethod(onBeforeEditorAttr.First().methodName) : null;
        var onAfterEditorAttr = targetType.GetCustomAttributes(typeof(OnAfterEditorAttribute))
          .Select(i => i as OnAfterEditorAttribute).ToArray();
        onAfterEditor = onAfterEditorAttr.Any() ? targetType.GetMethod(onAfterEditorAttr.First().methodName) : null;
        udonCustomEvents = UTUtils.GetUdonEvents(targetBeh);
        if (udonCustomEvents.Length == 1 && udonCustomEvents.First() == "no events found") {
          udonCustomEvents = new string[0];
        }
        buttons = targetType.GetMethods()
          .Where(i => i.GetCustomAttributes(typeof(ButtonAttribute)).Any()).ToArray();
      }
    }

    #endregion

    #region Field Order
    private void AddToFieldOrder(UTField field, SerializedProperty prop, bool addToFoldout = false,
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

    private void AddToFieldOrder(UTField field, SerializedProperty prop,
      ref Dictionary<string, UTFieldType> targetObj) {
      // we reuse ListView rendering for the arrays
      if (field.isArray && !field.isInListView) {
        listPaginations.Add(prop.displayName, 0);
        utilsShown.Add(prop.displayName, false);
        listViews.Add(prop.displayName, new List<UTField> {field});
        targetObj.Add(prop.displayName, UTFieldType.ListView);
        return;
      }

      if (field.isInListView) {
        if (!listViews.ContainsKey(field.listViewName)) {
          utilsShown.Add(field.listViewName, false);
          listPaginations.Add(field.listViewName, 0);
          listViews.Add(field.listViewName, new List<UTField> {field});
          targetObj.Add(field.listViewName, UTFieldType.ListView);
        }
        else {
          listViews[field.listViewName].Add(field);
        }

        return;
      }

      if (field.isInHorizontal) {
        if (!horizontalViews.ContainsKey(field.horizontalName)) {
          horizontalViews.Add(field.horizontalName, new List<UTField> {field});
          targetObj.Add(field.horizontalName, UTFieldType.Horizontal);
        }
        else {
          horizontalViews[field.horizontalName].Add(field);
        }

        return;
      }
      else {
        targetObj.Add(field.name, UTFieldType.Regular);
      }
    }

    private enum UTFieldType {
      Regular,
      Array,
      ListView,
      Horizontal,
      Foldout,
      Tab,
      Button
    }
    
    public struct UTField {
      public string name;
      public FieldInfo fieldInfo;
      public List<Attribute> attributes;
      public List<UTPropertyAttribute> uiAttrs;
      public MethodInfo onValueChaged;
      public bool isValueChangeAtomic;
      public bool isValueChangedFull;
      public bool isValueChangedWithObject;
      public bool isArray;
      public bool isInListView;
      public string listViewName;
      public string listViewColumnName;
      public MethodInfo listViewAddMethod;
      public string listViewAddTitle;
      public bool isInHorizontal;
      public string horizontalName;
      public bool isInFoldout;
      public string foldoutName;
      public bool isInTabGroup;
      public string tabGroupName;
      public string tabSaveTarget;
      public bool isDisabled;
      public bool showLabel;
      
      public UTField(SerializedProperty prop) {
        name = prop.name;
        fieldInfo = prop.serializedObject.targetObject.GetType().GetField(prop.name);
        var tType = prop.serializedObject.targetObject.GetType();
        var field = tType.GetField(prop.name, UTUtils.flags);
        attributes = field != null
          ? field.GetCustomAttributes(typeof(Attribute), true).Select(i => i as Attribute).ToList()
          : new List<Attribute>();
        uiAttrs = attributes.OfType<UTPropertyAttribute>().ToList();
        isArray = prop.isArray && prop.type != "string" && prop.type != "String";
        var lVAttr = attributes.Find(i => i.GetType() == typeof(ListViewAttribute)) as ListViewAttribute;
        isInListView = lVAttr != null;
        listViewName = isInListView ? lVAttr.name : null;
        var lvColumnNameAttr = attributes.OfType<LVHeaderAttribute>().ToList();
        listViewColumnName = lvColumnNameAttr.Any() ? lvColumnNameAttr.First().title : null;
        listViewAddMethod = isInListView && !String.IsNullOrEmpty(lVAttr.addMethodName) ? tType.GetMethod(lVAttr.addMethodName) : null;
        listViewAddTitle = isInListView ? lVAttr.addButtonText : null;
        var hAttr = attributes.Find(i => i.GetType() == typeof(HorizontalAttribute)) as HorizontalAttribute;
        // we do not support combining horizontal with general arrays or list views
        isInHorizontal = !isArray && !isInListView && hAttr != null;
        horizontalName = isInHorizontal ? hAttr.name : null;
        var fAttr = attributes.Find(i => i.GetType() == typeof(FoldoutGroupAttribute)) as FoldoutGroupAttribute;
        isInFoldout = fAttr != null;
        foldoutName = isInFoldout ? fAttr.name : null;
        var tAttr = attributes.Find(i => i.GetType() == typeof(TabGroupAttribute)) as TabGroupAttribute;
        isInTabGroup = tAttr != null;
        tabGroupName = isInTabGroup ? tAttr.name : null;
        tabSaveTarget = isInTabGroup ? tAttr.variableName : null;
        var disabledAttr = attributes.Find(i => i is DisabledAttribute) as DisabledAttribute;
        isDisabled = disabledAttr != null;
        var showLabelAttr = attributes.Find(i => i is ShowLabelAttribute) as ShowLabelAttribute;
        showLabel = showLabelAttr != null;

        // Value Changed Handling
        var vChangeAttr = attributes.OfType<OnValueChangedAttribute>().ToArray();
        onValueChaged = vChangeAttr.Any() ? tType.GetMethod(vChangeAttr.First().methodName) : null;
        if (onValueChaged != null) {
          var vChangeParams = onValueChaged.GetParameters();
          isValueChangeAtomic = vChangeParams.Length == 2 && vChangeParams[1].ParameterType == typeof(int);
          if (isInListView) {
            isValueChangeAtomic = vChangeParams.Length >= 3 && vChangeParams.ToList()
                                    .GetRange(0, vChangeParams.Length - 2)
                                    .TrueForAll(i => i.ParameterType == typeof(SerializedProperty)) &&
                                  vChangeParams[vChangeParams.Length - 1].ParameterType == typeof(int);
          }

          isValueChangedFull = vChangeParams.Length == 1 && vChangeParams[0].ParameterType.IsArray;
          if (isInListView) {
            isValueChangedFull = vChangeParams.Length >= 2 &&
                                 vChangeParams.ToList().TrueForAll(i => i.ParameterType.IsArray);
          }

          isValueChangedWithObject =
            vChangeParams.Length == 2 && vChangeParams[0].ParameterType == typeof(SerializedObject);
          if (isInListView) {
            isValueChangedWithObject =
              vChangeParams.Length >= 3 && vChangeParams[0].ParameterType == typeof(SerializedObject);
          }
        }
        else {
          isValueChangeAtomic = false;
          isValueChangedFull = false;
          isValueChangedWithObject = false;
        }
      }
    }
    #endregion
  }
}

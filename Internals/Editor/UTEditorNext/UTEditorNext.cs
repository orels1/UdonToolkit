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
            DrawFoldout(fieldEntry.Key, ref c);
            break;
          }
          case UTEditor.UTFieldType.ListView: {
            DrawListView(fieldEntry.Key, ref c);
            break;
          }
          case UTEditor.UTFieldType.Horizontal: {
            DrawHorizontalGroup(fieldEntry.Key, ref c);
            break;
          }
        }
      }
    }

    private void DrawField(string propName, ref VisualElement c, bool hideLabel = false) {
      var prop = serializedObject.FindProperty(propName);
      var field = fieldCache[propName];
      var fieldEl = new VisualElement {
        name = $"utFieldContainer_{propName}"
      };
      var el = new PropertyField();
      el.BindProperty(prop);
      el.name = $"utField_{propName}";
      if (hideLabel) {
        el.Q<Label>(null, "unity-label")?.AddToClassList("hidden");
      }
      
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
      if (!field.isInHorizontal && !field.isInListView) {
        DrawGUIStep(GUIStep.Before, field.uiAttrs, prop, ref fieldEl);
      }

      BindChangeEvents(prop, el, o => {
        HandleFieldChange(field);
      });
      // OnGUI
      // we keep the original field to handle the change events, but hide it visually in favor of the custom UI
      foreach (var uiAttr in field.uiAttrs) {
        if (uiAttr.GetType().GetMethod("OnGUI")?.DeclaringType == uiAttr.GetType()) {
          var imgC = new IMGUIContainer();
          imgC.onGUIHandler = () => {uiAttr.OnGUI(prop);};
          c.Add(imgC);
          fieldEl.AddToClassList("hidden");
          break;
        }
        if (uiAttr.GetType().GetMethod("CreateGUI")?.DeclaringType == uiAttr.GetType()) {
          c.Add(uiAttr.CreateGUI(prop, el, () => {
            serializedObject.ApplyModifiedProperties();
            UdonSharpEditorUtility.CopyProxyToUdon(t);
          }));
          fieldEl.AddToClassList("hidden");
          break;
        }
      }
      fieldEl.Add(el);
      
      // AfterGUI
      if (!field.isInHorizontal && !field.isInListView) {
        DrawGUIStep(GUIStep.After, field.uiAttrs, prop, ref fieldEl);
      }

      c.Add(fieldEl);
    }

    private void HandleFieldChange(UTEditor.UTField field) {
      if (field.onValueChaged != null) {
        var prop = serializedObject.FindProperty(field.name);
        field.onValueChaged.Invoke(t, new object[] { prop });
        serializedObject.ApplyModifiedProperties();
      }
      UdonSharpEditorUtility.CopyProxyToUdon(t);
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

    // Horizontal groups draw their children's Before/After GUI directly in the definition order
    private void DrawHorizontalGroup(string propName, ref VisualElement c) {
      var horizontalFields = horizontalViews[propName];
      var fieldCont = new VisualElement();
      fieldCont.AddToClassList("col");
      var row = new VisualElement();
      row.AddToClassList("row");
      row.AddToClassList("horizontalGroup__row");
      var headerAttr = horizontalFields[0].uiAttrs.OfType<HorizontalAttribute>().First();

      if (!headerAttr.hideHeader) {
        fieldCont.AddToClassList("horizontalGroup");
        var header = new Label();
        header.AddToClassList("horizontalGroup__header");
        header.text = headerAttr.name;
        fieldCont.Add(header);
      }
      fieldCont.Add(row);
      
      // BeforeGUI
      foreach (var field in horizontalFields) {
        var prop = serializedObject.FindProperty(field.name);
        DrawGUIStep(GUIStep.Before, field.uiAttrs, prop, ref c);
      }

      foreach (var field in horizontalFields) {
        DrawField(field.name, ref row, !field.attributes.OfType<ShowLabelAttribute>().Any());
      }
      
      // AfterGUI
      foreach (var field in horizontalFields) {
        var prop = serializedObject.FindProperty(field.name);
        DrawGUIStep(GUIStep.After, field.uiAttrs, prop, ref c);
      }
      c.Add(fieldCont);
    }

    private void DrawListView(string propName, ref VisualElement c) {
      var listViewFields = listViews[propName];
      var propsList = listViewFields.Select(i => serializedObject.FindProperty(i.name)).ToList();
      var listView = new VisualElement();
      listView.AddToClassList("listView");
      listView.EnableInClassList("expanded", propsList[0].isExpanded);

      var header = new VisualElement();
      header.AddToClassList("header");
      var foldoutArrow = new VisualElement();
      foldoutArrow.AddToClassList("foldoutArrow");
      var headerText = new Label {
        text = listViewFields[0].listViewName
      };
      headerText.AddToClassList("header__text");
      listView.EnableInClassList("expanded", propsList[0].isExpanded);
      foldoutArrow.EnableInClassList("rotated", propsList[0].isExpanded);
      header.RegisterCallback<MouseUpEvent>(evt => {
        propsList[0].isExpanded = !propsList[0].isExpanded;
        listView.EnableInClassList("expanded", propsList[0].isExpanded);
        foldoutArrow.EnableInClassList("rotated", propsList[0].isExpanded);
        serializedObject.ApplyModifiedProperties();
        UdonSharpEditorUtility.CopyProxyToUdon(t);
      });

      header.Add(foldoutArrow);
      header.Add(headerText);
      listView.Add(header);

      var body = new VisualElement();
      body.AddToClassList("listView__body");
      
      var arrSizeField = new IntegerField();
      arrSizeField.bindingPath = $"{propsList[0].name}.Array.size";
      arrSizeField.Bind(serializedObject);
      arrSizeField.AddToClassList("hidden");
      c.Add(arrSizeField);
      
      // handle array elements removal / addition
      arrSizeField.RegisterValueChangedCallback(evt => {
        var fieldsToRemove = new List<PropertyField>();
        var fieldsToAdd = new List<int>();
        var paths = new List<string>();
        for (int i = 0; i < propsList[0].arraySize; i++) {
          var elPropPath = propsList[0].GetArrayElementAtIndex(i).propertyPath;
          paths.Add(elPropPath);
          if (body.Query<PropertyField>()
            .Where(f => f.bindingPath == elPropPath).First() == null) {
            fieldsToAdd.Add(i);
          }
        }

        body.Query(null, "row").ForEach(f => {
          var firstField = f.Q<PropertyField>();
          if (!paths.Contains(firstField.bindingPath)) {
            fieldsToRemove.Add(firstField);
          }
        });

        foreach (var field in fieldsToRemove) {
          body.Remove(field.parent);
        }

        foreach (var field in fieldsToAdd) {
          var fieldCont = DrawListViewRow(propsList, listViewFields, field, body);
          body.Add(fieldCont);
          if (field == 0) {
            fieldCont.PlaceBehind(body.Children().ToArray()[0]);
          }
          else {
            fieldCont.PlaceInFront(body.Children().ToArray()[field - 1]);
          }
        }
      });
      
      // initial field creation
      var listSize = propsList[0].arraySize;
      // force the same size for all arrays
      foreach (var prop in propsList) {
        prop.arraySize = listSize;
        serializedObject.ApplyModifiedProperties();
        UdonSharpEditorUtility.CopyProxyToUdon(t);
      }
      for (int i = 0; i < listSize; i++) {
        var fieldCont = DrawListViewRow(propsList, listViewFields, i, body);
        body.Add(fieldCont);
      }

      var addElement = new Button {
        text = "Add Element",
        name = $"{propName}_addButton"
      };
      addElement.clicked += () => {
        foreach (var prop in propsList) {
          prop.arraySize += 1;
        }
        serializedObject.ApplyModifiedProperties();
        HandleFieldChange(listViewFields[0]);
      };
      
      body.Add(addElement);
      listView.Add(body);
      
      for (int i = 0; i < listViewFields.Count; i++) {
        DrawGUIStep(GUIStep.Before, listViewFields[i].uiAttrs, propsList[i], ref c);
      }
      
      // Inject list view into layout
      c.Add(listView);

      for (int i = 0; i < listViewFields.Count; i++) {
        DrawGUIStep(GUIStep.After, listViewFields[i].uiAttrs, propsList[i], ref c);
      }
    }

    private VisualElement DrawListViewRow(List<SerializedProperty> propsList, List<UTEditor.UTField> listViewFields, int index, VisualElement body) {
      var fieldCont = new VisualElement();
      fieldCont.AddToClassList("row");
      var j = 0;
      foreach (var prop in propsList) {
        var el = new PropertyField(prop.GetArrayElementAtIndex(index));
        el.AddToClassList("field");
        if (prop.GetArrayElementAtIndex(index).propertyType == SerializedPropertyType.Boolean) {
          el.AddToClassList("field__bool");
        }

        if (j == propsList.Count - 1) {
          el.AddToClassList("last");
        }
        el.Bind(serializedObject);
        el.Q<Label>(null, "unity-label")?.AddToClassList("hidden");
        var changeIndex = j;
        BindChangeEvents(prop.GetArrayElementAtIndex(index), el, o => {
          HandleFieldChange(listViewFields[changeIndex]);
        });
        fieldCont.Add(el);

        foreach (var uiAttr in listViewFields[j].uiAttrs) {
          if (uiAttr.GetType().GetMethod("OnGUI")?.DeclaringType == uiAttr.GetType() && !(uiAttr is PopupAttribute)) {
            var imgC = new IMGUIContainer();
            imgC.onGUIHandler = () => {uiAttr.OnGUI(prop);};
            fieldCont.Add(imgC);
            el.AddToClassList("hidden");
            break;
          }
          if (uiAttr.GetType().GetMethod("CreateGUI")?.DeclaringType == uiAttr.GetType() && !(uiAttr is PopupAttribute)) {
            fieldCont.Add(uiAttr.CreateGUI(prop, el, () => {
              serializedObject.ApplyModifiedProperties();
              UdonSharpEditorUtility.CopyProxyToUdon(t);
            }));
            el.AddToClassList("hidden");
            break;
          }
        }
        j++;
      }
        
      var removeBtn = new Button {
        text = ""
      };
      removeBtn.AddToClassList("listView__button");
      removeBtn.AddToClassList("remove");
      removeBtn.clicked += () => {
        var removeIndex = removeBtn.parent.parent.IndexOf(removeBtn.parent);
        foreach (var prop in propsList) {
          var currLength = prop.arraySize;
          prop.DeleteArrayElementAtIndex(removeIndex);
          if (prop.arraySize == currLength) { // if it is an object reference - we have to delete twice!
            prop.DeleteArrayElementAtIndex(removeIndex);
          }
        }
        body.Remove(removeBtn.parent);
        serializedObject.ApplyModifiedProperties();
        HandleFieldChange(listViewFields[0]);
      };
      fieldCont.Add(removeBtn);
      return fieldCont;
    }

    private void DrawFoldout(string propName, ref VisualElement c) {
      var foldout = foldouts[propName].First();
      UTEditor.UTField field;
      switch (foldout.Value) {
        case UTEditor.UTFieldType.Horizontal: {
          field = horizontalViews[foldout.Key].First();
          break;
        }
        case UTEditor.UTFieldType.ListView: {
          field = listViews[foldout.Key].First();
          break;
        }
        default: {
          field = fieldCache[foldout.Key];
          break;
        }
      }

      var foldoutCont = new VisualElement {
        name = $"utFieldContainer_{propName}"
      };
      foldoutCont.AddToClassList("foldout");
      var header = new VisualElement();
      header.AddToClassList("header");
      var foldoutArrow = new VisualElement();
      foldoutArrow.AddToClassList("foldoutArrow");
      var headerText = new Label {
        text = field.foldoutName
      };
      headerText.AddToClassList("header__text");
      var prop = serializedObject.FindProperty(field.name);
      foldoutCont.EnableInClassList("expanded", prop.isExpanded);
      foldoutArrow.EnableInClassList("rotated",prop.isExpanded);
      header.RegisterCallback<MouseUpEvent>(evt => {
        prop.isExpanded = !prop.isExpanded;
        foldoutCont.EnableInClassList("expanded", prop.isExpanded);
        foldoutArrow.EnableInClassList("rotated",prop.isExpanded);
        serializedObject.ApplyModifiedProperties();
        UdonSharpEditorUtility.CopyProxyToUdon(t);
      });
      header.Add(foldoutArrow);
      header.Add(headerText);
      foldoutCont.Add(header);

      var foldoutBody = new VisualElement();
      foldoutBody.AddToClassList("foldout__body");
      HandleFields(foldouts[propName], ref foldoutBody);
      foldoutCont.Add(foldoutBody);
      
      c.Add(foldoutCont);
    }
    
    private enum GUIStep {
      Before,
      After
    }

    private void DrawGUIStep(GUIStep step, List<UTPropertyAttribute> uiAttrs, SerializedProperty prop, ref VisualElement c) {
      foreach (var uiAttr in uiAttrs) {
        if (uiAttr.GetType().GetMethod(step == GUIStep.Before ? "BeforeGUI" : "AfterGUI")?.DeclaringType == uiAttr.GetType()) {
          var imgC = new IMGUIContainer();
          imgC.onGUIHandler = () => {
            if (step == GUIStep.Before) {
              uiAttr.BeforeGUI(prop);
            }
            else {
              uiAttr.AfterGUI(prop);
            }
          };
          c.Add(imgC);
        }
        if (uiAttr.GetType().GetMethod(step == GUIStep.Before ? "CreateBeforeGUI" : "CreateAfterGUI")?.DeclaringType == uiAttr.GetType()) {
          c.Add(step == GUIStep.Before ? uiAttr.CreateBeforeGUI(prop) : uiAttr.CreateAfterGUI(prop));
        }
      }
    }

    private void BindChangeEvents(SerializedProperty property, PropertyField field, Action<object> handleChange) {
      switch (property.propertyType) {
        case SerializedPropertyType.Boolean: {
          field.RegisterCallback<ChangeEvent<bool>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Bounds: {
          field.RegisterCallback<ChangeEvent<Bounds>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Character: {
          field.RegisterCallback<ChangeEvent<char>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Color: {
          field.RegisterCallback<ChangeEvent<Color>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Enum: {
          field.RegisterCallback<ChangeEvent<string>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Float: {
          field.RegisterCallback<ChangeEvent<float>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Gradient: {
          field.RegisterCallback<ChangeEvent<Gradient>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Integer: {
          field.RegisterCallback<ChangeEvent<int>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Quaternion: {
          field.RegisterCallback<ChangeEvent<Quaternion>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Rect: {
          field.RegisterCallback<ChangeEvent<Rect>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.String: {
          field.RegisterCallback<ChangeEvent<string>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Vector2: {
          field.RegisterCallback<ChangeEvent<Vector2>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Vector3: {
          field.RegisterCallback<ChangeEvent<Vector3>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.Vector4: {
          field.RegisterCallback<ChangeEvent<Vector4>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.AnimationCurve: {
          field.RegisterCallback<ChangeEvent<AnimationCurve>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.BoundsInt: {
          field.RegisterCallback<ChangeEvent<BoundsInt>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.LayerMask: {
          field.RegisterCallback<ChangeEvent<LayerMask>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        case SerializedPropertyType.RectInt: {
          field.RegisterCallback<ChangeEvent<RectInt>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
        default: {
          field.RegisterCallback<ChangeEvent<Object>>(evt => {
            handleChange(evt.newValue);
          });
          break;
        }
      }
    }
  }
}

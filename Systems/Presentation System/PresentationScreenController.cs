using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  public class PresentationScreenController : UdonSharpBehaviour {
    [SectionHeader("Material Swap")]
    public bool swapMaterials;

    
    [ListView("Material Swap Targets")][LVHeader("Meshes")]
    public MeshRenderer[] targetMeshes;
    [ListView("Material Swap Targets")][LVHeader("Default Mats")]
    public Material[] defaultMaterials;
    [ListView("Material Swap Targets")][LVHeader("Active Mats")]
    public Material[] presentationActiveMaterials;

    [SectionHeader("Object Toggles")]
    public bool toggleObjects;

    [ListView("On Talk Start")] [LVHeader("Objects")]
    public GameObject[] talkStartObjects;

    [ListView("On Talk Start")] [LVHeader("Actions")][Popup("@objectActions")]
    public int[] talkStartActions;
    
    [ListView("On Talk End")] [LVHeader("Objects")]
    public GameObject[] talkEndObjects;

    [ListView("On Talk End")] [LVHeader("Actions")][Popup("@objectActions")]
    public int[] talkEndActions;

    [NonSerialized] public string[] objectActions = new[] {
      "Enable",
      "Disable",
      "Toggle"
    };
    
    private void Start() {
    }

    public void HandleTalkStart() {
      if (swapMaterials) {
        SwapMaterials(true);
      }

      if (!toggleObjects) return;
      ToggleObjects(talkStartObjects, talkStartActions);
    }
    
    public void HandleTalkEnd() {
      if (swapMaterials) {
        SwapMaterials(false);
      }

      if (!toggleObjects) return;
      ToggleObjects(talkEndObjects, talkEndActions);
    }

    private void SwapMaterials(bool target) {
      for (int i = 0; i < targetMeshes.Length; i++) {
        if (targetMeshes[i].materials.Length > 1) {
          targetMeshes[i].materials[0] = target ? presentationActiveMaterials[i] : defaultMaterials[i];
          continue;
        }
        targetMeshes[i].material = target ? presentationActiveMaterials[i] : defaultMaterials[i];
      }
    }

    private void ToggleObjects(GameObject[] objects, int[] actions) {
      for (int i = 0; i < objects.Length; i++) {
        switch (actions[i]) {
          case 0: {
            objects[i].SetActive(true);
            break;
          }
          case 1: {
            objects[i].SetActive(false);
            break;
          }
          case 2: {
            objects[i].SetActive(!objects[i].activeSelf);
            break;
          }
        }
      }
    }
  }
}

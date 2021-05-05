using UdonSharp;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Voice Controller")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  public class PresentationVoiceController : UdonSharpBehaviour {
    public PresentationPlayer presentationPlayer;
    [SectionHeader("Access Control")]
    [Toggle]
    [HelpBox("You can limit the people who can get voice boost using the whitelist of names below", "@restrictAccess")]
    public bool restrictAccess;
    [HideIf("@!restrictAccess")]
    public string[] allowedUsers;

    [SectionHeader("General")] [RangeSlider(0f, 500f)][OnValueChanged("AdjustBoost")]
    public float rangeBoost = 250f;

    [HelpBox("Be careful with this setting, setting the range boost above should be enough in most cases")][RangeSlider(-5f, 5f)]
    public float gainTweak;
    [HideInInspector]
    public float gainAdjust;
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    // this auto-corrects for the distance boost based on the settings recommended by Phasedragon
    // https://twitter.com/Phasedragoon/status/1321252784688173056
    public void AdjustBoost(SerializedProperty prop) {
      if (prop.floatValue < 100) {
        prop.serializedObject.FindProperty("gainAdjust").floatValue = remap(prop.floatValue, 0, 100, 0, 10) * -1;
      } else {
        prop.serializedObject.FindProperty("gainAdjust").floatValue = remap(prop.floatValue, 100, 500, 10, 15) * -1;
      }
    }
    
    private float remap(float s, float a1, float a2, float b1, float b2) {
      return b1 + (s-a1)*(b2-b1)/(a2-a1);
    }
    #endif

    [SectionHeader("Adjustment Rules")]
    [HelpBox("If you want to boost the voice when the player enters an area - use this option and add as many PresentationVoiceZones as you like")]
    public bool useZones;

    [HelpBox("If you want to boost the voice when the presenter starts the presentation - use this option")]
    public bool adjustOnPresentationStart;

    [HideInInspector] public float personalSpeakerOffset = 1f;

    private VRCPlayerApi owner;
    private bool ownerLeft;

    public override void OnPlayerLeft(VRCPlayerApi player) {
      if (owner == player) {
        ownerLeft = true;
      }
    }

    public void HandleTalkStart() {
      ownerLeft = false;
      if (!adjustOnPresentationStart) return;
      owner = Networking.GetOwner(presentationPlayer.gameObject);
      owner.SetVoiceGain(15 + gainAdjust * personalSpeakerOffset + gainTweak);
      owner.SetPlayerTag("isSpeaker", "true");
      owner.SetVoiceDistanceFar(25 + rangeBoost);
    }

    public void HandleTalkEnd() {
      ownerLeft = false;
      if (!adjustOnPresentationStart) return;
      owner = Networking.GetOwner(presentationPlayer.gameObject);
      owner.SetVoiceGain(15);
      owner.SetPlayerTag("isSpeaker", "false");
      owner.SetVoiceDistanceFar(25);
    }

    public void NormalizeVolume() {
      if (!adjustOnPresentationStart) return;
      if (ownerLeft) return;
      if (owner == null) return;
      owner.SetVoiceGain(15);
      owner.SetVoiceDistanceFar(25);
    }

    public void EnterZone(VRCPlayerApi player) {
      if (!useZones) return;
      player.SetVoiceGain(15 + gainAdjust * personalSpeakerOffset + gainTweak);
      player.SetVoiceDistanceFar(25 + rangeBoost);
    }
    
    public void ExitZone(VRCPlayerApi player) {
      if (!useZones) return;
      player.SetVoiceGain(15);
      player.SetVoiceDistanceFar(25);
    }
    
  }
}

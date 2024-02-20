using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Voice Zone")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  [HelpMessage("This object will not fire the zone enter/exit triggers if its disabled, for your convenience")]
  public class PresentationVoiceZone : UdonSharpBehaviour {
    [HelpBox("This object requires a trigger collider to function", "CheckCollider")]
    [HelpBox("It is recommended to put this object onto a MirroReflection layer", "CheckLayer")]
    public PresentationVoiceController voiceController;
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public bool CheckCollider() {
      var col = GetComponent<Collider>();
      if (col == null) {
        return true;
      }

      return !col.isTrigger;
    }

    public bool CheckLayer() {
      return gameObject.layer != 18;
    }
    #endif

    public override void OnPlayerTriggerEnter(VRCPlayerApi player) {
      if (!gameObject.activeSelf) return;
      // we do not need to adjust our own voice
      if (player.isLocal) return;
      voiceController.EnterZone(player);
    }
    
    public override void OnPlayerTriggerExit(VRCPlayerApi player) {
      if (!gameObject.activeSelf) return;
      // we do not need to adjust our own voice
      if (player.isLocal) return;
      voiceController.ExitZone(player);
    }
  }
}

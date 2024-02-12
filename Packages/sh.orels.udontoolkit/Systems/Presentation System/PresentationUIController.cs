using System;
using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("UI Controller")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  public class PresentationUIController : UdonSharpBehaviour {
    public PresentationPlayer presentationPlayer;
    public GameObject presentationUi;
    
    [SectionHeader("Ownership Changes")]
    public GameObject[] ownerObjects;
    public GameObject[] publicObjects;
    public Collider ownerCollider;
    public Collider publicCollider;
    
    [SectionHeader("Text Elements")]
    public TextMeshProUGUI talkSelectorTitle;
    public TextMeshProUGUI slideIndexText;
    public TextMeshProUGUI timeLeftText;

    private string[] talkNames;
    private int selectorTalkIndex;

    private int totalSlides;
    private int slideIndex;
    private float timeLeft;

    private float heightShift;
    
    private void Start() {
      var talks = presentationPlayer.talks;
      if (!Networking.IsOwner(presentationPlayer.gameObject)) {
        ownerCollider.enabled = false;
      }
      if (talks.Length == 0) return;
      talkNames = new string[talks.Length];
      for (int i = 0; i < talks.Length; i++) {
        talkNames[i] = talks[i].talkTitle;
      }

      talkSelectorTitle.text = talkNames[0];
    }

    public void SelectorPrev() {
      if (talkNames.Length == 0) return;
      selectorTalkIndex = Mathf.Clamp(selectorTalkIndex - 1, 0, talkNames.Length - 1);
      talkSelectorTitle.text = talkNames[selectorTalkIndex];
    }

    public void SelectorNext() {
      if (talkNames.Length == 0) return;
      selectorTalkIndex = Mathf.Clamp(selectorTalkIndex + 1, 0, talkNames.Length - 1);
      talkSelectorTitle.text = talkNames[selectorTalkIndex];
    }

    public void SelectorConfirm() {
      presentationPlayer.SelectTalk(selectorTalkIndex);
    }

    public void HandleTakeControl() {
      ownerCollider.enabled = true;
      publicCollider.enabled = false;
      foreach (var o in ownerObjects) {
        o.SetActive(true);
      }

      foreach (var o in publicObjects) {
        o.SetActive(false);
      }
    }
    
    public void HandleLoseControl() {
      ownerCollider.enabled = false;
      publicCollider.enabled = true;
      foreach (var o in ownerObjects) {
        o.SetActive(false);
      }

      foreach (var o in publicObjects) {
        o.SetActive(true);
      }
    }

    public void LowerUi() {
      if (heightShift < -1.5) return;
      heightShift -= 0.05f;
      presentationUi.transform.position = presentationUi.transform.position + Vector3.up * -0.05f;
    }
    
    public void RaiseUi() {
      if (heightShift > 2) return;
      heightShift += 0.05f;
      presentationUi.transform.position = presentationUi.transform.position + Vector3.up * 0.05f;
    }

    public void HandleTalkStart() {
      slideIndex = 1;
      var currTalkIndex = presentationPlayer.talkIndex;
      var currTalk = presentationPlayer.talks[currTalkIndex];
      Debug.Log($"received start callback {currTalk}, {currTalkIndex}");
      selectorTalkIndex = currTalkIndex;
      talkSelectorTitle.text = talkNames[selectorTalkIndex];
      totalSlides = currTalk.slideCount;
      timeLeft = currTalk.talkLength;
      slideIndexText.text = $"{slideIndex} of {totalSlides}";
    }

    public void HandleTalkEnd() {
      slideIndexText.text = "--";
      timeLeftText.text = "--";
      timeLeft = 0;
    }

    public void HandleNextSlide() {
      slideIndex++;
      slideIndexText.text = $"{slideIndex} of {totalSlides}";
    }

    public void HandlePrevSlide() {
      slideIndex--;
      slideIndexText.text = $"{slideIndex} of {totalSlides}";
    }

    private void Update() {
      if (timeLeft <= 0) return;
      timeLeft -= Time.deltaTime / 60;
      timeLeftText.text = $"{Mathf.CeilToInt(timeLeft)}m left";
    }
  }
}

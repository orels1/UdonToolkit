using System;
using UdonSharp;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Presentation Talk")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  public class PresentationTalk : UdonSharpBehaviour {
    public VRCUrl talkUrl;
    public string talkTitle;
    public string talkAuthor;
    [Popup("@lengthOptions")] public float talkLength = 30;

    [NonSerialized] public float[] lengthOptions = new[] {15f, 30f, 45f, 60f, 90f, 120f};


    [FoldoutGroup("Configuration Wizard")] public int slideCount = 1;

    [FoldoutGroup("Configuration Wizard")]
    [HelpBox(
      "Duration of individual slides. Set this to the length of each slide you set in your Presentation Software export settings")]
    public float slideDuration = 3f;

    [FoldoutGroup("Configuration Wizard")]
    [Toggle]
    [HelpBox(
      "Clicking this button will auto-populate the Slides list with the amount of slides and durations specified above")]
    [OnValueChanged("ConfigureTalkSlides")]
    public bool configureTalk;

    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void ConfigureTalkSlides(SerializedProperty prop) {
      var slides = prop.serializedObject.FindProperty("slideDurations");
      slides.ClearArray();
      slides.arraySize = slideCount;
      for (int i = 0; i < slideCount; i++) {
        slides.GetArrayElementAtIndex(i).floatValue = slideDuration;
      }

      prop.boolValue = false;
    }
    #endif

    [HelpBox(
      "Check the Autoplay checkbox if the slide contains animations / video, that will make the slide play till the end after switching")]
    [ListView("Slides")]
    public float[] slideDurations;

    [ListView("Slides")] [LVHeader("Autoplay")]
    public bool[] shouldAutoplay;

    [HideInInspector] public PresentationVideoPlayer mainPlayerController;
    [HideInInspector] public PresentationVideoPlayer lookaheadPlayerController;

    private BaseVRCVideoPlayer mainPlayer;
    private BaseVRCVideoPlayer lookaheadPlayer;

    private bool isOwner;

    private void Start() {
      isOwner = Networking.IsOwner(gameObject);
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player) {
      isOwner = Networking.IsOwner(gameObject);
    }

    public override void OnPlayerLeft(VRCPlayerApi player) {
      isOwner = Networking.IsOwner(gameObject);
    }

    public void Prepare(int slideIndex) {
      if (mainPlayerController == null) return;
      Debug.Log("Prepping talk");
      mainPlayer = mainPlayerController.vPlayer;
      lookaheadPlayer = lookaheadPlayerController.vPlayer;

      mainPlayerController.currUrl = talkUrl;
      currSlide = slideIndex;
      var currSlideStart = GetSlideStart();
      mainPlayerController.LoadAndSeek(currSlideStart);

      if (!isOwner) {
        lookaheadPlayer.Stop();
        lookaheadPlayerController.enabled = false;
        return;
      }

      lookaheadPlayerController.enabled = true;
      lookaheadPlayerController.currUrl = talkUrl;
      lookaheadPlayerController.LoadAndSeek(GetNextSlideEnd(currSlideStart));
    }

    [HideInInspector] public int currSlide;
    private bool playingContinuously;
    private float shouldStopAt;

    public void Play() {
      mainPlayerController.Play();
      mainPlayerController.SetTime(0);
      mainPlayerController.Pause();
      currSlide = 0;
      if (!isOwner) {
        return;
      }
      
      lookaheadPlayerController.Play();
      if (slideDurations.Length > 1) {
        Debug.Log($"setting lookahead time to {GetNextSlideEnd(0)}");
        lookaheadPlayerController.SetTime(GetNextSlideEnd(0));
      }
      else {
        lookaheadPlayerController.SetTime(slideDurations[0] + 0.3f);
      }

      lookaheadPlayerController.Pause();
      Debug.Log("Started slides");
    }

    // for late joiners
    public void SeekToSlide(int slideIndex) {
      PauseContinuous();
      mainPlayerController.Play();
      currSlide = Mathf.Clamp(slideIndex, 0, slideDurations.Length - 1);
      var startTime = GetSlideStart();
      mainPlayerController.SetTime(startTime + 0.3f);
      Debug.Log($"ut: seeked to slide {currSlide}, time {startTime}");
      if (isOwner) {
        if (currSlide + 1 != slideDurations.Length) {
          lookaheadPlayer.Play();
          lookaheadPlayer.SetTime(GetNextSlideEnd(startTime));
          lookaheadPlayer.Pause();
        }
      }

      if (!shouldAutoplay[currSlide]) {
        mainPlayerController.Pause();
      }
      else {
        playingContinuously = true;
        if (currSlide + 1 == slideDurations.Length) {
          shouldStopAt = float.MaxValue;
        }
        else {
          shouldStopAt = startTime + slideDurations[currSlide];
        }
      }
    }

    private float GetSlideStart() {
      var res = 0f;
      for (int i = 0; i < currSlide; i++) {
        res += slideDurations[i];
      }

      return res == 0f ? 0.3f : res;
    }

    private float GetNextSlideEnd(float startTime) {
      if (currSlide + 1 == slideDurations.Length) return startTime;
      if (currSlide - 1 < 0) return slideDurations[0] + slideDurations[1] - 0.3f;
      return startTime + slideDurations[currSlide] + slideDurations[currSlide + 1] - 0.3f;
    }

    public void NextSlide() {
      if (currSlide + 1 == slideDurations.Length) return;
      PauseContinuous();
      currSlide += 1;
      mainPlayerController.Play();
      var startTime = GetSlideStart();
      mainPlayerController.SetTime(startTime + 0.3f);
      if (isOwner) {
        if (currSlide + 1 != slideDurations.Length) {
          lookaheadPlayerController.Play();
          lookaheadPlayerController.SetTime(GetNextSlideEnd(startTime));
          lookaheadPlayerController.Pause();
        }
      }

      if (!shouldAutoplay[currSlide]) {
        mainPlayerController.Pause();
      }
      else {
        playingContinuously = true;
        if (currSlide + 1 == slideDurations.Length) {
          shouldStopAt = float.MaxValue;
        }
        else {
          shouldStopAt = startTime + slideDurations[currSlide];
        }
      }

      // Debug.Log($"Next Slide {currSlide} {mainPlayer.GetTime()}");
    }

    public void PrevSlide() {
      if (currSlide == 0) return;
      PauseContinuous();
      currSlide -= 1;
      mainPlayerController.Play();
      lookaheadPlayerController.Play();
      var startTime = GetSlideStart();
      mainPlayerController.SetTime(startTime + 0.3f);
      if (isOwner) {
        lookaheadPlayerController.SetTime(GetNextSlideEnd(startTime));
        lookaheadPlayerController.Pause();
      }
      if (!shouldAutoplay[currSlide]) {
        mainPlayerController.Pause();
      }
      else {
        playingContinuously = true;
        shouldStopAt = startTime + slideDurations[currSlide];
      }

      // Debug.Log($"Prev Slide, {currSlide}, {mainPlayerController.GetTime()}");
    }

    public void PlayContinuous() {
      mainPlayerController.Play();
      var startTime = GetSlideStart();
      shouldStopAt = startTime + slideDurations[currSlide];
      playingContinuously = true;
      Debug.Log("Playing Slides");
    }

    public void PauseContinuous() {
      if (!playingContinuously) return;
      mainPlayerController.Pause();
      playingContinuously = false;
      Debug.Log("Paused Slides");
    }

    private void Update() {
      if (!playingContinuously) return;
      var currTime = mainPlayer.GetTime();
      if (currTime > shouldStopAt - 0.1f) {
        Debug.Log("Reached slide end, pausing");
        PauseContinuous();
      }
    }

    public void Stop() {
      playingContinuously = false;
      mainPlayerController.Stop();
      lookaheadPlayerController.Stop();
    }
  }
}

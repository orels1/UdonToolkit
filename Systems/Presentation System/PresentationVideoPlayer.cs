using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [CustomName("Presentation Video Player")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  public class PresentationVideoPlayer : UdonSharpBehaviour {
    public BaseVRCVideoPlayer vPlayer;
    public VRCUrl currUrl;
    [HideInInspector] public float queuedTime;
    private bool waitingToSeek;

    public void LoadAndSeek(float time) {
      vPlayer.LoadURL(currUrl);
      queuedTime = time;
      waitingToSeek = true;
    }

    public void Play() {
      vPlayer.Play();
    }

    public void SetTime(float time) {
      vPlayer.SetTime(time);
    }

    public void Pause() {
      vPlayer.Pause();
    }

    public void Stop() {
      vPlayer.Stop();
    }

    private bool retrying;
    private float timeoutEnd;
      
    public override void OnVideoError(VideoError videoError) {
      switch (videoError) {
        case VideoError.RateLimited:
          retrying = true;
          timeoutEnd = Time.timeSinceLevelLoad + 5.5f;
          break;
        case VideoError.Unknown:
          Debug.Log($"ut: got video error {videoError}");
          break;
        case VideoError.InvalidURL:
          Debug.Log($"ut: got video error {videoError}");
          break;
        case VideoError.AccessDenied:
          Debug.Log($"ut: got video error {videoError}");
          break;
        case VideoError.PlayerError:
          Debug.Log($"ut: got video error {videoError}");
          retrying = true;
          timeoutEnd = Time.timeSinceLevelLoad + 5.5f;
          break;
        default:
          Debug.Log($"ut: got video error {videoError}");
          break;
      }
    }

    private void Update() {
      if (!retrying) return;
      if (Time.timeSinceLevelLoad < timeoutEnd) return;
      retrying = false;
      vPlayer.LoadURL(currUrl);
    }

    public override void OnVideoReady() {
      vPlayer.Play();
      if (waitingToSeek) {
        Debug.Log($"{name} seeking to {queuedTime}");
        vPlayer.SetTime(queuedTime);
        waitingToSeek = false;
      }
      else {
        vPlayer.SetTime(1);
      }
      vPlayer.Pause();
    }
  }
}

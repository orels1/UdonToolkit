
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FlightSystemDebugger : UdonSharpBehaviour {
  public void OnHoverStart() {
    Debug.Log("ut: Hover Start");
  }

  public void OnHoverEnd() {
    Debug.Log("ut: Hover End");
  }

  public void OnGlideStart() {
    Debug.Log("ut: Glide Start");
  }

  public void OnGlideEnd() {
    Debug.Log("ut: Glide End");
  }

  public void OnFlightStart() {
    Debug.Log("ut: Flight Start");
  }

  public void OnFlightEnd() {
    Debug.Log("ut: Flight End");
  }
}

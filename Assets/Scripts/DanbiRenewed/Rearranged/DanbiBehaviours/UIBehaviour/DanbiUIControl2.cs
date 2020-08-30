using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIControl2 : MonoBehaviour {
    internal enum DanbiUIStatus {
      NONE, Room, Panorama, Reflector, Camera, Simulator
    };

    [Readonly, SerializeField]
    DanbiUIStatus UIStatus = DanbiUIStatus.NONE;

    [Readonly, SerializeField]
    Button Button_Room;

    [Readonly, SerializeField]
    Button Button_Panorama;

    [Readonly, SerializeField]
    Button Button_Reflector;

    [Readonly, SerializeField]
    Button Button_Camera;

    [Readonly, SerializeField]
    Button Button_Simulator;

    void Start() {
      // 1. Get resources.
      foreach (var e in GetComponentsInChildren<Button>()) {
        if (e.name.Contains("Room")) {
          Button_Room = e;
          Button_Room.onClick.AddListener(OnRoomMenuButtonClicked);
        }

        if (e.name.Contains("Panorama")) {
          Button_Panorama = e;
          Button_Panorama.onClick.AddListener(OnPanoramaMenuButtonClicked);
        }

        if (e.name.Contains("Reflector")) {
          Button_Reflector = e;
          Button_Reflector.onClick.AddListener(OnReflectorMenuButtonClicked);
        }

        if (e.name.Contains("Camera")) {
          Button_Camera = e;
          Button_Camera.onClick.AddListener(OnCameraMenuButtonClicked);
        }

        if (e.name.Contains("Simulator")) {
          Button_Simulator = e;
          Button_Simulator.onClick.AddListener(OnSimulatorMenuButtonClicked);
        }
      }
    }

    void OnRoomMenuButtonClicked() {

    }

    void OnPanoramaMenuButtonClicked() {

    }

    void OnReflectorMenuButtonClicked() {

    }

    void OnCameraMenuButtonClicked() {

    }

    void OnSimulatorMenuButtonClicked() {

    }

  };
};

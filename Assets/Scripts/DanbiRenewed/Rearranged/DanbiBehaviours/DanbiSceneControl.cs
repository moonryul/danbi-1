using System.Collections;
using System.Collections.Generic;


using UnityEngine.SceneManagement;
using UnityEngine;

namespace Danbi {
  public class DanbiSceneControl : MonoBehaviour {
    public static DanbiSceneControl Inst { get; internal set; }
    /// <summary>
    /// 
    /// </summary>
    public int CurrentSceneIndex { get; }
    /// <summary>
    /// 
    /// </summary>
    public string CurrentSceneName { get; }


    void Awake() {
      if (Inst.Null()) {
        DontDestroyOnLoad(this);
        Inst = this;
      }
    }

    bool IsValidScene(string sceneName) => string.Equals(CurrentSceneName, sceneName);

    bool IsValidScene(int sceneIndex) => (CurrentSceneIndex == sceneIndex);


  };
};

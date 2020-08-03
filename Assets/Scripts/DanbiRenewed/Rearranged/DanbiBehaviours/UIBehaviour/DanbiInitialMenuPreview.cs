using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiInitialMenuPreview : MonoBehaviour {
    [SerializeField]
    RenderTexture Preview;
    public RenderTexture preview { get => Preview; set => Preview = value; }    

    [SerializeField, Readonly]
    RawImage PreviewTarget;

    [SerializeField]
    Text NotSelected;

    void Start() {
      if (Preview.Null()) {
        Debug.LogError($"Preview RenderTexture isn't set", this);
      }

      PreviewTarget = GetComponent<RawImage>();
      PreviewTarget.texture = Preview;
    }

    public void StartRenderPreview() {
      NotSelected.gameObject.SetActive(false);

    }    
  };
};

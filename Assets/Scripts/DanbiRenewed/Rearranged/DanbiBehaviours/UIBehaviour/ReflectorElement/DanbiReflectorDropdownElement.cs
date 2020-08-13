using UnityEngine;
using System.Collections;

namespace Danbi {
  public class DanbiReflectorDropdownElement : MonoBehaviour {
    [SerializeField, Header("Prefab linked to the UI Element.")]
    protected GameObject PfUIElement;

    public GameObject InstantiatedUIElement { get; set; }

    [SerializeField]
    protected float Height;
    public float height => Height;

    [SerializeField]
    protected float Radius;
    public float radius => Radius;

    protected void Start() {
      InstantiatedUIElement = Instantiate<GameObject>(PfUIElement, transform, false);
    }

  };
};

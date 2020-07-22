using System;

using UnityEngine;
using UnityEngine.UI;

public class DanbiUIControl : MonoBehaviour {
  [Readonly, SerializeField, Header("Used for the result name.")]
  InputField InputField_SaveFile;

  public InputField saveFile { get => InputField_SaveFile; set => InputField_SaveFile = value; }


};

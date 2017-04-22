using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

[ExecuteInEditMode]
public class GDropDownOptions : MonoBehaviour,ISerializationCallbackReceiver
{
    public Dropdown dropdown;
    public string options {
        get {
            return string.Join("\n", (from opt in dropdown.options select opt.text).ToArray());
        }
        set {
            if (value == "") {
                dropdown.options = new List<Dropdown.OptionData>();
            } else {
                dropdown.options = new List<Dropdown.OptionData>(from t in value.Split('\n') select new Dropdown.OptionData(t));
            }
        }
    }

    public void OnAfterDeserialize()
    {
        if(dropdown == null) {
            dropdown = GetComponent<Dropdown>();
        }
    }

    public void OnBeforeSerialize()
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//用来在编辑时保持图片大小不变
[ExecuteInEditMode]
public class GEffectKeepImageNativeSize : MonoBehaviour
{
#if UNITY_EDITOR
    void Update () {
        Image image = GetComponent<Image>();
        if (image) {
            image.SetNativeSize();
        }
	}
#endif
}

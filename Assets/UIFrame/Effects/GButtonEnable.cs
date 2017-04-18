using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 启用和禁用Button按钮，包括禁止交互，禁止点击动画，按钮变成灰色
/// </summary>

[ExecuteInEditMode]
public class GButtonEnable : MonoBehaviour
{
    public Material disableMaterial;
    public Button buttonTarget;
    public GClickEffect clickEffect;
    public Image[] imageTargets;
    public Text[] textTargets;
    [Range(0, 1)]
    public float disableAphpa = 0.6f;

    public bool _enable;

    public bool enable {
        get {
            return buttonTarget ? buttonTarget.interactable : false;
        }
        set {
            if (buttonTarget) {
                buttonTarget.interactable = value;
                if(clickEffect)clickEffect.enabled = value;
                for (int i = 0; i < imageTargets.Length; i++) {
                    if (imageTargets[i]) {
                        imageTargets[i].material = value ? null : disableMaterial;
                        Color color = imageTargets[i].color;
                        color.a = value ? 1 : disableAphpa;
                        imageTargets[i].color = color;
                    }
                }
                for (int i = 0; i < textTargets.Length; i++) {
                    if (textTargets[i]) {
                        textTargets[i].material = value ? null : disableMaterial;
                        Color color = textTargets[i].color;
                        color.a = value ? 1 : disableAphpa;
                        textTargets[i].color = color;
                    }
                }
            }
        }
    }

    public void OnValidate()
    {
        enable = _enable;
    }
}

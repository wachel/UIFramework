using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class GProgressBar : MonoBehaviour
{
    public enum FillType
    {
        ImageFill,
        ImageScaleRight,
        ImageScaleLeft,
    }

    public FillType fillType;
    public Image target;
    public Image follow;
    public float followSpeed = 5;

    public float Value {
        get {
            return m_Value;
        }
        set {
            m_Value = value;
            OnValidate();
        }
    }
    [Range(0, 1)]
    public float m_Value;
    float m_OldValue;
    public void OnValidate()
    {
        SetImageSize(target,m_Value);
    }

    void SetImageSize(Image img,float val)
    {
        if (img) {
            RectTransform rectTrans = img.rectTransform as RectTransform;
            RectTransform parentRectTrans = img.transform.parent as RectTransform;
            if (fillType == FillType.ImageFill) {
                img.fillAmount = val;
            } else if (fillType == FillType.ImageScaleRight) {
                rectTrans.offsetMax = new Vector2(parentRectTrans.rect.width * (val - 1), rectTrans.offsetMax.y);
            } else if (fillType == FillType.ImageScaleLeft) {
                rectTrans.offsetMin = new Vector2(parentRectTrans.rect.width * (-val), rectTrans.offsetMin.y);
            }
        }
    }

    public void OnEnable()
    {
        m_OldValue = m_Value;
    }

    public void Update()
    {
        if (Application.isPlaying) {
            float step = Time.deltaTime * followSpeed;
            float diff = m_Value - m_OldValue;
            if (Mathf.Abs(diff) > step) {
                m_OldValue += Mathf.Sign(diff) * step;
            } else {
                m_OldValue = m_Value;
            }
        } else {
            m_OldValue = m_Value;
        }
        SetImageSize(follow, m_OldValue);
    }
}

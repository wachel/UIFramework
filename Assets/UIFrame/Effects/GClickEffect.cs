using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 按钮点击动画效果：包括放大效果，缩小效果
/// </summary>
public class GClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public enum EffectScaleType
    {
        Enlarge,
        Shrink,
    }

    public enum EffectCondition
    {
        MouseDown,
        MouseClick,
    }

    public RectTransform target;
    public EffectScaleType scaleType;
    public EffectCondition condition;

    float downDuration = 0.1f;
    float upDuration = 0.15f;

    float enlargeScale = 1.15f;
    float shrinkScale = 0.9f;

    public void OnEnable()
    {
        target.localScale = Vector3.one;
    }

    public IEnumerator DoEffect(float scale0,float scale1,float scale2,float duration01,float duration12)
    {
        if (target == null) {
            target = transform as RectTransform;
        }
        float time0 = Time.time;
        float time1 = time0 + duration01;
        float time2 = time1 + duration12;
        
        while (Time.time < time2) {
            if(Time.time < time1) {
                target.localScale = Vector3.one * Mathf.Lerp(scale0, scale1, (Time.time - time0) / duration01);
            } else {
                target.localScale = Vector3.one * Mathf.Lerp(scale1, scale2, (Time.time - time1) / duration12);
            }
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (condition == EffectCondition.MouseDown) {
            if (target == null) {
                target = transform as RectTransform;
            }
            target.localScale = Vector3.one * (scaleType == EffectScaleType.Enlarge?enlargeScale:shrinkScale);
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (condition == EffectCondition.MouseDown) {
            StartCoroutine(DoEffect(scaleType == EffectScaleType.Enlarge ? enlargeScale : shrinkScale,1,1,upDuration,0));
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(condition == EffectCondition.MouseClick) {
            StartCoroutine(DoEffect(1,scaleType == EffectScaleType.Enlarge ? enlargeScale : shrinkScale,1,downDuration,upDuration ));
        }
    }
}

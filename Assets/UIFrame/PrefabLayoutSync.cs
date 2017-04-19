using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 将这个节点下的Layout计算结果同步到另一个节点
/// 目的：因为编辑时的Preview节点跟实际编辑的节点不是同一个，所以用这个组件将实际layout结果同步到编辑节点
/// </summary>
[ExecuteInEditMode]
public class PrefabLayoutSync : MonoBehaviour
{
    public RectTransform shadow;
#if UNITY_EDITOR
    void Update () {
        //注意，shadow第一个子节点为shadow的Preview，是隐藏的，需要跳过去
        if (!Application.isPlaying) {
            while (transform.childCount > shadow.childCount - 1) {
                DestroyImmediate(transform.GetChild(shadow.childCount - 1).gameObject);
            }
            while (transform.childCount < shadow.childCount - 1) {
                GameObject obj = new GameObject("__element__");
                obj.AddComponent<RectTransform>();
                obj.transform.SetParent(transform, false);
            }

            for (int i = 0; i < shadow.childCount - 1; i++) {
                RectTransform child = transform.GetChild(i) as RectTransform;
                RectTransform shadowChild = shadow.GetChild(i + 1) as RectTransform;
                child.gameObject.SetActive(shadowChild.gameObject.activeSelf);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            for (int i = 0; i < shadow.childCount - 1; i++) {
                RectTransform child = transform.GetChild(i) as RectTransform;
                RectTransform shadowChild = shadow.GetChild(i + 1) as RectTransform;
                shadowChild.anchorMin = child.anchorMin;
                shadowChild.anchorMax = child.anchorMax;
                shadowChild.anchoredPosition = child.anchoredPosition;
                shadowChild.sizeDelta = child.sizeDelta;
            }
        }
    }
#endif
}

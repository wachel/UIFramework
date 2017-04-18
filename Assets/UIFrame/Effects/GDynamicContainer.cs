using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动态元素容器，如背包格子、邮件列表等，第一个元素是隐藏的
/// </summary>
public class GDynamicContainer : MonoBehaviour
{
    public int elementNumForTest { get; set; }
    //void OnEnable () {
    //    if (Application.isPlaying) {
    //        AddTestElement();
    //    }
    //}

    public void AddTestElements()
    {
        Transform[] childs = transform.GetComponentsInChildren<Transform>(true);
        if (childs.Length >= 2) {
            GameObject elementTemplate = childs[1].gameObject;
            for (int i = transform.childCount - 1; i < elementNumForTest; i++) {
                GameObject element = Instantiate<GameObject>(elementTemplate);
                element.transform.SetParent(transform, false);
                element.SetActive(true);
            }
        }
    }
}

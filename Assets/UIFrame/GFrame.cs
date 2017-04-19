using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 窗口的最外层节点。拖到此节点下的Widget会自动替换成PrefabInstance，启动时会遍历子节点，将PefabInstance恢复成Widget
/// 目的：
/// 1.起标记作用，让改节点的所有子节点知道需要从Widget转换到PrefabInstance
/// 2.编辑界面时预览
/// 3.游戏启动时调用DoInstantiate()实例化
/// 4.生成部分View代码
/// </summary>

public class GFrame : MonoBehaviour
{
    public static string SavePath { get { return "Assets/UI"; } }

    [System.NonSerialized]
    public bool isDebuging = true;

    public void OnEnable()
    {
        StartCoroutine(DebugLoad());
    }

    IEnumerator DebugLoad()
    {
        yield return null;
        if (isDebuging) {
            DoInstantiate(true);
        }
    }

    Dictionary<int, GameObject> targetRedirect = new Dictionary<int, GameObject>();
    List<GWidget> widgets = new List<GWidget>();
    List<GPrefabInstance> loaders = new List<GPrefabInstance>();

    void ReplaceNode(Transform node,bool debug)
    {
        for (int i = 0; i < node.transform.childCount; i++) {
            ReplaceNode(node.transform.GetChild(0),debug); //每次都处理第一个，因为处理过的会放到最后
        }
        GPrefabInstance loader = node.GetComponent<GPrefabInstance>();
        if (loader && loader.prefab) {
            //实例化
            GWidget widget = Instantiate<GWidget>(loader.prefab);
            targetRedirect[loader.gameObject.GetInstanceID()] = widget.gameObject;
            widgets.Add(widget);
            loaders.Add(loader);

            widget.name = loader.name;
            widget.transform.SetParent(node.parent, false);

            //设置大小和位置
            (widget.transform as RectTransform).anchorMin = (loader.transform as RectTransform).anchorMin;
            (widget.transform as RectTransform).anchorMax = (loader.transform as RectTransform).anchorMax;
            (widget.transform as RectTransform).anchoredPosition = (loader.transform as RectTransform).anchoredPosition;
            (widget.transform as RectTransform).sizeDelta = (loader.transform as RectTransform).sizeDelta;

            //更新属性
            loader.UpdatePreviewValues(widget);
            loader.BindTestEvent(widget);

            if (widget.containerPanel != null) {
                if(widget.isDynamicContainer && widget.containerPanel.transform.childCount == 1) {//如果是动态添加的容器（如邮件列表），并且有一个模板元素
                    GDynamicContainer dynamicContainer = widget.containerPanel.GetComponent<GDynamicContainer>();
                    if (dynamicContainer) {
                        loader.transform.GetChild(0).gameObject.SetActive(false);//第0个元素为模板，隐藏起来
                        if (debug) {
                            dynamicContainer.AddTestElements();//debug模式下显示一些内容元素
                        }
                    }
                } else {
                    while(loader.transform.childCount > 0) {
                        Transform child = loader.transform.GetChild(0);
                        child.SetParent(widget.containerPanel,false);//普通静态容器，将子节点放到容器节点下
                    }
                }
            } else {
                while (loader.transform.childCount > 0) {
                    loader.transform.GetChild(0).SetParent(widget.transform, false);//非容器，将子节点放到widget节点下
                }
            }

            widget.gameObject.SetActive(loader.gameObject.activeSelf);
            Destroy(widget);
            //将loader从源节点移除
            loader.transform.SetParent(null);
        } else {
            node.SetAsLastSibling();
        }
    }

    public void DoInstantiate(bool debug)
    {
        targetRedirect.Clear();
        widgets.Clear();
        loaders.Clear();

        for (int i = 0; i < transform.childCount; i++) {
            ReplaceNode(transform.GetChild(0),debug);//每次都处理第一个，因为处理过的会放到最后
        }

        //重设动作目标
        for (int i = 0; i < widgets.Count; i++) {
            for (int j = 0; j < widgets[i].eventInfos.Count; j++) {
                targetRedirect.TryGetValue(widgets[i].eventInfos[j].targetA.GetInstanceID(), out widgets[i].eventInfos[j].targetA);
                targetRedirect.TryGetValue(widgets[i].eventInfos[j].targetB.GetInstanceID(), out widgets[i].eventInfos[j].targetB);
            }
        }

        //删除Prefab节点
        foreach (var item in loaders) {
            Destroy(item.gameObject);
        }
    }
}

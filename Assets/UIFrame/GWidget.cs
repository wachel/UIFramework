using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有的Widget都需要挂此节点
/// 功能：
/// 1：导出能够允许编辑的变量名
/// 2：标记是否是固定大小，是否是容器，是否是动态容器等
/// </summary>

public class GWidget : MonoBehaviour
{
    public bool fixXSize;               //是否是固定大小的如icon，方便编辑
    public bool fixYSize;
    public RectTransform containerPanel;//容器Panel，如果是Layout等，需要设置此节点
    public bool isDynamicContainer;     //是否是动态容器，如背包列表，邮件列表等

    [HideInInspector]
    public string descption;            //描述

    [HideInInspector]
    public List<GExportPorpertyInfo> propertyInfos = new List<GExportPorpertyInfo>();   //要导出的属性，可以在编辑界面时设置此值

    [HideInInspector]
    public List<GExportEventInfo> eventInfos = new List<GExportEventInfo>();            //要导出的动作，可以在预览界面是进行简单交互

    [HideInInspector]
    public List<GExportToScriptInfo> exportToScriptInfos = new List<GExportToScriptInfo>();//要导出到程序的变量

}


[System.Serializable]
public class GExportPorpertyInfo
{
    public GameObject target;
    public string propertyName;
    public string rename;
}

[System.Serializable]
public class GExportToScriptInfo
{
    public GameObject target;
    public string type;
    public string rename;
}

[System.Serializable]
public class GExportEventInfo
{
    public GameObject sender;
    public string eventName;
    public string rename;

    public GameObject targetA;
    public GameObject targetB;
    public enum ActionType
    {
        无,
        隐藏,
        显示,
        隐藏A并显示B,
    }
    public ActionType actionType;
    public static int GetTargetNum(ActionType t)
    {
        int[] nums = { 0, 1, 1, 2, 1 };
        return nums[(int)t];
    }
    public void Execute()
    {
        switch (actionType) {
            case ActionType.无: break;
            case ActionType.隐藏: targetA.SetActive(false); break;
            case ActionType.显示: targetA.SetActive(true); targetA.transform.SetAsLastSibling(); break;
            case ActionType.隐藏A并显示B: targetA.SetActive(false); targetB.SetActive(true); targetB.transform.SetAsLastSibling(); break;
            default:
                break;
        }
    }
}
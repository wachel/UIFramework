using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用来记录一个Prefab，启动时Instance出来
/// 功能：
/// 1：编辑时，在场景中创建一个不能选中、不能编辑的Preview节点
/// 2：编辑时允许修改Prefab上的某些值，并保存在keyValues中，启动时设置到Instance出来的节点中
/// 3：如果prefab中有layout组件，则维持layout子节点显示正常
/// </summary>

[ExecuteInEditMode]
public class GPrefabInstance : MonoBehaviour, ISerializationCallbackReceiver
{
    public GWidget prefab;
    public GWidget preview;
    public List<PrefabKeyValue> keyValues = new List<PrefabKeyValue>();
    public List<GTestAction> actions = new List<GTestAction>();
    public List<Transform> children = new List<Transform>();//Unity有个bug，如果有不保存的子节点,并且用代码调整过siblingIndex，在序列化和反序列化后会导致siblingIndex错误，所以这里手动保存一份顺序列表
    public void OnEnable()
    {
        UpdateChildrenSiblingIndex();
        Instantiate();
    }

    public void OnBeforeSerialize()
    {
        children.Clear();
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).name != "__preview__") {
                children.Add(transform.GetChild(i));
            }
        }
    }

    public void OnAfterDeserialize() { }

    public void UpdateChildrenSiblingIndex()
    {
        if (transform.childCount == children.Count) {
            for (int i = 0; i < transform.childCount; i++) {
                children[i].SetSiblingIndex(i);
            }
        }
    }


#if UNITY_EDITOR
    public void Update()
    {
        //OnPostRender();
        //return;
        if (!Application.isPlaying) {
            if (prefab) {
                RectTransform childRect = prefab.transform as RectTransform;
                RectTransform rect = (transform as RectTransform);
                Vector2 sizeDelta = (transform as RectTransform).sizeDelta;
                if (prefab.fixXSize && childRect.anchorMin.x == childRect.anchorMax.x && rect.anchorMin.x == rect.anchorMax.x) {
                    sizeDelta.x = childRect.sizeDelta.x;
                }
                if (prefab.fixYSize && childRect.anchorMin.y == childRect.anchorMax.y && rect.anchorMin.y == rect.anchorMax.y) {
                    sizeDelta.y = childRect.sizeDelta.y;
                }
                (transform as RectTransform).sizeDelta = sizeDelta;
            }
        }
    }
#endif
    //编辑模式下绑定测试动作
    public void BindTestEvent(GWidget preview)
    {
        //return;
        for (int i = 0; i < preview.eventInfos.Count; i++) {
            string[] component_event = preview.eventInfos[i].eventName.Split('.');
            if (component_event.Length == 2) {
                GTestAction action = GetAction(preview.eventInfos[i].eventName, false);
                if (action != null) {
                    preview.eventInfos[i].actionType = action.actionType;
                    preview.eventInfos[i].targetA = action.targetA;
                    preview.eventInfos[i].targetB = action.targetB;
                    Component c = preview.eventInfos[i].sender.GetComponent(component_event[0]);
                    if (c is Button) {
                        if (component_event[1] == "onClick") {
                            (c as Button).onClick.AddListener(preview.eventInfos[i].Execute);
                        }
                    }
                }
            }
        }
    }

    //编辑模式的实例化
    public void Instantiate()
    {
        if (!Application.isPlaying) {
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (child.gameObject.name == "__preview__") {
                    DestroyImmediate(child.gameObject);
                }
            }
            if (prefab) {
                Transform[] trans = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++) {
                    trans[i] = transform.GetChild(i);
                }

                preview = GameObject.Instantiate<GWidget>(prefab, transform, false);

                preview.name = "__preview__";
                preview.gameObject.SetActive(true);
                
                preview.transform.SetAsFirstSibling();

                (preview.transform as RectTransform).anchoredPosition = Vector2.zero;
                (preview.transform as RectTransform).anchorMin = Vector2.zero;
                (preview.transform as RectTransform).anchorMax = Vector2.one;
                (preview.transform as RectTransform).sizeDelta = Vector2.zero;

                SetNodeFlag(preview.transform, HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.NotEditable);

                UpdatePreviewValues(preview);

                if (preview.containerPanel) {
                    preview.containerPanel.gameObject.AddComponent<PrefabLayoutSync>().shadow = transform as RectTransform;
                }

            }
        }
    }


    //将编辑过的属性值应用到控件上
    public void UpdatePreviewValues(GWidget widget)
    {
        //return;
        for (int i = 0; i < widget.propertyInfos.Count; i++) {
            string[] cn = widget.propertyInfos[i].propertyName.Split('.');
            object target = null;
            PropertyInfo pi = null;
            if (cn.Length == 2) {
                target = widget.propertyInfos[i].target.GetComponent(cn[0]);
                pi = target.GetType().GetProperty(cn[1]);
            } else if (cn.Length == 1) {
                target = widget.propertyInfos[i].target;
                pi = target.GetType().GetProperty(cn[0]);
            }
            if (pi != null) {
                string key = widget.propertyInfos[i].propertyName;
                if (widget.propertyInfos[i].target != widget.gameObject) {//如果属性的目标不是prefab根节点的话，要在key上携带目标节点的名称
                    key = widget.propertyInfos[i].target.name + "." + key;
                }
                object val = GetValue(key, pi.PropertyType, pi.GetValue(target, null));
                pi.SetValue(target, val, null);
            }
        }
    }

    //设置所有子节点的flag
    public static void SetNodeFlag(Transform node, HideFlags flags)
    {
        Transform[] trans = node.GetComponentsInChildren<Transform>();
        foreach (var t in trans) {
            t.gameObject.hideFlags = flags;
        }
    }

    public GTestAction GetAction(string name, bool autoAddIfNotFound)
    {
        for (int i = 0; i < actions.Count; i++) {
            if (actions[i].name == name) {
                return actions[i];
            }
        }
        if (autoAddIfNotFound) {
            GTestAction action = new GTestAction();
            action.name = name;
            actions.Add(action);
            return action;
        }
        return null;
    }

    public object GetValue(string key, System.Type type, object defaultValue)
    {
        for (int i = 0; i < keyValues.Count; i++) {
            if (keyValues[i].key == key) {
                object val = keyValues[i].Get(type);
                if (type.IsEnum) {
                    if (val != null) {
                        return val;
                    }
                } else {
                    return val;
                }
            }
        }
        SetValue(key, defaultValue);
        return defaultValue;
    }
    public void SetValue(string key, object value)
    {
        for (int i = 0; i < keyValues.Count; i++) {
            if (keyValues[i].key == key) {
                keyValues[i].Set(value);
                return;
            }
        }
        PrefabKeyValue kv = new PrefabKeyValue();
        kv.key = key;
        kv.Set(value);
        keyValues.Add(kv);
    }
}


[System.Serializable]
public class PrefabKeyValue
{
    public string key;

    public string stringValue;
    public bool boolValue;
    public int intValue;
    public float floatValue;
    public Sprite spriteValue;
    public GameObject objectValue;
    public Vector4 vectorValue;

    public void Set(object val)
    {
        if (val != null) {
            var type = val.GetType();
            if (type == typeof(string)) {
                stringValue = (string)val;
            } else if (type == typeof(int)) {
                intValue = (int)val;
            } else if (type == typeof(bool)) {
                boolValue = (bool)val;
            } else if (type == typeof(float)) {
                floatValue = (float)val;
            } else if (type == typeof(Sprite)) {
                spriteValue = (Sprite)val;
            } else if (type.IsEnum) {
                intValue = System.Convert.ToInt32(val);
            } else if (type == typeof(GameObject)) {
                objectValue = (GameObject)val;
            } else if (type == typeof(Vector2)) {
                vectorValue = (Vector2)val;
            } else if (type == typeof(Vector3)) {
                vectorValue = (Vector3)val;
            } else if (type == typeof(Color)) {
                Color c = (Color)val;
                vectorValue = new Vector4(c.r, c.g, c.b, c.a);
            }
        } else {
            stringValue = "";
        }
    }

    public object Get(System.Type type)
    {
        if (type == typeof(string)) {
            return stringValue;
        } else if (type == typeof(int)) {
            return intValue;
        } else if (type == typeof(bool)) {
            return boolValue;
        } else if (type == typeof(float)) {
            return floatValue;
        } else if (type == typeof(Sprite)) {
            return spriteValue;
        } else if (type.IsEnum) {
            return System.Enum.ToObject(type, intValue);
        } else if (type == typeof(GameObject)) {
            return objectValue;
        } else if (type == typeof(Vector2)) {
            return new Vector2(vectorValue.x, vectorValue.y);
        } else if (type == typeof(Vector3)) {
            return (Vector3)vectorValue;
        } else if (type == typeof(Color)) {
            return new Color(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
        }
        return null;
    }
}

[System.Serializable]
public class GTestAction
{
    public string name;
    public GameObject targetA;
    public GameObject targetB;
    public GExportEventInfo.ActionType actionType;
}
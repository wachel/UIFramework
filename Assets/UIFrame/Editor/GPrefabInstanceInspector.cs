using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UI;

[CanEditMultipleObjects]
[CustomEditor(typeof(GPrefabInstance))]
public class GPrefabInstanceInspector : Editor
{
    GPrefabInstance firstTarget;
    GPrefabInstance[] allTargets; 

    public void OnEnable()
    {
        firstTarget = serializedObject.targetObjects[0] as GPrefabInstance;
        allTargets = new GPrefabInstance[serializedObject.targetObjects.Length];
        for (int i = 0; i < allTargets.Length; i++) {
            allTargets[i] = serializedObject.targetObjects[i] as GPrefabInstance;
        }
    }

    static bool hasMultipleDifferentValues(GPrefabInstance[] targets,string key,System.Type type,object defaultValue)
    {
        object firstValue = targets[0].GetValue(key, type, defaultValue);
        for (int i = 0; i < targets.Length; i++) {
            object val = targets[i].GetValue(key, type, defaultValue);
            if(firstValue == null) {
                return val == null;
            }
            else if (!firstValue.Equals(val)) {
                return true;
            }
        }
        return false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //检查能否同时编辑
        bool canMultipleEdit = true;
        if (serializedObject.isEditingMultipleObjects) {
            for(int i = 1; i< allTargets.Length; i++) {
                if(allTargets[i].prefab != firstTarget.prefab) {//只有全部是相同的prefab才能同时编辑
                    canMultipleEdit = false;
                }
            }
        }
        if (!canMultipleEdit) {
            GUILayout.Label("只能同时编辑相同prefab的项");
            return;
        }

        //Prefab替换功能
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        GWidget newPrefab = (GWidget)EditorGUILayout.ObjectField("prefab", firstTarget.prefab, typeof(GWidget), false);
        bool reload = GUILayout.Button("Reload", GUILayout.Width(60));
        if (EditorGUI.EndChangeCheck() || reload) {
            for (int i = 0; i < allTargets.Length; i++) {
                if (allTargets[i].preview) {
                    GameObject.DestroyImmediate(allTargets[i].preview.gameObject);
                    allTargets[i].preview = null;
                }
                allTargets[i].prefab = newPrefab;
                allTargets[i].Instantiate();
            }
        }

        if (GUILayout.Button("Edit", GUILayout.Width(50))){
            GWidgetInspector.StartEditWidget(firstTarget.prefab);
        }

        EditorGUILayout.EndHorizontal();
        //base.OnInspectorGUI();
        if (firstTarget.gameObject.scene.name != null) {
            GWidget prefab = firstTarget.prefab;
            //属性设置
            bool bChangeAnyOne = false;
            for(int i = 0;i<prefab.propertyInfos.Count; i++) {
                string propertyName = prefab.propertyInfos[i].propertyName;
                string key = propertyName;
                if(prefab.propertyInfos[i].target != prefab.gameObject) {//如果属性的目标不是prefab根节点的话，要在key上携带目标节点的名称
                    key = prefab.propertyInfos[i].target.name + "." + key;
                }
                PropertyInfo pi = null;
                string[] cn = propertyName.Split('.');
                string label = prefab.propertyInfos[i].rename;
                object defaultValue = null;
                if (cn.Length == 2) {
                    if (label == "") {
                        label = propertyName;
                    }
                    MonoBehaviour mb = (MonoBehaviour)prefab.propertyInfos[i].target.GetComponent(cn[0]);
                    pi = mb.GetType().GetProperty(cn[1]);
                    defaultValue = pi.GetValue(mb, null);
                } else if(cn.Length == 1){
                    pi = prefab.propertyInfos[i].target.GetType().GetProperty(cn[0]);
                    defaultValue = pi.GetValue(prefab.propertyInfos[i].target, null);
                }
                if (pi != null) {
                    if (hasMultipleDifferentValues(allTargets, key, pi.PropertyType, defaultValue)) {
                        EditorGUI.showMixedValue = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    object oldValue = firstTarget.GetValue(key, pi.PropertyType, defaultValue);
                    object newValue = null;
                    if (pi.PropertyType == typeof(bool)) {
                        newValue = EditorGUILayout.Toggle(label, (bool)oldValue);
                    } else if (pi.PropertyType == typeof(string)) {
                        newValue = EditorGUILayout.TextField(label, (string)oldValue);
                    } else if (pi.PropertyType == typeof(Sprite)) {
                        newValue = EditorGUILayout.ObjectField(label, (Object)oldValue, typeof(Sprite), false);
                    } else if (pi.PropertyType.IsEnum) {
                        newValue = EditorGUILayout.EnumPopup(label, (System.Enum)oldValue);
                    } else if (pi.PropertyType == typeof(GameObject)) {
                        newValue = EditorGUILayout.ObjectField(label, (Object)oldValue, typeof(GameObject), false);
                    } else if (pi.PropertyType == typeof(int)) {
                        newValue = EditorGUILayout.IntField(label, (int)oldValue);
                    } else if (pi.PropertyType == typeof(float)) {
                        newValue = EditorGUILayout.FloatField(label, (float)oldValue);
                    }else if(pi.PropertyType == typeof(Vector2)) {
                        newValue = EditorGUILayout.Vector2Field(label, (Vector2)oldValue);
                    } else if (pi.PropertyType == typeof(Vector3)) {
                        newValue = EditorGUILayout.Vector3Field(label, (Vector3)oldValue);
                    }
                    EditorGUI.showMixedValue = false;
                    if (EditorGUI.EndChangeCheck()) {
                        bChangeAnyOne = true;
                        for (int t = 0; t < allTargets.Length; t++) {
                            allTargets[t].SetValue(key, newValue);
                        }
                    }
                }
            }

            if (bChangeAnyOne) {
                for (int t = 0; t < allTargets.Length; t++) {
                    allTargets[t].UpdatePreviewValues(allTargets[t].preview);
                    EditorUtility.SetDirty(allTargets[t]);
                }
            }

            if (!serializedObject.isEditingMultipleObjects) {
                //事件绑定
                for (int i = 0; i < prefab.eventInfos.Count; i++) {
                    string[] cn = prefab.eventInfos[i].eventName.Split('.');
                    if (cn.Length == 2) {
                        string label = prefab.eventInfos[i].rename;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(label);
                        GTestAction action = firstTarget.GetAction(prefab.eventInfos[i].eventName, true);
                        if (action != null) {
                            action.actionType = (GExportEventInfo.ActionType)EditorGUILayout.EnumPopup(action.actionType);
                            if (GExportEventInfo.GetTargetNum(action.actionType) == 1) {
                                GUILayout.Label("target:");
                                action.targetA = (GameObject)EditorGUILayout.ObjectField(action.targetA, typeof(GameObject), true);
                            } else if (GExportEventInfo.GetTargetNum(action.actionType) == 2) {
                                GUILayout.Label("A:");
                                action.targetA = (GameObject)EditorGUILayout.ObjectField(action.targetA, typeof(GameObject), true);
                                GUILayout.Label("B:");
                                action.targetB = (GameObject)EditorGUILayout.ObjectField(action.targetB, typeof(GameObject), true);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        if (!serializedObject.isEditingMultipleObjects) {
            EditorGUILayout.Space();
        
            EditorGUILayout.TextArea(GetScriptString2(firstTarget));
        }
    }

    static string GetScriptString(GPrefabInstance firstTarget)
    {
        string script = "";
        GFrame frame = firstTarget.GetComponentInParent<GFrame>();
        if (frame) {
            //script += "findPath = \"" + GUtility.GetPath(frame.transform, firstTarget.transform) + "\";\n";
            script += "trans = frame.Find(\"" + GUtility.GetPath(frame.transform, firstTarget.transform) + "\");\n";
        }
        GWidget prefab = firstTarget.prefab;
        for (int i = 0; i < prefab.exportToScriptInfos.Count; i++) {
            GExportToScriptInfo property = prefab.exportToScriptInfos[i];
            string path = GUtility.GetPath(firstTarget.transform, property.target.transform);
            if (path.StartsWith("/")) {
                path = path.Substring(1);
            }
            script += "trans.Find(\"" + path + "\").";
            script += "GetComponent <" + property.type + "> ();";
            script += "//" + property.rename + "\n";
        }
        return script;
    }

    static string GetScriptString2(GPrefabInstance firstTarget)
    {
        string script = "";
        GFrame frame = firstTarget.GetComponentInParent<GFrame>();
        if (frame) {
            script += "相对于Frame的路径:\n";
            script += "    \"" + GUtility.GetPath(frame.transform, firstTarget.transform) + "\"\t//" + GetTypes(firstTarget.prefab.transform) + "\n";
        }
        script += "\n";
        GWidget prefab = firstTarget.prefab;
        script += "内部结构:\n";
        script += GetTreeString(prefab.transform,"    ");
        return script;
    }

    static string GetTreeString(Transform trans,string prefix)
    {
        string result = "";
        for(int i = 0; i<trans.childCount; i++) {
            result += prefix + "\"" +trans.GetChild(i).name + "\"\t//" + GetTypes(trans) + "\n";
            result += GetTreeString(trans.GetChild(i),prefix + "    ");
        }
        return result;
    }

    static string GetTypes(Transform trans)
    {
        string result = "";
        Component[] components = trans.GetComponents<Component>();
        for(int i = 0; i < components.Length; i++) {
            if(!(components[i] is Transform) && !(components[i] is RectTransform) && !(components[i] is CanvasRenderer) && !(components[i] is GWidget) && !(components[i] is GPrefabInstance)) {
                result += "<" + components[i].GetType().Name + ">";
            }
        }
        return result;
    }

}

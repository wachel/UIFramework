using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(GWidget))]
public class GWidgetInspector : Editor
{
    ReorderableList propertyList;   //要导出的可编辑的属性列表
    ReorderableList eventList;      //要导出的事件列表
    ReorderableList exportToScriptList;//要导出到脚本的变量列表
    GWidget widget;

    public void OnEnable()
    {
        widget = target as GWidget;
        if (!Application.isPlaying) {
            if (widget.GetComponentInParent<GFrame>()) {
                if (widget.name != "__preview__") {
                    Object parentObject = PrefabUtility.GetPrefabParent(widget);
                    if (parentObject) {
                        string path = AssetDatabase.GetAssetPath(parentObject);
                        GWidget prefab = AssetDatabase.LoadAssetAtPath<GWidget>(path);
                        if (prefab) {
                            //add prefab instance
                            GameObject obj = new GameObject(widget.name);
                            if(widget.defaultName != "") {
                                obj.name = widget.defaultName;
                            }
                            obj.AddComponent<RectTransform>();
                            GPrefabInstance gpi = obj.AddComponent<GPrefabInstance>();
                            gpi.prefab = prefab;
                            obj.transform.SetParent(widget.transform.parent, false);
                            obj.transform.SetSiblingIndex(widget.transform.GetSiblingIndex());

                            gpi.Instantiate();

                            GameObject.DestroyImmediate(widget.gameObject);
                        }
                    }
                }
            }
        }
        propertyList = InitExportPropertyList();
        eventList = InitExportEventList();
        exportToScriptList = InitExportToScriptList();
    }


    //显示属性列表
    ReorderableList InitExportPropertyList()
    {
        ReorderableList list = new ReorderableList(widget.propertyInfos, typeof(GExportPorpertyInfo), true, true, true, true);
        list.drawHeaderCallback = (Rect rect) => {
            GUI.BeginGroup(rect);
            GUILayout.BeginArea(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 20));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            EditorGUILayout.LabelField("object", GUILayout.Width(80));
            EditorGUILayout.LabelField("property",GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("rename", GUILayout.Width(120));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.EndGroup();
        };

        list.drawElementCallback = (rect, index, active, focused) => {
            GUI.BeginGroup(rect);
            GUILayout.BeginArea(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 20));
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            widget.propertyInfos[index].target = (GameObject)EditorGUILayout.ObjectField(widget.propertyInfos[index].target, typeof(GameObject), true, GUILayout.Width(80));
            if (widget.propertyInfos[index].target) {
                List<string> names = new List<string>();
                int selected = 0;
                names.Add("active");
                if (widget.propertyInfos[index].target.GetComponent<Text>()) {
                    names.Add("Text.text");
                    names.Add("Text.color");
                    names.Add("Text.fontSize");
                }
                if (widget.propertyInfos[index].target.GetComponent<Image>()) {
                    names.Add("Image.sprite");
                    names.Add("Image.color");
                    names.Add("Image.enabled");
                }
                if (widget.propertyInfos[index].target.GetComponent<GDynamicContainer>()) {
                    names.Add("GDynamicContainer.elementNumForTest");
                }
                if (widget.propertyInfos[index].target.GetComponent<GGridLayout>()) {
                    names.Add("GGridLayout.cellWidth");
                    names.Add("GGridLayout.cellHeight");
                }
                if (widget.propertyInfos[index].target.GetComponent<GButtonEnable>()) {
                    names.Add("GButtonEnable.enable");
                }
                if (widget.propertyInfos[index].target.GetComponent<GridLayoutGroup>()) {
                    names.Add("GridLayoutGroup.cellSize");
                    names.Add("GridLayoutGroup.spacing");
                    names.Add("GridLayoutGroup.startCorner");
                    names.Add("GridLayoutGroup.childAlignment");
                    names.Add("GridLayoutGroup.startAxis");
                }
                if (widget.propertyInfos[index].target.GetComponent<GSpriteID>()) {
                    names.Add("GSpriteID.id");
                }
                for (int i = 0; i < names.Count; i++) {
                    if (names[i] == widget.propertyInfos[index].propertyName) {
                        selected = i;
                    }
                }
                if (names.Count > 0) {
                    widget.propertyInfos[index].propertyName = names[EditorGUILayout.Popup(selected, names.ToArray())];
                    widget.propertyInfos[index].rename = EditorGUILayout.TextField(widget.propertyInfos[index].rename, GUILayout.Width(120));
                }
                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(target);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.EndGroup();
        };
        return list;
    }

    //显示动作列表
    ReorderableList InitExportEventList()
    {
        ReorderableList list = new ReorderableList(widget.eventInfos, typeof(GExportEventInfo), true, true, true, true);
        list.drawHeaderCallback = (Rect rect) => {
            GUI.BeginGroup(rect);
            GUILayout.BeginArea(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 20));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            EditorGUILayout.LabelField("object", GUILayout.Width(100));
            EditorGUILayout.LabelField("event", GUILayout.Width(100));
            EditorGUILayout.LabelField("rename");
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.EndGroup();
        };

        list.drawElementCallback = (rect, index, active, focused) => {
            GUI.BeginGroup(rect);
            GUILayout.BeginArea(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 20));
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            widget.eventInfos[index].sender = (GameObject)EditorGUILayout.ObjectField(widget.eventInfos[index].sender, typeof(GameObject), true, GUILayout.Width(100));
            if (widget.eventInfos[index].sender) {
                List<string> names = new List<string>();
                int selected = 0;
                if (widget.eventInfos[index].sender.GetComponent<Button>()) {
                    names.Add("Button.onClick");
                }
                for (int i = 0; i < names.Count; i++) {
                    if (names[i] == widget.eventInfos[index].eventName) {
                        selected = i;
                    }
                }
                if (names.Count > 0) {
                    selected = EditorGUILayout.Popup(selected, names.ToArray(), GUILayout.Width(100));
                    widget.eventInfos[index].eventName = names[selected];
                    widget.eventInfos[index].rename = EditorGUILayout.TextField(widget.eventInfos[index].rename);
                }
                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(target);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.EndGroup();
        };
        return list;
    }

    public ReorderableList InitExportToScriptList()
    {
        ReorderableList list = new ReorderableList(widget.exportToScriptInfos, typeof(GExportToScriptInfo), true, true, true, true);
        list.drawHeaderCallback = (Rect rect) => {
            GUI.BeginGroup(rect);
            GUILayout.BeginArea(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 20));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(20));
            EditorGUILayout.LabelField("object", GUILayout.Width(80));
            EditorGUILayout.LabelField("type", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("rename", GUILayout.Width(120));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.EndGroup();
        };

        list.drawElementCallback = (rect, index, active, focused) => {
            GUI.BeginGroup(rect);
            GUILayout.BeginArea(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 20));
            GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            widget.exportToScriptInfos[index].target = (GameObject)EditorGUILayout.ObjectField(widget.exportToScriptInfos[index].target, typeof(GameObject), true, GUILayout.Width(80));
            if (widget.exportToScriptInfos[index].target) {
                List<string> types = new List<string>();
                Component[] components = widget.exportToScriptInfos[index].target.GetComponents<Component>();
                for(int i = 0; i<components.Length; i++) {
                    types.Add(components[i].GetType().Name);
                }
                int selected = 0;
                for (int i = 0; i < types.Count; i++) {
                    if (types[i] == widget.exportToScriptInfos[index].type) {
                        selected = i;
                    }
                }
                if (types.Count > 0) {
                    widget.exportToScriptInfos[index].type = types[EditorGUILayout.Popup(selected, types.ToArray())];
                }
                widget.exportToScriptInfos[index].rename = EditorGUILayout.TextField(widget.exportToScriptInfos[index].rename, GUILayout.Width(120));
                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(target);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.EndGroup();
        };
        return list;
    }

    public static void StartEditWidget(GWidget widget)
    {
        EditorSceneManager.UnloadSceneAsync(EditorSceneManager.GetActiveScene());
        EditorSceneManager.NewSceneCreatedCallback fun = (UnityEngine.SceneManagement.Scene scene, NewSceneSetup setup, NewSceneMode mode) => {
            EditorSceneManager.SetActiveScene(scene);
            //创建canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            new GameObject("EventSystem").AddComponent<UnityEngine.EventSystems.EventSystem>();

            //遮挡背景
            //GameObject bkgObj = new GameObject("background");
            //bkgObj.transform.SetParent(canvas.transform,false);
            //Image image = bkgObj.AddComponent<Image>();
            //image.rectTransform.sizeDelta = Vector2.zero;
            //image.rectTransform.anchorMin = Vector2.zero;
            //image.rectTransform.anchorMax = Vector2.one;
            //image.rectTransform.anchoredPosition = Vector2.zero;
            //image.color = Color.black;

            //将要编辑的组件放进来
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(widget.gameObject, scene);
            obj.transform.SetParent(canvasObj.transform, false);
            obj.SetActive(true);
            Selection.activeObject = obj;
        };
        EditorSceneManager.newSceneCreated += fun;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.newSceneCreated -= fun;
    }

    public override void OnInspectorGUI()
    {
        if (widget.gameObject.scene.name == null) {
            if (GUILayout.Button("编辑组件")) {
                StartEditWidget(widget);
            }
        }
        else if(widget.gameObject.scene.name == ""){
            GUILayout.BeginHorizontal();
            bool bClose = false;
            if (GUILayout.Button("完成编辑")) {
                Object parentObject = PrefabUtility.GetPrefabParent(widget);
                PrefabUtility.ReplacePrefab(widget.gameObject , parentObject, ReplacePrefabOptions.ConnectToPrefab);//apply
                bClose = true;
            }
            if (GUILayout.Button("放弃修改")) {
                bClose = true;
            }
            GUILayout.EndHorizontal();

            if (bClose) {
                EditorSceneManager.CloseScene(widget.gameObject.scene, true);
                int sceneCount = EditorSceneManager.sceneCount;
                if(sceneCount >= 1) {
                    //EditorSceneManager.GetSceneAt(0).
                }
                return;
            }
        }

        base.OnInspectorGUI();

        GUILayout.Label("Export Property List");
        propertyList.DoLayoutList();
        GUILayout.Label("Export Event List");
        eventList.DoLayoutList();
        GUILayout.Label("Export To Script List");
        exportToScriptList.DoLayoutList();
    }

    
}

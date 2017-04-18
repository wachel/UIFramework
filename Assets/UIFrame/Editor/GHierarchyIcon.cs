using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 用来在Hierarchy窗口元素上画一个小绿点
/// </summary>
[InitializeOnLoad]
class GHierarchyIcon
{
    static Texture2D texture;
    static List<int> markedObjects;
    static GUIStyle style = new GUIStyle();

    static GHierarchyIcon()
    {
        style.normal.textColor = new Color(0.2f,0.2f,0.8f);
        // Init
        //texture = AssetDatabase.LoadAssetAtPath("Assets/Textures/greenpoint.png", typeof(Texture2D)) as Texture2D;
        texture = CreateTexture(16,16);
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }
     
    static void HierarchyItemCB(int instanceID, Rect selectionRect)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj) {
            GameObject go = obj as GameObject;
            if (go.GetComponent<GPrefabInstance>()) {
                Rect r = new Rect(selectionRect);
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(go.name));
                r.x += textSize.x;
                r.y += 4;
                r.width = 12;
                GUI.Label(r, texture);
            }
        }
    }

    static Texture2D CreateTexture(int w,int h)
    {
        Texture2D temp = new Texture2D(16, 16);
        Color[] colors = new Color[16 * 16];
        for(int i = 0; i < w; i++) {
            for(int j = 0; j < h; j++) {
                Color color = new Color(0.41f,0.73f,0.11f,1f);
                float dx = (i - w / 2);
                float dy = (j - h / 2);
                float df = 1 - Mathf.Sqrt(dx * dx + dy * dy) / (w/2);
                df = Mathf.Pow(Mathf.Clamp01(df) + 0.8f,17);
                color.a = Mathf.Clamp01(df);
                colors[j * 16 + i] = color;
            }
        }
        temp.SetPixels(colors);
        temp.Apply();
        return temp;
    }

}
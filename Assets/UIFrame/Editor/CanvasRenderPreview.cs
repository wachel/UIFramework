using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(CanvasRenderer))]
public class CanvasRenderPreview : Editor
{
    PreviewRenderUtility _previewRenderUtility;
    List<UIMesh> meshes = new List<UIMesh>();
    Vector2 _drag;

    CanvasRenderer renderer;
    Canvas canvas;

    public void OnEnable()
    {
        renderer = target as CanvasRenderer;
        if (renderer.gameObject.scene.name == null && renderer.transform.parent == null) {
            GameObject canvasObj = new GameObject("__canvas_for_preview__");
            canvasObj.hideFlags = HideFlags.DontSave;
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject rootObj = new GameObject("root");
            RectTransform root = rootObj.AddComponent<RectTransform>();
            root.anchorMin = Vector2.one * 0.5f;
            root.anchorMax = Vector2.one * 0.5f;
            root.sizeDelta = new Vector2(500, 400);

            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(renderer.gameObject);
            Vector2 canvasSize = (canvas.transform as RectTransform).rect.size;

            root.transform.SetParent(canvas.transform,false);
            obj.transform.SetParent(root.transform, false);
            obj.SetActive(true);

            meshes = UIToMeshConverter.CreateMeshList(obj.transform as RectTransform);

            DestroyImmediate(canvas.gameObject);
        }
    }


    private void ValidateData()
    {
        if (_previewRenderUtility == null) {
            _previewRenderUtility = new PreviewRenderUtility();

            _previewRenderUtility.m_Camera.transform.position = new Vector3(0, 0, -6);
            _previewRenderUtility.m_Camera.transform.rotation = Quaternion.identity;
        }
    }

    public override bool HasPreviewGUI()
    {
        if (renderer.gameObject.scene.name == null && renderer.transform.parent == null) {
            ValidateData();
            return true;
        }
        return false;
    }

    Rect ExpandRect(Rect a, Rect b)
    {
        return Rect.MinMaxRect(
            Mathf.Min(a.xMin, b.xMin),
            Mathf.Min(a.yMin, b.yMin),
            Mathf.Max(a.xMax, b.xMax),
            Mathf.Max(a.yMax, b.yMax)
        );
    }


    //画预览图
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        bool isPrefab = renderer.gameObject.scene.name == null;
        if (isPrefab) {
            _drag = Drag2D(_drag, r);

            if (Event.current.type == EventType.Repaint) {
                _previewRenderUtility.BeginPreview(r, background);

                Rect rect = new Rect();
                if (meshes.Count > 0) {
                    rect = meshes[0].rect;
                }
                for (int i = 1; i < meshes.Count; i++) {
                    rect = ExpandRect(rect, meshes[i].rect);
                }
                for (int i = 0; i < meshes.Count; i++) {
                    if (meshes[i].graphic is Text) {
                        (meshes[i].graphic as Text).FontTextureChanged();
                    }
                    meshes[i].material.mainTexture = meshes[i].graphic.mainTexture;
                    meshes[i].material.color = meshes[i].graphic.color;
                    meshes[i].material.SetVector("_TextureSampleAdd", (meshes[i].graphic is Text) ? new Color(1, 1, 1, 0) : new Color(0, 0, 0, 0));
                    _previewRenderUtility.DrawMesh(meshes[i].mesh, meshes[i].matrix, meshes[i].material, 0);
                    _previewRenderUtility.m_Camera.transform.position = new Vector3(rect.center.x + _drag.x, rect.center.y - _drag.y, 0) + _previewRenderUtility.m_Camera.transform.forward * -60f;
                    _previewRenderUtility.m_Camera.orthographic = true;
                    _previewRenderUtility.m_Camera.orthographicSize = Mathf.Max(rect.width, rect.height);
                    _previewRenderUtility.m_Camera.nearClipPlane = 0.1f;
                    _previewRenderUtility.m_Camera.farClipPlane = 100;
                    _previewRenderUtility.m_Camera.Render();

                }
                Texture resultRender = _previewRenderUtility.EndPreview();
                GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
            }
        }
    }

    public override void OnPreviewSettings()
    {
        if (GUILayout.Button("Reset Camera", EditorStyles.whiteMiniLabel))
            _drag = Vector2.zero;
    }

    void OnDestroy()
    {
        if (_previewRenderUtility != null) {
            _previewRenderUtility.Cleanup();
        }
    }

    public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
    {
        int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
        Event current = Event.current;
        switch (current.GetTypeForControl(controlID)) {
            case EventType.MouseDown:
                if (position.Contains(current.mousePosition) && position.width > 50f) {
                    GUIUtility.hotControl = controlID;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID) {
                    GUIUtility.hotControl = 0;
                }
                EditorGUIUtility.SetWantsMouseJumping(0);
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID) {
                    scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                    scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                    current.Use();
                    GUI.changed = true;
                }
                break;
        }
        return scrollPosition;
    }
}

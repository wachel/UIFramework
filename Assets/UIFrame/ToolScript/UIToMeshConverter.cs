using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class UIMesh
{
    public UIMesh(Mesh mesh, Graphic graphic, Material mat, Matrix4x4 matrix, Rect rect)
    {
        this.material = mat; this.graphic = graphic; this.mesh = mesh; this.matrix = matrix; this.rect = rect;
    }
    public Material material;
    public Mesh mesh;
    public Matrix4x4 matrix;
    public Rect rect;
    public Graphic graphic;
}

public class UIToMeshConverter
{
    static readonly VertexHelper s_VertexHelper = new VertexHelper();
    static Dictionary<string, MethodInfo> mi_OnPopulateMesh = new Dictionary<string, MethodInfo>();

    static void OnPopulateMesh(Graphic grap, VertexHelper vh)
    {
        if(!mi_OnPopulateMesh.ContainsKey(grap.GetType().Name)) {
            mi_OnPopulateMesh[grap.GetType().Name] = grap.GetType().GetMethod("OnPopulateMesh", BindingFlags.NonPublic | BindingFlags.Instance, System.Type.DefaultBinder, new[] { typeof(VertexHelper) }, null);
        }
        mi_OnPopulateMesh[grap.GetType().Name].Invoke(grap, new object[] { vh });
    }

    static Mesh CreateMesh(Graphic image)
    {
        Mesh workerMesh = new Mesh();
        RectTransform rectTransform = image.transform as RectTransform;
        if (rectTransform != null && rectTransform.rect.width >= 0f && rectTransform.rect.height >= 0f) {
            OnPopulateMesh(image, s_VertexHelper);
        } else {
            s_VertexHelper.Clear();
        }
        List<Component> list = new List<Component>();
        image.gameObject.GetComponents(typeof(IMeshModifier), list);
        for (int i = 0; i < list.Count; i++) {
            ((IMeshModifier)list[i]).ModifyMesh(s_VertexHelper);
        }
        s_VertexHelper.FillMesh(workerMesh);
        workerMesh.RecalculateBounds();
        return workerMesh;
    }

    static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

    public static List<UIMesh> CreateMeshList(RectTransform transform)
    {
        List<UIMesh> result = new List<UIMesh>();
        Graphic[] graphics = transform.GetComponentsInChildren<Graphic>();
        for(int i = 0; i < graphics.Length; i++) {
            if (graphics[i].enabled) {

                Mesh mesh = CreateMesh(graphics[i]);
                Material mat = new Material(Shader.Find("Hidden/UI_Preview"));
                result.Add(new UIMesh(mesh,graphics[i], mat, graphics[i].transform.localToWorldMatrix, RectTransformToScreenSpace(graphics[i].rectTransform)));
            }
        }
        return result;
    }
}



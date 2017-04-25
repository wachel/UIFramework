using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 供在UI中预览3D角色
/// </summary>
[ExecuteInEditMode]
public class GAvatarRenderer : MonoBehaviour
{
    public GameObject scenePrefab;      //3D背景，包括光源
    public GameObject avatarPrefab;     //角色
    public Animator animator;           //角色上的动画
    public string enterStateOnEnable;   //开始时要播放的动画

    public float cameraDepthForOrder = 2;
    public float fov = 45;
    public string layerName = "2DPreview";
    public float pitch = -20;
    public float yaw = 0;
    public bool autoDistance = false;
    public float distance = 3.0f;
    public float distanceScale = 1.0f;

    GameObject sceneObj;
    GameObject previewObj;
    Camera previewCamera;
    RectTransform rect;
    public Camera uiWorldSpaceCamera { set; get; }
    Bounds avatarBounds;


    public void OnEnable()
    {
        rect = transform as RectTransform;
        int layer = LayerMask.NameToLayer("2DPreview");

        if (scenePrefab) {
            sceneObj = Instantiate<GameObject>(scenePrefab);
            sceneObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
            SetLayer(sceneObj.transform, layer);
            Light[] lights = sceneObj.transform.GetComponentsInChildren<Light>();
            foreach(var l in lights) {
                l.cullingMask = 1 << layer;
            }
        }

        PostPrefabChange();

        GameObject camObj = new GameObject("2d_preview_camera");
        camObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
        previewCamera = camObj.AddComponent<Camera>();
        previewCamera.fieldOfView = fov;
        previewCamera.cullingMask = 1 << layer;
        previewCamera.allowHDR = false;
        previewCamera.allowMSAA = false;
        previewCamera.clearFlags = CameraClearFlags.Depth;
        previewCamera.depth = cameraDepthForOrder;

        uiWorldSpaceCamera = GetComponentInParent<Canvas>().worldCamera;
        Update();
    }

    public void PostPrefabChange()
    {
        int layer = LayerMask.NameToLayer("2DPreview");
        previewObj = Instantiate<GameObject>(avatarPrefab);
        previewObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
        if (animator) {
            animator.Play(enterStateOnEnable);
        }

        previewObj.transform.localScale = Vector3.one;
        previewObj.transform.position = Vector3.zero;
        SetLayer(previewObj.transform, layer);
        avatarBounds = CalcBounds(previewObj.transform);
    }

    public void PlayAnimation(string stateName)
    {
        if (animator) {
            animator.Play(stateName);
        }
    }

    public void Update()
    {
        previewCamera.fieldOfView = fov;
        float height = avatarBounds.size.y * 1.5f;
        float showDistance = distance * distanceScale;
        if (autoDistance) {
            showDistance = (height * 0.5f) / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * distanceScale;
        }
        previewCamera.transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * Quaternion.AngleAxis(pitch,Vector3.left);
        previewCamera.transform.position = avatarBounds.center - previewCamera.transform.forward * showDistance;
        Rect screenRect = RectTransformToScreenSpaceNormalized(rect, uiWorldSpaceCamera);
        previewCamera.rect = screenRect;
        previewObj.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up);
    }

    public void OnDisable()
    {
        if (previewObj) {
            DestroyImmediate(previewObj);
        }
        if (previewCamera) {
            DestroyImmediate(previewCamera.gameObject);
        }
        if (sceneObj) {
            DestroyImmediate(sceneObj);
        }
    }

    public void SetLayer(Transform trans, int layer)
    {
        trans.gameObject.layer = layer;
        for (int i = 0; i < trans.childCount; i++) {
            SetLayer(trans.GetChild(i), layer);
        }
    }

    public Bounds CalcBounds(Transform trans)
    {
        Bounds bounds = new Bounds(trans.position, Vector3.zero);
        Renderer[] renderers = trans.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

    public static Rect RectTransformToScreenSpaceNormalized(RectTransform transform, Camera uiWorldSpaceCamera)
    {
        Rect screenSpaceRect = RectTransformToScreenSpace(transform);
        if (uiWorldSpaceCamera) {
            screenSpaceRect.x -= uiWorldSpaceCamera.transform.position.x;
            screenSpaceRect.y -= uiWorldSpaceCamera.transform.position.y;
        }
        return new Rect(screenSpaceRect.xMin / Screen.width + 0.5f, screenSpaceRect.yMin / Screen.height + 0.5f, screenSpaceRect.width / Screen.width, screenSpaceRect.height / Screen.height);
    }



}

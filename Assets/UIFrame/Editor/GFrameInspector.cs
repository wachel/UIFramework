using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

[CustomEditor(typeof(GFrame))]
public class GFrameInspector:Editor
{
    public override void OnInspectorGUI()
    {
        GFrame frame = target as GFrame;
        //if (GUILayout.Button("生成代码")) {
        //    string script = CreateClassScript(frame);
        //    File.WriteAllText(GFrame.SavePath + "/" + GetName(frame.name) + ".cs", script);
        //    AssetDatabase.Refresh();
        //}
        EditorGUILayout.TextArea(CreateDeclearScript(frame) + CreateBindScript(frame));
    }

    static string GetName(string name)
    {
        return name.Replace('.','_').Replace('/','_').Replace(' ','_').Replace("(","").Replace(")","");
    }

    //static string CreateClassScript(GFrame frame)
    //{
    //    string result = "";
    //    result += "using UnityEngine;\n";
    //    result += "using UnityEngine.UI;\n\n";
    //    result += "//该文件由GFrameInspector.cs自动生成，请不要手动修改\n";
    //    result += "namespace UIFrame{\n";
    //    result += "    public class " + GetName(frame.name) + "\n";
    //    result += "    {\n";
    //
    //    //属性列表
    //    ForEachProperty(frame, (GPrefabInstance loader, GExportToScriptInfo property) => {
    //        result += "        public " + property.type.Replace('+','.') + " " + GetName(loader,property) + ";";
    //        result += "    \n";
    //    });
    //     
    //    //绑定函数
    //    result += "        public void BindProperty(Transform frame)\n";
    //    result += "        {\n";
    //    ForEachProperty(frame, (GPrefabInstance loader, GExportToScriptInfo property) => {
    //        result += "            " + GetName(loader, property) + " = frame.Find(\"" + GUtility.GetPath(frame.transform,property.target.transform) + "\").GetComponent<" + property.type + ">();";
    //        result += "\n";
    //    });
    //    result += "        }\n";
    //
    //    result += "    }\n}\n";
    //    return result;
    //}

    static string CreateDeclearScript(GFrame frame)
    {
        string result = "";
        ForEachProperty(frame, (GPrefabInstance loader, GRuntimeLib lib) => {
            if (lib) {
                result += "public " + lib.GetType().Name.Replace('+', '.') + " " + GetName(loader.name) + ";";
                result += "\n";
            }
        });
        return result + "\n";
    }

    static string CreateBindScript(GFrame frame)
    {
        string result = "";
        //绑定函数
        result += "public void BindProperty(Transform frame)\n";
        result += "{\n";
        ForEachProperty(frame, (GPrefabInstance loader, GRuntimeLib lib) => {
            if (lib) {
                result += "    " + GetName(loader.name) + " = frame.Find(\"" + GUtility.GetPath(frame.transform, loader.transform) + "\").GetComponent<" + lib.GetType().Name + ">();";
                result += "\n";
            }
        });
        result += "}\n";
        return result;
    }

    static void ForEachProperty(GFrame frame, System.Action<GPrefabInstance,GRuntimeLib> fun)
    {
        //GWidget[] widgets = frame.transform.GetComponentsInChildren<GWidget>();
        //for (int i = 0; i < widgets.Length; i++) {
        //    for(int p = 0; p < widgets[i].exportToScriptInfos.Count; p++) {
        //        fun(widgets[i], widgets[i].exportToScriptInfos[p]);
        //    }
        //}

        GPrefabInstance[] loaders = frame.transform.GetComponentsInChildren<GPrefabInstance>();
        for (int i = 0; i < loaders.Length; i++) {
            //for(int p = 0; p < loaders[i].prefab.exportToScriptInfos.Count; p++) {
                fun(loaders[i],loaders[i].prefab.GetComponent<GRuntimeLib>());
            //}
        }
    }

    static string GetName(GPrefabInstance loader,GExportToScriptInfo property)
    {
        return GetName(property.type) + "_" + GetName(loader.name);
    }
}

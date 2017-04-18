using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GUtility
{
    public static string GetPath(Transform root, Transform target)
    {
        if(target == root) {
            return "";
        }
        if (target.parent == root) {
            return target.name;
        } else if(target.parent){
            if (target.name == "__preview__") {
                return GetPath(root, target.parent);
            } else{
                return GetPath(root, target.parent) + "/" + target.name;
            } 
        }
        return "";
    }
}

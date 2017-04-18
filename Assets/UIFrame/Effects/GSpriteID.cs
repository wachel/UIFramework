using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GSpriteID : MonoBehaviour
{
    public Image target;
    public GSpriteArray spriteArray;
    public int _id;
    public int id { get { return _id; } set{ _id = value; OnValidate(); } }

    public void OnValidate()
    {
        target.sprite = spriteArray.GetSprite(id);
    }
}

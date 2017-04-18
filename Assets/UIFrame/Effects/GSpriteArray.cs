using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GSpriteItem
{
    public int id;
    public Sprite sprite;
}

public class GSpriteArray : MonoBehaviour
{
    public List<GSpriteItem> sprites = new List<GSpriteItem>();

    public Sprite GetSprite(int id)
    {
        GSpriteItem result = sprites.Find((GSpriteItem item) => { return item.id == id; });
        return result!=null? result.sprite:null;
    }

    public void AddSprite(int id,Sprite sprite)
    {
        GSpriteItem item = new GSpriteItem();
        item.id = id;
        item.sprite = sprite;
        sprites.Add(item);
    }
}

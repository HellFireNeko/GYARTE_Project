using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Model List", menuName = "Data/Player model list")]
public class PlayerModelSelector : ScriptableObject
{
    public List<PlayerModelReturnValues> PlayerModelObjects = new List<PlayerModelReturnValues>();

    public PlayerModelReturnValues? GetItemById(int id)
    {
        if (PlayerModelObjects.Count > id)
        {
            return PlayerModelObjects[id];
        }
        return null;
    }
}

[Serializable]
public struct PlayerModelReturnValues
{
    public RenderTexture Texture;
    public GameObject Model;

    public PlayerModelReturnValues(RenderTexture texture, GameObject model)
    {
        Texture = texture;
        Model = model;
    }
}

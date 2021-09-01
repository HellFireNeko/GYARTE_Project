using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscordPlayer : MonoBehaviour
{
    public long ClientID;
    public Transform CamPoint;
    public GameObject Cam;
    public TextMesh Name;

    public void Init()
    {
        Instantiate(Cam, CamPoint);
    }

    public void SetName(string name)
    {
        Name.text = name;
    }

    public string GetPositionAndRotation()
    {
        return $"{transform.position.x},{transform.position.y},{transform.position.z},{transform.rotation.eulerAngles.x},{transform.rotation.eulerAngles.y},{transform.rotation.eulerAngles.z}";
    }

    public void SetPositionAndRotation(string vals)
    {
        var args = vals.Split(',');
        var pos = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
        var eulers = new Vector3(float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5]));
        transform.position = pos;
        transform.rotation = Quaternion.Euler(eulers);
    }
}

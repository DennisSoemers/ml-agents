using System.Collections.Generic;
using UnityEngine;

public class Sound
{
    private List<Rigidbody> objects;
    private float minimalSpeed;
    private int r;

    public Sound()
    {
        objects = new List<Rigidbody>();
        r=15;
        minimalSpeed=0;
    }

    public void AddObject(Rigidbody t)
    {
        objects.Add(t);
    }

    public List<Vector3> getObjects(Rigidbody curr)
    {
        List<Vector3> res = new List<Vector3>();
        foreach (Rigidbody t in objects)
        {
            if (IsObjectMakingSound(t, curr))
            {
                res.Add(t.transform.position);
            }
            else
            {
                res.Add(Vector3.zero);
            }
        }
        return res;
    }

    private bool IsObjectMakingSound(Rigidbody t, Rigidbody curr)
    {
        Vector3 relativePosition = (curr.transform.position - t.transform.position);
        if (curr != t && relativePosition.magnitude < r && t.velocity.magnitude != minimalSpeed)
            return true;
        return false;
    }
}
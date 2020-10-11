using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLine : MonoBehaviour
{
    public LineRenderer line;
    public List<Transform> trans;

    //List<Vector3> pos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (trans == null || trans.Count == 0)
            return;

        for (int i = 0; i < trans.Count; i++)
        {
            Transform t = trans[i];
            line.SetPosition(i, t.position);
        }
    }

    public void Setup(List<Transform> t)
    {
        trans = t;
        line.positionCount = trans.Count;
    }
}

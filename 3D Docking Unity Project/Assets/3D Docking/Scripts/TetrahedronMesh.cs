using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TetrahedronMesh : MonoBehaviour
{
    public Transform[] vertices;

    Mesh mesh;

    void Start()
    {
        GenerateMesh0();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void GenerateMesh()
    {
        mesh = new Mesh
        {
            name = "tetrahedron"
        };

        List<Vector3> vs = new List<Vector3>();

        for (int i = 0; i < 4; ++i)
            vs.Add(vertices[i].localPosition);

        List<int> ts = new List<int>();

        ts.Add(0);
        ts.Add(1);
        ts.Add(3);

        ts.Add(1);
        ts.Add(2);
        ts.Add(3);

        ts.Add(1);
        ts.Add(0);
        ts.Add(2);

        ts.Add(0);
        ts.Add(3);
        ts.Add(2);

        mesh.vertices = vs.ToArray();
        mesh.triangles = ts.ToArray();
    }

    void GenerateMesh0()
    {
        mesh = new Mesh
        {
            name = "tetrahedron"
        };

        List<Vector3> vs = new List<Vector3>();

        vs.Add(vertices[0].localPosition);
        vs.Add(vertices[1].localPosition);
        vs.Add(vertices[3].localPosition);

        vs.Add(vertices[1].localPosition);
        vs.Add(vertices[2].localPosition);
        vs.Add(vertices[3].localPosition);

        vs.Add(vertices[1].localPosition);
        vs.Add(vertices[0].localPosition);
        vs.Add(vertices[2].localPosition);

        vs.Add(vertices[0].localPosition);
        vs.Add(vertices[3].localPosition);
        vs.Add(vertices[2].localPosition);

        List<int> ts = new List<int>();
        for (int i = 0; i < 12; ++i)
            ts.Add(i);

        mesh.vertices = vs.ToArray();
        mesh.triangles = ts.ToArray();
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {

    }
}

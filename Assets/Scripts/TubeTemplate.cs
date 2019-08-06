using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public sealed class TubeTemplate : ScriptableObject {

    [SerializeField] int _subdivisions = 6;
    public int subdivisions {
        get { return Mathf.Clamp (_subdivisions, 2, 64); }
    }

    [SerializeField] int _segments = 256;
    public int segments {
        get { return Mathf.Clamp (_segments, 4, 1024); }
    }

    Mesh mesh;

    void OnValidate () {
        _subdivisions = Mathf.Clamp (_subdivisions, 2, 64);
        _segments = Mathf.Clamp (_segments, 4, 1024);
    }

    public Mesh GetMesh () {
        if (mesh == null) {
            mesh = new Mesh ();
            mesh.name = "Tube Template";
        }

        // vertices
        var vertices = new List<Vector3> ();
        vertices.Add (new Vector3 (0, -1, 0)); // head
        for (var i = 0; i < _segments + 1; i++) { // body
            for (var j = 0; j < _subdivisions; j++) {
                var phi = Mathf.PI * 2 * j / _subdivisions;
                vertices.Add (new Vector3 (phi, 0, i));
            }
        }
        vertices.Add (new Vector3 (0, 1, _segments)); // tail

        // indices
        var indices = new List<int> ();
        for (var i = 0; i < _subdivisions - 1; i++) { // head
            indices.Add (0);
            indices.Add (i + 2);
            indices.Add (i + 1);
        }
        indices.Add (0);
        indices.Add (1);
        indices.Add (_subdivisions);

        var idx = 1; // body
        for (var i = 0; i < _segments; i++) {
            for (var j = 0; j < _subdivisions - 1; j++) {
                indices.Add (idx);
                indices.Add (idx + 1);
                indices.Add (idx + _subdivisions);
                indices.Add (idx + 1);
                indices.Add (idx + 1 + _subdivisions);
                indices.Add (idx + _subdivisions);
                idx += 1;
            }
            indices.Add (idx);
            indices.Add (idx + 1 - _subdivisions);
            indices.Add (idx + _subdivisions);
            indices.Add (idx + 1 - _subdivisions);
            indices.Add (idx + 1);
            indices.Add (idx + _subdivisions);
            idx += 1;
        }

        for (var i = 0; i < _subdivisions - 1; i++) { // tail
            indices.Add (idx + i);
            indices.Add (idx + i + 1);
            indices.Add (idx + _subdivisions);
        }
        indices.Add (idx + _subdivisions - 1);
        indices.Add (idx);
        indices.Add (idx + _subdivisions);

        mesh.Clear ();
        mesh.SetVertices (vertices);
        mesh.SetIndices (indices.ToArray (), MeshTopology.Triangles, 0);
        mesh.UploadMeshData (true);
        return mesh;
    }

}
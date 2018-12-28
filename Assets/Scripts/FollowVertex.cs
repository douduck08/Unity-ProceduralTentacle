using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowVertex : MonoBehaviour {

    [SerializeField] MeshFilter targetMesh;
    [SerializeField] List<GameObject> entities;
    [SerializeField, Range (0.05f, 2f)] float smoothTime = 0.05f;

    public class MovingData {
        public Vector3 position;
        public Vector3 velocity;
    }

    List<MovingData> movingDatas = new List<MovingData> ();

    void Start () {
        var vertices = targetMesh.sharedMesh.vertices;
        for (int i = 0; i < vertices.Length && i < entities.Count; i++) {
            movingDatas.Add (new MovingData () { position = vertices[i] });
        }
    }

    void Update () {
        var localToWorldMatrix = targetMesh.transform.localToWorldMatrix;
        for (int i = 0; i < movingDatas.Count; i++) {
            entities[i].transform.position = Vector3.SmoothDamp (entities[i].transform.position, localToWorldMatrix.MultiplyPoint (movingDatas[i].position), ref movingDatas[i].velocity, smoothTime);
        }
    }

}
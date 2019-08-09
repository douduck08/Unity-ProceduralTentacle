using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TubeRenderer : MonoBehaviour {

    [Header ("Debug")]
    [SerializeField] bool displayLine;
    [SerializeField] bool displayDirection;
    [SerializeField] bool displayRegionCircle;
    [SerializeField] bool displayRegionLink;

    [Header ("Tube")]
    [SerializeField] TubeTemplate tubeTemplate;
    [SerializeField] Material material;
    [SerializeField] float radius = 1f;

    [Header ("Poisson Disc Sampling Instancing")]
    [SerializeField] bool enableInstancing;
    [SerializeField] float distributeRadius = 1f;
    [SerializeField] float regionRadius = 1f;

    [Header ("Hermite Curve")]
    [SerializeField] Transform[] nodes;

    Mesh tubeMesh;
    int subMeshIndex = 0;
    List<Vector2> offsetList;

    ComputeBuffer argsBuffer;
    ComputeBuffer positionBuffer;
    ComputeBuffer tangentBuffer;
    ComputeBuffer normalBuffer;

    const int MAX_TUBE_NUMBER = 64;

    void Start () {
        if (tubeTemplate == null || material == null) {
            this.enabled = false;
            return;
        }

        tubeMesh = tubeTemplate.GetMesh ();
        material.SetInt ("_SegmentNumber", tubeTemplate.segments + 1);

        var instanceCount = 1;
        if (enableInstancing) {
            var region = new Vector2 (regionRadius, regionRadius);
            offsetList = PoissonDiscSampler.GeneratePoints (distributeRadius, region * 2).Select (p => (p - region)).Where (p => p.magnitude < regionRadius).ToList ();
            if (offsetList.Count > MAX_TUBE_NUMBER) {
                Debug.LogWarning ("Instancing number reached MAX_TUBE_NUMBER = " + MAX_TUBE_NUMBER);
                offsetList = offsetList.Take (MAX_TUBE_NUMBER).ToList ();
            }
            instanceCount = offsetList.Count;
        }

        argsBuffer = new ComputeBuffer (1, 5 * sizeof (uint), ComputeBufferType.IndirectArguments);
        UpdateArgsBuffer (instanceCount);

        positionBuffer = new ComputeBuffer ((tubeTemplate.segments + 1) * instanceCount, 4 * 3);
        tangentBuffer = new ComputeBuffer ((tubeTemplate.segments + 1) * instanceCount, 4 * 3);
        normalBuffer = new ComputeBuffer ((tubeTemplate.segments + 1) * instanceCount, 4 * 3);

        material.SetBuffer ("_PositionBuffer", positionBuffer);
        material.SetBuffer ("_TangentBuffer", tangentBuffer);
        material.SetBuffer ("_NormalBuffer", normalBuffer);
    }

    void OnDestroy () {
        if (argsBuffer != null) argsBuffer.Release ();
        if (positionBuffer != null) positionBuffer.Release ();
        if (tangentBuffer != null) tangentBuffer.Release ();
        if (normalBuffer != null) normalBuffer.Release ();
    }

    void Update () {
        UpdateTubeData ();

        if (enableInstancing) {
            Graphics.DrawMeshInstancedIndirect (tubeMesh, subMeshIndex, material, new Bounds (Vector3.zero, Vector3.one * 1000), argsBuffer);
        } else {
            Graphics.DrawMesh (tubeMesh, Vector3.zero, Quaternion.identity, material, 0);
        }
    }

    void UpdateArgsBuffer (int instanceCount) {
        var args = new uint[5] { 0, 0, 0, 0, 0 };
        if (tubeMesh != null) {
            args[0] = (uint) tubeMesh.GetIndexCount (subMeshIndex);
            args[1] = (uint) instanceCount;
            args[2] = (uint) tubeMesh.GetIndexStart (subMeshIndex);
            args[3] = (uint) tubeMesh.GetBaseVertex (subMeshIndex);
        }
        argsBuffer.SetData (args);
    }

    void UpdateTubeData () {
        if (enableInstancing) {
            List<Vector3> allPoints = new List<Vector3> (), allTangents = new List<Vector3> (), allNormals = new List<Vector3> ();
            for (int i = 0; i < offsetList.Count; i++) {
                Vector3[] points, tangents, normals;
                points = HermiteCurve.GetPoints (nodes, offsetList[i], tubeTemplate.segments, out tangents, out normals);

                allPoints.AddRange (points);
                allTangents.AddRange (tangents);
                allNormals.AddRange (normals);
            }

            positionBuffer.SetData (allPoints);
            tangentBuffer.SetData (allTangents);
            normalBuffer.SetData (allNormals);
        } else {
            Vector3[] points, tangents, normals;
            points = HermiteCurve.GetPoints (nodes, tubeTemplate.segments, out tangents, out normals);

            positionBuffer.SetData (points);
            tangentBuffer.SetData (tangents);
            normalBuffer.SetData (normals);
        }

        material.SetFloat ("_Radius", Mathf.Max (0f, radius));
    }

    void OnDrawGizmos () {
        if (tubeTemplate == null || tubeTemplate.segments < 1) {
            return;
        }

        if (displayRegionCircle) {
            Gizmos.color = Color.white;
            for (int i = 0; i < nodes.Length; i++) {
                var center = nodes[i].position;
                var right = nodes[i].right * nodes[i].localScale.x;
                var up = nodes[i].up * nodes[i].localScale.y;
                for (int j = 0; j < 16; j++) {
                    var phi1 = Mathf.PI * 2 * (j / 16f);
                    var phi2 = Mathf.PI * 2 * ((j + 1) / 16f);
                    var pointA = center + right * regionRadius * Mathf.Cos (phi1) + up * regionRadius * Mathf.Sin (phi1);
                    var pointB = center + right * regionRadius * Mathf.Cos (phi2) + up * regionRadius * Mathf.Sin (phi2);
                    Gizmos.DrawLine (pointA, pointB);
                }
            }
        }

        if (displayRegionLink) {
            Gizmos.color = Color.yellow;
            for (int i = 1; i < nodes.Length; i++) {
                var center = nodes[i].position;
                var right = nodes[i].right * nodes[i].localScale.x;
                var up = nodes[i].up * nodes[i].localScale.y;
                var preCenter = nodes[i - 1].position;
                var preRight = nodes[i - 1].right * nodes[i - 1].localScale.x;
                var preUp = nodes[i - 1].up * nodes[i - 1].localScale.y;
                for (int j = 0; j < 8; j++) {
                    var phi1 = Mathf.PI * 2 * (j / 8f);
                    var point = center + right * regionRadius * Mathf.Cos (phi1) + up * regionRadius * Mathf.Sin (phi1);
                    var prePoint = preCenter + preRight * regionRadius * Mathf.Cos (phi1) + preUp * regionRadius * Mathf.Sin (phi1);
                    Gizmos.DrawLine (prePoint, point);
                }
            }
        }

        if (displayLine || displayDirection) {
            Vector3[] points, tangents, normals;
            points = HermiteCurve.GetPoints (nodes, tubeTemplate.segments, out tangents, out normals);
            points = HermiteCurve.GetPoints (nodes, tubeTemplate.segments);

            if (displayLine) {
                Gizmos.color = Color.white;
                for (int i = 1; i < points.Length; i++) {
                    Gizmos.DrawLine (points[i - 1], points[i]);
                }
            }

            if (displayDirection) {
                for (int i = 0; i < points.Length; i++) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine (points[i], points[i] + tangents[i] * 0.1f);

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine (points[i], points[i] + normals[i] * 0.1f);

                    var binormal = Vector3.Cross (tangents[i], normals[i]).normalized;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine (points[i], points[i] + binormal * 0.1f);
                }
            }
        }
    }
}
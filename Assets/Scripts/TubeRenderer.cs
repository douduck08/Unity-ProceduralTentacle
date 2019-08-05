using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeRenderer : MonoBehaviour {

    [SerializeField] int subdivisions;
    [SerializeField] Transform[] nodes;

    Vector3 GetHermiteCurvePoint (Transform from, Transform to, float t) {
        var pointA = from.position;
        var pointB = to.position;
        var dirA = from.forward * from.localScale.z;
        var dirB = to.forward * to.localScale.z;

        var t2 = t * t;
        var t3 = t2 * t;
        var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
        return point;
    }

    Vector3[] GetHermiteCurve (Transform from, Transform to, int subdivisions) {
        var pointA = from.position;
        var pointB = to.position;
        var dirA = from.forward * from.localScale.z;
        var dirB = to.forward * to.localScale.z;

        Vector3[] points = new Vector3[subdivisions + 1];
        for (int i = 0; i <= subdivisions; i++) {
            var t = 1f * i / subdivisions;
            var t2 = t * t;
            var t3 = t2 * t;
            var point = (2 * t3 - 3 * t2 + 1) * pointA + (t3 - 2 * t2 + t) * dirA + (-2 * t3 + 3 * t2) * pointB + (t3 - t2) * dirB;
            points[i] = point;
        }
        return points;
    }

    void OnDrawGizmos () {
        if (subdivisions < 1) {
            return;
        }

        for (int idx = 1; idx < nodes.Length; idx++) {
            if (nodes[idx - 1] != null && nodes[idx] != null) {
                var points = GetHermiteCurve (nodes[idx - 1], nodes[idx], subdivisions);
                for (int i = 1; i < points.Length; i++) {
                    Gizmos.DrawLine (points[i - 1], points[i]);
                }
            }
        }
    }
}
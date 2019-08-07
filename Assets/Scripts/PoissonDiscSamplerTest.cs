using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiscSamplerTest : MonoBehaviour {

    [SerializeField] float radius = 1;
    [SerializeField] Vector2 regionSize = Vector2.one;
    [SerializeField] float displayRadius = 1;

    List<Vector2> points;

    void OnValidate () {
        points = PoissonDiscSampler.GeneratePoints (radius, regionSize);
    }

    void OnDrawGizmos () {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube (regionSize / 2f, regionSize);
        if (points != null) {
            foreach (var point in points) {
                Gizmos.DrawSphere (point, displayRadius);
            }
        }
    }
}
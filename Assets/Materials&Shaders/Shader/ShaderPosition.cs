using System;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderPosition : MonoBehaviour
{
    [SerializeField][Min(0.01f)] private float radius = 1f;
    void Update()
    {
        Shader.SetGlobalVector("_Position", transform.position);
        Shader.SetGlobalFloat("_Radius", radius);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

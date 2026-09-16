using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class ShaderPosition : MonoBehaviour
{
    [SerializeField][Min(0.01f)] private float radius = 1f;
    [SerializeField] private string namePos; 
    [SerializeField] private string nameRadius; 
    private float _speed = 0.05f;

    private void Start()
    {
        radius = 1f;
    }

    void Update()
    {
        Shader.SetGlobalVector(namePos, transform.position);
        Shader.SetGlobalFloat(nameRadius, radius);
    }

    public void ChangeSize()
    {
        StartCoroutine(ChangeSizeCoroutine());
    }
    
    public void ChangeSize(float newSize)
    {
        StartCoroutine(ChangeSizeCoroutine(newSize));
    }
    
    private IEnumerator ChangeSizeCoroutine()
    {
        while (radius < 999f)
        {
            radius = Mathf.Lerp(radius, 1000f, _speed/10 * Time.deltaTime);

            if (radius > 170f) _speed = 1f;

            yield return null;
        }

        radius = 1000f;

        GameManager.instance.ChangeWorldState(false);
    }
    
    private IEnumerator ChangeSizeCoroutine(float target)
    {
        while (radius < target - 10f)
        {
            radius = Mathf.Lerp(radius, target, _speed*10 * Time.deltaTime);
            
            yield return null;
        }

        radius = target;

        yield return new WaitForSeconds(.5f);

        while (radius > 1f)
        {
            radius = Mathf.Lerp(radius, 0f, _speed*100 * Time.deltaTime);
            yield return null;
        }

        radius = 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

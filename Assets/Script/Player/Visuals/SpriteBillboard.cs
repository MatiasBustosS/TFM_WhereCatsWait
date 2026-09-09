using UnityEngine;

/// <summary>
/// Utilidad opcional para el enfoque "sprites dentro de un entorno 3D":
/// mantiene el sprite del personaje orientado hacia la cámara en todo
/// momento. Útil porque la cámara de Where Cats Wait no es perfectamente
/// perpendicular siempre (zoom contextual, encuadres cinematográficos con
/// splines — ver apartado 4.4.2 del GDD), así que sin esto el sprite podría
/// verse "de canto" en ciertos ángulos.
///
/// No sustituye al flip de FelineMovement: esto sólo orienta el plano del
/// sprite hacia la cámara, el flip horizontal sigue gestionando hacia qué
/// lado mira Yui.
/// </summary>
public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        Vector3 lookDirection = transform.position - targetCamera.transform.position;
        lookDirection.y = 0f; // el sprite se mantiene en pie, sin inclinarse
        if (lookDirection.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}

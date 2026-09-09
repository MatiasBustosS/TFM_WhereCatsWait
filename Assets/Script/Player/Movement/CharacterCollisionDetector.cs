using UnityEngine;

/// <summary>
/// Encapsula toda la detección de colisiones manual del personaje en 3D:
/// suelo, escalones y paredes. Sustituye al CharacterController de Unity
/// por raycasts propios, tal y como se decidió para Where Cats Wait.
///
/// IMPORTANTE: los semi-anchos del collider se cachean UNA sola vez en
/// Awake, en vez de leer Collider.bounds en cada frame. Esto es justo lo
/// que evita el bug que vimos en la versión 2D, donde rotar el modelo para
/// cambiar de dirección hacía que los raycasts "colapsaran" hacia el centro
/// de la cápsula. Aquí el cambio de dirección se resuelve con un flip de
/// sprite (ver PlayerMovement), así que este transform nunca debería rotar.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterCollisionDetector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CapsuleCollider capsule;

    [Header("Suelo")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask solidLayer;

    [Header("Escalones")]
    [Tooltip("Altura máxima de escalón que Yui puede subir automáticamente.")]
    [SerializeField] private float stepHeight = 0.3f;
    [SerializeField] private float stepSearchDistance = 0.4f;

    [Header("Pared (chequeo simple, se usa en el aire)")]
    [SerializeField] private float wallCheckDistance = 0.15f;

    private float _radius;
    private float _halfHeight;

    public bool IsGrounded { get; private set; }
    public bool IsBlockedByWall { get; private set; }
    public RaycastHit GroundHit { get; private set; }

    private void Awake()
    {
        if (capsule == null) capsule = GetComponent<CapsuleCollider>();

        _radius = capsule.radius * transform.lossyScale.x;
        _halfHeight = (capsule.height * 0.5f) * transform.lossyScale.y;
    }

    private Vector3 Center => transform.position + capsule.center;

    public void CheckGround()
    {
        Vector3 origin = Center + Vector3.down * (_halfHeight - 0.02f);
        IsGrounded = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, solidLayer);
        GroundHit = hit;
    }

    /// <summary>
    /// Se usa mientras el personaje está en el suelo y se mueve. Si hay un
    /// escalón bajo (hueco libre por encima) lo sube suavemente. Si hay
    /// algo bloqueando tanto abajo como arriba, es una pared: devuelve
    /// false para que PlayerMovement frene el avance horizontal.
    /// </summary>
    public bool TryStepOrBlock(float dir, Rigidbody rb, float stepSmoothness)
    {
        IsBlockedByWall = false;
        if (Mathf.Abs(dir) < 0.01f) return true;

        float sign = Mathf.Sign(dir);
        Vector3 direction = Vector3.right * sign;
        Vector3 sideCenter = Center + direction * _radius;

        Vector3 lowerOrigin = sideCenter + Vector3.down * (_halfHeight - 0.05f);
        Vector3 upperOrigin = lowerOrigin + Vector3.up * stepHeight;

        bool hitLower = Physics.Raycast(lowerOrigin, direction, out RaycastHit lowerHit, stepSearchDistance, solidLayer);
        if (!hitLower) return true; // nada en el camino

        bool hitUpper = Physics.Raycast(upperOrigin, direction, stepSearchDistance + 0.05f, solidLayer);
        if (!hitUpper)
        {
            // Hueco libre arriba -> es un escalón, subimos suavemente.
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, lowerHit.point.y + 0.05f, stepSmoothness * Time.fixedDeltaTime);
            rb.position = pos;
            return true;
        }

        // Bloqueado también arriba -> demasiado alto para subir, es pared.
        IsBlockedByWall = true;
        return false;
    }

    /// <summary>
    /// Chequeo de pared simplificado (sin lógica de escalón), pensado para
    /// usarse en el aire, donde no tiene sentido "subir" nada.
    /// </summary>
    public bool CheckWallSimple(float dir)
    {
        if (Mathf.Abs(dir) < 0.01f) return false;

        float sign = Mathf.Sign(dir);
        Vector3 origin = Center + Vector3.right * (sign * _radius * 0.9f);
        bool blocked = Physics.Raycast(origin, Vector3.right * sign, wallCheckDistance, solidLayer);
        IsBlockedByWall = blocked;
        return blocked;
    }

    private void OnDrawGizmosSelected()
    {
        if (capsule == null) capsule = GetComponent<CapsuleCollider>();
        float radius = capsule.radius * transform.lossyScale.x;
        float halfHeight = (capsule.height * 0.5f) * transform.lossyScale.y;
        Vector3 center = transform.position + capsule.center;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(center + Vector3.down * (halfHeight - 0.02f), Vector3.down * groundCheckDistance);

        Gizmos.color = Color.cyan;
        Vector3 sideCenter = center + Vector3.right * radius;
        Vector3 lowerOrigin = sideCenter + Vector3.down * (halfHeight - 0.05f);
        Gizmos.DrawRay(lowerOrigin, Vector3.right * stepSearchDistance);
        Gizmos.DrawRay(lowerOrigin + Vector3.up * stepHeight, Vector3.right * (stepSearchDistance + 0.05f));
    }
}

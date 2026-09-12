using UnityEngine;

/// <summary>
/// Base común para todas las habilidades de la Sintonía Felina (vista,
/// oído, y en el futuro garra/equilibrio/sigilo/salto felino). Gestiona la
/// máquina de estados Recarga -> Disponible -> Activa -> Recarga descrita
/// en el GDD y expone eventos para que el HUD (rueda radial, iconos de
/// orejas/ojos) reaccione sin que esta clase sepa nada de interfaz.
///
/// Cada habilidad concreta sólo tiene que implementar QUÉ pasa al activarse,
/// mientras dura activa, y al desactivarse — el temporizador y el cambio de
/// estados ya están resueltos aquí.
/// </summary>
public abstract class FelineAbilityBase : MonoBehaviour
{
    [Header("Configuración de la habilidad")]
    [SerializeField] protected string abilityName = "Habilidad Felina";
    [SerializeField] protected float activeDuration = 3f;
    [SerializeField] protected float cooldownDuration = 6f;
    [Tooltip("Actívalo para pruebas rápidas. En el juego real, desbloquéala con Unlock() al restaurar el vínculo con el gato correspondiente.")]
    [SerializeField] protected bool startUnlocked = false;

    [Header("Eventos (para HUD / interfaz)")]
    public FelineAbilityStateUnityEvent OnStateChanged;
    [Tooltip("0-1: progreso de recarga (1 = disponible), o tiempo restante mientras está activa.")]
    public FloatUnityEvent OnProgressChanged;

    public string AbilityName => abilityName;
    public FelineAbilityState State { get; private set; } = FelineAbilityState.Available;
    public bool IsUnlocked { get; private set; }

    private float _timer;

    protected virtual void Awake()
    {
        IsUnlocked = startUnlocked;
        SetState(FelineAbilityState.Available);
    }

    protected virtual void Update()
    {
        if (!IsUnlocked) return;

        switch (State)
        {
            case FelineAbilityState.Active:
                _timer -= Time.deltaTime;
                OnProgressChanged?.Invoke(Mathf.Clamp01(_timer / activeDuration));
                TickActive(Time.deltaTime);
                if (_timer <= 0f) Deactivate();
                break;

            case FelineAbilityState.Recharging:
                _timer -= Time.deltaTime;
                OnProgressChanged?.Invoke(1f - Mathf.Clamp01(_timer / cooldownDuration));
                if (_timer <= 0f) SetState(FelineAbilityState.Available);
                break;
        }
    }

    /// Llamar al restaurar el vínculo con el gato que otorga esta habilidad.
    public void Unlock()
    {
        IsUnlocked = true;
    }

    /// Intenta activar la habilidad. Devuelve false si no está desbloqueada
    /// o no está disponible (en uso o en recarga).
    public bool TryActivate()
    {
        if (!IsUnlocked || State != FelineAbilityState.Available) return false;

        _timer = activeDuration;
        SetState(FelineAbilityState.Active);
        OnActivate();
        return true;
    }

    /// Corta la habilidad antes de tiempo (por ejemplo, al iniciar un diálogo).
    public void ForceDeactivate()
    {
        if (State == FelineAbilityState.Active) Deactivate();
    }

    private void Deactivate()
    {
        OnDeactivate();
        _timer = cooldownDuration;
        SetState(FelineAbilityState.Recharging);
    }

    private void SetState(FelineAbilityState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    /// Se llama una única vez al activarse la habilidad.
    protected abstract void OnActivate();

    /// Se llama cada frame mientras la habilidad está activa.
    protected abstract void TickActive(float deltaTime);

    /// Se llama una única vez al terminar la duración activa, antes de
    /// entrar en recarga.
    protected abstract void OnDeactivate();
}

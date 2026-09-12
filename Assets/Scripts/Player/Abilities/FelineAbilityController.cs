using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Punto único de entrada para la Sintonía Felina. Traduce el esquema de
/// control de la Tabla 11 del GDD en llamadas sobre FelineAbilityBase:
///  - Click izquierdo: activa la habilidad seleccionada.
///  - Shift izquierdo / click derecho: abre o cierra la rueda de habilidades.
///  - Rueda del ratón: navega entre las habilidades desbloqueadas.
///
/// Este script NO dibuja ninguna interfaz. El HUD (rueda radial, iconos de
/// estado) se suscribe a los eventos expuestos aquí.
/// </summary>
public class FelineAbilityController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference activateAbility;
    [SerializeField] private InputActionReference toggleWheel;
    [SerializeField] private InputActionReference cycleAbility;

    [Header("Habilidades disponibles (en este vertical slice: Vista y Oído)")]
    [SerializeField] private List<FelineAbilityBase> abilities = new();

    [Header("Eventos (para HUD)")]
    public IntUnityEvent OnAbilitySelected;
    public BoolUnityEvent OnWheelToggled;
    public FelineAbilityUnityEvent OnAbilityUnlocked;

    private int _selectedIndex;
    private bool _wheelOpen;

    public FelineAbilityBase Current => abilities.Count > 0 ? abilities[_selectedIndex] : null;

    private void OnEnable()
    {
        activateAbility.action.Enable();
        toggleWheel.action.Enable();
        cycleAbility.action.Enable();

        activateAbility.action.performed += OnActivatePressed;
        toggleWheel.action.performed += OnWheelPressed;
        cycleAbility.action.performed += OnCycle;
    }

    private void OnDisable()
    {
        activateAbility.action.performed -= OnActivatePressed;
        toggleWheel.action.performed -= OnWheelPressed;
        cycleAbility.action.performed -= OnCycle;

        activateAbility.action.Disable();
        toggleWheel.action.Disable();
        cycleAbility.action.Disable();
    }

    private void OnActivatePressed(InputAction.CallbackContext ctx)
    {
        if (HudManager.Instance != null && HudManager.Instance.IsTarget) return;
        Current?.TryActivate();
    }

    private void OnWheelPressed(InputAction.CallbackContext ctx)
    {
        _wheelOpen = !_wheelOpen;
        OnWheelToggled?.Invoke(_wheelOpen);
    }

    private void OnCycle(InputAction.CallbackContext ctx)
    {
        if (!_wheelOpen || abilities.Count == 0) return;

        float scroll = ctx.ReadValue<float>();
        if (Mathf.Abs(scroll) < 0.01f) return;

        int step = scroll > 0f ? 1 : -1;
        _selectedIndex = NextUnlockedIndex(_selectedIndex, step);
        OnAbilitySelected?.Invoke(_selectedIndex);
    }

    private int NextUnlockedIndex(int startIndex, int step)
    {
        int index = startIndex;
        for (int i = 0; i < abilities.Count; i++)
        {
            index = (index + step + abilities.Count) % abilities.Count;
            if (abilities[index].IsUnlocked) return index;
        }
        return startIndex;
    }

    /// <summary>
    /// Llamar al restaurar el vínculo con el gato correspondiente (ver
    /// apartado 2.4.3 del GDD): 0 = Vista Felina, 1 = Oído Felino, según el
    /// orden en el que asignes la lista "abilities" en el Inspector.
    /// </summary>
    public void UnlockAbility(int index)
    {
        if (index < 0 || index >= abilities.Count) return;
        abilities[index].Unlock();
        OnAbilityUnlocked?.Invoke(abilities[index]);
    }
}

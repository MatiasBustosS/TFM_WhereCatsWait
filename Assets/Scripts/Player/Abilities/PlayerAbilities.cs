using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    [SerializeField] private InputActionReference useAbility;
    [SerializeField] private InputActionReference changeAbility;
    
    [SerializeField] private float hearingRange = 20f;
    [SerializeField] private float abilityCooldown = 5f;
    [SerializeField] private ShaderPosition YuiShader;
    [SerializeField] private ParticleSystem particle;
    private bool canUseAbility = true;
    
    private enum Ability
    {
        CatVision,
        CatHearing
    }

    [SerializeField] private Ability currentAbility = Ability.CatVision;

    private void OnEnable()
    {
        useAbility.action.performed += UseAbility;
        changeAbility.action.performed += ChangeAbility;

        useAbility.action.Enable();
        changeAbility.action.Enable();
    }

    private void OnDisable()
    {
        useAbility.action.performed -= UseAbility;
        changeAbility.action.performed -= ChangeAbility;

        useAbility.action.Disable();
        changeAbility.action.Disable();
    }

    private void UseAbility(InputAction.CallbackContext context)
    {
        if (!canUseAbility)
            return;

        switch (currentAbility)
        {
            case Ability.CatVision:
                UseCatVision();
                break;

            case Ability.CatHearing:
                UseCatHearing();
                break;
        }

        StartCoroutine(AbilityCooldown());
    }

    private void ChangeAbility(InputAction.CallbackContext context)
    {
        int abilityCount = System.Enum.GetValues(typeof(Ability)).Length;

        currentAbility = (Ability)
            (((int)currentAbility + 1) % abilityCount);
    }

    private void UseCatVision()
    {
        particle.Play();
        YuiShader.ChangeSize(50f);
    }

    private void UseCatHearing()
    {
        foreach (SoundEmitter emitter in SoundEmitter.AllEmitters)
        {
            float distance = Vector3.Distance(
                transform.position,
                emitter.transform.position
            );

            if (distance <= hearingRange)
            {
                emitter.EmitSound();
            }
        }
    }
    
    private IEnumerator AbilityCooldown()
    {
        canUseAbility = false;

        yield return new WaitForSeconds(abilityCooldown);

        canUseAbility = true;
    }
    
}

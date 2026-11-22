using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    [SerializeField] private CameraMovement2 cameraMovement;
    [SerializeField] private InputActionReference useAbility;
    [SerializeField] private InputActionReference changeAbility;
    
    [SerializeField] private float hearingRange = 30f;
    [SerializeField] private float abilityCooldown = 5f;
    [SerializeField] private ShaderPosition YuiShader;
    [SerializeField] private ParticleSystem particle;

    [Header("Unlocked abilities")]
    [SerializeField] private bool visionUnlocked = true;
    [SerializeField] private bool hearingUnlocked = true;
    [SerializeField] private bool clawsUnlocked;

    private CatElectricArcs[] abilityVisuals;
    private bool canUseAbility = true;
    private bool waitingForResumeClick;
    private bool returningToStartingPosition;
    private bool abilityModeVisible;
    
    [SerializeField] private CatAbilityType currentAbility = CatAbilityType.CatVision;
    private CatAbilityType previewAbility;

    private void Awake()
    {
        abilityVisuals = FindObjectsByType<CatElectricArcs>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        if (!IsAbilityUnlocked(currentAbility))
            currentAbility = GetFirstUnlockedAbility();

        previewAbility = currentAbility;
        SetAbilityVisuals(false, currentAbility);
    }

    private void Update()
    {
        if (!waitingForResumeClick
            || !abilityModeVisible
            || cameraMovement.isSelectingAbility
            || returningToStartingPosition
            || Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        int direction = scroll > 0f ? 1 : -1;
        if (TryGetAdjacentUnlockedAbility(previewAbility, direction, out CatAbilityType nextAbility))
        {
            previewAbility = nextAbility;
            SetAbilityVisuals(true, previewAbility);
        }
    }

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
        abilityModeVisible = false;
        SetAbilityVisuals(false, currentAbility);
    }

    private void UseAbility(InputAction.CallbackContext context)
    {
        if (waitingForResumeClick)
        {
            if (!cameraMovement.isSelectingAbility
                && !returningToStartingPosition)
            {
                returningToStartingPosition = true;
                if (!cameraMovement.ReturnToStartingPosition(0.7f))
                {
                    returningToStartingPosition = false;
                    return;
                }

                currentAbility = previewAbility;
                abilityModeVisible = false;
                SetAbilityVisuals(false, currentAbility);
                StartCoroutine(ResumeAfterCameraReturn());
            }

            return;
        }

        if (!canUseAbility || !IsAbilityUnlocked(currentAbility))
            return;

        UseSelectedAbility(currentAbility);
    }

    private void ChangeAbility(InputAction.CallbackContext context)
    {
        if (waitingForResumeClick)
            return;

        if (cameraMovement == null)
        {
            Debug.LogError("PlayerAbilities requires a CameraMovement reference.", this);
            return;
        }

        if (GameManager.instance == null || GameManager.instance.RailPlayerMovement == null)
        {
            Debug.LogError("PlayerAbilities requires a GameManager with a RailPlayerMovement reference.", this);
            return;
        }

        if (!cameraMovement.MoveCameraInFront(0.7f))
            return;

        previewAbility = currentAbility;

        GameManager.instance.RailPlayerMovement.SetCanMove(false);
        waitingForResumeClick = true;
        StartCoroutine(ShowAbilityVisualsAfterCameraTransition());
    }

    private IEnumerator ShowAbilityVisualsAfterCameraTransition()
    {
        while (cameraMovement.isSelectingAbility)
            yield return null;

        if (waitingForResumeClick)
        {
            abilityModeVisible = true;
            SetAbilityVisuals(true, previewAbility);
        }
    }

    private void SetAbilityVisuals(bool visible, CatAbilityType selectedAbility)
    {
        if (abilityVisuals == null)
            return;

        foreach (CatElectricArcs visual in abilityVisuals)
        {
            if (visual != null)
                visual.SetAbilityMode(
                    visible && IsAbilityUnlocked(visual.AbilityType),
                    selectedAbility);
        }
    }

    public void UnlockAbility(CatAbilityType ability)
    {
        SetAbilityUnlocked(ability, true);
    }

    public void SetAbilityUnlocked(CatAbilityType ability, bool unlocked)
    {
        bool wasUnlocked = IsAbilityUnlocked(ability);
        if (wasUnlocked == unlocked)
            return;

        bool hadUnlockedAbility = HasAnyUnlockedAbility();
        SetAbilityUnlockedState(ability, unlocked);

        if (unlocked && !hadUnlockedAbility)
        {
            currentAbility = ability;
        }
        else if (!IsAbilityUnlocked(currentAbility))
        {
            currentAbility = GetFirstUnlockedAbility();
        }

        if (abilityModeVisible)
        {
            if (!IsAbilityUnlocked(previewAbility))
                previewAbility = GetFirstUnlockedAbility();
            SetAbilityVisuals(true, previewAbility);
        }
    }

    public bool IsAbilityUnlocked(CatAbilityType ability)
    {
        switch (ability)
        {
            case CatAbilityType.CatVision:
                return visionUnlocked;
            case CatAbilityType.CatHearing:
                return hearingUnlocked;
            case CatAbilityType.CatClaw:
                return clawsUnlocked;
            default:
                return false;
        }
    }

    private void SetAbilityUnlockedState(CatAbilityType ability, bool unlocked)
    {
        switch (ability)
        {
            case CatAbilityType.CatVision:
                visionUnlocked = unlocked;
                break;
            case CatAbilityType.CatHearing:
                hearingUnlocked = unlocked;
                break;
            case CatAbilityType.CatClaw:
                clawsUnlocked = unlocked;
                break;
        }
    }

    private bool HasAnyUnlockedAbility()
    {
        return visionUnlocked || hearingUnlocked || clawsUnlocked;
    }

    private CatAbilityType GetFirstUnlockedAbility()
    {
        if (visionUnlocked) return CatAbilityType.CatVision;
        if (hearingUnlocked) return CatAbilityType.CatHearing;
        if (clawsUnlocked) return CatAbilityType.CatClaw;
        return CatAbilityType.CatVision;
    }

    private bool TryGetAdjacentUnlockedAbility(
        CatAbilityType fromAbility,
        int direction,
        out CatAbilityType nextAbility)
    {
        int abilityCount = System.Enum.GetValues(typeof(CatAbilityType)).Length;
        for (int step = 1; step < abilityCount; step++)
        {
            int candidateIndex = ((int)fromAbility + direction * step) % abilityCount;
            if (candidateIndex < 0)
                candidateIndex += abilityCount;

            CatAbilityType candidate = (CatAbilityType)candidateIndex;
            if (!IsAbilityUnlocked(candidate))
                continue;

            nextAbility = candidate;
            return true;
        }

        nextAbility = fromAbility;
        return false;
    }

    private IEnumerator ResumeAfterCameraReturn()
    {
        while (cameraMovement.isSelectingAbility)
            yield return null;

        GameManager.instance.RailPlayerMovement.SetCanMove(true);
        waitingForResumeClick = false;
        returningToStartingPosition = false;
    }

    private void UseSelectedAbility(CatAbilityType ability)
    {
        switch (ability)
        {
            case CatAbilityType.CatVision:
                UseCatVision();
                break;

            case CatAbilityType.CatHearing:
                UseCatHearing();
                break;

            case CatAbilityType.CatClaw:
                return;

            default:
                return;
        }

        StartCoroutine(AbilityCooldown());
    }

    private void UseCatVision()
    {
        particle.Play();
        YuiShader.ChangeSize(50f);
    }

    private void UseCatHearing()
    {
        if (hearingRange < 0f)
        {
            Debug.LogError("PlayerAbilities hearing range cannot be negative.", this);
            return;
        }

        IReadOnlyList<SoundEmitter> emitters = SoundEmitter.ActiveEmitters;
        float hearingRangeSquared = hearingRange * hearingRange;
        bool foundAudibleEmitter = false;

        for (int i = 0; i < emitters.Count; i++)
        {
            SoundEmitter emitter = emitters[i];
            if (emitter == null || !emitter.CanEmitSound)
                continue;

            Vector3 offset = emitter.transform.position - transform.position;
            if (offset.sqrMagnitude > hearingRangeSquared)
                continue;

            emitter.EmitSound();
            foundAudibleEmitter = true;
        }

        if (!foundAudibleEmitter)
        {
            if (SoundIndicatorManager.Instance != null)
                SoundIndicatorManager.Instance.ShowHearingPulse();
            else
                Debug.LogError("PlayerAbilities requires a SoundIndicatorManager to show hearing feedback.", this);

            Debug.LogWarning($"Cat hearing found no active sound emitters within {hearingRange} units.", this);
        }
    }
    
    private IEnumerator AbilityCooldown()
    {
        canUseAbility = false;

        yield return new WaitForSeconds(abilityCooldown);

        canUseAbility = true;
    }
    
}

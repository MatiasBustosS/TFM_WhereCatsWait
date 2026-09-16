using UnityEngine;
using UnityEngine.UI;

public class SoundIndicator : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public RectTransform RectTransform { get; private set; }

    public Vector3 WorldPosition { get; private set; }

    public bool IsFinished => currentTime <= 0f;

    private float currentTime;
    private float totalDuration;

    private Color originalColor;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();

        if (iconImage == null) iconImage = GetComponent<Image>();

        if (iconImage != null) originalColor = iconImage.color;
    }

    public void Initialize(Vector3 worldPosition, float duration, Sprite icon = null)
    {
        WorldPosition = worldPosition;

        totalDuration = duration;
        currentTime = duration;

        if (iconImage != null)
        {
            if (icon != null) iconImage.sprite = icon;

            originalColor = iconImage.color;
        }
    }

    public void Tick(float deltaTime)
    {
        currentTime -= deltaTime;

        if (iconImage == null)
            return;

        float normalizedTime = Mathf.Clamp01(currentTime / totalDuration);

        Color color = originalColor;

        color.a = normalizedTime;

        iconImage.color = color;
    }
}
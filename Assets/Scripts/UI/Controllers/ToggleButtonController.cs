using Coffee.UIEffects;
using Lux.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButtonController : MonoBehaviour
{
    public bool IsEnabledByDefault = false;
    public float ColorTransitionDuration = 0.2f;

    public UIGradient BackgroundImage;
    public Color disabledColor1, disabledColor2, enabledColor1, enabledColor2;

    [HideInInspector]
    public class OnClickEvent : UnityEvent<bool>
    { }

    public OnClickEvent onClick;
    private Animator animator;
    private bool isEnabled;
    private Color color1, color2;

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            isEnabled = value;

            if (isEnabled)
            {
                animator.SetTrigger("Enable");
            }
            else
            {
                animator.SetTrigger("Disable");
            };

            onClick.Invoke(isEnabled);
        }
    }


    private void Awake()
    {
        if (onClick == null)
            onClick = new OnClickEvent();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        IsEnabled = IsEnabledByDefault;

        SetColors();
        BackgroundImage.color1 = color1;
        BackgroundImage.color2 = color2;
    }

    private void SetColors()
    {
        if (IsEnabled)
        {
            color1 = enabledColor1;
            color2 = enabledColor2;
        }
        else
        {
            color1 = disabledColor1;
            color2 = disabledColor2;
        }
    }

    public void OnToggleButtonClicked()
    {
        IsEnabled = !IsEnabled;
        SetColors();
        StartCoroutine(ColorLerp(new Color[] { color1, color2 }, new Ref<UIGradient>(BackgroundImage)));
    }

    private IEnumerator ColorLerp(Color[] targetColors, Ref<UIGradient> targetObject)
    {
        Color initialValue1 = targetObject.Value.color1, initialValue2 = targetObject.Value.color2;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / ColorTransitionDuration)
        {
            Color newValue1 = Color.Lerp(initialValue1, targetColors[0], t);
            Color newValue2 = Color.Lerp(initialValue2, targetColors[1], t);
            targetObject.Value.color1 = newValue1;
            targetObject.Value.color2 = newValue2;
            yield return new WaitForEndOfFrame();
        }
        targetObject.Value.color1 = targetColors[0];
        targetObject.Value.color2 = targetColors[1];
    }
}

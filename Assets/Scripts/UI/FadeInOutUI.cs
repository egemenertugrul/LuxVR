using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOutUI : MonoBehaviour
{
    public float FadeDuration;
    private CanvasGroup cg;

    void OnValidate()
    {
        FadeDuration = Mathf.Clamp(FadeDuration, 0.01f, float.MaxValue); 
    }

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    private void EnableDisableInput(bool disableInput)
    {
        cg.interactable = !disableInput;
        cg.blocksRaycasts = !disableInput;
    }

    public void FadeIn(bool disableInput = false)
    {
        EnableDisableInput(disableInput);

        StartCoroutine(FadeInOut(1));
    }

    public void FadeOut(bool disableInput = false)
    {
        EnableDisableInput(disableInput);

        StartCoroutine(FadeInOut(0));
    }

    private IEnumerator FadeInOut(float trg)
    {
        var initialValue = cg.alpha;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / FadeDuration)
        {
            float newValue = Mathf.Lerp(initialValue, trg, t);
            cg.alpha = newValue;
            yield return new WaitForEndOfFrame();
        }
        cg.alpha = trg;
    }
}

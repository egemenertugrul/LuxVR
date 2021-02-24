using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ClickableImage : Selectable // MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClick;
    public override void OnPointerDown(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}

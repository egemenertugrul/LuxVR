using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// source: https://www.feelouttheform.net/unity3d-links-textmeshpro/

[RequireComponent(typeof(TMP_Text))]
public class VisitLink : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_Text pTextMeshPro = GetComponent<TMP_Text>();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, eventData.position, Lux.LuxManager.Instance.DashboardUICanvas.worldCamera);
        if (linkIndex != -1)
        { // was a link clicked?
            TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
            var linkID = linkInfo.GetLinkID();
            Application.OpenURL(linkID);
        }
    }

}
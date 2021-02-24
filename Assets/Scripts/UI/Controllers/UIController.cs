using Lux;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lux.UI
{
    public abstract class UIController : MonoBehaviour
    {
        protected LuxManager lux;
        protected UIManager uim;
        protected LocationManager lm;

        private CanvasGroup cg;

        protected void Awake()
        {
            lux = LuxManager.Instance;
            if (lux.SettingsService == null)
            {
                Debug.LogError("Settings service is not initialized.");
            }

            uim = UIManager.Instance;
            if (uim == null)
            {
                Debug.LogError("UI Manager is not initialized.");
            }


            lm = LocationManager.Instance;
            if (lm == null)
            {
                Debug.LogError("Location Manager is not initialized.");
            }

            cg = GetComponent<CanvasGroup>();
        }

        public void Enable()
        {
            cg.alpha = 1;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        public void Disable()
        {
            cg.alpha = 0.3f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        abstract internal void UpdateUI();
    }
}
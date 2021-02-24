using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lux.UI
{
    public class SettingsUIController : UIController, IModalView
    {
        public UIController TimesUIController, TemperaturesUIController, BrightnessUIController;
        public Toggle OverlayToggle, AutorunToggle;
        private Animator anim;
        void Awake()
        {
            base.Awake();
            anim = GetComponent<Animator>();
        }

        public void Show()
        {
            anim.SetTrigger("Show");
        }

        public void Hide()
        {
            anim.SetTrigger("Hide");
        }

        public void OverlayToggleValueChanged(bool isMarked)
        {
            lux.ToggleOverlay(isMarked);
            UpdateUI();
        }

        public void AutorunToggleValueChanged(bool isMarked)
        {
            lux.ToggleAutorun(isMarked);
        }

        internal override void UpdateUI()
        {
            TimesUIController.UpdateUI();
            TemperaturesUIController.UpdateUI();
            BrightnessUIController.UpdateUI();

            OverlayToggle.SetIsOnWithoutNotify(lux.IsOverlayMode);
            if (lux.IsOverlayMode)
            {
                TemperaturesUIController.Disable();
            } else
            {
                TemperaturesUIController.Enable();
            }

            AutorunToggle.SetIsOnWithoutNotify(lux.IsAutorunMode);
        }
    }
}
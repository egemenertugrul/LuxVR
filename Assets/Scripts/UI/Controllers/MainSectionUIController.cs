using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lux.UI
{
    public class MainSectionUIController : UIController
    {
        public RadialProgressBarUI radialProgressBarUI;
        public ToggleButtonController enableDisableButton;

        protected void Awake()
        {
            base.Awake();
            enableDisableButton.onClick.AddListener(OnEnableDisableButtonClicked);
        }

        public void OnEnableDisableButtonClicked(bool isEnabled)
        {
            if (isEnabled)
            {
                lux.Enable();
            }
            else
            {
                lux.Disable();
            }
        }

        public void OnCloseButtonClicked()
        {
            uim.CloseApplication();
        }

        public void OnSettingsButtonClicked()
        {
            uim.ShowSettings();
        }

        public void OnAboutButtonClicked()
        {
            uim.ShowAbout();
        }

        public void OnCyclePreviewButtonClicked()
        {
            //var currentValue = lux.IsCyclePreviewEnabled;
            //lux.IsCyclePreviewEnabled = !currentValue;
            lux.IsCyclePreviewEnabled = !lux.IsCyclePreviewEnabled;
            if (!lux.IsCyclePreviewEnabled)
            {
                radialProgressBarUI.ClockUpdateRate = -1; // reset to default
            }
            else
            {
                radialProgressBarUI.ClockUpdateRate = 0.011f; // 90hz
            }
        }
        void Update()
        {

        }

        internal override void UpdateUI()
        {
            radialProgressBarUI.UpdateAllView();
        }
    }
}
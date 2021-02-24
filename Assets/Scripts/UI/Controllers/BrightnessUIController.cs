using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lux.Extensions;
using TMPro;
using UnityEngine.UI;

namespace Lux.UI
{
    public class BrightnessUIController : UIController, ISliderUI
    {
        public TextMeshProUGUI DayBrightnessText, NightBrightnessText;
        public Slider DayBrightnessSlider, NightBrightnessSlider;

        protected void Awake()
        {
            base.Awake();
        }

        public void OnDayBrightnessChanged(float newVal)
        {
            lux.SettingsService.DayBrightness = newVal;
            UpdateUI();
        }

        public void OnNightBrightnessChanged(float newVal)
        {
            lux.SettingsService.NightBrightness = newVal;
            UpdateUI();
        }

        public void UpdateText()
        {
            DayBrightnessText.text = $"{(int)(lux.SettingsService.DayBrightness * 100)} %";
            NightBrightnessText.text = $"{(int)(lux.SettingsService.NightBrightness * 100)} %";
        }

        public void SyncSlider()
        {
            DayBrightnessSlider.SetValueWithoutNotify((float)lux.SettingsService.DayBrightness);
            NightBrightnessSlider.SetValueWithoutNotify((float)lux.SettingsService.NightBrightness);
        }

        internal override void UpdateUI()
        {
            SyncSlider();
            UpdateText();
        }

        //protected override void Update()
        //{

        //}
    }
}
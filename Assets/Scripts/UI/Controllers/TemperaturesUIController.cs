using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lux.Extensions;
using TMPro;
using UnityEngine.UI;

namespace Lux.UI
{
    public class TemperaturesUIController : UIController, ISliderUI
    {
        public TextMeshProUGUI DayTemperatureText, NightTemperatureText;
        public Slider DayTemperatureSlider, NightTemperatureSlider;

        protected void Awake()
        {
            base.Awake();
        }

        public void OnDayTemperatureChanged(float newVal)
        {
            var value = newVal.Remap(0, 1, lux.SettingsService.MinimumTemperature, lux.SettingsService.MaximumTemperature);
            lux.SettingsService.DayTemperature = value;
            UpdateUI();
        }

        public void OnNightTemperatureChanged(float newVal)
        {
            var value = lux.SettingsService.NightTemperature = newVal.Remap(0, 1, lux.SettingsService.MinimumTemperature, lux.SettingsService.MaximumTemperature);
            lux.SettingsService.NightTemperature = value;
            UpdateUI();
        }

        public void UpdateText()
        {
            DayTemperatureText.text = $"{lux.SettingsService.DayTemperature.ToString("0")} K";
            NightTemperatureText.text = $"{lux.SettingsService.NightTemperature.ToString("0")} K";
        }

        public void SyncSlider()
        {
            DayTemperatureSlider.SetValueWithoutNotify(((float) lux.SettingsService.DayTemperature).Remap(lux.SettingsService.MinimumTemperature, lux.SettingsService.MaximumTemperature, 0, 1));
            NightTemperatureSlider.SetValueWithoutNotify(((float)lux.SettingsService.NightTemperature).Remap(lux.SettingsService.MinimumTemperature, lux.SettingsService.MaximumTemperature, 0, 1));
        }

        internal override void UpdateUI()
        {
            SyncSlider();
            UpdateText();
        }

        //void Update()
        //{

        //}
    }
}
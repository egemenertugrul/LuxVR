using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lux.UI
{
    public class TimesUIController : UIController, ISliderUI
    {
        public TextMeshProUGUI SunriseText, SunsetText, TransitionText;
        public Slider SunriseSlider, SunsetSlider, TransitionSlider;

        protected void Awake()
        {
            base.Awake();
        }

        public void OnTimeSyncButtonClicked()
        {
            lm.UpdateLocation((isSuccess)=> {
                if (isSuccess)
                {
                    lux.SettingsService.IsManualSunriseSunsetEnabled = false;
                } else
                {
                    lux.SettingsService.IsManualSunriseSunsetEnabled = true;
                }

                UpdateUI();
            });
        }

        public void OnSunriseChanged(float newVal)
        {
            lux.SettingsService.IsManualSunriseSunsetEnabled = true;

            lux.SettingsService.ManualSunrise = new Domain.TimeOfDay((int)Math.Floor(newVal / 6), (int)(newVal % 6) * 10); // 00:00, 00:10 ... 23:50, 144 slider values
            UpdateText();
        }

        public void OnSunsetChanged(float newVal)
        {
            lux.SettingsService.IsManualSunriseSunsetEnabled = true;

            lux.SettingsService.ManualSunset = new Domain.TimeOfDay((int)Math.Floor(newVal / 6), (int)(newVal % 6) * 10); // 00:00, 00:10 ... 23:50 - every 10 mins, 144 slider values
            UpdateText();
        }

        public void OnTransitionChanged(float newVal)
        {
            lux.SettingsService.ConfigurationTransitionDuration = new TimeSpan((int)Math.Floor(newVal / 6), (int)(newVal % 6) * 10, 0); // 00:00, 00:10 ...
            UpdateText();
        }

        private int GetSliderValueFromTimeSpan(TimeSpan ts)
        {
            return ts.Hours * 6 + ts.Minutes / 10;
        }

        public void SyncSlider()
        {
            SunriseSlider.SetValueWithoutNotify(GetSliderValueFromTimeSpan(lux.SolarTimes.Sunrise.AsTimeSpan()));
            SunsetSlider.SetValueWithoutNotify(GetSliderValueFromTimeSpan(lux.SolarTimes.Sunset.AsTimeSpan()));
            TransitionSlider.SetValueWithoutNotify(GetSliderValueFromTimeSpan(lux.SettingsService.ConfigurationTransitionDuration));
        }

        public void UpdateText()
        {
            SunriseText.text = $"{lux.SolarTimes.Sunrise.AsTimeSpan().Hours.ToString("00")}:{lux.SolarTimes.Sunrise.AsTimeSpan().Minutes.ToString("00")}";
            SunsetText.text = $"{lux.SolarTimes.Sunset.AsTimeSpan().Hours.ToString("00")}:{lux.SolarTimes.Sunset.AsTimeSpan().Minutes.ToString("00")}";
            TransitionText.text = $"{lux.SettingsService.ConfigurationTransitionDuration.Hours.ToString("00")}:{lux.SettingsService.ConfigurationTransitionDuration.Minutes.ToString("00")}";
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
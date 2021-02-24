using Lux.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;

namespace Lux.Services
{
    public class GammaService
    {
        private readonly SettingsService _settingsService;
        public const string k_pch_SteamVR_HmdDisplayColorGainR_Float = "hmdDisplayColorGainR";
        public const string k_pch_SteamVR_HmdDisplayColorGainG_Float = "hmdDisplayColorGainG";
        public const string k_pch_SteamVR_HmdDisplayColorGainB_Float = "hmdDisplayColorGainB";
        private CVRSettings VRSettings;

        private bool _isDeviceContextsValid;
        private bool _isGammaValid;

        private ColorConfiguration? _lastConfiguration;
        private DateTimeOffset _lastUpdateTimestamp = DateTimeOffset.MinValue;

        public GammaService(SettingsService settingsService, CVRSettings ovrSettings)
        {
            _settingsService = settingsService;
            VRSettings = ovrSettings;
        }
        private bool IsSignificantChange(ColorConfiguration configuration) =>
   _lastConfiguration == null ||
   Math.Abs(configuration.Temperature - _lastConfiguration.Value.Temperature) > 15 ||
   Math.Abs(configuration.Brightness - _lastConfiguration.Value.Brightness) > 0.01;

        private bool IsGammaStale() =>
            !_isGammaValid /* ||
            _settingsService.IsGammaPollingEnabled */ &&
            (DateTimeOffset.Now - _lastUpdateTimestamp).Duration() > TimeSpan.FromSeconds(1);

        public Color GetColor(ColorConfiguration configuration)
        {
            float redColor = (float)(GetRed(configuration) * configuration.Brightness);
            float greenColor = (float)(GetGreen(configuration) * configuration.Brightness);
            float blueColor = (float)(GetBlue(configuration) * configuration.Brightness);

            return new Color(redColor, greenColor, blueColor);
        }

        public void SetGamma(ColorConfiguration configuration)
        {
            if (!IsGammaStale() && !IsSignificantChange(configuration))
                return;

            if(VRSettings == null)
            {
                Debug.LogError("VR Settings is not initialized. Make sure a compatible VR headet is plugged in.");
                return;
            }

            //EnsureValidDeviceContext();

            EVRSettingsError vrSettingsError = EVRSettingsError.None;

            Color color = GetColor(configuration);

            VRSettings.SetFloat(OpenVR.k_pch_SteamVR_Section, k_pch_SteamVR_HmdDisplayColorGainR_Float, color.r, ref vrSettingsError);
            VRSettings.SetFloat(OpenVR.k_pch_SteamVR_Section, k_pch_SteamVR_HmdDisplayColorGainG_Float, color.g, ref vrSettingsError);
            VRSettings.SetFloat(OpenVR.k_pch_SteamVR_Section, k_pch_SteamVR_HmdDisplayColorGainB_Float, color.b, ref vrSettingsError);

            if (vrSettingsError != EVRSettingsError.None)
            {
                Debug.LogError("SetFloat Error [HmdDisplayColorGain]: " + vrSettingsError.ToString());
            }

            _isGammaValid = true;
            _lastConfiguration = configuration;
            _lastUpdateTimestamp = DateTimeOffset.Now;
            //Debug.Log($"Updated gamma to {configuration}.");
        }

        private static double GetRed(ColorConfiguration configuration)
        {
            // Algorithm taken from http://tannerhelland.com/4435/convert-temperature-rgb-algorithm-code

            if (configuration.Temperature > 6600)
                return Math.Pow(configuration.Temperature / 100 - 60, -0.1332047592) * 329.698727446 / 255;

            return 1;
        }

        private static double GetGreen(ColorConfiguration configuration)
        {
            // Algorithm taken from http://tannerhelland.com/4435/convert-temperature-rgb-algorithm-code

            if (configuration.Temperature > 6600)
                return Math.Pow(configuration.Temperature / 100 - 60, -0.0755148492) * 288.1221695283 / 255;

            return (Math.Log(configuration.Temperature / 100) * 99.4708025861 - 161.1195681661) / 255;
        }

        private static double GetBlue(ColorConfiguration configuration)
        {
            // Algorithm taken from http://tannerhelland.com/4435/convert-temperature-rgb-algorithm-code

            if (configuration.Temperature >= 6600)
                return 1;

            if (configuration.Temperature <= 1900)
                return 0;

            return (Math.Log(configuration.Temperature / 100 - 10) * 138.5177312231 - 305.0447927307) / 255;
        }
    }
}
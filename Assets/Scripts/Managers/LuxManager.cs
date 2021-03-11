using Lux.Domain;
using Lux.Extensions;
using Lux.Services;
using Lux.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Lux
{
    public class LuxManager : Singleton<LuxManager>
    {
        [HideInInspector] public UnityEvent onStarted, onStopped;

        public SettingsService SettingsService { get => _settingsService; set => _settingsService = value; }
        public GammaService GammaService { get => _gammaService; set => _gammaService = value; }

        private SettingsService _settingsService = new SettingsService();
        private GammaService _gammaService;

        public float LuxConfigUpdateRate = 0.05f;
        public int ApplicationIdleFrameRate = 10;
        public int ApplicationTargetFrameRate = 91;

        public Canvas DashboardUICanvas;
        public ColorOverlay ActiveOverlay;

        [HideInInspector] public bool IsOn;
        [HideInInspector] public bool IsOverlayMode;
        [HideInInspector] public bool IsAutorunMode; // TODO: Better put them in a Config class

        //private IDisposable? _enableAfterDelayRegistration;

        [HideInInspector] public OVR_Handler OVRHandler = OVR_Handler.instance;


        private bool connectedToOpenVR;

        private bool isEnabled = false;
        public bool IsEnabled { get => isEnabled; 
            set { 
                isEnabled = value;
                ((MainSectionUIController)UIManager.Instance.MainSection).enableDisableButton.IsEnabled = isEnabled; // TODO: Refactor
            } }

        public bool IsPaused { get; private set; }

        public bool IsCyclePreviewEnabled { get; set; }

        public bool IsActive => IsEnabled && !IsPaused || IsCyclePreviewEnabled;

        public DateTimeOffset Instant { get; private set; } = DateTimeOffset.Now;

        public SolarTimes SolarTimes
        {
            get
            {
                _lastSolarTimes = !_settingsService.IsManualSunriseSunsetEnabled && _settingsService.Location.Value != null
                    ? SolarTimes.Calculate(_settingsService.Location.Value, Instant)
                    : new SolarTimes(_settingsService.ManualSunrise, _settingsService.ManualSunset);
                return _lastSolarTimes;
            }
        }

        public TimeOfDay SunriseStart => Cycle.GetSunriseStart(
            SolarTimes.Sunrise,
            _settingsService.ConfigurationTransitionDuration,
            _settingsService.ConfigurationTransitionOffset
        );

        public TimeOfDay SunriseEnd => Cycle.GetSunriseEnd(
            SolarTimes.Sunrise,
            _settingsService.ConfigurationTransitionDuration,
            _settingsService.ConfigurationTransitionOffset
        );

        public TimeOfDay SunsetStart => Cycle.GetSunsetStart(
            SolarTimes.Sunset,
            _settingsService.ConfigurationTransitionDuration,
            _settingsService.ConfigurationTransitionOffset
        );

        public TimeOfDay SunsetEnd => Cycle.GetSunsetEnd(
            SolarTimes.Sunset,
            _settingsService.ConfigurationTransitionDuration,
            _settingsService.ConfigurationTransitionOffset
        );

        public double TemperatureOffset { get; set; }

        public double BrightnessOffset { get; set; }

        public ColorConfiguration TargetConfiguration => IsActive
            ? Cycle
                .GetInterpolatedConfiguration(
                    SolarTimes,
                    _settingsService.DayConfiguration,
                    _settingsService.NightConfiguration,
                    _settingsService.ConfigurationTransitionDuration,
                    _settingsService.ConfigurationTransitionOffset,
                    Instant)
                .WithOffset(
                    TemperatureOffset,
                    BrightnessOffset)
            : _settingsService.IsDefaultToDayConfigurationEnabled
                ? _settingsService.DayConfiguration
                : ColorConfiguration.Default;

        public ColorConfiguration CurrentConfiguration { get; set; } = ColorConfiguration.Default;

        public ColorConfiguration AdjustedDayConfiguration => _settingsService.DayConfiguration.WithOffset(
            TemperatureOffset,
            BrightnessOffset
        );

        public ColorConfiguration AdjustedNightConfiguration => _settingsService.NightConfiguration.WithOffset(
            TemperatureOffset,
            BrightnessOffset
        );

        public CycleState CycleState
        {
            get
            {
                if (CurrentConfiguration != TargetConfiguration)
                {
                    return CycleState.Transition;
                }
                else if (!IsEnabled)
                {
                    return CycleState.Disabled;
                }
                else if (IsPaused)
                {
                    return CycleState.Paused;
                }
                else if (CurrentConfiguration == AdjustedDayConfiguration)
                {
                    return CycleState.Day;
                }
                else if (CurrentConfiguration == AdjustedNightConfiguration)
                {
                    return CycleState.Night;
                }
                return CycleState.Transition;
            }
        }

        private SolarTimes _lastSolarTimes;
        private bool isUpdating;
        private float frameTime;

        void Awake()
        {
            OnOverlayInvisible();

            OVRHandler.onOpenVRChange += OnOpenVRChange;
        }

        void OnOpenVRChange(bool connected)
        {
            connectedToOpenVR = connected;
            _gammaService = new GammaService(_settingsService, OVRHandler.Settings); // TODO: refactor

            //if (!connected)
            //{
            //    onSteamVRDisconnect.Invoke();
            //    ovrHandler.ShutDownOpenVR();

            if (connectedToOpenVR)
            {
                Debug.Log("Connected to OpenVR");
                LoadPlayerPreferences();

                ToggleOverlay(IsOverlayMode);
                StartUpdate();
                onStarted.Invoke();
            }
            else
            {
                Debug.Log("OpenVR Disconnected");
                StopUpdate();
                onStopped.Invoke();

                CloseApplication();
            }
        }

        //private void Start()
        //{
        //    //OnOpenVRChange(true); // TEST
        //}

        void StartUpdate()
        {
            isUpdating = true;

            //InvokeRepeating("UpdateInstant", 0f, 0.05f);
            //InvokeRepeating("UpdateConfiguration", 0f, 0.05f);
        }

        void StopUpdate()
        {
            isUpdating = false;

            //CancelInvoke("UpdateInstant");
            //CancelInvoke("UpdateConfiguration");
        }

        internal void ToggleOverlay(bool isOn)
        {
            IsOverlayMode = isOn;
            ActiveOverlay.gameObject.SetActive(IsOverlayMode);

            UpdateConfiguration();

            //if (IsOverlayMode)
            //    _gammaService.SetGamma(ColorConfiguration.Default);
        }

        internal void ToggleAutorun(bool isMarked)
        {
            IsAutorunMode = isMarked;
            OVRHandler.ToggleAutorunWithSteamVR(IsAutorunMode);
        }

        private void UpdateInstant()
        {
            // If in cycle preview mode - advance quickly until full cycle
            if (IsCyclePreviewEnabled)
            {
                // Cycle is supposed to end 1 full day past current real time
                var targetInstant = DateTimeOffset.Now + TimeSpan.FromDays(1);

                Instant = Instant.StepTo(targetInstant, TimeSpan.FromMinutes(5));
                if (Instant >= targetInstant)
                    IsCyclePreviewEnabled = false;
            }
            // Otherwise - synchronize instant with system clock
            else
            {
                Instant = DateTimeOffset.Now;
            }
        }

        private void UpdateConfiguration()
        {
            if (_gammaService == null)
            {
                return;
            }

            var isSmooth = _settingsService.IsConfigurationSmoothingEnabled && !IsCyclePreviewEnabled;

            CurrentConfiguration = isSmooth
                ? CurrentConfiguration.StepTo(TargetConfiguration, 30, 0.008)
                : TargetConfiguration;


            if (IsOverlayMode)
            {
                //ActiveOverlay.SetGamma(_gammaService.GetColor(CurrentConfiguration));

                _gammaService.SetGamma(ColorConfiguration.Default);
                ActiveOverlay.SetBrightness(CurrentConfiguration.Brightness);
            }
            else
            {
                _gammaService.SetGamma(CurrentConfiguration);
            }
        }

        public void Enable() => IsEnabled = true;

        public void Disable() => IsEnabled = false;

        //public void DisableTemporarily(TimeSpan duration)
        //{
        //    _enableAfterDelayRegistration?.Dispose();
        //    _enableAfterDelayRegistration = Timer.QueueDelayedAction(duration, Enable);
        //    IsEnabled = false;
        //}

        private void SavePlayerPreferences()
        {
            PlayerPrefs.SetString("Sunrise", SettingsService.TimeOfDaySerializer.Serialize(_lastSolarTimes.Sunrise));
            PlayerPrefs.SetString("Sunset", SettingsService.TimeOfDaySerializer.Serialize(_lastSolarTimes.Sunset));
            PlayerPrefs.SetString("Transition", _settingsService.ConfigurationTransitionDuration.ToString());

            PlayerPrefs.SetFloat("Day_Temperature", (float)_settingsService.DayTemperature);
            PlayerPrefs.SetFloat("Night_Temperature", (float)_settingsService.NightTemperature);

            PlayerPrefs.SetFloat("Day_Brightness", (float)_settingsService.DayBrightness);
            PlayerPrefs.SetFloat("Night_Brightness", (float)_settingsService.NightBrightness);

            PlayerPrefsExtensions.SetBool("Is_Enabled", IsEnabled);
            PlayerPrefsExtensions.SetBool("Is_Overlay", IsOverlayMode);
            PlayerPrefsExtensions.SetBool("Is_Autorun", IsAutorunMode);
        }

        private void LoadPlayerPreferences()
        {
            if (PlayerPrefs.HasKey("Sunrise") && PlayerPrefs.HasKey("Sunset") && PlayerPrefs.HasKey("Transition"))
            {
                _settingsService.IsManualSunriseSunsetEnabled = true;

                _settingsService.ManualSunrise = SettingsService.TimeOfDaySerializer.Deserialize(PlayerPrefs.GetString("Sunrise"));
                _settingsService.ManualSunset = SettingsService.TimeOfDaySerializer.Deserialize(PlayerPrefs.GetString("Sunset"));
                _settingsService.ConfigurationTransitionDuration = TimeSpan.Parse(PlayerPrefs.GetString("Transition"));
            }

            if (PlayerPrefs.HasKey("Day_Temperature") && PlayerPrefs.HasKey("Night_Temperature")
                && PlayerPrefs.HasKey("Day_Brightness") && PlayerPrefs.HasKey("Night_Brightness"))
            {
                _settingsService.DayTemperature = PlayerPrefs.GetFloat("Day_Temperature");
                _settingsService.NightTemperature = PlayerPrefs.GetFloat("Night_Temperature");

                _settingsService.DayBrightness = PlayerPrefs.GetFloat("Day_Brightness");
                _settingsService.NightBrightness = PlayerPrefs.GetFloat("Night_Brightness");
            }

            if (PlayerPrefs.HasKey("Is_Enabled"))
            {
                IsEnabled = PlayerPrefsExtensions.GetBool("Is_Enabled");
            }


            if (PlayerPrefs.HasKey("Is_Overlay"))
            {
                IsOverlayMode = PlayerPrefsExtensions.GetBool("Is_Overlay");
            }

            if (PlayerPrefs.HasKey("Is_Autorun"))
            {
                IsAutorunMode = PlayerPrefsExtensions.GetBool("Is_Autorun");
            }
        }

        public void OnOverlayVisible()
        {
            Application.targetFrameRate = ApplicationTargetFrameRate;
            QualitySettings.vSyncCount = 1;
        }

        public void OnOverlayInvisible()
        {
            Application.targetFrameRate = ApplicationIdleFrameRate;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            if (isUpdating)
            {
                frameTime += Time.deltaTime;
                if(frameTime > LuxConfigUpdateRate)
                {
                    frameTime = 0;
                    UpdateInstant();
                    UpdateConfiguration();

                    print(Application.targetFrameRate);
                }
            }
        }


        internal void CloseApplication()
        {
            Application.Quit();
        }

        private void OnApplicationQuit()
        {
            StopUpdate();
            SavePlayerPreferences();
            _gammaService?.SetGamma(ColorConfiguration.Default);
            if (OVRHandler.OpenVRConnected)
                OVRHandler.ShutDownOpenVR();
        }
    }
}
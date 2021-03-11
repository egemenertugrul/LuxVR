using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lux.Domain;
using Lux.Extensions;
using Lux.Converters;
using UnityEngine.UI;
using System;
using TMPro;
using System.Globalization;

namespace Lux.UI
{
    public class RadialProgressBarUI : MonoBehaviour
    {
        public GameObject LoadingBarPrefab;

        public float IconDistance, TextDistance;
        public Color sunriseColor, dayColor, sunsetColor, nightColor;
        public GameObject SunIcon, SunriseIcon, SunsetIcon, MoonIcon;
        public GameObject SunriseText, SunsetText;
        public GameObject TemperatureText, BrightnessText, ClockText;

        protected LuxManager lux;

        private TimeOfDay sunriseStart, sunriseEnd, sunsetStart, sunsetEnd, dayTime, nightTime;
        private GameObject sunriseBar, dayTimeBar, sunsetBar, nightTimeBar;

        public GameObject ClockHand;

        private float _defaultClockUpdateRate = 1f;
        private float _clockUpdateRate;

        public float ClockUpdateRate
        {
            get => _clockUpdateRate;
            set
            {
                if (value <= 0)
                {
                    _clockUpdateRate = _defaultClockUpdateRate;
                }
                else
                {
                    _clockUpdateRate = value;
                }
                RestartClockRoutine();

                Invoke("UpdateClock", 0.06f); // lux.UpdateInstant + 0.01, TODO: Refactor, add constants
            }
        }

        private Coroutine _clockRoutine;

        void Awake()
        {
            lux = LuxManager.Instance; // TODO: refactor

            sunriseBar = Instantiate(LoadingBarPrefab, transform);
            dayTimeBar = Instantiate(LoadingBarPrefab, transform);
            sunsetBar = Instantiate(LoadingBarPrefab, transform);
            nightTimeBar = Instantiate(LoadingBarPrefab, transform);

            sunriseBar.transform.SetAsFirstSibling(); // Execution order is important
            sunsetBar.transform.SetAsFirstSibling();
            dayTimeBar.transform.SetAsFirstSibling();
            nightTimeBar.transform.SetAsFirstSibling();

            sunriseBar.name = "SunriseBar";
            dayTimeBar.name = "DayTimeBar";
            sunsetBar.name = "SunsetBar";
            nightTimeBar.name = "NightTimeBar";

            UpdateAllView();

            //InvokeRepeating("UpdateClock", 0, 1f);
            RestartClockRoutine();
        }

        private void RestartClockRoutine()
        {
            if (_clockRoutine != null)
            {
                StopCoroutine(_clockRoutine);
            }
            _clockRoutine = StartCoroutine(UpdateClockRoutine());
        }

        IEnumerator UpdateClockRoutine()
        {
            while (true)
            {
                UpdateClock();
                yield return new WaitForSeconds(ClockUpdateRate);
            }
        }

        public void UpdateAllView()
        {
            sunriseStart = lux.SunriseStart;
            sunriseEnd = lux.SunriseEnd;
            sunsetStart = lux.SunsetStart;
            sunsetEnd = lux.SunsetEnd;

            UpdateArc(sunriseBar, SunriseIcon, sunriseStart, sunriseEnd, sunriseColor);
            UpdateArc(dayTimeBar, SunIcon, sunriseEnd, sunsetEnd, dayColor);
            UpdateArc(sunsetBar, SunsetIcon, sunsetStart, sunsetEnd, sunsetColor);
            UpdateArc(nightTimeBar, MoonIcon, sunsetEnd, sunriseStart, nightColor);

            UpdateFloatingText(SunriseIcon, SunriseText, sunriseEnd);
            UpdateFloatingText(SunsetIcon, SunsetText, sunsetStart);

            UpdateText(TemperatureText, $"{lux.CurrentConfiguration.Temperature.ToString("0")} K");
            UpdateText(BrightnessText, $"{(lux.CurrentConfiguration.Brightness * 100).ToString("0")}%");
        }

        private void UpdateArc(GameObject progressBar, GameObject icon, TimeOfDay beginTime, TimeOfDay endTime, Color color)
        {
            CalculateRotation(progressBar, beginTime);
            CalculateFillAmount(progressBar, beginTime, endTime);
            UpdateIcon(icon, beginTime, endTime);
            progressBar.GetComponent<Image>().color = color;
        }

        private void UpdateText(GameObject textObject, string textString)
        {
            textObject.GetComponent<TextMeshProUGUI>().text = textString;
        }

        private void UpdateFloatingText(GameObject icon, GameObject text, TimeOfDay time)
        {
            float iconRotation = icon.transform.localEulerAngles.z;
            float angle = Mathf.Deg2Rad * (iconRotation + 90);
            float distance = TextDistance + IconDistance;
            text.transform.localPosition = new Vector3(Mathf.Cos(angle) * distance, Mathf.Clamp(Mathf.Sin(angle), -0.9f, 0.9f) * distance, 0);
            UpdateText(text, time.ToString());
        }

        private void UpdateIcon(GameObject icon, TimeOfDay beginTime, TimeOfDay endTime)
        {
            double beginTimeFraction = beginTime.DayFraction, endTimeFraction = endTime.DayFraction;
            if (endTimeFraction - beginTimeFraction < 0)
            {
                endTimeFraction = endTimeFraction + 1;
            }

            double rotation = FractionToDegreesConverter.Instance.Convert((endTimeFraction + beginTimeFraction) / 2);
            Vector3 rotationVec = new Vector3(0, 0, (float)-rotation + 180);
            icon.transform.rotation = Quaternion.Euler(rotationVec);

            float angle = Mathf.Deg2Rad * (rotationVec.z + 90);
            icon.transform.localPosition = new Vector3(Mathf.Cos(angle) * IconDistance, Mathf.Sin(angle) * IconDistance, 0);
        }

        //private void 

        private void CalculateRotation(GameObject progressBar, TimeOfDay beginTime)
        {
            double rotation = FractionToDegreesConverter.Instance.Convert(beginTime.DayFraction);
            progressBar.transform.rotation = Quaternion.Euler(0, 0, (float)-rotation);
        }

        private void CalculateFillAmount(GameObject progressBar, TimeOfDay beginTime, TimeOfDay endTime)
        {
            double beginTimeFraction = beginTime.DayFraction, endTimeFraction = endTime.DayFraction;
            if (endTimeFraction - beginTimeFraction < 0)
            {
                endTimeFraction = endTimeFraction + 1;
            }
            progressBar.GetComponent<Image>().fillAmount = (float)(endTimeFraction - beginTimeFraction);
        }

        private void UpdateClock()
        {
            ClockText.GetComponent<TextMeshProUGUI>().text = lux.Instant.DateTime.ToString("HH:mm");
            CalculateRotation(ClockHand, new TimeOfDay(lux.Instant));
        }

        void Update()
        {

        }
    }
}
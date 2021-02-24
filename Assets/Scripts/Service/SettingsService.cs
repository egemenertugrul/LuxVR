using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Lux.Domain;

namespace Lux.Services
{
    public class SettingsService
    {
        // General
        public readonly float MinimumTemperature = 1900f;
        public readonly float MaximumTemperature = 6600f;

        public double NightTemperature
        {
            get => NightConfiguration.Temperature;
            set
            {
                NightConfiguration = new ColorConfiguration(value, NightBrightness);

                if (NightTemperature > DayTemperature)
                    DayTemperature = NightTemperature;
            }
        }

        public double DayTemperature
        {
            get => DayConfiguration.Temperature;
            set
            {
                DayConfiguration = new ColorConfiguration(value, DayBrightness);

                if (DayTemperature < NightTemperature)
                    NightTemperature = DayTemperature;
            }
        }

        public double NightBrightness
        {
            get => NightConfiguration.Brightness;
            set
            {
                NightConfiguration = new ColorConfiguration(NightTemperature, value);

                if (NightBrightness > DayBrightness)
                    DayBrightness = NightBrightness;
            }
        }

        public double DayBrightness
        {
            get => DayConfiguration.Brightness;
            set
            {
                DayConfiguration = new ColorConfiguration(DayTemperature, value);

                if (DayBrightness < NightBrightness)
                    NightBrightness = DayBrightness;
            }
        }

        public ColorConfiguration NightConfiguration { get; set; } = new ColorConfiguration(3900, 0.85);

        public ColorConfiguration DayConfiguration { get; set; } = new ColorConfiguration(6600, 1);

        public TimeSpan ConfigurationTransitionDuration { get; set; } = TimeSpan.FromMinutes(40);

        public double ConfigurationTransitionOffset { get; set; }

        // Location

        public GeoLocation? Location { get => LocationManager.Instance.Location; }

        public bool IsManualSunriseSunsetEnabled { get; set; } = true;

        [JsonProperty("ManualSunriseTime"), JsonConverter(typeof(TimeOfDayJsonConverter))]
        public TimeOfDay ManualSunrise { get; set; } = new TimeOfDay(07, 20);

        [JsonProperty("ManualSunsetTime"), JsonConverter(typeof(TimeOfDayJsonConverter))]
        public TimeOfDay ManualSunset { get; set; } = new TimeOfDay(16, 30);

        //public bool IsAutoStartEnabled { get; set; }

        //public bool IsAutoUpdateEnabled { get; set; } = true;

        public bool IsDefaultToDayConfigurationEnabled { get; set; }

        public bool IsConfigurationSmoothingEnabled { get; set; } = true;
        private class TimeOfDayJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(TimeOfDay);

            public override void WriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer)
            {
                if (value is TimeOfDay timeOfDay)
                    writer.WriteValue(timeOfDay.AsTimeSpan());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var raw = (string)reader.Value;
                return TimeOfDay.TryParse(raw) ?? default;
            }
        }

        public static class TimeOfDaySerializer
        {
            public static string Serialize(TimeOfDay value)
            {
                return value.AsTimeSpan().ToString();
            }

            public static TimeOfDay Deserialize(string value)
            {
                return TimeOfDay.TryParse(value).GetValueOrDefault();
            }
        }

        public bool IsGammaPollingEnabled { get; set; }
    }
}
using Lux.Domain;
using Lux.Domain.Internal.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class LocationManager : Singleton<LocationManager>
{
    public GeoLocation? Location { get; set; }

    public bool IsSuccess { get; private set; }

    public bool IsBusy { get; private set; }

    public bool CanAutoDetectLocation => !IsBusy;

    //private readonly HttpClient _httpClient;

    public void UpdateLocation(Action<bool> callback)
    {
        if (CanAutoDetectLocation)
            StartCoroutine(AutoDetectLocation(callback));
    }

    private IEnumerator GetLocationAsync(Action<bool, GeoLocation> callback)
    {
        const string url = "http://ip-api.com/json";
        var www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("An error occurred while fetching location.");
            callback(false, default(GeoLocation));
        }
        else
        {
            var jsonText = www.text;
            var json = JObject.Parse(jsonText);

            var latitude = json["lat"].ToObject<double>();
            var longitude = json["lon"].ToObject<double>();
            callback(true, new GeoLocation(latitude, longitude));
        }
        yield return null;
    }


    public IEnumerator AutoDetectLocation(Action<bool> callback = null)
    {
        IsBusy = true;
        IsSuccess = false;

        yield return GetLocationAsync((isSuccess, retVal) => { IsSuccess = isSuccess; Location = retVal; });

        callback?.Invoke(IsSuccess);

        IsBusy = false;
    }

    //public async Task<GeoLocation> GetLocationAsync(string query)
    //{
    //    var queryEncoded = WebUtility.UrlEncode(query);

    //    var url = $"https://nominatim.openstreetmap.org/search?q={queryEncoded}&format=json";
    //    var json = await _httpClient.GetJsonAsync<JObject>(url);

    //    var firstLocationJson = json.First; // TODO: check

    //    var latitude = firstLocationJson["lat"].ToObject<double>(); // .GetDoubleOrCoerce();
    //    var longitude = firstLocationJson["lon"].ToObject<double>();

    //    return new GeoLocation(latitude, longitude);
    //}


    //public string? LocationQuery { get; set; }


    //public bool CanSetLocation =>
    //    !IsBusy &&
    //    !string.IsNullOrWhiteSpace(LocationQuery) &&
    //    LocationQuery != Location?.ToString();

    //public IEnumerator SetLocation()
    //{
    //    if (string.IsNullOrWhiteSpace(LocationQuery))
    //        yield break;

    //    IsBusy = true;
    //    IsLocationError = false;

    //    try
    //    {
    //        Location =
    //            //GeoLocation.TryParse(LocationQuery) ??
    //            //await _locationProvider.GetLocationAsync(LocationQuery);
    //            await _locationProvider.GetLocationAsync(LocationQuery);
    //    }
    //    catch
    //    {
    //        IsLocationError = true;
    //    }
    //    finally
    //    {
    //        IsBusy = false;
    //    }
    //}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lux.Extensions
{
    public static class PlayerPrefsExtensions
    {
        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, (value ? 1 : 0));
        }

        public static bool GetBool(string key)
        {
            return PlayerPrefs.GetInt(key) != 0;
        }
    }
}

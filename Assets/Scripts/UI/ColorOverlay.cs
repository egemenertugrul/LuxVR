using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lux
{
    public class ColorOverlay : MonoBehaviour
    {
        //private MeshRenderer mr;
        private Unity_Overlay uo;

        void Awake()
        {
            //mr = GetComponent<MeshRenderer>();
            uo = GetComponent<Unity_Overlay>();
        }

        void Update()
        {

        }

        public void SetBrightness(double brightness)
        {
            uo.SetOpacity(1 - (float) brightness + 0.001f);
        }

        //public void SetGamma(Color color)
        //{
        //    uo.colorTint = color;
        //    //mr.material.SetColor(color);
        //}
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lux.UI
{
    public class AboutUIController : UIController, IModalView
    {
        private Animator anim;
        void Awake()
        {
            base.Awake();
            anim = GetComponent<Animator>();
        }

        public void Show()
        {
            anim.SetTrigger("Show");
        }

        public void Hide()
        {
            anim.SetTrigger("Hide");
        }


        internal override void UpdateUI()
        {
        }
    }
}
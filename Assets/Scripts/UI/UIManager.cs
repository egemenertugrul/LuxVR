using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lux.UI
{
    public class UIManager : Singleton<UIManager>
    {
        public Unity_Overlay DashboardOverlay;

        public UIController MainSection, SettingsSection, AboutSection;
        public CanvasGroup ModalBackground;

        private IModalView activeModalView;

        private LuxManager lux;
        private bool isUpdating;

        private FadeInOutUI mb_fio;
        private ClickableImage mb_ci;

        void Awake()
        {
            lux = LuxManager.Instance;
            lux.onStarted.AddListener(() =>
            { // Update UI once
                MainSection.UpdateUI();
                SettingsSection.UpdateUI();
                isUpdating = true;
            });

            lux.onStopped.AddListener(() =>
            {
                isUpdating = false;
            });


            mb_fio = ModalBackground.GetComponent<FadeInOutUI>();
            mb_ci = ModalBackground.GetComponent<ClickableImage>();
            mb_ci.onClick.AddListener(() =>
            {
                HideModalView(); // can refactor here, decouple from specifically SettingsUIController
            });
            mb_fio.FadeOut(true);
        }

        internal void CloseApplication()
        {
            lux.CloseApplication();
        }

        public void ShowSettings()
        {
            activeModalView = (SettingsUIController)SettingsSection;
            ShowModalView();
        }

        public void ShowAbout()
        {
            activeModalView = (AboutUIController)AboutSection;
            ShowModalView();
        }

        public void ShowModalView()
        {
            mb_fio.FadeIn(false);
            activeModalView.Show();

            //isExpanded = true;
            //ExpandView();
        }

        public void HideModalView()
        {
            mb_fio.FadeOut(true);
            activeModalView?.Hide();
            activeModalView = null;
        }

        private void Update()
        {
            if (isUpdating)
            {
                MainSection.UpdateUI();
            }
        }
    }
}
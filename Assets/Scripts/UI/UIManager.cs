using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lux.UI
{
    public class UIManager : Singleton<UIManager>
    {
        public Unity_Overlay DashboardOverlay;
        //public Vector2 ShrunkenSize = new Vector2(60, 65), ExpandedSize = new Vector2(115.55f, 65);
        //public float ViewSizeTransitionDuration = 0.2f;
        //public int PreferredResolutionHeight = 720;

        public UIController MainSection, SettingsSection, AboutSection;
        public CanvasGroup ModalBackground;

        private IModalView activeModalView;

        private LuxManager lux;
        private bool isUpdating;

        private FadeInOutUI mb_fio;
        private ClickableImage mb_ci;
        //private bool isExpanded;


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
            //ShrinkView(() =>
            //{
            //    isExpanded = false;
            //});
        }

        //public void ExpandView(Action callback = null)
        //{
        //    if (isExpanded)
        //    {
        //        return;
        //    }
        //    StartCoroutine(ChangeViewSizeCoroutine(true, ExpandedSize, callback));
        //}

        //public void ShrinkView(Action callback = null)
        //{
        //    if (!isExpanded)
        //    {
        //        return;
        //    }
        //    StartCoroutine(ChangeViewSizeCoroutine(false, ShrunkenSize, callback));
        //}

        //private int WidthFromAspectRatio(float ar, int height)
        //{
        //    return (int)Mathf.Floor(ar * height);
        //}

        //private IEnumerator ChangeViewSizeCoroutine(bool expand, Vector2 targetSize, Action callback)
        //{
        //    Vector2 initialValue;

        //    if (expand)
        //    {
        //        initialValue = ShrunkenSize;
        //    }
        //    else
        //    {
        //        initialValue = ExpandedSize;
        //    }

        //    for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / ViewSizeTransitionDuration)
        //    {
        //        Vector2 newSize = Vector2.Lerp(initialValue, targetSize, t);

        //        DashboardOverlay.renderTexWidthOverride = WidthFromAspectRatio((newSize.x / newSize.y), PreferredResolutionHeight);
        //        DashboardOverlay.renderTexHeightOverride = PreferredResolutionHeight;
        //        DashboardOverlay.ResetTexture();
        //        yield return new WaitForEndOfFrame();
        //    }

        //    DashboardOverlay.renderTexWidthOverride = WidthFromAspectRatio((targetSize.x / targetSize.y), PreferredResolutionHeight);
        //    DashboardOverlay.renderTexHeightOverride = PreferredResolutionHeight;
        //    DashboardOverlay.ResetTexture();

        //    callback?.Invoke();
        //}


        private void Update()
        {
            if (isUpdating)
            {
                MainSection.UpdateUI();
                //SettingsSection.Update();
            }
        }
    }
}
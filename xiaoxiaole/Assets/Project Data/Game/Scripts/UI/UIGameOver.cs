using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using System.Collections.Generic;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [SerializeField] RectTransform safeAreaRectTransform;
        
        [SerializeField] UIScaleAnimation levelFailed;
        [SerializeField] UIFadeAnimation backgroundFade;

        [SerializeField] Button menuButton;
        [SerializeField] Button replayButton;
        [SerializeField] Button reviveButton;

        [SerializeField] UIScaleAnimation menuButtonScalable;
        [SerializeField] UIScaleAnimation replayButtonScalable;
        [SerializeField] UIScaleAnimation reviveButtonScalable;

        [SerializeField] LivesIndicator livesIndicator;
        [SerializeField] AddLivesPanel addLivesPanel;

        private TweenCase continuePingPongCase;
        public string clickid;
        private StarkAdManager starkAdManager;
        public override void Initialise()
        {
            menuButton.onClick.AddListener(MenuButton);
            replayButton.onClick.AddListener(ReplayButton);
            reviveButton.onClick.AddListener(ReviveButton);

            LivesManager.AddIndicator(livesIndicator);
            NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            levelFailed.Hide(immediately: true);
            menuButtonScalable.Hide(immediately: true);
            replayButtonScalable.Hide(immediately: true);
            reviveButtonScalable.Hide(immediately: true);

            float fadeDuration = 0.3f;
            backgroundFade.Show(fadeDuration);

            Tween.DelayedCall(fadeDuration * 0.8f, delegate
            {
                levelFailed.Show();
                ShowInterstitialAd("1lcaf5895d5l1293dc",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
                menuButtonScalable.Show(scaleMultiplier: 1.05f, delay: 0.75f);
                replayButtonScalable.Show(scaleMultiplier: 1.05f, delay: 0.75f);
                reviveButtonScalable.Show(scaleMultiplier: 1.05f, delay: 0.25f);

                continuePingPongCase = reviveButtonScalable.RectTransform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                UIController.OnPageOpened(this);
            });

        }
        /// <summary>
        /// 播放插屏广告
        /// </summary>
        /// <param name="adId"></param>
        /// <param name="errorCallBack"></param>
        /// <param name="closeCallBack"></param>
        public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
        {
            starkAdManager = StarkSDK.API.GetStarkAdManager();
            if (starkAdManager != null)
            {
                var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
                mInterstitialAd.Load();
                mInterstitialAd.Show();
            }
        }
        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(0.3f);

            Tween.DelayedCall(0.3f, delegate
            {

                if (continuePingPongCase != null && continuePingPongCase.IsActive)
                    continuePingPongCase.Kill();

                UIController.OnPageClosed(this);
            });
        }

        #endregion

        #region Buttons 

        private void ReviveButton()
        {
            ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {

                    AudioController.PlaySound(AudioController.Sounds.buttonSound);
                    ReviveCallback(true);


                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
            
            //AdsManager.ShowRewardBasedVideo(ReviveCallback);
        }

        private void ReviveCallback(bool watchedRV)
        {
            //if (!watchedRV) return;

            UIController.HidePage<UIGameOver>();
            UIController.ShowPage<UIGame>();

            GameController.Revive();
        }

        private void ReplayButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);


            if (LivesManager.Lives > 0)
            {
                LivesManager.RemoveLife();

                UIController.HidePage<UIGameOver>();
                GameController.ReplayLevel();
            }
            else
            {
                addLivesPanel.Show();
            }
        }

        private void MenuButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<UIGameOver>(() =>
            {
                GameController.ReturnToMenu();
            });
        }

        #endregion
        public void getClickid()
        {
            var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
            if (launchOpt.Query != null)
            {
                foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                    if (kv.Value != null)
                    {
                        Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                        if (kv.Key.ToString() == "clickid")
                        {
                            clickid = kv.Value.ToString();
                        }
                    }
                    else
                    {
                        Debug.Log(kv.Key + "<-参数-> " + "null ");
                    }
            }
        }

        public void apiSend(string eventname, string clickid)
        {
            TTRequest.InnerOptions options = new TTRequest.InnerOptions();
            options.Header["content-type"] = "application/json";
            options.Method = "POST";

            JsonData data1 = new JsonData();

            data1["event_type"] = eventname;
            data1["context"] = new JsonData();
            data1["context"]["ad"] = new JsonData();
            data1["context"]["ad"]["callback"] = clickid;

            Debug.Log("<-data1-> " + data1.ToJson());

            options.Data = data1.ToJson();

            TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
               response => { Debug.Log(response); },
               response => { Debug.Log(response); });
        }


        /// <summary>
        /// </summary>
        /// <param name="adId"></param>
        /// <param name="closeCallBack"></param>
        /// <param name="errorCallBack"></param>
        public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
        {
            starkAdManager = StarkSDK.API.GetStarkAdManager();
            if (starkAdManager != null)
            {
                starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
            }
        }
    }
}
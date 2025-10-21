using System;
using System.Collections.Generic;
using UnityEngine;
using AppHarbrSDK.ThirdParty.MiniJson;
using AppHarbrSDK.Internal;
namespace AppHarbrSDK
{
    public class AppHarbrSdkCallbacks : MonoBehaviour
    {
#if UNITY_EDITOR
    private static AppHarbrSdkCallbacks instance;
#endif

        public static AppHarbrSdkCallbacks Instance
        {
#if UNITY_EDITOR
        get
        {
            if (instance != null) return instance;

            instance = new GameObject("AppHarbrSdkCallbacks", typeof(AppHarbrSdkCallbacks)).GetComponent<AppHarbrSdkCallbacks>();
            DontDestroyOnLoad(instance);

            return instance;
        }
#else
            get; private set;
#endif
        }

        public static event Action<string> OnAppHarbrInitializationComplete;

        public static event Action<AHIncidentData> OnAdBlockedEvent;
        public static event Action<AHIncidentData> OnAdIncidentEvent;
        public static event Action<AHIncidentData> OnAdAnalyzedEvent;

#if !UNITY_EDITOR
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
#endif

        internal void HandleBackgroundCallback(string propsStr)
        {
            try
            {
                ForwardEvent(propsStr);
            }
            catch (Exception exception)
            {
                var eventProps = Json.Deserialize(propsStr) as Dictionary<string, object>;
                if (eventProps == null) return;

                var eventName = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "name", "");
                Debug.Log("Unable to notify ad delegate due to an error in the publisher callback '" + eventName + "' due to exception: " + exception.Message);
                Debug.LogException(exception);
            }
        }

        internal void ForwardEvent(string eventPropsStr)
        {
            var eventProps = Json.Deserialize(eventPropsStr) as Dictionary<string, object>;
            if (eventProps == null)
            {
                Debug.Log("Failed to forward event for serialized event data: " + eventPropsStr);
                return;
            }

            var eventName = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "name", "");

            if (eventName == "OnSdkInitializedFailureEvent")
            {
                var failureReason = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "failureReason", "");
                MainThreadDispatcher.InvokeOnMainThread(() => OnAppHarbrInitializationComplete?.Invoke(failureReason));
            }
            else if (eventName == "OnSdkInitializedSuccessEvent")
            {
                ProcessWatchAppAds();
                MainThreadDispatcher.InvokeOnMainThread(() => OnAppHarbrInitializationComplete?.Invoke(null));
            }
            else if (eventName == "OnAdBlockEvent")
            {
                if (OnAdBlockedEvent == null)
                {
                    return;
                }

                var adUnitId = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "adUnitId", "");
                var adFormatIntValue = AppHarbrSdkUtils.GetIntFromDictionary(eventProps, "adFormat");
                var adFormatEnumValue = (AHAdFormat)Enum.ToObject(typeof(AHAdFormat), adFormatIntValue);
                var adBlockReasons = AppHarbrSdkUtils.GetListFromDictionary(eventProps, "reasons", null);
                var shouldLoadNewAd = AppHarbrSdkUtils.GetBoolFromDictionary(eventProps, "shouldLoadNewAd", false);
                List<string> blockReasonsList = adBlockReasons.ConvertAll(obj => obj.ToString());
                var userDefineApp = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "userDefineApp", "");
                var userDefineDomain = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "userDefineDomain", "");

                var ahIncidentData = new AHIncidentData(
                    unitId: adUnitId,
                    adNetwork: AHAdSdk.None,
                    creativeId: "",
                    adFormat: adFormatEnumValue,
                    shouldLoadNewAd: shouldLoadNewAd,
                    blockReasons: blockReasonsList,
                    reportReasons: null,
                    userDefinedApplication: userDefineApp,
                    userDefinedDomain: userDefineDomain,
                    analyzeResult: AHAdAnalyzedResult.NoResultAnalyzed
                );
                MainThreadDispatcher.InvokeOnMainThread(() => OnAdBlockedEvent?.Invoke(ahIncidentData));
            }
            else if (eventName == "OnAdIncidentEvent")
            {
                if (OnAdIncidentEvent == null)
                {
                    return;
                }

                var adUnitId = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "adUnitId", "");
                var adNetworkIntValue = AppHarbrSdkUtils.GetIntFromDictionary(eventProps, "adNetwork");
                var adNetworkEnumValue = (AHAdSdk)Enum.ToObject(typeof(AHAdSdk), adNetworkIntValue);
                var creativeId = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "creativeId", "");
                var adFormatIntValue = AppHarbrSdkUtils.GetIntFromDictionary(eventProps, "adFormat");
                var adFormatEnumValue = (AHAdFormat)Enum.ToObject(typeof(AHAdFormat), adFormatIntValue);
                var adBlockReasons = AppHarbrSdkUtils.GetListFromDictionary(eventProps, "blockReasons", null);
                List<string> blockReasonsList = adBlockReasons.ConvertAll(obj => obj.ToString());
                var adReportReasons = AppHarbrSdkUtils.GetListFromDictionary(eventProps, "reportReasons", null);
                List<string> reportReasonsList = adReportReasons.ConvertAll(obj => obj.ToString());
                var shouldLoadNewAd = AppHarbrSdkUtils.GetBoolFromDictionary(eventProps, "shouldLoadNewAd", false);
                var userDefineApp = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "userDefineApp", "");
                var userDefineDomain = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "userDefineDomain", "");

                var ahIncidentData = new AHIncidentData(
                    unitId: adUnitId,
                    adNetwork: adNetworkEnumValue,
                    creativeId: creativeId,
                    adFormat: adFormatEnumValue,
                    shouldLoadNewAd: shouldLoadNewAd,
                    blockReasons: blockReasonsList,
                    reportReasons: reportReasonsList,
                    userDefinedApplication: userDefineApp,
                    userDefinedDomain: userDefineDomain,
                    analyzeResult: AHAdAnalyzedResult.NoResultAnalyzed
                );

                MainThreadDispatcher.InvokeOnMainThread(() => OnAdIncidentEvent?.Invoke(ahIncidentData));
            }
            else if (eventName == "OnAdAnalyzedEvent")
            {
                if (OnAdAnalyzedEvent == null)
                {
                    return;
                }

                var adUnitId = AppHarbrSdkUtils.GetStringFromDictionary(eventProps, "adUnitId", "");
                var adNetworkIntValue = AppHarbrSdkUtils.GetIntFromDictionary(eventProps, "adNetwork");
                var adNetworkEnumValue = (AHAdSdk)Enum.ToObject(typeof(AHAdSdk), adNetworkIntValue);
                var adFormatIntValue = AppHarbrSdkUtils.GetIntFromDictionary(eventProps, "adFormat");
                var adFormatEnumValue = (AHAdFormat)Enum.ToObject(typeof(AHAdFormat), adFormatIntValue);
                var resultIntValue = AppHarbrSdkUtils.GetIntFromDictionary(eventProps, "analyzedResult");
                var resultEnumValue = (AHAdAnalyzedResult)Enum.ToObject(typeof(AHAdAnalyzedResult), resultIntValue);

                var ahIncidentData = new AHIncidentData(
                    unitId: adUnitId,
                    adNetwork: adNetworkEnumValue,
                    creativeId: "",
                    adFormat: adFormatEnumValue,
                    shouldLoadNewAd: false,
                    blockReasons: null,
                    reportReasons: null,
                    userDefinedApplication: "",
                    userDefinedDomain: "",
                    analyzeResult: resultEnumValue
                );
                MainThreadDispatcher.InvokeOnMainThread(() => OnAdAnalyzedEvent?.Invoke(ahIncidentData));

            }
        }

        public void ProcessWatchAppAds()
        {
            var watchAppAds = AppHarbr.SdkConfiguration?.WatchAppHarbrAds;

            if (watchAppAds == null) return;

            ProcessBannerAds(watchAppAds.BannerAdUnitIds);
            ProcessMRECAds(watchAppAds.MRecAdUnitIds);
            ProcessFullScreenAds(watchAppAds.InterstitialAdUnitIds, AppHarbr.WatchInterstitial);
            ProcessFullScreenAds(watchAppAds.RewardedAdUnitIds, AppHarbr.WatchRewarded);
            ProcessFullScreenAds(watchAppAds.RewardedInterstitialAdUnitIds, AppHarbr.WatchRewardedInterstitial);
        }

        private void ProcessBannerAds(List<WatchAppHarbrBannerAd> bannerAds)
        {
            if (bannerAds == null || bannerAds.Count == 0) return;

            foreach (var ad in bannerAds)
            {
                if (string.IsNullOrEmpty(ad.AdUnitId)) continue;

                if (ad.PositionX >= 0.0f && ad.PositionY >= 0.0f)
                {
                    AppHarbr.WatchBanner(ad.AdUnitId, ad.PositionX, ad.PositionY);
                }
                else if (!string.IsNullOrEmpty(ad.BannerPosition))
                {
                    AppHarbr.WatchBanner(ad.AdUnitId, ad.BannerPosition);
                }
                else
                {
                    AppHarbr.WatchBanner(ad.AdUnitId);
                }
            }
        }

        private void ProcessMRECAds(List<WatchAppHarbrBannerAd> mrecAds)
        {
            if (mrecAds == null || mrecAds.Count == 0) return;

            foreach (var ad in mrecAds)
            {
                if (string.IsNullOrEmpty(ad.AdUnitId)) continue;

                if (ad.PositionX >= 0.0f && ad.PositionY >= 0.0f)
                {
                    AppHarbr.WatchMRec(ad.AdUnitId, ad.PositionX, ad.PositionY);
                }
                else if (!string.IsNullOrEmpty(ad.BannerPosition))
                {
                    AppHarbr.WatchMRec(ad.AdUnitId, ad.BannerPosition);
                }
            }
        }

        private void ProcessFullScreenAds(List<string> adUnitIds, Action<string> adAction)
        {
            if (adUnitIds == null || adUnitIds.Count == 0) return;

            foreach (var adUnitId in adUnitIds)
            {
                if (!string.IsNullOrEmpty(adUnitId))
                {
                    adAction.Invoke(adUnitId);
                }
            }
        }
    }
}

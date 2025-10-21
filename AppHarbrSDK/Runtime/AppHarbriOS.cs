using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOT;
using System.Runtime.InteropServices;
using System;
using AppHarbrSDK.ThirdParty.MiniJson;

namespace AppHarbrSDK
{
    public class AppHarbriOS
    {

#if UNITY_IOS

    private delegate void AHUnityBackgroundCallback(string args);
    #region Initialization

    [DllImport("__Internal")]
    private static extern void appHarbrInitializeSdk(int mediationSdk, string apiKey);

    public static void Initialize(AHAdSdk mediationSdk, AHSdkConfiguration sdkConfiguration)
    {
       if(!sdkConfiguration.HasIOSAPIKey()){
            return;
       }

        AppHarbr.SdkConfiguration = sdkConfiguration;
        appHarbrSetBackgroundCallback(BackgroundCallback);

        SetMuted(sdkConfiguration.MuteAd);
        int interstitialTimeLimit = sdkConfiguration.InterstitialAdTimeLimit;
        if (sdkConfiguration.LimitFullscreenAdsInSeconds > 0) {
          interstitialTimeLimit = sdkConfiguration.LimitFullscreenAdsInSeconds;
        }

        SetInterstitialAdTimeLimit(interstitialTimeLimit);

        SetRewardedAdTimeLimit(sdkConfiguration.RewardedAdTimeLimit);

        if (sdkConfiguration.TargetedAdNetworks != null) {
            int[] array = Array.ConvertAll(sdkConfiguration.TargetedAdNetworks, value => (int) value);
            string arrayString = string.Join(",", array);
            SetTargetedAdNetworks(arrayString);
        }

        if (sdkConfiguration.SpecificAdNetworkToLimitTime != null) {
            int[] array = Array.ConvertAll(sdkConfiguration.SpecificAdNetworkToLimitTime, value => (int) value);
            string arrayString = string.Join(",", array);
            SetSpecificAdNetworksForTimeout(arrayString);
        }

        if (sdkConfiguration.SpecificAdNetworkToLimitInterstitialTime != null) {
            int[] array = Array.ConvertAll(sdkConfiguration.SpecificAdNetworkToLimitInterstitialTime, value => (int) value);
            string arrayString = string.Join(",", array);
            SetSpecificAdNetworksForTimeout(arrayString);
        }

        if (sdkConfiguration.SpecificAdNetworkToLimitRewardedTime != null) {
            int[] array = Array.ConvertAll(sdkConfiguration.SpecificAdNetworkToLimitRewardedTime, value => (int) value);
            string arrayString = string.Join(",", array);
            SetSpecificAdNetworksForRewardedTimeout(arrayString);
        }

        if (sdkConfiguration.AhSdkDebug != null) {
            SetDebug(sdkConfiguration.AhSdkDebug.IsDebug);
        }

        if (sdkConfiguration.InterstitialTimeLimitConfig != null) {
            SetTimeLimitConfig(sdkConfiguration.InterstitialTimeLimitConfig);
        }

        if (sdkConfiguration.RewardedTimeLimitConfig != null) {
            SetTimeLimitConfig(sdkConfiguration.RewardedTimeLimitConfig);
        }

        SetUnityVersion(AppHarbr.Version);
        appHarbrInitializeSdk((int)mediationSdk, sdkConfiguration.IOSApiKey);
    }

    #endregion Initialization

    [DllImport("__Internal")]
    private static extern void appHarbrSetBackgroundCallback(AHUnityBackgroundCallback backgroundCallback);

    [MonoPInvokeCallback(typeof(AHUnityBackgroundCallback))]
    internal static void BackgroundCallback(string propsStr)
    {
        AppHarbrSdkCallbacks.Instance.HandleBackgroundCallback(propsStr);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetUnityVersion(string appharbrUnityVersion);

    private static void SetUnityVersion(string appharbrUnityVersion)
    {
        appHarbrSetUnityVersion(appharbrUnityVersion);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetMuted(bool muted);

    private static void SetMuted(bool muted)
    {
        appHarbrSetMuted(muted);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetInterstitialAdTimeLimit(double seconds);

    private static void SetInterstitialAdTimeLimit(double seconds)
    {
        appHarbrSetInterstitialAdTimeLimit(seconds);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetTargetedAdNetworks(string adNetworks);

    private static void SetTargetedAdNetworks(string adNetworks)
    {
        appHarbrSetTargetedAdNetworks(adNetworks);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetSpecificAdNetworksForTimeout(string adNetworks);

    private static void SetSpecificAdNetworksForTimeout(string adNetworks)
    {
        appHarbrSetSpecificAdNetworksForTimeout(adNetworks);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetRewardedAdTimeLimit(double seconds);

    private static void SetRewardedAdTimeLimit(double seconds)
    {
        appHarbrSetRewardedAdTimeLimit(seconds);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetSpecificAdNetworksForRewardedTimeout(string adNetworks);

    private static void SetSpecificAdNetworksForRewardedTimeout(string adNetworks)
    {
        appHarbrSetSpecificAdNetworksForRewardedTimeout(adNetworks);
    }


    [DllImport("__Internal")]
    private static extern void appHarbrSetDebug(bool debug);

    private static void SetDebug(bool debug)
    {
        appHarbrSetDebug(debug);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrWatchInterstitial(string adUnitIdentifier);

    public static void WatchInterstitial(string adUnitIdentifier)
    {
        appHarbrWatchInterstitial(adUnitIdentifier);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrUnwatchInterstitial(string adUnitIdentifier);

    public static void UnwatchInterstitial(string adUnitIdentifier)
    {
        appHarbrUnwatchInterstitial(adUnitIdentifier);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrWatchRewarded(string adUnitIdentifier);

    public static void WatchRewarded(string adUnitIdentifier)
    {
        appHarbrWatchRewarded(adUnitIdentifier);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrUnwatchRewarded(string adUnitIdentifier);

    public static void UnwatchRewarded(string adUnitIdentifier)
    {
        appHarbrUnwatchRewarded(adUnitIdentifier);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrWatchRewardedInterstitial(string adUnitIdentifier);

    public static void WatchRewardedInterstitial(string adUnitIdentifier)
    {
        appHarbrWatchRewardedInterstitial(adUnitIdentifier);
    }
    
    [DllImport("__Internal")]
    private static extern void appHarbrUnwatchRewardedInterstitial(string adUnitIdentifier);

    public static void UnwatchRewardedInterstitial(string adUnitIdentifier)
    {
        appHarbrUnwatchRewardedInterstitial(adUnitIdentifier);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrWatchBanner(string adUnitIdentifier);

    public static void WatchBanner(string adUnitIdentifier)
    {
        appHarbrWatchBanner(adUnitIdentifier);
    }

    public static void WatchBanner(string adUnitId, string bannerPosition)
    {
        WatchBanner(adUnitId);
    }

    public static void WatchBanner(string adUnitId, float x, float y)
    {
        WatchBanner(adUnitId);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrUnwatchBanner(string adUnitIdentifier);

    public static void UnwatchBanner(string adUnitIdentifier)
    {
        appHarbrUnwatchBanner(adUnitIdentifier);
    }

    [DllImport("__Internal")]
    private static extern int getAppHarbrInterstitialState(string adUnitIdentifier);

    public static AHAdStateResult GetInterstitialState(string adUnitIdentifier)
    {
        int adState = getAppHarbrInterstitialState(adUnitIdentifier);
        AHAdStateResult adStateResult = (AHAdStateResult)Enum.ToObject(typeof(AHAdStateResult), adState);
        return adStateResult;
    }

    [DllImport("__Internal")]
    private static extern int getAppHarbrRewardedState(string adUnitIdentifier);

    public static AHAdStateResult GetRewardedState(string adUnitIdentifier)
    {
        int adState = getAppHarbrRewardedState(adUnitIdentifier);
        AHAdStateResult adStateResult = (AHAdStateResult)Enum.ToObject(typeof(AHAdStateResult), adState);
        return adStateResult;
    }

    [DllImport("__Internal")]
    private static extern int getAppHarbrRewardedInterstitialState(string adUnitIdentifier);

    public static AHAdStateResult GetRewardedInterstitialState(string adUnitIdentifier)
    {
        int adState = getAppHarbrRewardedInterstitialState(adUnitIdentifier);
        AHAdStateResult adStateResult = (AHAdStateResult)Enum.ToObject(typeof(AHAdStateResult), adState);
        return adStateResult;
    }

    [DllImport("__Internal")]
    private static extern void appHarbrSetTimeLimitConfig(string timeLimitConfigJson);

    private static void SetTimeLimitConfig(AHTimeLimitConfig timeLimitConfigJson)
    {
        string jsonString = Json.Serialize(timeLimitConfigJson.ToDictionary());

        appHarbrSetTimeLimitConfig(jsonString);
    }

    [DllImport("__Internal")]
    private static extern void appHarbrLaunchIntegrationDashboard(int mediationSdk);

    public static void LaunchIntegrationDashboard(AHAdSdk mediationSdk)
    {
        if (AppHarbr.SdkConfiguration == null)
        {
            Debug.Log("Please Init AppHarbr SDK First");
            return;
        }

        if (AppHarbr.SdkConfiguration.AhSdkDebug == null
              || AppHarbr.SdkConfiguration.AhSdkDebug.IsDebug == false)
        {
            Debug.Log("Please Set AppHarbr SDK Debug On");
            return;
        }

        appHarbrLaunchIntegrationDashboard((int)mediationSdk);
    }

    public static void WatchMRec(string adUnitId, string mrecPosition)
    {
        WatchBanner(adUnitId);
    }

    public static void WatchMRec(string adUnitId, float x, float y)
    {
        WatchBanner(adUnitId);
    }
#endif

    }

    public static class AHTimeLimitConfigExtensions
    {
        public static Dictionary<string, object> ToDictionary(this AHTimeLimitConfig config)
        {
            // Create the outer dictionary
            var dict = new Dictionary<string, object>
        {
            { "AdFormat", (int)config.AdFormat } // Convert AHAdFormat enum to its int value
        };

            // Create the inner dictionary for "TimeLimitInSeconds"
            var timeLimitInSecondsDict = new Dictionary<string, int[]>();
            foreach (var entry in config.TimeLimitInSeconds)
            {
                // Convert the timeout key to string and AHAdSdk[] to int[]
                var sdkIntArray = new int[entry.Value.Length];
                for (int i = 0; i < entry.Value.Length; i++)
                {
                    sdkIntArray[i] = (int)entry.Value[i]; // Convert each AHAdSdk to its int value
                }

                timeLimitInSecondsDict[entry.Key.ToString()] = sdkIntArray;
            }

            dict["TimeLimitInSeconds"] = timeLimitInSecondsDict;

            return dict;
        }
    }
}
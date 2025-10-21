using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace AppHarbrSDK
{
    public class AppHarbrAndroid : AndroidAdWatcher
    {
        private static string AHSDKDEBUG_CLASS_NAME = "com.appharbr.sdk.configuration.AHSdkDebug";
        private static string APPHARBR_CONFIGURATION_WRAPPER_CLASS = "com.appharbr.unity.mediation.AHConfigurationWrapperBuilder";
        //private static BackgroundCallbackProxy backgroundCallback = new BackgroundCallbackProxy();

        public static void Initialize(AHAdSdk ahAdSdk, AHSdkConfiguration sdkConfiguration)
        {
            try
            {
                if (!sdkConfiguration.HasAndroidAPIKey())
                {
                    return;
                }
                AppHarbr.SdkConfiguration = sdkConfiguration;

                appHarbrClass = new AndroidJavaClass(APPHARBR_CLASS_NAME);
                ahUnityMediatorsClass = new AndroidJavaClass(APPHARBR_GATEWAY_CLASS);

                AndroidJavaObject configObj = setupConfigObj(sdkConfiguration);
                if (ahUnityMediatorsClass != null)
                {
                    ahUnityMediatorsClass.CallStatic("setUnityVersion", (string)AppHarbr.Version);
                    ahUnityMediatorsClass.CallStatic(
                        "initialize",
                        (int)ahAdSdk,
                        getApplicationContext(),
                        configObj,
                        new BackgroundCallbackProxy()
                        );
                }
                else
                {
                    Debug.Log("Cannot find AhUnityMediatorsClass!");
                }
            }
            catch (Exception e)
            {
                Debug.Log("Problem while initializing AppHarbr: " + e.Message + "\n" + e.StackTrace);
            }
        }

        private static AndroidJavaObject setupConfigObj(AHSdkConfiguration configuration)
        {
            string temporaryTargetedNetworks = null;
            if (configuration.TargetedAdNetworks != null)
            {
                int[] array = Array.ConvertAll(configuration.TargetedAdNetworks, value => (int)value);
                temporaryTargetedNetworks = string.Join(",", array);
            }

            string temporarySpecificAdNetworkToLimitTime = null;
            if (configuration.SpecificAdNetworkToLimitInterstitialTime != null)
            {
                int[] array = Array.ConvertAll(configuration.SpecificAdNetworkToLimitInterstitialTime, value => (int)value);
                temporarySpecificAdNetworkToLimitTime = string.Join(",", array);
            }

            if (configuration.SpecificAdNetworkToLimitTime != null)
            {
                int[] array = Array.ConvertAll(configuration.SpecificAdNetworkToLimitTime, value => (int)value);
                temporarySpecificAdNetworkToLimitTime = string.Join(",", array);
            }

            string temporarySpecificAdNetworkToLimitRewardedTime = null;
            if (configuration.SpecificAdNetworkToLimitRewardedTime != null)
            {
                int[] array = Array.ConvertAll(configuration.SpecificAdNetworkToLimitRewardedTime, value => (int)value);
                temporarySpecificAdNetworkToLimitRewardedTime = string.Join(",", array);
            }

            int interstitialTimeLimit = configuration.InterstitialAdTimeLimit;
            if (configuration.LimitFullscreenAdsInSeconds > 0) {
                interstitialTimeLimit = configuration.LimitFullscreenAdsInSeconds;
            }
            return new AndroidJavaObject(
                APPHARBR_CONFIGURATION_WRAPPER_CLASS,
                configuration.AndroidApiKey,
                temporaryTargetedNetworks,
                GetAhDebugObject(configuration),
                configuration.MuteAd,
                interstitialTimeLimit,
                temporarySpecificAdNetworkToLimitTime,
                configuration.RewardedAdTimeLimit,
                temporarySpecificAdNetworkToLimitRewardedTime,
                GetAhTimeLimitConfigSerializeString(configuration.InterstitialTimeLimitConfig),
                GetAhTimeLimitConfigSerializeString(configuration.RewardedTimeLimitConfig)
                );
        }

        private static AndroidJavaObject GetAhDebugObject(AHSdkConfiguration configuration)
        {
            AndroidJavaObject debugObj = null;

            if (configuration.AhSdkDebug != null)
            {
                debugObj = new AndroidJavaObject(
                    AHSDKDEBUG_CLASS_NAME,
                    configuration.AhSdkDebug.IsDebug
                );

                foreach (var domain in configuration.AhSdkDebug.BlockDomains)
                {
                    debugObj.Call<AndroidJavaObject>("withBlockDomain", domain);
                }
            } else {
                debugObj = new AndroidJavaObject(
                AHSDKDEBUG_CLASS_NAME,
                false
                    );
            }
            return debugObj;
        }

        private static string GetAhTimeLimitConfigSerializeString(AHTimeLimitConfig configuration)
        {
            var sb = new StringBuilder();
            if (configuration != null) {
                var timeLimitDic = configuration.TimeLimitInSeconds;
                foreach (var kvp in timeLimitDic)
                {
                    // Convert AHAdSdk[] to int[] (IDs)
                    var ids = Array.ConvertAll(kvp.Value, value => (int)value);
                    sb.Append($"{kvp.Key}:[{string.Join(",", ids)}];");
                }

                // Remove the trailing semicolon
                if (sb.Length > 0 && sb[sb.Length - 1] == ';')
                {
                    sb.Length -= 1;
                }
            }

            return sb.ToString();
        }


        private static AndroidJavaObject getApplicationContext()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");
            return context;
        }

        internal class BackgroundCallbackProxy : AndroidJavaProxy
        {
            public BackgroundCallbackProxy() : base("com.appharbr.unity.mediation.NativeToUnityBridge") { }

            public void onEvent(string propsStr)
            {
                Debug.Log("Got data from Java -> " + propsStr);
                AppHarbrSdkCallbacks.Instance.HandleBackgroundCallback(propsStr);
                Debug.Log("Data was forwarded to HandleBackgroundCallback !!!! ");
            }
        }
    }
}
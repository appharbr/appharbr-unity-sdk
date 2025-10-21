#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS)

using System;

namespace AppHarbrSDK
{

    public class AppHarbrUnsupportedEditor
    {
        #region Initialization

        public static void Initialize(AHAdSdk mediationSdk, AHSdkConfiguration sdkConfiguration)
        {

        }

        #endregion Initialization

        public static void WatchInterstitial(string adUnitIdentifier)
        {

        }

        public static void UnwatchInterstitial(string adUnitIdentifier)
        {

        }

        public static void WatchRewarded(string adUnitIdentifier)
        {

        }

        public static void UnwatchRewarded(string adUnitIdentifier)
        {

        }

        public static void WatchRewardedInterstitial(string adUnitIdentifier)
        {

        }

        public static void UnwatchRewardedInterstitial(string adUnitIdentifier)
        {

        }

        public static void WatchBanner(string adUnitIdentifier)
        {

        }

        public static void WatchBanner(string adUnitId, string bannerPosition)
        {

        }

        public static void WatchBanner(string adUnitId, float x, float y)
        {

        }

        public static void UnwatchBanner(string adUnitIdentifier)
        {

        }

        public static AHAdStateResult GetInterstitialState(string adUnitIdentifier)
        {
            return AHAdStateResult.Unknown;
        }

        public static AHAdStateResult GetRewardedState(string adUnitIdentifier)
        {
            return AHAdStateResult.Unknown;
        }

        public static AHAdStateResult GetRewardedInterstitialState(string adUnitIdentifier)
        {
            return AHAdStateResult.Unknown;
        }

        public static void WatchMRec(string adUnitId, string mrecPosition)
        {
            // unsupported
        }

        public static void WatchMRec(string adUnitId, float x, float y)
        {
            // unsupported
        }

        public static void LaunchIntegrationDashboard(AHAdSdk mediationSdk)
        {

        }
    }
}

#endif

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public abstract class AndroidAdWatcher: AndroidAdState{

    protected static string APPHARBR_GATEWAY_CLASS = "com.appharbr.unity.mediation.AHUnityMediators";
    protected static AndroidJavaClass ahUnityMediatorsClass;

    public static void WatchBanner(string adUnitId)
    {
        WatchAd("watchBanner", adUnitId);
    }

    public static void WatchInterstitial(string adUnitId)
    {
        WatchAd("watchInterstitial", adUnitId);
    }

    public static void WatchRewarded(string adUnitId)
    {
        WatchAd("watchRewarded", adUnitId);
    }

    public static void WatchRewardedInterstitial(string adUnitId)
    {
        WatchAd("watchRewardedInterstitial", adUnitId);
    }

    public static void WatchBanner(string adUnitId, string bannerPosition)
    {
        WatchAd("watchBanner", adUnitId, bannerPosition);
    }

    public static void WatchMRec(string adUnitId, string mrecPosition)
    {
        WatchAd("WatchMRec", adUnitId, mrecPosition);
    }

    public static void WatchBanner(string adUnitId, float x, float y)
    {
        WatchAdWithPosition("watchBanner", adUnitId, x, y);
    }

    public static void WatchMRec(string adUnitId, float x, float y)
    {
        WatchAdWithPosition("WatchMRec", adUnitId, x, y);
    }

    private static void WatchAd(string adFormat, string adUnitId, string position = null)
    {
        try
        {
            if (ahUnityMediatorsClass == null)
            {
                return;
            }
            if(position == null){
                ahUnityMediatorsClass.CallStatic(adFormat, adUnitId);
            }
            else {
                ahUnityMediatorsClass.CallStatic(adFormat, adUnitId, position);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Problem with " + adFormat + " with ad unit id [" + adUnitId + "]\n" + e.Message + "\n" + e.StackTrace);
        }
    }

    private static void WatchAdWithPosition(string adFormat, string adUnitId, float x, float y){
        try
        {
            if (ahUnityMediatorsClass == null)
            {
                return;
            }
            ahUnityMediatorsClass.CallStatic(adFormat, adUnitId, x, y);
        }
        catch (Exception e)
        {
            Debug.Log("Problem in " + adFormat + " with ad unit id [" + adUnitId + "]\n" + e.Message + "\n" + e.StackTrace);
        }
    }
    
    public static void UnwatchBanner(string adUnitId)
    {
        Unwatch(AHAdFormat.Banner, adUnitId);
    }

    public static void UnwatchInterstitial(string adUnitId)
    {
        Unwatch(AHAdFormat.Interstitial, adUnitId);
    }

    public static void UnwatchRewarded(string adUnitId)
    {
        Unwatch(AHAdFormat.Rewarded, adUnitId);
    }

    public static void UnwatchRewardedInterstitial(string adUnitId)
    {
        Unwatch(AHAdFormat.RewardedInterstitial, adUnitId);
    }

    public static void Unwatch(AHAdFormat adFormat, string adUnitId)
    {
        try
        {
            if (ahUnityMediatorsClass == null)
            {
                return;
            }
            ahUnityMediatorsClass.CallStatic("unwatch", (int)adFormat, adUnitId);
        }
        catch (Exception e)
        {
            Debug.Log("Problem while Unwatching " + adFormat + " with ad unit id [" + adUnitId + "]\n" + e.Message + "\n" + e.StackTrace);
        }
    }

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
      try
        {
            if (ahUnityMediatorsClass == null)
            {
                return;
            }

            ahUnityMediatorsClass.CallStatic("launchIntegrationDashboard", (int)mediationSdk);
        }
        catch (Exception e)
        {
            Debug.Log("Problem with Launch Integration Dashboard " + mediationSdk  + "\n" + e.Message + "\n" + e.StackTrace);
        }
      }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppHarbrSDK.Internal;

public class AppHarbr :
#if UNITY_EDITOR
    AppHarbrUnsupportedEditor
#elif UNITY_ANDROID
    AppHarbrAndroid
#elif UNITY_IPHONE || UNITY_IOS
    AppHarbriOS
#else
    AppHarbrUnsupportedEditor
#endif
{
    public static AHSdkConfiguration SdkConfiguration = null;
    private const string version = "1.18.5";

    /// <summary>
    /// Gets the current AppHarbr plugin version.
    /// </summary>
    public static string Version => version;
}

public class AHSdkConfiguration
{
    public string AndroidApiKey { get; }
    public string IOSApiKey { get; }
    public AHAdSdk[] TargetedAdNetworks { get; }
    public AHSdkDebug AhSdkDebug { get; }
    public bool MuteAd { get; }
    [Obsolete("InterstitialAdTimeLimit is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public int InterstitialAdTimeLimit { get; }
    [Obsolete("SpecificAdNetworkToLimitInterstitialTime is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public AHAdSdk[] SpecificAdNetworkToLimitInterstitialTime { get; }
    [Obsolete("RewardedAdTimeLimit is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public int RewardedAdTimeLimit { get; }
    [Obsolete("SpecificAdNetworkToLimitRewardedTime is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public AHAdSdk[] SpecificAdNetworkToLimitRewardedTime { get; }
    [Obsolete("LimitFullscreenAdsInSeconds is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public int LimitFullscreenAdsInSeconds { get; }
    [Obsolete("SpecificAdNetworkToLimitTime is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public AHAdSdk[] SpecificAdNetworkToLimitTime { get; }
    [Obsolete("InterstitialTimeLimitConfig is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public AHTimeLimitConfig InterstitialTimeLimitConfig { get; }
    [Obsolete("RewardedTimeLimitConfig is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
    public AHTimeLimitConfig RewardedTimeLimitConfig { get; }
    public WatchAppHarbrAds WatchAppHarbrAds { get; }

    public AHSdkConfiguration(
        string androidApiKey = null,
        string iOSApiKey = null,
        AHAdSdk[] targetedAdNetworks = null,
        AHSdkDebug ahSdkDebug = null,
        bool muteAd = false,
        int limitFullscreenAdsInSeconds = 0,
        AHAdSdk[] specificAdNetworkToLimitTime = null,
        int interstitialAdTimeLimit = 0,
        AHAdSdk[] specificAdNetworkToLimitInterstitialTime = null,
        int rewardedAdTimeLimit = 0,
        AHAdSdk[] specificAdNetworkToLimitRewardedTime = null,
        AHTimeLimitConfig interstitialTimeLimitConfig = null,
        AHTimeLimitConfig rewardedTimeLimitConfig = null,
        WatchAppHarbrAds watchAppHarbrAds = null)
    {
        MainThreadDispatcher.InitializeIfNeeded();
        InitCallbacks();

        AndroidApiKey = androidApiKey;
        IOSApiKey = iOSApiKey;

        if (!HasAndroidAPIKey() && !HasIOSAPIKey())
        {
            invokeFailCallback();
            return;
        }

        TargetedAdNetworks = targetedAdNetworks;

        AhSdkDebug = ahSdkDebug;
        MuteAd = muteAd;
        LimitFullscreenAdsInSeconds = limitFullscreenAdsInSeconds;
        InterstitialAdTimeLimit = interstitialAdTimeLimit;
        RewardedAdTimeLimit = rewardedAdTimeLimit;
        SpecificAdNetworkToLimitRewardedTime = specificAdNetworkToLimitRewardedTime;
        InterstitialTimeLimitConfig = interstitialTimeLimitConfig;
        RewardedTimeLimitConfig = rewardedTimeLimitConfig;

        // Use the internal method to check the arrays
        if (IsValidAdNetworkArray(targetedAdNetworks))
        {
            TargetedAdNetworks = targetedAdNetworks;
        }

        if (IsValidAdNetworkArray(specificAdNetworkToLimitTime))
        {
            SpecificAdNetworkToLimitTime = specificAdNetworkToLimitTime;
        }

        if (IsValidAdNetworkArray(specificAdNetworkToLimitInterstitialTime))
        {
            SpecificAdNetworkToLimitInterstitialTime = specificAdNetworkToLimitInterstitialTime;
        }

        if (IsValidAdNetworkArray(specificAdNetworkToLimitRewardedTime))
        {
            SpecificAdNetworkToLimitRewardedTime = specificAdNetworkToLimitRewardedTime;
        }
        WatchAppHarbrAds = watchAppHarbrAds;
    }

    private bool IsValidAdNetworkArray(AHAdSdk[] adNetworkArray)
    {
        return adNetworkArray != null &&
               adNetworkArray.Length > 0 &&
               !(adNetworkArray.Length == 1 && adNetworkArray[0] == AHAdSdk.None);
    }

    private void invokeFailCallback()
    {
        try
        {
            string jsonString = @"{
                                ""failureReason"": ""Eather Android or iOS API key should be provided!"",
                                ""name"": ""OnSdkInitializedFailureEvent""
                               }";
            AppHarbrSdkCallbacks.Instance.ForwardEvent(jsonString);
        }
        catch (Exception e)
        {
            Debug.Log("Problem while trying to call AppHarbrSdkCallback -> OnSdkInitializedFailureEvent: " + e.Message);
        }
    }

    public bool HasAndroidAPIKey()
    {
        return AndroidApiKey != null && AndroidApiKey.Length > 0;
    }

    public bool HasIOSAPIKey()
    {
        return IOSApiKey != null && IOSApiKey.Length > 0;
    }

    private void InitCallbacks()
    {
        if (AppHarbrSdkCallbacks.Instance != null)
        {
            return;
        }
        var type = typeof(AppHarbrSdkCallbacks);
        var mgr = new GameObject("AppHarbrSdkCallbacks", type).GetComponent<AppHarbrSdkCallbacks>(); // Its Awake() method sets Instance.
        if (AppHarbrSdkCallbacks.Instance != mgr)
        {
            Debug.Log("An instance of AppHarborSdkCallbacks already exists in the scene. Please remove the script from your scene.");
        }
    }

}

public class AHSdkDebug
{
    public bool IsDebug { get; }
    public List<string> BlockDomains { get; }

    public AHSdkDebug(bool isDebug = true, List<string> blockDomains = null)
    {
        this.IsDebug = isDebug;
        this.BlockDomains = blockDomains ?? new List<string>();
    }
}

public class AHIncidentData
{
    public string UnitId { get; }
    public AHAdSdk AdNetwork { get; }
    public string CreativeId { get; }
    public AHAdFormat AdFormat { get; }
    public bool ShouldLoadNewAd { get; }
    public List<string> BlockReasons { get; }
    public List<string> ReportReasons { get; }
    public string UserDefinedApplication { get; }
    public string UserDefinedDomain { get; }
    public AHAdAnalyzedResult AnalyzeResult { get; }

    public AHIncidentData(
        string unitId = "",
        AHAdSdk adNetwork = AHAdSdk.None,
        string creativeId = "",
        AHAdFormat adFormat = AHAdFormat.Banner,
        bool shouldLoadNewAd = false,
        List<string> blockReasons = null,
        List<string> reportReasons = null,
        string userDefinedApplication = "",
        string userDefinedDomain = "",
        AHAdAnalyzedResult analyzeResult = AHAdAnalyzedResult.NoResultAnalyzed)
    {
        UnitId = unitId;
        AdNetwork = adNetwork;
        CreativeId = creativeId;
        AdFormat = adFormat;
        ShouldLoadNewAd = shouldLoadNewAd;
        BlockReasons = blockReasons ?? new List<string>();
        ReportReasons = reportReasons ?? new List<string>();
        UserDefinedApplication = userDefinedApplication;
        UserDefinedDomain = userDefinedDomain;
        AnalyzeResult = analyzeResult;
    }
}

public class WatchAppHarbrAds {
    public List<WatchAppHarbrBannerAd> BannerAdUnitIds { get; }
    public List<WatchAppHarbrBannerAd> MRecAdUnitIds { get; }
    public List<string> InterstitialAdUnitIds { get; }
    public List<string> RewardedAdUnitIds { get; }
    public List<string> RewardedInterstitialAdUnitIds { get; }

    public WatchAppHarbrAds(List<WatchAppHarbrBannerAd> bannerAdUnitIds = null,
                            List<WatchAppHarbrBannerAd> mRecAdUnitIds = null,
                            List<string> interstitialAdUnitIds = null,
                            List<string> rewardedAdUnitIds = null,
                            List<string> rewardedInterstitialAdUnitIds = null)
    {
        BannerAdUnitIds = bannerAdUnitIds;
        MRecAdUnitIds = mRecAdUnitIds;
        InterstitialAdUnitIds = interstitialAdUnitIds;
        RewardedAdUnitIds = rewardedAdUnitIds;
        RewardedInterstitialAdUnitIds = rewardedInterstitialAdUnitIds;
    }
}


public class WatchAppHarbrBannerAd
{
  public string AdUnitId { get; }
  public float PositionX { get; }
  public float PositionY { get; }
  public string BannerPosition { get; }

  public WatchAppHarbrBannerAd(string adUnitId = null,
                         float positionX = -1.0f,
                         float positionY = -1.0f,
                         string bannerPosition = null)
  {
    AdUnitId = adUnitId;
    PositionX = positionX;
    PositionY = positionY;
    BannerPosition = bannerPosition;
  }
}

[Obsolete("AHTimeLimitConfig is deprecated and will be removed in a future release (January 2026). Please use Time Limit only from the Publisher UI Dashboard Settings.")]
public class AHTimeLimitConfig
 {
    public AHAdFormat AdFormat { get; }
    public Dictionary<int, AHAdSdk[]> TimeLimitInSeconds { get; set; }

    public AHTimeLimitConfig(AHAdFormat adFormat)
    {
        AdFormat = adFormat;
        TimeLimitInSeconds = new Dictionary<int, AHAdSdk[]>();
    }

    /// <summary>
    /// Appends a configuration for a specific timeout value and ad networks.
    /// </summary>
    /// <param name="timeout">The timeout value in seconds.</param>
    /// <param name="timeLimit">List of Ad Networks. An empty list applies the timeout to all ad networks.</param>
    /// <returns>The current TimeLimitConfig instance with the updated configuration.</returns>
    public AHTimeLimitConfig AppendConfig(int timeout, AHAdSdk[] timeLimit)
    {
        TimeLimitInSeconds[timeout] = timeLimit;
        return this;
    }
}

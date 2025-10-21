using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

namespace AppHarbrSDK
{
    public abstract class AndroidAdState
    {

        protected static string APPHARBR_CLASS_NAME = "com.appharbr.sdk.engine.AppHarbr";
        protected static AndroidJavaClass appHarbrClass;

        public static AHAdStateResult GetInterstitialState(string adUnitId)
        {
            return GetAdState("getInterstitialState", adUnitId);
        }

        public static AHAdStateResult GetRewardedState(string adUnitId)
        {
            return GetAdState("getRewardedState", adUnitId);
        }

        public static AHAdStateResult GetRewardedInterstitialState(string adUnitId)
        {
            return GetAdState("getRewardedInterstitialState", adUnitId);
        }

        private static AHAdStateResult GetAdState(string adFormat, string adUnitId)
        {
            try
            {
                if (appHarbrClass == null)
                {
                    Debug.Log("Cannot find AppHarbr class!");
                    return AHAdStateResult.Unknown;
                }
                AndroidJavaObject state = appHarbrClass.CallStatic<AndroidJavaObject>(adFormat, adUnitId);
                int enumOrdinal = state.Call<int>("ordinal");
                AHAdStateResult adStateResult = (AHAdStateResult)Enum.ToObject(typeof(AHAdStateResult), enumOrdinal);
                return adStateResult;
            }
            catch (Exception e)
            {
                Debug.Log("Problem in " + adFormat + " with ad unit id [" + adUnitId + "] Will return Unknown\n" + e.Message + "\n" + e.StackTrace);
            }
            return AHAdStateResult.Unknown;
        }

    }
}
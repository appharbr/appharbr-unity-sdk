using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace AppHarbrSDK.Internal {
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher instance;

        private static List<Action> adEventsQueue = new List<Action>();
        private static volatile bool adEventsQueueEmpty = true;

        private void Update()
        {
            if (adEventsQueueEmpty) return;

            var actionsToExecute = new List<Action>();
            lock (adEventsQueue)
            {
                actionsToExecute.AddRange(adEventsQueue);
                adEventsQueue.Clear();
                adEventsQueueEmpty = true;
            }


            foreach (var action in actionsToExecute)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.Log("Caught exception while sending Appharbr event on UI thread " + e);
                }
            }
        }

        public static void InitializeIfNeeded()
        {
            if (instance != null) return;

            var dispatcher = new GameObject("MainThreadDispatcher");
            dispatcher.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(dispatcher);
            instance = dispatcher.AddComponent<MainThreadDispatcher>();
        }

#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS)
        public static MainThreadDispatcher Instance
        {
            get
            {
                InitializeIfNeeded();
                return instance;
            }
        }
#endif

        public static void InvokeOnMainThread(Action action)
        {
            if (action != null)
            {
                lock (adEventsQueue)
                {
                    adEventsQueue.Add(action);
                    adEventsQueueEmpty = false;
                }
            }
        }
    }
}

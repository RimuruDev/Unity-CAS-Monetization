﻿//  Copyright © 2024 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;
using System.Collections.Generic;

namespace CAS.Android
{
    internal sealed class CASManagerClient : IInternalManager
    {
        private AdEventsProxy _interstitialProxy;
        private AdEventsProxy _rewardedProxy;
        private AdEventsProxy _appReturnProxy;
        private AdEventsProxy _appOpenProxy;
        private AndroidJavaObject _managerBridge;
        private LastPageAdContent _lastPageAdContent = null;

        internal InitCallbackProxy _initProxy;
        private readonly List<IAdView> _adViews = new List<IAdView>();

        public string managerID { get; private set; }
        public bool isTestAdMode { get; set; }
        public InitialConfiguration initialConfig { get; set; }

        public LastPageAdContent lastPageAdContent
        {
            get { return _lastPageAdContent; }
            set
            {
                if (value != _lastPageAdContent)
                {
                    _lastPageAdContent = value;
                    string json = value == null ? null : JsonUtility.ToJson(value);
                    _managerBridge.Call("setLastPageAdContent", json);
                }
            }
        }

        #region Ad Events
        public event Action OnInterstitialAdLoaded
        {
            add { _interstitialProxy.OnAdLoaded += value; }
            remove { _interstitialProxy.OnAdLoaded -= value; }
        }
        public event CASEventWithAdError OnInterstitialAdFailedToLoad
        {
            add { _interstitialProxy.OnAdFailed += value; }
            remove { _interstitialProxy.OnAdFailed -= value; }
        }
        public event Action OnInterstitialAdShown
        {
            add { _interstitialProxy.OnAdShown += value; }
            remove { _interstitialProxy.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnInterstitialAdOpening
        {
            add { _interstitialProxy.OnAdOpening += value; }
            remove { _interstitialProxy.OnAdOpening -= value; }
        }
        public event CASEventWithMeta OnInterstitialAdImpression
        {
            add { _interstitialProxy.OnAdImpression += value; }
            remove { _interstitialProxy.OnAdImpression -= value; }
        }
        public event CASEventWithError OnInterstitialAdFailedToShow
        {
            add { _interstitialProxy.OnAdFailedToShow += value; }
            remove { _interstitialProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnInterstitialAdClicked
        {
            add { _interstitialProxy.OnAdClicked += value; }
            remove { _interstitialProxy.OnAdClicked -= value; }
        }
        public event Action OnInterstitialAdClosed
        {
            add { _interstitialProxy.OnAdClosed += value; }
            remove { _interstitialProxy.OnAdClosed -= value; }
        }


        public event Action OnRewardedAdLoaded
        {
            add { _rewardedProxy.OnAdLoaded += value; }
            remove { _rewardedProxy.OnAdLoaded -= value; }
        }
        public event CASEventWithAdError OnRewardedAdFailedToLoad
        {
            add { _rewardedProxy.OnAdFailed += value; }
            remove { _rewardedProxy.OnAdFailed -= value; }
        }
        public event Action OnRewardedAdShown
        {
            add { _rewardedProxy.OnAdShown += value; }
            remove { _rewardedProxy.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnRewardedAdOpening
        {
            add { _rewardedProxy.OnAdOpening += value; }
            remove { _rewardedProxy.OnAdOpening -= value; }
        }
        public event CASEventWithMeta OnRewardedAdImpression
        {
            add { _rewardedProxy.OnAdImpression += value; }
            remove { _rewardedProxy.OnAdImpression -= value; }
        }
        public event CASEventWithError OnRewardedAdFailedToShow
        {
            add { _rewardedProxy.OnAdFailedToShow += value; }
            remove { _rewardedProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnRewardedAdClicked
        {
            add { _rewardedProxy.OnAdClicked += value; }
            remove { _rewardedProxy.OnAdClicked -= value; }
        }
        public event Action OnRewardedAdCompleted
        {
            add { _rewardedProxy.OnAdCompleted += value; }
            remove { _rewardedProxy.OnAdCompleted -= value; }
        }
        public event Action OnRewardedAdClosed
        {
            add { _rewardedProxy.OnAdClosed += value; }
            remove { _rewardedProxy.OnAdClosed -= value; }
        }

        public event Action OnAppReturnAdShown
        {
            add { _appReturnProxy.OnAdShown += value; }
            remove { _appReturnProxy.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnAppReturnAdOpening
        {
            add { _appReturnProxy.OnAdOpening += value; }
            remove { _appReturnProxy.OnAdOpening -= value; }
        }
        public event CASEventWithMeta OnAppReturnAdImpression
        {
            add { _appReturnProxy.OnAdImpression += value; }
            remove { _appReturnProxy.OnAdImpression -= value; }
        }
        public event CASEventWithError OnAppReturnAdFailedToShow
        {
            add { _appReturnProxy.OnAdFailedToShow += value; }
            remove { _appReturnProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnAppReturnAdClicked
        {
            add { _appReturnProxy.OnAdClicked += value; }
            remove { _appReturnProxy.OnAdClicked -= value; }
        }
        public event Action OnAppReturnAdClosed
        {
            add { _appReturnProxy.OnAdClosed += value; }
            remove { _appReturnProxy.OnAdClosed -= value; }
        }

        public event Action OnAppOpenAdLoaded
        {
            add { _appOpenProxy.OnAdLoaded += value; }
            remove { _appOpenProxy.OnAdLoaded -= value; }
        }
        public event CASEventWithAdError OnAppOpenAdFailedToLoad
        {
            add { _appOpenProxy.OnAdFailed += value; }
            remove { _appOpenProxy.OnAdFailed -= value; }
        }
        public event Action OnAppOpenAdShown
        {
            add { _appOpenProxy.OnAdShown += value; }
            remove { _appOpenProxy.OnAdShown -= value; }
        }
        public event CASEventWithMeta OnAppOpenAdImpression
        {
            add { _appOpenProxy.OnAdImpression += value; }
            remove { _appOpenProxy.OnAdImpression -= value; }
        }
        public event CASEventWithError OnAppOpenAdFailedToShow
        {
            add { _appOpenProxy.OnAdFailedToShow += value; }
            remove { _appOpenProxy.OnAdFailedToShow -= value; }
        }
        public event Action OnAppOpenAdClicked
        {
            add { _appOpenProxy.OnAdClicked += value; }
            remove { _appOpenProxy.OnAdClicked -= value; }
        }
        public event Action OnAppOpenAdClosed
        {
            add { _appOpenProxy.OnAdClosed += value; }
            remove { _appOpenProxy.OnAdClosed -= value; }
        }
        #endregion

        #region Initialization
        internal CASManagerClient() { }

        internal IInternalManager Init(CASInitSettings initData)
        {
            managerID = initData.targetId;
            isTestAdMode = initData.IsTestAdMode();
            _initProxy = new InitCallbackProxy(this, initData);
            _interstitialProxy = new AdEventsProxy(AdType.Interstitial);
            _rewardedProxy = new AdEventsProxy(AdType.Rewarded);
            _appReturnProxy = new AdEventsProxy(AdType.Interstitial);
            _appOpenProxy = new AdEventsProxy(AdType.AppOpen);

            using (var builder = new AndroidJavaObject(CASJavaBridge.bridgeBuilderClass))
            {
                if (initData.IsTestAdMode())
                    builder.Call("enableTestMode");

                if (!string.IsNullOrEmpty(initData.userID))
                    builder.Call("setUserId", initData.userID);

                if (initData.consentFlow != null)
                {
                    if (!initData.consentFlow.isEnabled)
                        builder.Call("disableConsentFlow");
                    else
                        builder.Call("withConsentFlow", new CASConsentFlowClient(initData.consentFlow).obj);
                }
                if (initData.extras != null)
                {
                    foreach (var extra in initData.extras)
                    {
                        builder.Call("addExtras", extra.Key, extra.Value);
                    }
                }

                builder.Call("setCallbacks", _initProxy, _interstitialProxy, _rewardedProxy, _appReturnProxy, _appOpenProxy);

                _managerBridge = builder.Call<AndroidJavaObject>("build",
                    initData.targetId, Application.unityVersion, (int)initData.allowedAdFlags);
            }
            return this;
        }

        public void HandleInitEvent(CASInitCompleteEvent initEvent, InitCompleteAction initAction)
        {
            if (initialConfig != null)
            {
                if (initEvent != null)
                    initEvent(initialConfig);
                if (initAction != null)
                    initAction(initialConfig.error == null, initialConfig.error);
                return;
            }
            _initProxy.complete += initEvent;
            _initProxy.completeDeprecated += initAction;
        }
        #endregion

        public bool IsEnabledAd(AdType adType)
        {
            return _managerBridge.Call<bool>("isEnabled", (int)adType);
        }

        public void SetEnableAd(AdType adType, bool enabled)
        {
            _managerBridge.Call("enableAd", (int)adType, enabled);
        }

        public bool IsReadyAd(AdType adType)
        {
            return _managerBridge.Call<bool>("isAdReady", (int)adType);
        }

        public void LoadAd(AdType adType)
        {
            _managerBridge.Call("loadAd", (int)adType);
        }

        public void ShowAd(AdType adType)
        {
            _managerBridge.Call("showAd", (int)adType);
        }

        [UnityEngine.Scripting.Preserve]
        public bool TryOpenDebugger()
        {
            return _managerBridge.Call<bool>("tryOpenDebugger");
        }

        public void SetAutoShowAdOnAppReturn(AppReturnAdType type)
        {
            _managerBridge.Call("setAutoShowAdOnAppReturn", (int)type);
        }

        public void SetAppReturnAdsEnabled(bool enable)
        {
            SetAutoShowAdOnAppReturn(enable ? AppReturnAdType.Interstitial : AppReturnAdType.None);
        }

        public void SkipNextAppReturnAds()
        {
            _managerBridge.Call("skipNextReturnAds");
        }

        public IAdView GetAdView(AdSize size)
        {
            if (size < AdSize.Banner)
                throw new ArgumentException("Invalid AdSize " + size.ToString());
            for (int i = 0; i < _adViews.Count; i++)
            {
                if (_adViews[i].size == size)
                    return _adViews[i];
            }
            var callback = new AdEventsProxy(AdType.Banner);
            var bridge = _managerBridge.Call<AndroidJavaObject>("createAdView", callback, (int)size);
            var view = new CASViewClient(this, size, bridge, callback);
            _adViews.Add(view);
            return view;
        }

        public void RemoveAdViewFromFactory(IAdView view)
        {
            _adViews.Remove(view);
        }
    }
}
#endif

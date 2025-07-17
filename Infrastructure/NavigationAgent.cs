/// <summary>
/// NavigationAgent
/// </summary>
/// <author>CGC_10_田中 ミノル</author>

#nullable enable

using System;
using System.Collections.Generic;
using via;
using via.attribute;
using via.navigation;

namespace app
{
    public class NavigationAgent : via.Behavior, IDisposable
    {
        public enum NavigationState
        {
            Idle,
            Ready,
            Navigation,
            Complete,
            Fail
        }

        #region Fields
        private NavigationState _naviState = NavigationState.Idle;
        private vec3 _frameMove = vec3.Zero;
        private NavigationSurface _surface = null!;

        private vec3 _destPos;
        private GameObject? _destObj;

        private bool _isStarted = false;
        private bool _arrivedLatch;

        private int _failCount = 0;
        private float _retryTimer = 0f;
        private const int MaxRetry = 5;
        private const float BaseRetryDelay = 0.5f; // 秒
        #endregion

        #region Properties
        /// <summary>
        /// ナビゲーションの現在の状態
        /// </summary>
        public NavigationState NaviState => _naviState;

        /// <summary>
        /// 現在フレームの移動方向
        /// </summary>
        public vec3 FrameMove => _frameMove;

        /// <summary>
        /// 到達判定
        /// </summary>
        public bool IsArrived
        {
            get
            {
                bool r = _arrivedLatch;
                _arrivedLatch = false;
                return r;
            }
        }
        #endregion

        #region Methods

        public sealed class DelegateHandle
        {
            public Action<NavigationInfo>? OnUpdate;
            public Action? OnComplete;
            public Action<FailReport>? OnFail;
        }

        [Hide]
        public readonly DelegateHandle Delegates = new();

        /// <summary>
        /// 正常に機能しないので非推奨
        /// </summary>
        public void SetDestination(vec3 worldPos)
        {
            if (_naviState =!= NavigationState.Complete && vector.distance(_destPos, worldPos) < 0.01f)
                return;

            // via.debug.infoLine($"[Nav] Start Request Target: {worldPos}");
            _destObj = null;
            _destPos = worldPos;
            _isStarted = false;
            _failCount = 0;
            _retryTimer = 0f;
            _naviState = NavigationState.Ready;
        }

        public void SetDestination(GameObject target)
        {
            // via.debug.infoLine($"[Nav] Start Request Target: {target.Name}");
            _destObj = target;
            _isStarted = false;
            _failCount = 0;
            _retryTimer = 0f;
            _naviState = NavigationState.Ready;
        }

        public void Stop()
        {
            _destObj = null;
            _destPos = GameObject.Transform.Position;
            _isStarted = false;
            _failCount = 0;
            _retryTimer = 0f;
            _naviState = NavigationState.Idle;
        }

        public override void start()
        {
            _surface = GameObject.getSameComponent<NavigationSurface>() 
                ?? throw new InvalidOperationException("NavigationSurface がアタッチされていません。");

            // デリゲート登録
            var h = _surface.DelegateHandle;
            h.NavigationUpdateDelegate += OnNavUpdate;
            h.NavigationCompleteDelegate += OnNavComplete;
            h.NavigationFailDelegate += OnNavFail;
        }

        public override void update()
        {
            // via.debug.infoLine($"[Nav] Now State: {_naviState}");
            float dt = Application.DeltaTime;

            if (_naviState == NavigationState.Ready && !_isStarted)
            {
                if (_destObj != null)
                {
                    _surface.TargetGameObject = _destObj;
                    _surface.start();
                }
                else
                {
                    _surface.start(GameObject.Transform.Position, _destPos);
                }
                _isStarted = true;
            }

            // 失敗後のリトライ
            if (_naviState == NavigationState.Fail && _retryTimer > 0f)
            {
                _retryTimer -= dt;
                if (_retryTimer <= 0f && _failCount <= MaxRetry)
                {
                    // via.debug.infoLine($"[Nav] Retry {_failCount} / {MaxRetry}");
                    _naviState = NavigationState.Ready;
                    _isStarted = false;
                }
            }
        }

        public override void onDestroy()
        {
            if (_surface != null)
            {
                var h = _surface.DelegateHandle;
                h.NavigationUpdateDelegate -= OnNavUpdate;
                h.NavigationCompleteDelegate -= OnNavComplete;
                h.NavigationFailDelegate -= OnNavFail;
            }
        }

        public void Dispose() => onDestroy();

        #endregion
        #region Delegates
        private void OnNavUpdate(NavigationInfo info)
        {
            _frameMove = info.NavigationVector;
            _naviState = NavigationState.Navigation;
            Delegates.OnUpdate?.Invoke(info);

            // via.debug.infoLine($"[Nav] Move = {info.NavigationVector}");
        }

        private void OnNavComplete()
        {
            _frameMove = vec3.Zero;
            _naviState = NavigationState.Complete;
            Delegates.OnComplete?.Invoke();

            _arrivedLatch = true;
            _failCount = 0;

            // via.debug.infoLine("[Nav] Complete");
        }

        private void OnNavFail(FailReport report)
        {
            _frameMove = vec3.Zero;
            _naviState = NavigationState.Fail;
            Delegates.OnFail?.Invoke(report);

            via.debug.infoLine("[Nav] Fail");

            _failCount++;
            if (_failCount > MaxRetry)
            {
                via.debug.errorLine("[Nav] MaxRetry reached. Navigation aborted.");
                return;
            }

            _retryTimer = BaseRetryDelay * math.pow(2f, _failCount - 1);
            via.debug.infoLine($"[Nav] Next retry in {_retryTimer}s");

            // エラー分類表示
            debug.infoline("Navigation失敗");
            switch (report.getFailLevel())
            {
                case via.navigation.FailReport.FailLevel.Upper:
                    debug.infoLine("UpperLayer");
                    break;
                case via.navigation.FailReport.FailLevel.Lower:
                    debug.infoLine("LowerLayer");
                    break;
            }

            if (report.IsFailed(via.navigation.FailReport.FailAttribute.DestPosNotSpecified))
                debug.infoLine("目的地座標未設定");

            if (report.IsFailed(via.navigation.FailReport.FailAttribute.StartNodeNotFound))
                debug.infoLine("開始ノードが見つからない");

            if (report.IsFailed(via.navigation.FailReport.FailAttribute.DestNodeNotFound))
                debug.infoLine("目的地ノードが見つからない");

            if (report.IsFailed(via.navigation.FailReport.FailAttribute.PathNotFound))
                debug.infoLine("経路が見つからない: from:" + report.StartNodeID + " to:" + report.DestNodeID + " " + report.DestPos.ToString());
        }
        #endregion
    }
}

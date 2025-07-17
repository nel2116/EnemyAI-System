//======================================================================
// <summary>
//   IAiState
// </summary>
// <author>CGC_10_田中 ミノル</author>
//======================================================================
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app
{
    /// <summary>
    /// Stateパターン用：1ステートが備える3つのフック。
    /// </summary>
    public interface IAiState
    {
        /// <summary>
        /// ステート遷移直後に1度だけ呼ばれる
        /// </summary>
        void Enter();

        /// <summary>
        /// 毎フレーム呼ばれる更新処理
        /// </summary>
        void Tick(float dt);

        /// <summary>
        /// 別のステートへ遷移する直前に1度だけ呼ばれる
        /// </summary>
        void Exit();
    }
}

/// <summary>
/// AttackStats
/// </summary>
/// <author>CGC_10_田中 ミノル</author>
using System;
using System.Collections.Generic;
using via;
using via.attribute;

namespace app.enemy.shared
{
    public readonly record struct AttackStats(float AttackPower, float CooldownSeconds)
    {
        public static readonly AttackStats Default = new(10.0f, 1.5f);
    }
}

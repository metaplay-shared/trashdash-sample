using UnityEngine;
using System;
using System.Collections;
using Game.Logic;

public class ExtraLife : Consumable
{
    protected const int k_MaxLives = 3;
    protected const int k_CoinValue = 10;

    public override string GetConsumableName()
    {
        return "Life";
    }

    public override ConsumableType GetConsumableType()
    {
        return ConsumableType.EXTRALIFE;
    }

    public override bool CanBeUsed(CharacterInputController c)
    {
        if (c.currentLife == c.maxLife)
            return false;

        return true;
    }

    public override IEnumerator Started(CharacterInputController c)
    {
        yield return base.Started(c);
        if (c.currentLife < k_MaxLives)
            c.currentLife += 1;
		else
            c.coins += k_CoinValue;
    }
}

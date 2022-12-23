using System;
using UnityEngine;

[System.Serializable]
public class ChargeSystem {

    public event Action<ChargeSystem> OnChargesAmountChanged;

    [field: SerializeField] public StatInt MaxCharges { get; set; }

    [field: SerializeField] public int CurrentCharges { get; set; }

    [field: SerializeField] public ChargeReplenishmentType ChargeReplenishmentType { get; set; }

    [field: SerializeField] public StatInt DefaultChargesUseRate { get; set; }
    [field: SerializeField] public StatInt DefaultChargesReplenishmentRateOneByOne { get; set; }

    private bool _initialized = false;

    private void Initialize() {
        if (_initialized) return;
        _initialized = true;

        if (DefaultChargesUseRate.GetValue() <= 0) {
            DefaultChargesUseRate = new StatInt(null, 1, 1, int.MaxValue);
        }
        if (DefaultChargesReplenishmentRateOneByOne.GetValue() == 0) {
            DefaultChargesReplenishmentRateOneByOne = new StatInt(null, 1, 1, int.MaxValue);
        }
        CurrentCharges = MaxCharges.GetValue();
    }

    public bool ChargeSystemBeingUsed() {
        Initialize();

        if (MaxCharges.GetValue() > 1) {
            return true;
        }

        return false;
    }

    public bool HasCharges(int chargesRequired = 0) {
        if (!ChargeSystemBeingUsed()) return false;

        if (chargesRequired <= 0) {
            chargesRequired = DefaultChargesUseRate.GetValue();
        }

        if (CurrentCharges >= chargesRequired) return true;

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chargesToReplenish">Amount of charges to replenish, default is 0 which results in replenishment based 
    /// on charge replenishment type</param>
    /// <returns>False if not all charges have been replenished, True if all charges are present</returns>
    public bool ReplenishCharges(int chargesToReplenish = 0) {
        if (!ChargeSystemBeingUsed()) return true;

        int maxChargesValue = MaxCharges.GetValue();

        if (chargesToReplenish == 0) {
            if (CurrentCharges < maxChargesValue) {
                if (ChargeReplenishmentType == ChargeReplenishmentType.OneByOne) {
                    CurrentCharges += DefaultChargesReplenishmentRateOneByOne.GetValue();
                } else if (ChargeReplenishmentType == ChargeReplenishmentType.AllAtOnce) {
                    CurrentCharges = maxChargesValue;
                }

                if (CurrentCharges > maxChargesValue) {
                    CurrentCharges = maxChargesValue;
                }
            }
        } else {
            CurrentCharges += chargesToReplenish;
        }

        OnChargesAmountChanged?.Invoke(this);

        if (CurrentCharges < maxChargesValue) return false;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chargesToConsume"></param>
    /// <returns>True if cooldown is required to start, False if no cooldown required yet</returns>
    public bool ConsumeCharges(int chargesToConsume = 0) {
        Initialize();

        if (chargesToConsume == 0) {
            chargesToConsume = DefaultChargesUseRate.GetValue();
        }

        if (!HasCharges(chargesToConsume)) return false;

        //Debug.Log("decremented charge by " + chargesToConsume);

        CurrentCharges -= chargesToConsume;

        OnChargesAmountChanged?.Invoke(this);

        if (((MaxCharges.GetValue() - DefaultChargesUseRate.GetValue()) >= CurrentCharges && ChargeReplenishmentType == ChargeReplenishmentType.OneByOne)
            || (CurrentCharges == 0 && ChargeReplenishmentType == ChargeReplenishmentType.AllAtOnce)) {
            return true;
        }

        return false;
    }
}

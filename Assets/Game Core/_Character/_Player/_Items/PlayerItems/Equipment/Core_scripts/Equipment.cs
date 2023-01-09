using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Inventory/Equipment")]
public class Equipment : Item {

    public EquipmentSlot equipmentSlot;
    [field: SerializeField]
    public List<ItemBaseStat> ActiveBaseStats { get; private set; } = new List<ItemBaseStat>();
    [SerializeField]
    private List<ItemBaseStat> rollableStats = new List<ItemBaseStat>();

    [SerializeField] private int minBaseStats = 2;
    [SerializeField] private int maxBaseStats = 5;

    private int baseStatsRolled = 0;
    private int resistancesRolled = 0;

    [SerializeField] private float minBaseStatPercentualValue;

    [field: SerializeField]
    public List<Enchantment> ActiveEnchantments { get; private set; } = new List<Enchantment>();
    private List<Enchantment> rollableEnchantments;

    private int enchantmentsToRoll;
    private int enchantmentsRolled;

    public override bool Use() {
        if (!CanUse()) return false;

        if (EquipmentManager.Instance.Equip(this)) {
            _ = base.Use();
            RemoveFromInventoryOrDestack();
        } else
            return false;

        return true;
    }

    public override void OnItemClickOperation(ItemClickOperation itemClickOperation) {
        if (itemClickOperation == ItemClickOperation.LeftDoubleClick) {
            _ = Use();
        } else if (!IsIdentified && itemClickOperation == ItemClickOperation.RightSingleClick) {
            RandomizeItemProperties();
            AdvancedTooltip.Instance.ShowTooltip(GetTooltip());
        }
    }

    public virtual void OnValidate() {
        if (IsIdentified) IsIdentified = false;

        for (int i = 0; i < ActiveBaseStats.Count; i++) {
            if (ActiveBaseStats[i].GetStatMultiplier() == 0) ActiveBaseStats[i].SetStatMultiplier(1);

            if (IsStatResistance(ActiveBaseStats[i].GetStatType())) {
                ActiveBaseStats[i].isResistance = true;
            } else {
                ActiveBaseStats[i].isResistance = false;
            }
        }

        for (int i = 0; i < rollableStats.Count; i++) {
            if (rollableStats[i].GetStatMultiplier() == 0) rollableStats[i].SetStatMultiplier(1);

            if (IsStatResistance(rollableStats[i].GetStatType())) {
                rollableStats[i].isResistance = true;
            } else {
                rollableStats[i].isResistance = false;
            }
        }

        if (minBaseStats == 0) minBaseStats = 2;
        if (maxBaseStats == 0) maxBaseStats = 5;

        /*for (int i = 0; i < ActiveEnchantments.Count; i++) {
           ActiveEnchantments[i].SetBaseValue(0);
       }*/
    }

    private bool IsStatResistance(CharacterStatType statType) {
        switch (statType) {
            case CharacterStatType.FireResistance:
                return true;
            case CharacterStatType.IceResistance:
                return true;
            case CharacterStatType.LightningResistance:
                return true;
            case CharacterStatType.PoisonResistance:
                return true;
            default:
                return false;
        }
    }

    public void IncreaseMinBaseStatPercentage(float value) {
        Mathf.Clamp01(minBaseStatPercentualValue += value);
    }

    public void RandomizeItemProperties() {
        EventManager evManager = EventManager.Instance;

        if (evManager != null) {
            evManager.OnEquipmentPreBaseStatsRandomization?.Invoke(this);
        }
        Profiler.BeginSample("item values randomization");
        RandomizeBaseStats();

        RandomizeEnchantments();

        RecalculateProperties();

        CalculateStatMultiplierValues();
        Profiler.EndSample();
        IsIdentified = true;
    }

    public void RandomizeBaseStats() {
        if (ActiveBaseStats.Count > 0) {
            RandomizePresentStats();
        }

        int baseStatsToRoll = Random.Range(minBaseStats, maxBaseStats + 1);

        baseStatsRolled = ActiveBaseStats.Count;

        if (ActiveBaseStats.Count >= baseStatsToRoll) return;

        for (int i = ActiveBaseStats.Count; i < baseStatsToRoll; i++) {

            if (rollableStats.Count == 0)
                break;

            int randomStat = Random.Range(0, rollableStats.Count);

            if (rollableStats[randomStat].isResistance != true) {

                ActiveBaseStats.Add(RandomizeStatValue(rollableStats[randomStat]));

                rollableStats.RemoveAt(randomStat);

                baseStatsRolled += 1;
            } else {
                RandomizeResistances();

                baseStatsRolled += 1;
            }
        }
    }

    private ItemBaseStat RandomizeStatValue(ItemBaseStat itembaseStat) {

        var minMax = DataStorage.GetBaseStatMinMaxValues(itembaseStat.GetStatType());

        itembaseStat.SetMinMax(minMax.min, minMax.max);

        minBaseStatPercentualValue = Mathf.Clamp(minBaseStatPercentualValue, 0, 1);

        itembaseStat.SetBaseValue(
            Random.Range(
                itembaseStat.GetMin() + ((itembaseStat.GetMax() - itembaseStat.GetMin()) * minBaseStatPercentualValue),
            itembaseStat.GetMax()
            ));

        itembaseStat.SetPercentageValue((itembaseStat.GetBaseValue() - itembaseStat.GetMin())
           / (itembaseStat.GetMax() - itembaseStat.GetMin()));

        float decrementalModifier = Mathf.Clamp(itembaseStat.GetPercentageValue() - 0.5f, 0f, 0.5f);
        if (decrementalModifier > 0) {
            float halfDecrementalModifier = decrementalModifier * 0.5f;

            float finalDecrementalValue = Random.Range(0, halfDecrementalModifier);

            if (decrementalModifier >= Random.Range(0f, 1f)) finalDecrementalValue += halfDecrementalModifier;

            itembaseStat.SetBaseValue(itembaseStat.GetBaseValue() - ((itembaseStat.GetMax() - itembaseStat.GetMin()) * finalDecrementalValue));
            itembaseStat.SetPercentageValue(itembaseStat.GetPercentageValue() - finalDecrementalValue);
            itembaseStat.SetFinalDecrementalMod(finalDecrementalValue);
        }

        if (itembaseStat.GetStatMultiplier() > 0) itembaseStat.SetBaseValue(itembaseStat.GetBaseValue() * itembaseStat.GetStatMultiplier());

        return itembaseStat;
    }

    private void RandomizePresentStats() {
        for (int i = 0; i < ActiveBaseStats.Count; i++) {

            _ = RandomizeStatValue(ActiveBaseStats[i]);

            rollableStats.Remove(rollableStats.Find(x => x.GetStatType() == ActiveBaseStats[i].GetStatType()));

            if (ActiveBaseStats[i].isResistance == true) {
                resistancesRolled += 1;
            }
        }
    }
    private void RandomizeResistances() {

        List<ItemBaseStat> possibleResistances = new List<ItemBaseStat>();

        for (int i = 0; i < rollableStats.Count; i++) {
            if (rollableStats[i].isResistance == true) {
                if (!ActiveBaseStats.Exists(x => x.GetStatType() == rollableStats[i].GetStatType())) {
                    possibleResistances.Add(rollableStats[i]);
                }
                rollableStats.RemoveAt(i);
                i--;
            }
        }

        if (possibleResistances.Count > 0) {

            int resistancesToRoll = Random.Range(1, possibleResistances.Count);

            for (int i = 0; i < resistancesToRoll; i++) {
                int randomStat = Random.Range(0, possibleResistances.Count);

                ActiveBaseStats.Add(RandomizeStatValue(possibleResistances[randomStat]));

                possibleResistances.RemoveAt(randomStat);

                resistancesToRoll--;
                i--;
                resistancesRolled += 1;
            }
        }
    }

    public void RandomizeEnchantments() {

        rollableEnchantments = DataStorage.GetRollableEnchantments(ActiveBaseStats);

        enchantmentsToRoll = itemRarity.RarityToEnchantmentCount();

        if (ActiveEnchantments.Count > 0) {
            RecalculatePresentEnchantments();
        }

        if (rollableEnchantments == null || enchantmentsToRoll <= enchantmentsRolled) return;

        for (int i = enchantmentsRolled; i < enchantmentsToRoll; i++) {
            int randomEnchIndex = Random.Range(0, rollableEnchantments.Count);

            if (!rollableEnchantments[randomEnchIndex].IsResistanceEnchantment()) {
                ActiveEnchantments.Add(RandomizeEnchantmentValue(rollableEnchantments[randomEnchIndex].GetCopy()));
                enchantmentsRolled++;
            } else {
                RandomizeResistanceEnchantments();
                enchantmentsRolled++;
            }
        }
    }

    private void RecalculatePresentEnchantments() {
        for (int i = 0; i < enchantmentsToRoll; i++) {
            if (!ActiveEnchantments[i].IsResistanceEnchantment()) {
                ActiveEnchantments[i] = ActiveEnchantments[i].GetCopy();
                _ = RandomizeEnchantmentValue(ActiveEnchantments[i]);
                enchantmentsRolled += 1;
            } else {
                List<BaseStatMultiplierEnchantment> presentResistanceEnchs = new List<BaseStatMultiplierEnchantment>();

                BaseStatMultiplierEnchantment bs = ActiveEnchantments[i] as BaseStatMultiplierEnchantment;
                if (bs == null) continue;

                bool canLoop = true;
                if (bs.GetEnchantmentBoostValue() == 0f) {
                    enchantmentsRolled += 1;
                    ActiveEnchantments[i] = bs.GetCopy();
                    bs = ActiveEnchantments[i] as BaseStatMultiplierEnchantment;
                    _ = RandomizeEnchantmentValue(bs);
                    presentResistanceEnchs.Add(bs);
                } else {
                    canLoop = false;
                }

                float enchVal = bs.GetEnchantmentBoostValue();
                float percentageEnchVal = bs.GetPercentageValue();

                for (int j = 0; j < ActiveBaseStats.Count; j++) {
                    if (!canLoop) break;
                    if (ActiveBaseStats[j].isResistance) {
                        int enchIndex = rollableEnchantments.FindIndex(x => x is BaseStatMultiplierEnchantment bs
                        && bs.GetStatType() == ActiveBaseStats[j].GetStatType()
                        && !presentResistanceEnchs.Exists(x => x.GetStatType() == bs.GetStatType()));

                        if (enchIndex == -1) continue;

                        Enchantment ench = rollableEnchantments[enchIndex].GetCopy();
                        ench.SetBaseValue(enchVal);
                        ench.SetPercentageValue(percentageEnchVal);
                        ActiveEnchantments.Add(ench);
                    }
                }
            }
        }
    }

    private Enchantment RandomizeEnchantmentValue(Enchantment enchantment) {
        if (!enchantment.IsCopy) {
            throw new System.Exception("You are trying to randomize value of enchantment that is not a copy. This is forbidden.");
        }

        enchantment.SetBaseValue(Random.Range(enchantment.GetMin(), enchantment.GetMax()));

        if (enchantment is BaseStatMultiplierEnchantment bs) {
            bs.SetPercentageValue((bs.GetEnchantmentBoostValue() - bs.GetMin()) / (bs.GetMax() - bs.GetMin()));
        } else {
            enchantment.SetPercentageValue((enchantment.GetBaseValue() - enchantment.GetMin()) / (enchantment.GetMax() - enchantment.GetMin()));
        }

        return enchantment;
    }

    private void RandomizeResistanceEnchantments() {
        for (int i = 0; i < ActiveBaseStats.Count; i++) {
            if (ActiveBaseStats[i].isResistance) {
                float enchVal = 0;
                float percentageEnchVal = 0;

                for (int j = 0; j < ActiveBaseStats.Count; j++) {
                    if (ActiveBaseStats[j].isResistance) {
                        int index = rollableEnchantments.FindIndex(x => x is BaseStatMultiplierEnchantment bs
                        && bs.GetStatType() == ActiveBaseStats[j].GetStatType());

                        if (index != -1) {
                            BaseStatMultiplierEnchantment bs = rollableEnchantments[index].GetCopy() as BaseStatMultiplierEnchantment;

                            if (enchVal == 0) {
                                RandomizeEnchantmentValue(bs);
                                enchVal = bs.GetEnchantmentBoostValue();
                                percentageEnchVal = bs.GetPercentageValue();
                            } else {
                                bs.SetBaseValue(enchVal);
                                bs.SetPercentageValue(percentageEnchVal);
                            }

                            ActiveEnchantments.Add(bs);
                        }
                    }
                }

                break;
            }
        }
    }

    private void CalculateStatMultiplierValues() {
        for (int i = 0; i < ActiveEnchantments.Count; i++) {
            if (ActiveEnchantments[i] is BaseStatMultiplierEnchantment bs) {
                if (bs.GetEnchantmentBoostValue() <= 0f) {
                    RemoveEnchAt(i);
                    continue;
                }

                ItemBaseStat itemBaseStat = ActiveBaseStats.Find(x => x.GetStatType() == bs.GetStatType());
                if (itemBaseStat != null) {
                    bs.CalculateAbsoluteBase(itemBaseStat.GetBaseValue());
                } else {
                    RemoveEnchAt(i);
                }
            } else if (ActiveEnchantments[i].GetBaseValue() <= 0f) {
                RemoveEnchAt(i);
            }

            void RemoveEnchAt(int index) {
                ActiveEnchantments.RemoveAt(index);
                i--;
            }
        }
    }

    public void RecalculateProperties() {
        if (resistancesRolled > 0) {
            for (int i = 0; i < ActiveBaseStats.Count; i++) {
                if (ActiveBaseStats[i].isResistance == true) {
                    ActiveBaseStats[i].SetBaseValue(ActiveBaseStats[i].GetBaseValue() * (5 - resistancesRolled));
                }
            }
        }

        float finalBaseStatMultiplier = 0.5f + maxBaseStats - baseStatsRolled;
        for (int i = 0; i < ActiveBaseStats.Count; i++) {
            ActiveBaseStats[i].SetBaseValue(ActiveBaseStats[i].GetBaseValue() * finalBaseStatMultiplier);
        }
    }

    public override void BuildTooltipText() {
        StringBuilder sb = new StringBuilder();

        ItemExtensions.AppendCoreItemDetails(sb, this);
        sb.AppendLine($"<size=18>Slot: {equipmentSlot}</size>");

        if (!IsIdentified) {
            sb.AppendLine();
            sb.AppendLine("<color=red>This item is not yet identified.</color>");
            itemTooltip = sb;
            return;
        }

        sb.AppendLine();
        sb.AppendLine("<b>Base stats</b>");
        sb.AppendLine($"<size=18>Base stats rolled: {baseStatsRolled}");
        for (int i = 0; i < ActiveBaseStats.Count; i++) {
            sb.Append(ActiveBaseStats[i].GetBaseValue() > 0 ? "+ " : " ")
                .AppendLine($"{ActiveBaseStats[i].GetBaseValue().StatValueToFormattedString(ActiveBaseStats[i].GetStatType())} " +
                $"{ActiveBaseStats[i].GetStatType().StatTypeToReadableString()} ({string.Format("{0:0.00}%", ActiveBaseStats[i].GetPercentageValue() * 100)})");
        }
        sb.Append("</size>");

        _ = sb.AppendLine();
        sb.AppendLine("<b>Enchantments</b>");
        sb.AppendLine($"<size=18>Enchantments rolled: {enchantmentsRolled}");
        List<Enchantment> includedEnchs = new List<Enchantment>();
        for (int i = 0; i < ActiveEnchantments.Count; i++) {
            BaseStatEnchantment baseStatEnchantment = ActiveEnchantments[i] as BaseStatEnchantment;
            if (baseStatEnchantment != null && ActiveEnchantments[i] is BaseStatMultiplierEnchantment bs) {
                if (includedEnchs.Contains(ActiveEnchantments[i])) continue;

                if (!bs.IsResistanceEnchantment()) {
                    sb.AppendLine($"{string.Format("+ {0:0.00}", bs.GetEnchantmentBoostValue() * 100)}% " +
                        $"({string.Format("+ {0:0.00}", ActiveEnchantments[i].GetBaseValue().StatValueToFormattedString(baseStatEnchantment.GetStatType()))}) " +
                    $"{baseStatEnchantment.GetStatType().StatTypeToReadableString()} on this item ({string.Format("{0:0.00}%", ActiveEnchantments[i].GetPercentageValue() * 100)})");
                } else {
                    int resStatCount = ActiveBaseStats.Count(x => x.isResistance);
                    //Debug.Log(resStatCount + ", " + name);
                    int resEnchCounted = 0;

                    sb.Append($"{string.Format("+ {0:0.00}", bs.GetEnchantmentBoostValue() * 100)}% (+ ");

                    for (int j = i; j < ActiveEnchantments.Count; j++) {
                        if (ActiveEnchantments[j].IsResistanceEnchantment()) {
                            resEnchCounted++;
                            if (resEnchCounted > resStatCount) break;
                            includedEnchs.Add(ActiveEnchantments[j]);
                            sb.Append($"<color=#{(ActiveEnchantments[j] as BaseStatEnchantment).GetStatType().ResistanceStatToColorRGB()}>" +
                                $"{string.Format("{0:0.00}", ActiveEnchantments[j].GetBaseValue().StatValueToFormattedString(baseStatEnchantment.GetStatType()))}</color>");
                            if (resEnchCounted < resStatCount) {
                                sb.Append(", ");
                            }
                        }
                    }

                    //i += resStatCount - 1;
                    sb.AppendLine($") of all resistances on this item ({string.Format("{0:0.00}%", ActiveEnchantments[i].GetPercentageValue() * 100)})");
                }
            } else {
                sb.AppendLine($"+ {ActiveEnchantments[i].GetBaseValue().StatValueToFormattedString(baseStatEnchantment.GetStatType())} " +
                    $"{baseStatEnchantment.GetStatType().StatTypeToReadableString()} ({string.Format("{0:0.00}%", ActiveEnchantments[i].GetPercentageValue() * 100)})");
            }
        }
        sb.Append("</size>");

        itemTooltip = sb;
    }
}





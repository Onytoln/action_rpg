using System.Collections.Generic;
using System.Text;

public static class AbilityExtensions {

    public static void AppendDamageTypeTooltips(StringBuilder sb, List<DamageTypeWeight> damageTypes) {
        int damageTypesCount = 0;
        for (int i = 0; i < damageTypes.Count; i++) {
            sb.Append($"<color=#{damageTypes[i].damageType.DamageTypeToColorRGB()}>{damageTypes[i].damageType} damage " +
                $"({damageTypes[i].damageWeight.StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)})</color>");
            if (damageTypes.Count > 1 && i != damageTypes.Count - 1) sb.Append(", ");
            damageTypesCount++;
            if (damageTypesCount > 1 && damageTypesCount < 999) { sb.AppendLine(); damageTypesCount = 999; }
        }
    }

    public static void AppendBenefitTooltips(StringBuilder sb, AbilityProperties ab, bool postDescriptionSpace = true) {
        if (ab.BenefitFromCriticalStrike.Length > 0 || ab.BenefitFromPenetration.Length > 0) {
            sb.AppendLine();
            sb.Append("Benefits from: ");
        } else {
            return;
        }

        bool noBenefits = true;

        bool valueOverZero = false;
        for (int i = 0; i < ab.BenefitFromCriticalStrike.Length; i++) {
            if (ab.BenefitFromCriticalStrike[i].CriticalStrikeBenefit.GetValue() > 0f) {
                valueOverZero = true;
                break;
            }
        }

        if (ab.BenefitFromCriticalStrike.Length == 4 && valueOverZero) {
            noBenefits = false;
            sb.AppendLine();
            bool prevAppended = false;

            for (int i = 0; i < ab.BenefitFromCriticalStrike.Length; i++) {
                if (ab.BenefitFromCriticalStrike[i].CriticalStrikeBenefit.GetValue() <= 0) continue;

                if (prevAppended) sb.Append(", ");
                sb.Append(string.Format(ab.BenefitFromCriticalStrike[i].CriticalStrikeBenefitType.CritBenefitToCustomizedString(),
                    ab.BenefitFromCriticalStrike[i].CriticalStrikeBenefit.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)));
                prevAppended = true;
            }
        }

        valueOverZero = false;

        for (int i = 0; i < ab.BenefitFromPenetration.Length; i++) {
            if (ab.BenefitFromPenetration[i].PenetrationBenefit.GetValue() > 0f) {
                valueOverZero = true;
                break;
            }
        }

        if (ab.BenefitFromPenetration.Length == 5 && valueOverZero) {
            noBenefits = false;
            sb.AppendLine();
            bool prevAppended = false;
            for (int i = 0; i < ab.BenefitFromPenetration.Length; i++) {
                if (ab.BenefitFromPenetration[i].PenetrationBenefit.GetValue() <= 0) continue;

                if (prevAppended) sb.Append(", ");
                sb.Append($"{ab.BenefitFromPenetration[i].PenetrationBenefitType.StatTypeToReadableString()} " +
                    $"+{ab.BenefitFromPenetration[i].PenetrationBenefit.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Percentage, 0)} (absolute)");

                prevAppended = true;
            }
        }

        if (noBenefits) sb.Append("no crit or penetration benefits.");

        if (postDescriptionSpace) sb.AppendLine();
    }

    public static void AppendEndCore(StringBuilder sb, SkillProperties skillProperties) {
        sb.AppendLine($"Cast time: {skillProperties.castTime.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute, 2)} s");
        sb.AppendLine($"Mana cost: {skillProperties.manaCost.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute, 0)}");
        sb.Append($"Cooldown: {skillProperties.cooldown.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute, 2)} s");
    }

    public static void AppendChargesTooltips(StringBuilder sb, ChargeSystem chargeSystem, float cooldown) {
        if (!chargeSystem.ChargeSystemBeingUsed()) return;

        sb.AppendLine();
        sb.AppendLine($"This ability has {chargeSystem.MaxCharges.GetValue()} charges.");
        if (chargeSystem.ChargeReplenishmentType == ChargeReplenishmentType.OneByOne) {
            sb.AppendLine($"Replenish {chargeSystem.DefaultChargesReplenishmentRateOneByOne.GetValue()} charge per" +
                $" {cooldown.StatValueToStringByStatStringType(StatStringType.Absolute)} s.");
        } else if (chargeSystem.ChargeReplenishmentType == ChargeReplenishmentType.AllAtOnce) {
            sb.AppendLine($"When all charges are used, replenish all charges in {cooldown.StatValueToStringByStatStringType(StatStringType.Absolute)} s.");
        }
    }

    public static void AppendBuffDescriptionNoStart(StringBuilder sb, BuffHolder[] bh,
        SEApplicationStringStyle style = SEApplicationStringStyle.None, params string[] text) {

        AppendBuffDescription(sb, bh, style: style, text: text);
    }

    public static void AppendBuffDescription(StringBuilder sb, BuffHolder[] bh, int startIndex = 0,
        SEApplicationStringStyle style = SEApplicationStringStyle.None, params string[] text) {
        if (bh.Length == 0) return;

        StringBuilder textToAppend = new StringBuilder();

        int appended = 0;
        textToAppend.AppendLine();
        textToAppend.AppendLine(SEApplicationString(style, "buff", text));
        textToAppend.AppendLine("-----------------------------------------------------------");

        for (int i = startIndex; i < bh.Length; i++) {
            if (!bh[i].ShowInTooltip || bh[i].stacksToApply.GetValue() <= 0) continue;
            textToAppend.AppendLine(bh[i].buffToApply.StatusEffectProperties.GetTooltip().ToString());
            textToAppend.AppendLine("-----------------------------------------------------------");
            appended++;
        }

        if (appended > 0) sb.Append(textToAppend.ToString());
    }

    public static void AppendBuffDescription(StringBuilder sb, BuffHolder bh) {
        sb.AppendLine("-----------------------------------------------------------");
        sb.AppendLine(bh.buffToApply.StatusEffectProperties.GetTooltip().ToString());
        sb.AppendLine("-----------------------------------------------------------");
    }

    public static void AppendDebuffDescriptionNoStart(StringBuilder sb, DebuffHolder[] dh,
        SEApplicationStringStyle style = SEApplicationStringStyle.None, params string[] text) {

        AppendDebuffDescription(sb, dh, style: style, text: text);
    }

    public static void AppendDebuffDescription(StringBuilder sb, DebuffHolder[] dh, int startIndex = 0,
    SEApplicationStringStyle style = SEApplicationStringStyle.None, params string[] text) {
        if (dh.Length == 0) return;

        StringBuilder textToAppend = new StringBuilder();

        int appended = 0;
        textToAppend.AppendLine();
        textToAppend.AppendLine(SEApplicationString(style, "debuff", text));
        textToAppend.AppendLine("-----------------------------------------------------------");

        for (int i = startIndex; i < dh.Length; i++) {
            if (!dh[i].ShowInTooltip || dh[i].stacksToApply.GetValue() <= 0) continue;
            textToAppend.AppendLine(dh[i].debuffToApply.StatusEffectProperties.GetTooltip().ToString());
            textToAppend.AppendLine("-----------------------------------------------------------");
            appended++;
        }

        if (appended > 0) sb.Append(textToAppend.ToString());
    }

    public static void AppendDebuffDescription(StringBuilder sb, DebuffHolder dh) {
        sb.AppendLine("-----------------------------------------------------------");
        sb.AppendLine(dh.debuffToApply.StatusEffectProperties.GetTooltip().ToString());
        sb.AppendLine("-----------------------------------------------------------");
    }

    public static void AppendSkillTypes(StringBuilder sb, SkillType[] st) {
        for (int i = 0; i < st.Length; i++) {
            sb.Append(st[i]);
            if (i < st.Length - 1) sb.Append(", ");
        }

        sb.AppendLine();
    }

    public static string SEApplicationString(SEApplicationStringStyle style, string seType, params string[] text) {
        string result = string.Empty;

        if (text == null || style == SEApplicationStringStyle.None) return result;

        switch (style) {
            case SEApplicationStringStyle.OnHit:
                if (text.Length == 2) {
                    result = $"Applies {text[0]} {seType} ({text[1]} stacks) on direct hit.";
                }
                break;
            case SEApplicationStringStyle.OnHitTwice:
                if (text.Length == 4) {
                    result = $"Applies {text[0]} {seType} on direct hit ({text[1]} stack/s) and on {text[2]} ({text[3]} stack/s).";
                }
                break;
            case SEApplicationStringStyle.OnCastEnd:
                if (text.Length == 2) {
                    result = $"Applies {text[0]} {seType} ({text[1]} stacks) when finished casting.";
                }
                break;
            default:
                break;
        }

        return result;
    }


}

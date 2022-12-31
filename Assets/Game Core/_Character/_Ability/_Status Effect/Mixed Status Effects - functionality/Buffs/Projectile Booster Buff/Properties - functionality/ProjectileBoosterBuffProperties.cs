using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Booster Buff Properties", menuName = "Status Effects/Buff Properties/Projectile Booster Buff Properties")]
public class ProjectileBoosterBuffProperties : StatusEffectProperties {
    public StatInt bonusProjectilePerStacks;
    public StatInt bonusScalePerStacks;
    public StatFloat bonusScaleValue;
    [field: SerializeField] public BoolControlComplex RemoveOnSkillFired { get; private set; } = new BoolControlComplex(BoolType.True, BoolCountEqualResult.Default);

    [field: SerializeField] public string SkillToBoostId { get; private set; }
    public SkillProperties SkillToBoostProperties { private get; set; }

    protected override void BuildTooltipText() {
        base.BuildTooltipText();

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        sb.AppendLine($"Increases projectile amount for your {SkillToBoostProperties.name} ability by 1 per every {bonusProjectilePerStacks.GetValue()} stacks");
        sb.AppendLine($"and projectile scale by {bonusScaleValue.GetValue().StatValueToStringByStatStringType(StatStringType.Absolute)}" +
            $" meters per {bonusScalePerStacks.GetValue()} stacks");

        sb.AppendLine();
        sb.AppendLine($"Casting {SkillToBoostProperties.name} will consume this buff fully.");

        sb.AppendLine();
        sb.Append($"Lasts for {duration.GetValue().StatValueToStringByStatStringTypeNoSpace(StatStringType.Seconds)}, refreshed upon re-application" +
            $" and can be stacked up to {maxStacks.GetValue()} stacks.");

        sb.Append("</size>");
        abilityTooltip = sb;
    }

    protected override void BuildCombatTooltip(StatusEffect se) {
        base.BuildCombatTooltip(se);

        StringBuilder sb = new StringBuilder();
        sb.Append($"<size={DataStorage.DefaultStatusEffectNameFontSize}><b>").Append($"{name}").AppendLine("</b></size>");
        sb.AppendLine($"<size={DataStorage.DefaultStatusEffectDescriptionFontSize}>");
        ProjectileBoosterBuff pbb = se as ProjectileBoosterBuff;
        sb.Append($"Increases {SkillToBoostProperties.name} ability projectile amount by {pbb.AddedProjectiles} and scale by" +
            $" {pbb.AddedScaleValue.StatValueToStringByStatStringTypeNoSpace(StatStringType.Meters)}.");

        sb.Append("</size>");
        combatTooltip = sb;
    }
}

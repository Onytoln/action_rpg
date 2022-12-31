using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class StatElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public StatType statTypeToAssign;
    public StatStringType statStringType;

    private CharacterStat assignedStat;

    [SerializeField] private Text statNameText;
    [SerializeField] private Text statValueText;

    private StringBuilder statTooltip;

    private bool isDirty = true;

    public void AddStat(CharacterStat stat) {
        assignedStat = stat;
        statNameText.text = stat.statName;
        RefreshStatValue();
    }

    public void RefreshStatValue() {
        isDirty = true;

        if (!(assignedStat is ScalableStatBase)) {
            statValueText.text = assignedStat.GetValue().StatValueToStringByStatStringType(statStringType);
        } else {
            statValueText.text = assignedStat.TotalUnscalableValue.StatValueToStringByStatStringType(StatStringType.Absolute)
                + $" ({assignedStat.GetValue().StatValueToStringByStatStringType(statStringType)})";
        }

        if (AdvancedTooltip.Instance.CompareShownTooltip(statTooltip)) {
            OnPointerEnter(null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (assignedStat == null) return;

        if (isDirty) {
            isDirty = false;
            StringBuilder sb = new StringBuilder();
            ScalableStatBase scalableStat = assignedStat as ScalableStatBase;
            Color color = new Color32(215, 58, 29, 255);
            sb.Append($"<size=35><color=#{ColorUtility.ToHtmlStringRGB(color)}>").Append(assignedStat.statName).AppendLine("</color></size>");
            sb.AppendLine();
            color = new Color32(166, 169, 56, 255);
            if (assignedStat.GetPrimaryValue() != 0) {
                //primary values
                if (scalableStat == null) {
                    sb.Append($"Primary value: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                        .Append(assignedStat.GetPrimaryValue().StatValueToStringByStatStringType(statStringType)).AppendLine("</color>");
                } else {
                    sb.Append($"Primary value: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                        .Append(assignedStat.GetPrimaryValue().StatValueToStringByStatStringType(StatStringType.Absolute)).AppendLine("</color>");
                }
            }

            //absolute mods
            if (scalableStat == null) {
                sb.Append($"Total value of absolute modifiers: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                    .Append(assignedStat.TotalAbsoluteMods.StatValueToStringByStatStringType(statStringType)).AppendLine("</color>");
            } else {
                sb.Append($"Total value of absolute modifiers: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                   .Append(assignedStat.TotalAbsoluteMods.StatValueToStringByStatStringType(StatStringType.Absolute)).AppendLine("</color>");
            }

            //relative and total mods
            sb.Append($"Total value of relative modifiers: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                .Append(assignedStat.TotalRelativeMods.StatValueToStringByStatStringType(StatStringType.Percentage)).AppendLine("</color>");
            sb.Append($"Total value of total modifiers: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                .Append(assignedStat.TotalTotalMods.StatValueToStringByStatStringType(StatStringType.Percentage)).AppendLine("</color>");

            if (scalableStat == null) {
                sb.Append($"Stat total value: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                .Append(assignedStat.GetValue().StatValueToStringByStatStringType(statStringType)).AppendLine("</color>");
            } else {
                sb.Append($"Stat total value: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
               .Append(assignedStat.TotalUnscalableValue.StatValueToStringByStatStringType(StatStringType.Absolute)).AppendLine("</color>");
            }

            //scalable scaled value and value for max
            if (scalableStat != null) {
                sb.AppendLine();

                sb.Append($"Stat total scaled value: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                 .Append(assignedStat.GetValue().StatValueToStringByStatStringType(statStringType)).AppendLine("</color>");

                sb.Append($"Value required to reach maximum scaled value: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                    .Append(scalableStat.GetValueNeededForMaxScaledValue()).AppendLine("</color>");
            }

            //total value
            if (scalableStat == null) {
                if (assignedStat.GetMaxPossibleValue() != float.PositiveInfinity) {
                    sb.AppendLine();

                    sb.Append($"Stat total value is capped at: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
                    .Append(assignedStat.GetMaxPossibleValue().StatValueToStringByStatStringType(statStringType)).AppendLine("</color>");
                }
            } else {
                sb.AppendLine();

                sb.Append($"Stat total scaled value is capped at: <color=#{ColorUtility.ToHtmlStringRGB(color)}>")
               .Append(assignedStat.GetMaxPossibleValue().StatValueToStringByStatStringType(statStringType)).AppendLine("</color>");
            }

            if (!string.IsNullOrEmpty(assignedStat.statDescription)) {
                sb.AppendLine();
                sb.Append(assignedStat.statDescription);
            }

            statTooltip = sb;
        }

        AdvancedTooltip.Instance.ShowTooltip(statTooltip);
    }

    public void OnPointerExit(PointerEventData eventData) {
        AdvancedTooltip.Instance.HideTooltip();
    }

}

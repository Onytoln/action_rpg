using UnityEngine;
using UnityEngine.UI;


public class SkillInventorySlot : MonoBehaviour
{
    public PlayerSkillTemplate assignedSkill { get; private set; }

    [SerializeField]
    private SkillInventoryDrag skillDragComponent;
    public SkillInventoryDrag SkillDragComponent {
        get => skillDragComponent;
    }

    [SerializeField]
    private Image skillIcon;
    public Image SkillIcon { get => skillIcon; }
    [SerializeField]
    private Image talentIcon_1;
    [SerializeField]
    private Image talentIcon_2;
    [SerializeField]
    private Image talentIcon_3;

    public void AddSkill(PlayerSkillTemplate skill) {
        assignedSkill = skill;
        skillIcon.sprite = assignedSkill.skillProperties.icon;
    }

    public void UnlockSkill() {

    }
}

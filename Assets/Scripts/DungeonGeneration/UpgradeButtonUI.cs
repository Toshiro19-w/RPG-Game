using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;

    private KeyCode assignedSkillKey;
    private UpgradePanelManager panelManager;

    public void Setup(KeyCode skillKey, Sprite icon, string name, int cost, UpgradePanelManager manager)
    {
        assignedSkillKey = skillKey;
        skillIcon.sprite = icon;
        skillNameText.text = name;
        costText.text = cost.ToString();
        panelManager = manager;

        upgradeButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        AudioManager.Instance.Play("click");
        panelManager.AttemptUpgrade(assignedSkillKey);
    }
}
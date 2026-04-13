using TMPro;
using UnityEngine;

public class UpgradeZoneView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleLabel;
    [SerializeField] private TextMeshProUGUI priceLabel;

    public void SetState(string title, int price, int level, int maxLevel)
    {
        if (titleLabel != null)
            titleLabel.text = title;

        if (priceLabel != null)
            priceLabel.text = $"E Upgrade {price} ({level}/{maxLevel})";
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}

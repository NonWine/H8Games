using TMPro;
using UnityEngine;

public class WinLoseView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleLabel;

    public void Show(CombatResult result)
    {
        if (root != null)
            root.SetActive(true);

        if (titleLabel != null)
            titleLabel.text = result == CombatResult.Win ? "Victory" : "Defeat";
    }
}

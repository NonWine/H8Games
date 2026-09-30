using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class BarracksRevealDebugTrigger : MonoBehaviour
{
    private static readonly FieldInfo BarracksStatsField = typeof(SquadBarracksSpawner)
        .GetField("barracksStats", BindingFlags.NonPublic | BindingFlags.Instance);

    [SerializeField] private SquadBarracksSpawner barracksSpawner;
    [SerializeField] private Key triggerKey = Key.F11;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[triggerKey].wasPressedThisFrame)
            Trigger();
    }

    [ContextMenu("Trigger Barracks Reveal")]
    public void Trigger()
    {
        if (barracksSpawner == null)
            return;

        BarracksStats stats = (BarracksStats)BarracksStatsField.GetValue(barracksSpawner);
        if (stats != null && stats.IsMaxLevel)
            stats.ResetRuntimeState();

        barracksSpawner.UpgradeLevel();
    }
}

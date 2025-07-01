using UnityEngine;

public class WiringDevice : Device
{
    private WiringManager wiringManager;

    protected override void UpdateStatusDisplay()
    {
        if (statusText == null) return;
        statusText.text = deviceName;
    }

    public override void StartMinigame()
    {
        wiringManager = WiringManager.Instance;
        if (wiringManager == null)
        {
            Debug.LogError("WiringDevice: WiringManager não encontrado na cena.");
            FailDevice();
            return;
        }

        wiringManager.OpenMinigame(WiringManager.CableVariation.StraightThrough, OnMinigameFinished);
    }

    private void OnMinigameFinished(bool success)
    {
        wiringManager.OnMinigameCompleted -= OnMinigameFinished;

        if (success)
        {
            CompleteDevice();
        }
        else
        {
            FailDevice();
        }
    }
} 
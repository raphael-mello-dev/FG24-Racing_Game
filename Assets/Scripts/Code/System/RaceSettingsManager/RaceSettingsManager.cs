using UnityEngine;

public enum RaceMode
{
    Arcade = 0,
    Custom = 1,
    Tournament = 2,
    Training = 3
}

public class RaceSettingsManager : MonoBehaviour
{
    public static RaceSettingsManager Instance;

    public RaceMode CurrentMode;
    public int LapsAmount;

    // Number of car oponents
    public int CarsOpAmount;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetOponentsAmount()
    {
        if (CurrentMode == RaceMode.Training)
            CarsOpAmount = 0;
        else
        {
            int[] op = { 4, 6, 8 };
            int rd = Random.Range(0, 2);
            CarsOpAmount = op[rd] - 1;
        }
    }

    public static void GetRaceMode(int mode) => Instance.CurrentMode = (RaceMode) mode;
}
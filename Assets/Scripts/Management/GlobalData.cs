using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GlobalData : MonoBehaviour, ISaveSystem
{
    public static GlobalData instance;

    #region Overall statistic
    private int totalTreatments;
    public int TotalTreatments { get { return totalTreatments; } set { totalTreatments = value; } }

    private int totalPatientsHealed;
    public int TotalPatientsHealed { get { return totalPatientsHealed; } set { totalPatientsHealed = value; } }

    private int totalPatientsLost;
    public int TotalPatientsLost { get { return totalPatientsLost; } set { totalPatientsLost = value; } }
    #endregion
    #region Shift statistics
    private int shiftTreatments;
    public int ShiftTreatments { get { return shiftTreatments; } set { shiftTreatments = value; } }

    private int shiftPatientsHealed;
    public int ShiftPatientsHealed { get { return shiftPatientsHealed; } set { shiftPatientsHealed = value; } }

    private int shiftPatientsLost;
    public int ShiftPatientsLost { get { return shiftPatientsLost; } set { shiftPatientsLost = value; } }
    #endregion

    #region variables needed from scene to scene
    private bool isSaveFileLoaded;
    public bool IsSaveFileLoaded { get { return isSaveFileLoaded; } set { isSaveFileLoaded = value; } }

    private int currentLevel = 1;
    public int CurrentLevel { get { return currentLevel; } set { currentLevel = value; } }
    private float currentStressLvl;
    public float CurrentStressLvl { get { return currentStressLvl; } set { currentStressLvl = value; } }
    #endregion

    private void Awake()
    {
        if (instance == null)       //Singleton
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);  //take GlobalData to the next scene
    }

    #region Setting/Resetting statistics

    public void SetPatientDeadStatistics()
    {
        totalPatientsLost++;
        shiftPatientsLost++;
    }

    public void SetPatientHealedStatistics()
    {
        totalTreatments++;
        shiftTreatments++;
        shiftPatientsHealed++;
        TotalPatientsHealed++;
    }

    public void SetPatientTreatmentStatistics()
    {
        shiftTreatments++;
        TotalTreatments++;
    }
    public void ResetTotalStatistics()
    {
        totalTreatments = 0;
        totalPatientsLost = 0;
        totalPatientsHealed = 0;
    }

    public void ResetShiftStatistics()
    {
        shiftPatientsHealed = 0;
        shiftPatientsLost = 0;
        shiftTreatments = 0;
    }
    #endregion

    #region Save/Load methods
    public void SaveData()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/SaveDataGlobalData.carry";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, GlobalData.instance.currentLevel);
        formatter.Serialize(stream, GlobalData.instance.shiftPatientsHealed);
        formatter.Serialize(stream, GlobalData.instance.shiftPatientsLost);
        formatter.Serialize(stream, GlobalData.instance.shiftTreatments);
        formatter.Serialize(stream, GlobalData.instance.totalPatientsHealed);
        formatter.Serialize(stream, GlobalData.instance.totalPatientsLost);
        formatter.Serialize(stream, GlobalData.instance.totalTreatments);
        stream.Close();
    }

    public void LoadData()
    {
        string path = Application.persistentDataPath + "/SaveDataGlobalData.carry";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GlobalData.instance.currentLevel = (int)formatter.Deserialize(stream);
            GlobalData.instance.shiftPatientsHealed = (int)formatter.Deserialize(stream);
            GlobalData.instance.shiftPatientsLost = (int)formatter.Deserialize(stream);
            GlobalData.instance.shiftTreatments = (int)formatter.Deserialize(stream);
            GlobalData.instance.totalPatientsHealed = (int)formatter.Deserialize(stream);
            GlobalData.instance.totalPatientsLost = (int)formatter.Deserialize(stream);
            GlobalData.instance.totalTreatments = (int)formatter.Deserialize(stream);

            stream.Close();

        }
    }
    #endregion
}

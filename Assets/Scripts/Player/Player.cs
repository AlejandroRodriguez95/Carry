using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, ISaveSystem
{


    #region Player variables
    [SerializeField] private float currentStressLvl;
    [Tooltip("This value multiplies the stress level that is carried on to the next level")]
    [Range(0, 1)]
    [SerializeField] private float savedStresslevelReduction;
    public float CurrentStressLvl { get { return currentStressLvl; } set { currentStressLvl = value; } }
    [SerializeField] private float maxStressLvl;
    public float MaxStressLvl { get { return maxStressLvl; } }
    private bool isAtPC;
    public bool IsAtPc { get { return isAtPC; } set { isAtPC = value; } }
    public int NoItemDamage { get { return noItemDamage; } }
    public Item currentItem { get; set; }
    public Patient currentPatient { get; set; }
    public bool IsInContact { get; set; }
    public bool IsDrinkingCoffee { get; set; }
    public bool IsHoldingItem { get; set; }
    #endregion
    [SerializeField] private int noItemDamage;
    [SerializeField] private Camera camera;
    [SerializeField] private Vector3 boxSize = Vector3.one;
    [SerializeField] private Animator animator;
    [SerializeField] Timer gameTime;
    private float destroyTimer = 0;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform destroyPosition;
    public static event Action<bool> e_OnDocumentationStart;
    private bool isLevelComplete;

    public Vector3 boxPos;
    bool isOutside;

    private void Awake()
    {
        IsHoldingItem = false;
        PopUp.e_OnPopUpTimeOut += TimeOutDamage;
        Timer.e_OnLevelCompleteSaveStressLvl += SaveStressLevel;
        Patient.e_onPatientIdleDeath += TimeOutDamage;
        if (SceneManager.GetActiveScene().name == "LevelComplete")
            isLevelComplete = true;
        else
            isLevelComplete = false;
    }



    private void Start()
    {
        //setting the correct stresslvl from scene to scene
        if (SceneManager.GetActiveScene().name == "Level 2" || SceneManager.GetActiveScene().name == "Level 3" ||
            SceneManager.GetActiveScene().name == "Level 4")
        {
            CurrentStressLvl = GlobalData.instance.CurrentStressLvl;
        }
        if (GlobalData.instance.IsSaveFileLoaded)
            LoadData();
    }

    private void Update()
    {
        if (isLevelComplete)
            PlayerLeavesHospital();
    }

    /// <summary>
    /// Player leaves the hospital in the LevelComplete Scene
    /// </summary>
    private void PlayerLeavesHospital()
    {

        float interpolation = 0.3f * Time.deltaTime;
        this.GetComponent<Animator>().SetBool("isWalking", true);
        destroyTimer += Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, destroyPosition.position, interpolation);
        if (destroyTimer >= 6) // the value is the time which will take the patient to be destroyed after being released
        {
            Destroy(gameObject);
            destroyTimer = 0;
        }
    }

    /// <summary>
    /// Interacting with Objects/ with Items and Patients
    /// </summary>
    public void Interact()
    {
        Collider[] objects = Physics.OverlapBox(transform.position + boxPos, boxSize);
        foreach (var obj in objects)
        {
            if (obj.GetComponent<Patient>())
                if(obj.GetComponent<Outline>().OutlineWidth <= 2f)
                    obj.GetComponent<Outline>().OutlineWidth += 0.08f;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var obj in objects)
            {
                if (obj.CompareTag("Item"))
                {
                    SoundManager.instance.PlayAudioClip(ESoundeffects.PickUpItem, GetComponent<AudioSource>());
                    //Pickup
                    animator.Play("Picking Up");
                    IsHoldingItem = true;
                    currentItem = obj.GetComponent<Item>();
                }
                if (obj.GetComponent<SpawnPoint>())
                {
                    obj.GetComponent<SpawnPoint>().IsFree = true;
                }
                if (obj.CompareTag("Patient"))
                {
                    currentPatient = obj.GetComponent<Patient>();
                    IsInContact = true;
                }
                if (obj.GetComponent<Computer>())
                {
                    e_OnDocumentationStart?.Invoke(false);
                    InteractWithLaptop(obj.GetComponent<Computer>());
                }
                if (obj.CompareTag("CoffeeMachine"))
                {
                    animator.Play("Picking Up");
                    IsDrinkingCoffee = true;
                }
            }
        }
    }

    private void InteractWithLaptop(Computer obj)
    {
        if (16 > gameTime.TimeInHours || obj.CanDoComputerThing == false)
            return;

        animator.SetBool("isWalking", false);
        Camera.main.GetComponent<CamPosition>().MovePoint.CameraOnPc = true;
        IsAtPc = true;
        if (obj.GetComponent<Computer>().CurrentPopUp != null)
        {
            Destroy(obj.GetComponent<Computer>().CurrentPopUp);
        }
        obj.GetComponent<Computer>().BeginDocumentation();
        if (Camera.main.GetComponent<CamPosition>().MovePoint.IsCameraFixed)
        {
            Camera.main.GetComponent<CamPosition>().MovePoint.IsCameraFixed = false;
        }
        GetComponent<NewPlayerMovement>().enabled = false;
        Camera.main.transform.rotation = obj.GetComponent<Computer>().DocumentationCamPos.rotation;
    }

    public void DropItem()
    {
        if (Input.GetKeyDown(KeyCode.F) && currentItem != null)
        {
            currentItem = null;
        }
    }

    #region Camera

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Inside"))
        {
            isOutside = true;
            StartCoroutine(ReduceStressLevel());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Inside"))
            isOutside = false;

        if (other.GetComponent<CamColliders>() != null)
        {
            Transform newPos = other.GetComponent<CamColliders>().NewPosition;
            if (camera.GetComponent<CamPosition>().currentPoint != newPos)
            {
                camera.GetComponent<CamPosition>().currentPoint = newPos;
                camera.GetComponent<CamPosition>().lastPoint = newPos;
            }
        }

    }

    #endregion
    #region Setting stress level
    /// <summary>
    /// When the player is outside of the building, the stresslevel will be decreased
    /// </summary>
    /// <returns></returns>
    IEnumerator ReduceStressLevel()
    {
        while (isOutside)
        {
            yield return new WaitForSeconds(3);
            CurrentStressLvl--;
        }
    }

    private void SaveStressLevel()
    {
        GlobalData.instance.CurrentStressLvl = currentStressLvl * savedStresslevelReduction;
    }

    private void TimeOutDamage(float damage)
    {
        CurrentStressLvl += damage;
    }
    #endregion
    #region Save/Load methods
    public void SaveData()
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/SaveDataPlayer.carry";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, currentStressLvl);
        formatter.Serialize(stream, IsInContact);
        formatter.Serialize(stream, IsDrinkingCoffee);
        formatter.Serialize(stream, IsHoldingItem);
        formatter.Serialize(stream, isAtPC);
        formatter.Serialize(stream, isOutside);

        stream.Close();
    }

    public void LoadData()
    {
        string path = Application.persistentDataPath + "/SaveDataPlayer.carry";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            currentStressLvl = (float)formatter.Deserialize(stream);
            IsInContact = (bool)formatter.Deserialize(stream);
            IsDrinkingCoffee = (bool)formatter.Deserialize(stream);
            IsHoldingItem = (bool)formatter.Deserialize(stream);
            isAtPC = (bool)formatter.Deserialize(stream);
            isOutside = (bool)formatter.Deserialize(stream);

            stream.Close();
        }
    }
    #endregion
}



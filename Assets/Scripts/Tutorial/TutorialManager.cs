using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] Timer gameTime;
    [SerializeField] GameObject laptop;
    [SerializeField] GameObject stressLevel;

    [Header("ItemStuff")]
    [SerializeField] GameObject itemSlot;
    [SerializeField] Item bandage;

    [Header("Doctor")]
    [SerializeField] GameObject documentationBubbleCanvas;
    [SerializeField] GameObject doctor;
    [SerializeField] Transform doctorDocPos;
    [SerializeField] Transform doctorLevelStartPos;
    [SerializeField] TMP_Text doctorTextField;

    [Header("Patient")]
    [SerializeField] GameObject patientTutorialPrefab;
    [SerializeField] Transform patientBedPos;
    [SerializeField] Transform patientSpawnPoint;

    [Header("PopUpStuff")]
    [SerializeField] Transform popUpPos;
    [SerializeField] GameObject popUpPrefab;
    [SerializeField] GameObject releasePopUpPrefab;
    [SerializeField] Transform laptopPopUpPos;
    [SerializeField] GameObject laptopPopUpPrefab;

    [Header("Camera Stuff")]
    [SerializeField] float interpolationValue;
    [SerializeField] GameObject cameraFixedPosition;
    [SerializeField] GameObject cameraMovePoint;
    [SerializeField] GameObject fixCamImage;
    [SerializeField] GameObject freeCamImage;
    bool isCamerafixed;

    [SerializeField] List<Texts> tutorialTexts;

    GameObject currentUIitem;
    GameObject currentPatient;
    GameObject currentPopUp;
    float realTime;
    float tutorialTimer;
    int textDirectionIndex = 0;
    bool isSpawned = false;

    #region TutorialCheckList

    bool IsWPressed = false;
    bool IsAPressed = false;
    bool IsSPressed = false;
    bool IsDPressed = false;
    bool IsSpacePressed = false;
    bool playerGrabItem = false;
    bool isLayingInBed = false;
    bool isPopUpSpawned = false;
    bool playerdroppedItem = false;
    bool releaseBool = true;
    bool timerBool = false;
    bool isInputCorrect = false;
    bool laptopPopUpSpawned = false;
    #endregion



    void Awake()
    {
        doctorTextField.text = $"{tutorialTexts[textDirectionIndex].Text} \n {tutorialTexts[textDirectionIndex].Text2} \n {tutorialTexts[textDirectionIndex].Text3}";
        documentationBubbleCanvas.gameObject.SetActive(false);
        stressLevel.GetComponent<Image>().fillAmount = 40;
        TutorialLoop();
    }

    private void Update()
    {
        CheckThatPlayerDontDie();

        DropItem();

        DontLetThePatientDie();

        CameraZoom();

        DontSpawnPatientsPopUp();

        //DeveloperTestFunction();

        MovePatientInBed();

        player.Interact();

        RealOrDoubleTimeCondition();

        SpawnPatient();

        ResetTime();

        CameraPositionReset();

        tutorialTimer += Time.deltaTime;

        TutorialLoop();
    }

    //JustToJumpToTheNextTutorialState #DeveloperFunction
    void DeveloperTestFunction()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            doctorTextField.text = $"{tutorialTexts[++textDirectionIndex].Text} \n {tutorialTexts[textDirectionIndex].Text2} \n {tutorialTexts[textDirectionIndex].Text3}";
        }
    }

    private void MovePatientInBed()
    {
        if (player.IsInContact)
        {
            player.currentPatient.transform.position = patientBedPos.position;
            player.currentPatient.transform.rotation = patientBedPos.rotation;
            player.currentPatient.GetComponent<Animator>().SetBool("isLaying", true);
            player.currentPatient.Healthbar.transform.parent.rotation = Quaternion.Euler(0, 0, 0); 
            patientBedPos.parent.GetComponent<Bed>().CurrentPatient = player.currentPatient;
            patientBedPos.parent.GetComponent<Bed>().IsPatientInBed = true;
            player.currentPatient.IsInBed = true;
        }
    }
    private void CheckListMovement()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            IsWPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            IsAPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            IsSPressed = true;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            IsDPressed = true;
        }
    }

    private void CheckForInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IsSpacePressed = true;
        }
    }

    /// <summary>
    /// This is the whole tutorialtext
    /// </summary>
    private void TutorialLoop()
    {
        if ((int)tutorialTimer == 5)
        {
            if (textDirectionIndex <= 2)
            {
                NextDirectionIndex();
                tutorialTimer = 0;
            }
        }
        //ToMove: WASD
        if (tutorialTexts[textDirectionIndex].NumberOfExecution == 3)
        {
            CheckListMovement();
            if (IsWPressed && IsAPressed && IsSPressed && IsDPressed)
            {
                NextDirectionIndex();
                IsDPressed = false;
                IsWPressed = false;
            }
        }
        //ToInteract: Space
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 4)
        {
            CheckForInteractionInput();
            if (IsSpacePressed)
            {
                NextDirectionIndex();
                IsSpacePressed = false;
            }
        }
        //Go and grab a item!
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 5)
        {
            if (player.currentItem != null && !playerGrabItem)
            {
                NextDirectionIndex();
                playerGrabItem = true;
                tutorialTimer = 0;
            }
        }
        //Drop Item
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 6)
        {
            if (Input.GetKeyDown(KeyCode.F) && !playerdroppedItem)
            {
                playerdroppedItem = true;
                player.currentItem = null;
                tutorialTimer = 0;
                NextDirectionIndex();
            }
        }
        //Great
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 7)
        {
            if (tutorialTimer >= 3)
            {
                NextDirectionIndex();
                tutorialTimer = 0;
            }
        }
        //Patient came in
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 8)
        {
            if (tutorialTimer >= 5)
            {
                NextDirectionIndex();
                tutorialTimer = 0;
            }
        }
        //Interact with patient to lay him in bed
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 9)
        {
            if (currentPatient.GetComponent<Patient>().IsInBed && !isLayingInBed)
            {
                NextDirectionIndex();
                isLayingInBed = true;
                tutorialTimer = 0;
            }
        }
        //Great
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 10)
        {
            if ((int)tutorialTimer >= 2)
            {
                NextDirectionIndex();
                tutorialTimer = 0;
            }
        }
        // Patient need your help
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 11)
        {
            if (!isPopUpSpawned && tutorialTimer >= 1)
            {
                SpawnPopUp();
            }
            if ((int)tutorialTimer >= 2)
            {
                NextDirectionIndex();
                isPopUpSpawned = true;
                tutorialTimer = 0;
            }
        }
        //grab the correct item
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 12)
        {
            if (currentPopUp == null)
            {
                SpawnPopUp();
            }
            if (player.currentItem != null)
            {
                if (player.currentItem.item.task == TaskType.Bandages)
                {
                    if (itemSlot.transform.childCount <= 0)
                        currentUIitem = Instantiate(bandage.item.UI_prefab, itemSlot.transform);

                    NextDirectionIndex();
                    player.IsInContact = false;
                }
            }
        }
        //Interact with the patient to heal him and hold space
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 13)
        {
            InterActWithPatient();
        }
        //great you did it
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 14)
        {
            if (tutorialTimer >= 3)
            {
                NextDirectionIndex();
                isPopUpSpawned = false;
                tutorialTimer = 0;
            }
        }
        //release patient
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 15)
        {
            ReleasePatient();
        }
        //Lets go to computer
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 16)
        {
            ComputerTask();
            tutorialTimer = 0;
        }
        else if (textDirectionIndex == 18)
        {
            if (tutorialTimer >= 5)
            {
                NextDirectionIndex();
                tutorialTimer = 0;
            }

        }
        //Litlle tip
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 19)
        {
            if (tutorialTimer >= 5)
            {
                NextDirectionIndex();
                tutorialTimer = 0;
            }
        }
        //You can zoom
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 20)
        {
            if (tutorialTimer >= 6)
            {
                NextDirectionIndex();
                tutorialTimer = 0;
            }
        }
        //Good Luck
        else if (tutorialTexts[textDirectionIndex].NumberOfExecution == 21)
        {
            if (tutorialTimer >= 4)
            {
                SceneManager.LoadScene(0);
            }
        }
    }
    /// <summary>
    /// Damages the patient
    /// </summary>
    /// <param name="patient"></param>
    private void Damage(Patient patient)
    {
        patient.HasPopUp = false;

        if (player.currentItem == null)
        {
            patient.Treatment(-player.NoItemDamage);
            player.CurrentStressLvl += player.NoItemDamage * 1;
        }
        else
        {
            patient.Treatment(-player.currentItem.item.restoreHealth);
            player.CurrentStressLvl += player.currentItem.item.restoreHealth * 1;
        }
    }

    public void RealTime()
    {
        realTime += Time.deltaTime;

        if ((int)realTime <= 9 && gameTime.TimeInHours <= 9)
            gameTime.TimeText.text = "0" + gameTime.TimeInHours.ToString() + ":" + "0" + (int)realTime;

        if ((int)realTime <= 9 && gameTime.TimeInHours > 9)
            gameTime.TimeText.text = gameTime.TimeInHours.ToString() + ":" + "0" + (int)realTime;

        if ((int)realTime >= 10 && gameTime.TimeInHours <= 9)
            gameTime.TimeText.text = "0" + gameTime.TimeInHours.ToString() + ":" + (int)realTime;

        if ((int)realTime > 9 && gameTime.TimeInHours > 9)
            gameTime.TimeText.text = gameTime.TimeInHours.ToString() + ":" + (int)realTime;

        if ((int)realTime == 60)
        {
            gameTime.TimeInHours++;
            realTime = 0;
            gameTime.TimeText.text = "0" + gameTime.TimeInHours.ToString() + ":" + (int)realTime;
        }
    }

    private void CameraZoom()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isCamerafixed = true;
        }
        if (isCamerafixed)
        {
            //FixCamera
            float interpolation = interpolationValue * Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, cameraFixedPosition.transform.position, interpolation);
            fixCamImage.gameObject.SetActive(true);
            freeCamImage.gameObject.SetActive(false);
        }
        if (Input.GetKey(KeyCode.E) && Camera.main.transform.position.y >= 6)
        {
            //Zoom Camera
            isCamerafixed = false;
            Camera.main.transform.position += Camera.main.transform.forward * 2 * Time.deltaTime;
            fixCamImage.gameObject.SetActive(false);
            freeCamImage.gameObject.SetActive(true);
        }
    }
    void ResetTime()
    {
        if (gameTime.TimeInHours == 18)
        {
            gameTime.TimeInHours = 17;
        }
    }
    void DestroyItem()
    {
        if (itemSlot.transform.childCount > 0)
        {
            for (int i = 0; i < itemSlot.transform.childCount; i++)
            {
                GameObject tempObj = itemSlot.transform.GetChild(i).gameObject;
                Destroy(tempObj);
            }
        }
    }
    void DropItem()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            DestroyItem();
        }
    }
    /// <summary>
    /// If the patients health is smaller than 30 he gets healed to 60
    /// </summary>
    void DontLetThePatientDie()
    {
        if (currentPatient != null)
            if (currentPatient.GetComponent<Patient>().CurrentHP < 30)
                currentPatient.GetComponent<Patient>().CurrentHP = 60;
    }
    //Just to dont get the patient implemented popUp
    void DontSpawnPatientsPopUp()
    {
        if (currentPatient != null)
        {
            currentPatient.GetComponent<Patient>().TimeTillPopUp = 1000000;
        }
    }
    void RealOrDoubleTimeCondition()
    {
        if (!timerBool)
        {
            gameTime.DoubledRealTime();
        }
        else
        {
            RealTime();
        }
    }
    void SpawnPatient()
    {
        if (textDirectionIndex == 7 && !isSpawned)
        {
            isSpawned = true;
            currentPatient = Instantiate(patientTutorialPrefab);
            currentPatient.transform.position = patientSpawnPoint.position;
            currentPatient.transform.eulerAngles = new Vector3(0, 90, 0);
        }
    }
    /// <summary>
    /// This resets the Camera Position after DocumentationTask
    /// </summary>
    void CameraPositionReset()
    {
        if (player.IsAtPc && tutorialTexts[textDirectionIndex].NumberOfExecution != 16)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Camera.main.transform.position = cameraFixedPosition.transform.position;
                Camera.main.transform.rotation = cameraFixedPosition.transform.rotation;
                laptop.GetComponent<Computer>().ClipBoardCanvas.SetActive(false);
                laptop.GetComponent<Computer>().Canvas.SetActive(false);
                player.GetComponent<NewPlayerMovement>().enabled = true;
                player.IsAtPc = false;
            }
        }
    }
    void InterActWithPatient()
    {
        if (currentPopUp == null)
        {
            SpawnPopUp();
        }
        if (player.IsInContact)
        {
            DestroyItem();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    currentPopUp.GetComponent<PopUp>().IsHealing = true;
                    player.GetComponent<Animator>().SetBool("isTreating", true);
                    currentPatient.GetComponent<Patient>().CurrentParticles = Instantiate(currentPatient.GetComponent<Patient>().HealingParticles, currentPatient.GetComponent<Patient>().transform);
                    ParticleSystem[] ParticleLoops = currentPatient.GetComponent<Patient>().GetComponentsInChildren<ParticleSystem>();
                }
            }
            else if (!Input.GetKey(KeyCode.Space))
            {
                Damage(currentPatient.GetComponent<Patient>());
                stressLevel.GetComponent<Image>().fillAmount += 0.1f;
                player.currentItem = null;
                Destroy(currentPatient.GetComponent<Patient>().CurrentParticles);
                Destroy(currentPopUp);
                textDirectionIndex = 11;
                player.IsInContact = false;
                currentPopUp.GetComponent<PopUp>().IsHealing = false;
                player.GetComponent<Animator>().SetBool("isTreating", false);
                player.GetComponent<NewPlayerMovement>().enabled = true;
                isPopUpSpawned = false;
                tutorialTimer = 0;
                doctorTextField.text = $"{tutorialTexts[textDirectionIndex].Text} \n {tutorialTexts[textDirectionIndex].Text2} \n {tutorialTexts[textDirectionIndex].Text3}";
            }

        }
        if (currentPopUp != null)
        {
            if (currentPopUp.GetComponent<PopUp>().RadialBarImage.fillAmount >= 1)
            {
                Destroy(currentPopUp);
                Destroy(currentPatient.GetComponent<Patient>().CurrentParticles);
                currentPopUp = null;
                player.GetComponent<Animator>().SetBool("isTreating", false);
                NextDirectionIndex();
                stressLevel.GetComponent<Image>().fillAmount -= 0.1f;
                currentPatient.GetComponent<Patient>().CurrentHP = 100;
                tutorialTimer = 0;
            }
        }
    }
    void ReleasePatient()
    {
        if (currentPatient != null)
        {
            currentPatient.GetComponent<Patient>().CurrentHP = 100;
            if (currentPatient.GetComponent<Patient>().CurrentHP >= 100 && !isPopUpSpawned)
            {
                SpawnReleasePopUp();
            }
            if (Input.GetKeyDown(KeyCode.Space) && player.IsInContact && releaseBool)
            {
                Destroy(currentPopUp);
                currentPopUp = null;
                player.IsInContact = false;
                currentPatient.GetComponent<Patient>().HealthBarCanvas.gameObject.SetActive(false);
                currentPatient.GetComponent<Patient>().PopUpCanvas.gameObject.SetActive(false);
                currentPatient.GetComponent<Animator>().SetBool("isWalking", true);
                currentPatient.transform.position = currentPatient.GetComponent<Patient>().LeaveHospital.position;
                currentPatient.GetComponent<Patient>().IsReleasing = true;
                currentPatient = null;
                releaseBool = false;
                gameTime.TimeInHours = 17;
                gameTime.RealTime = 0;
                timerBool = true;
                NextDirectionIndex();
            }
            if (currentPopUp == null)
            {
                isPopUpSpawned = false;
            }
        }
    }
    /// <summary>
    /// The Whole ComputerTask
    /// </summary>
    void ComputerTask()
    {
        if (!laptopPopUpSpawned)
        {
            laptopPopUpSpawned = true;
            currentPopUp = Instantiate(laptopPopUpPrefab, laptopPopUpPos);
            Vector3 lookDir = Camera.main.transform.forward;
            currentPopUp.transform.LookAt(currentPopUp.transform.position + lookDir);
        }
        if (currentPopUp == null)
        {
            laptopPopUpSpawned = false;
        }
        if (player.IsAtPc)
        {
            Destroy(currentPopUp);
            doctor.transform.position = doctorDocPos.position;
            doctor.transform.rotation = doctorDocPos.rotation;
            player.GetComponent<NewPlayerMovement>().enabled = false;
            documentationBubbleCanvas.gameObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (laptop.GetComponent<Computer>().ClipBoardCanvas.GetComponentInChildren<TMP_Text>().text == laptop.GetComponent<Computer>().Canvas.GetComponentInChildren<TMP_InputField>().text)
                {
                    isInputCorrect = true;
                }
                Camera.main.transform.position = cameraFixedPosition.transform.position;
                Camera.main.transform.rotation = cameraFixedPosition.transform.rotation;
                doctor.transform.position = doctorLevelStartPos.position;
                doctor.transform.rotation = doctorLevelStartPos.rotation;
                player.GetComponent<NewPlayerMovement>().enabled = true;
                documentationBubbleCanvas.gameObject.SetActive(false);
                laptop.GetComponent<Computer>().ClipBoardCanvas.SetActive(false);
                laptop.GetComponent<Computer>().Canvas.SetActive(false);
                tutorialTimer = 0;
                if (isInputCorrect)
                {
                    tutorialTimer = 0;
                    stressLevel.GetComponent<Image>().fillAmount -= 0.2f;
                    textDirectionIndex = 18;
                    SoundManager.instance.PlayAudioClip(ESoundeffects.ComputerSuccess, laptop.gameObject.GetComponent<AudioSource>());
                    NextDirectionIndex();
                }
                else
                {
                    if (stressLevel.GetComponent<Image>().fillAmount <= 0.75f)
                    {
                        stressLevel.GetComponent<Image>().fillAmount += 0.2f;

                    }
                    textDirectionIndex = 17;
                    tutorialTimer = 0;
                    SoundManager.instance.PlayAudioClip(ESoundeffects.ComputerFail, laptop.gameObject.GetComponent<AudioSource>());
                    NextDirectionIndex();
                }
            }
        }
    }
    /// <summary>
    /// Shows the next text of the tutorialDoctor
    /// </summary>
    void NextDirectionIndex()
    {
        doctorTextField.text = $"{tutorialTexts[++textDirectionIndex].Text} \n {tutorialTexts[textDirectionIndex].Text2} \n {tutorialTexts[textDirectionIndex].Text3}";
    }
    void SpawnPopUp()
    {
        isPopUpSpawned = true;
        currentPopUp = Instantiate(popUpPrefab, currentPatient.GetComponent<Patient>().PopUpCanvas);
        currentPatient.GetComponent<Patient>().PopUpCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        currentPopUp.transform.position = popUpPos.position;
        Vector3 lookDir = Camera.main.transform.forward;
        currentPopUp.transform.LookAt(currentPopUp.transform.position + lookDir);
    }
    void SpawnReleasePopUp()
    {
        isPopUpSpawned = true;
        currentPopUp = Instantiate(releasePopUpPrefab, currentPatient.GetComponent<Patient>().PopUpCanvas);
        currentPopUp.transform.position = popUpPos.position;
        Vector3 lookDir = Camera.main.transform.forward;
        currentPopUp.transform.LookAt(currentPopUp.transform.position + lookDir);
        player.IsInContact = false;
    }

    void CheckThatPlayerDontDie()
    {
        if (stressLevel.GetComponent<Image>().fillAmount > 0.7f)
            stressLevel.GetComponent<Image>().fillAmount = 0.5f;
    }
}

[System.Serializable]
public class Texts
{
    [SerializeField] string text;
    public string Text => text;

    [SerializeField] string text2;
    public string Text2 => text2;

    [SerializeField] string text3;
    public string Text3 => text3;

    [SerializeField] int numberOfExecution;
    public int NumberOfExecution => numberOfExecution;
}
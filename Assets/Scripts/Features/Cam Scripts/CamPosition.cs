using UnityEngine;

public class CamPosition : MonoBehaviour
{
    #region CamValues
    [Header("CameraSpeed")]
    [SerializeField] float cameraSpeedOut;
    [SerializeField] float cameraSpeedIn;
    [Header("CameraImg")]
    [SerializeField] private GameObject camImageUnfixed;
    [SerializeField] private GameObject camImageFixed;
    [Header("OtherValues")]
    [SerializeField] private Transform camPoints;
    [SerializeField] MovePoints movePoints;
    public MovePoints MovePoint { get { return movePoints; } set { movePoints = value; } }
    [SerializeField] public Transform currentPoint;
    [SerializeField] public Transform lastPoint;
    [SerializeField] Transform cameraOverviewPoint;
    public Transform CameraOverview => cameraOverviewPoint;
    Quaternion cameraRotation;
    public Quaternion CameraRotation => cameraRotation;
    private float interpolation;
    #endregion
    private void Awake()
    {
        cameraRotation = this.gameObject.transform.rotation;
    }

    private void Update()
    {
        FreeOrFixedCamera();
    }

    void FreeOrFixedCamera()
    {
        if (!movePoints.IsCameraFixed)
        {
            interpolation = Time.deltaTime * cameraSpeedIn;
            transform.position = Vector3.Lerp(transform.position, lastPoint.position, interpolation);
        }
        else
        {
            interpolation = Time.deltaTime * cameraSpeedOut;
            transform.position = Vector3.Lerp(transform.position, movePoints.OverViewPoint.position, interpolation);
        }

        if (Input.GetKey(KeyCode.Q) && transform.position.y < 15.249f && !MovePoint.CameraOnPc)
        {
            camImageUnfixed.gameObject.SetActive(false);
            camImageFixed.gameObject.SetActive(true);
            currentPoint = movePoints.OverViewPoint;
            transform.position += new Vector3(0, movePoints.CamHeight, -movePoints.CameraDirection) * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E) && transform.position.y > 10.174f && !MovePoint.CameraOnPc)
        {
            camImageFixed.gameObject.SetActive(false);
            camImageUnfixed.gameObject.SetActive(true);
            transform.position -= new Vector3(0, movePoints.CamHeight, -movePoints.CameraDirection) * Time.deltaTime;
        }
    }



}

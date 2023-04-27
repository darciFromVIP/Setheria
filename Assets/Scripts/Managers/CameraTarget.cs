
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class CameraTarget : MonoBehaviour, NeedsLocalPlayerCharacter
{
    private float cameraSpeed = 25;
    private float cameraRotationSpeed = 8;
    private float cameraZoomMin = 10;
    private float cameraZoomMax = 30;
    private int borderThickness = 5;
    private Vector3 followOffset;
    private PlayerCharacter localPlayerCharacter;
    [SerializeField] private bool cameraMouseControl = true;
    private List<InCameraWay> objectsInTheWay = new();
    private List<InCameraWay> objectsAlreadyTurnedOff = new();

    public Collider camBounds;

    public CinemachineVirtualCamera cam;

    private void Awake()
    {
        followOffset = cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }
    private void Start()
    {
        foreach (var item in FindObjectsOfType<Slider>(true))
        {
            if (item.name == "CameraMoveSpeed")
                item.onValueChanged.AddListener(CameraSpeedChanged);
            if (item.name == "CameraRotationSpeed")
                item.onValueChanged.AddListener(CameraRotationSpeedChanged);
            if (item.name == "CameraZoom")
                item.onValueChanged.AddListener(CameraZoomChanged);
        }
        Teleport(FindObjectOfType<WorldGenerator>().globalStartingPoint);
    }
    private void CameraSpeedChanged(float value)
    {
        cameraSpeed = value;
    }
    private void CameraRotationSpeedChanged(float value)
    {
        cameraRotationSpeed = value;
    }
    private void CameraZoomChanged(float value)
    {
        cameraZoomMax = value;
    }
    void Update()
    {
        if (!localPlayerCharacter)
            return;
        //Turn Off Objects near the Camera
        objectsInTheWay.Clear();
        //foreach (var item in Physics.CapsuleCastAll(cam.transform.position + cam.transform.right * 0.2f, cam.transform.position - cam.transform.right * 0.2f, 3, localPlayerCharacter.transform.position - cam.transform.position, 30))
        foreach (var item in Physics.RaycastAll(cam.transform.position, localPlayerCharacter.transform.position - cam.transform.position, 30))
        {
            if (item.collider.TryGetComponent(out InCameraWay obstacle))
            {
                if (!objectsInTheWay.Contains(obstacle))
                    objectsInTheWay.Add(obstacle);
            }
        }
        foreach (var item in objectsInTheWay)
        {
            if (!objectsAlreadyTurnedOff.Contains(item))
            {
                item.TurnOff();
                objectsAlreadyTurnedOff.Add(item);
            }    
        }
        for (int i = 0; i < objectsAlreadyTurnedOff.Count; i++)
        {
            if (!objectsInTheWay.Contains(objectsAlreadyTurnedOff[i]))
            {
                objectsAlreadyTurnedOff[i].TurnOn();
                objectsAlreadyTurnedOff.Remove(objectsAlreadyTurnedOff[i]);
            }
        }


        Vector3 rottarget = transform.rotation.eulerAngles;
        Vector3 pos = transform.position;

        if ((Input.mousePosition.y >= Screen.height - borderThickness && cameraMouseControl) || Input.GetKey(KeyCode.UpArrow))
        {
            pos = Vector3.MoveTowards(pos, pos + transform.forward, Time.deltaTime * cameraSpeed);
            pos = Vector3.MoveTowards(pos, pos - transform.right, Time.deltaTime * cameraSpeed);
        }

        if ((Input.mousePosition.y <= borderThickness && cameraMouseControl) || Input.GetKey(KeyCode.DownArrow))
        {
            pos = Vector3.MoveTowards(pos, pos - transform.forward, Time.deltaTime * cameraSpeed);
            pos = Vector3.MoveTowards(pos, pos + transform.right, Time.deltaTime * cameraSpeed);
        }

        if ((Input.mousePosition.x >= Screen.width - borderThickness && cameraMouseControl) || Input.GetKey(KeyCode.RightArrow))
        {
            pos = Vector3.MoveTowards(pos, pos + transform.forward, Time.deltaTime * cameraSpeed);
            pos = Vector3.MoveTowards(pos, pos + transform.right, Time.deltaTime * cameraSpeed);
        }

        if ((Input.mousePosition.x <= borderThickness && cameraMouseControl) || Input.GetKey(KeyCode.LeftArrow))
        {
            pos = Vector3.MoveTowards(pos, pos - transform.forward, Time.deltaTime * cameraSpeed);
            pos = Vector3.MoveTowards(pos, pos - transform.right, Time.deltaTime * cameraSpeed);
        }

        if (Input.GetKey(KeyCode.Mouse2) && Input.GetAxis("Mouse X") > 0)
        {
            rottarget.y += cameraRotationSpeed * Mathf.Abs(Input.GetAxis("Mouse X"));
        }

        if (Input.GetKey(KeyCode.Mouse2) && Input.GetAxis("Mouse X") < 0)
        {
            rottarget.y -= cameraRotationSpeed * Mathf.Abs(Input.GetAxis("Mouse X"));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            pos = new Vector3(localPlayerCharacter.transform.position.x, transform.position.y, localPlayerCharacter.transform.position.z);
        }

        float playerHeight = transform.position.y;
        //float groundLevel = Terrain.activeTerrain.SampleHeight(pos);  //Height based on Terrain
        
        Vector3 zoomDir = followOffset.normalized;
        if (Input.mouseScrollDelta.y < 0)
            followOffset += zoomDir;
        if (Input.mouseScrollDelta.y > 0)
            followOffset -= zoomDir;
        if (followOffset.magnitude < cameraZoomMin)
            followOffset = zoomDir * cameraZoomMin;
        if (followOffset.magnitude > cameraZoomMax)
            followOffset = zoomDir * cameraZoomMax;
        pos.y = Mathf.Clamp(Mathf.Lerp(pos.y, playerHeight, Time.deltaTime * 10), pos.y - 0.1f, pos.y + 0.1f);
        transform.rotation = Quaternion.Euler(rottarget);

        if (camBounds.bounds.Contains(pos + followOffset * 1.5f))
        {
            transform.position = pos;
            cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = Vector3.Lerp(cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset, followOffset, Time.deltaTime * cameraSpeed);
        }
        else
            followOffset = cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }

    public void Teleport(Transform dest)
    {
        transform.position = dest.position;
    }
    public void Teleport(Vector3 dest)
    {
        transform.position = dest;
    }

    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayerCharacter = player;
    }
}

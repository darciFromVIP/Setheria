
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEditor;
using HighlightPlus;

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
    private bool isFollowing = false;
    private bool isKeyPressed = false;
    private float cooldownTime = 0.2f; // Adjust this value as needed
    private float lastKeyPressTime = 0f;
    public bool zoomEnabled = true;

    public Collider camBounds;

    public CinemachineVirtualCamera cam;

    private SettingsManager settingsManager;
    public InputEnabledScriptable inputEnabled;

    private void Start()
    {
        followOffset = cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
        settingsManager = FindObjectOfType<SettingsManager>();
        foreach (var item in FindObjectsOfType<Slider>(true))
        {
            if (item.name == "CameraMoveSpeed")
                item.onValueChanged.AddListener(CameraSpeedChanged);
            if (item.name == "CameraRotationSpeed")
                item.onValueChanged.AddListener(CameraRotationSpeedChanged);
            if (item.name == "CameraZoom")
                item.onValueChanged.AddListener(CameraZoomChanged);
        }
        StartCoroutine(WaitForGlobalStartingPoint());
        FindObjectOfType<SystemMessages>(true).cameraTeleportEvent.AddListener(Teleport);
    }
    private IEnumerator WaitForGlobalStartingPoint()
    {
        var worldGen = FindObjectOfType<WorldGenerator>();
        while (worldGen.globalStartingPoint == null)
            yield return null;
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
        if (!localPlayerCharacter || !inputEnabled.inputEnabled)
            return;
       
        Vector3 rottarget = transform.rotation.eulerAngles;
        Vector3 pos = transform.position;

        if ((Input.mousePosition.y >= Screen.height - borderThickness && cameraMouseControl) || Input.GetKey(settingsManager.settings.cameraUp))
        {
            pos = Vector3.MoveTowards(pos, pos + transform.forward, Time.deltaTime * cameraSpeed);
            pos = Vector3.MoveTowards(pos, pos - transform.right, Time.deltaTime * cameraSpeed);
        }

        if ((Input.mousePosition.y <= borderThickness && cameraMouseControl) || Input.GetKey(settingsManager.settings.cameraDown))
        {
            pos = Vector3.MoveTowards(pos, pos - transform.forward, Time.deltaTime * cameraSpeed);
            pos = Vector3.MoveTowards(pos, pos + transform.right, Time.deltaTime * cameraSpeed);
        }

        if ((Input.mousePosition.x >= Screen.width - borderThickness && cameraMouseControl) || Input.GetKey(settingsManager.settings.cameraRight))
        {
            pos = Vector3.MoveTowards(pos, pos + transform.forward, Time.deltaTime * cameraSpeed);
            pos = Vector3.MoveTowards(pos, pos + transform.right, Time.deltaTime * cameraSpeed);
        }

        if ((Input.mousePosition.x <= borderThickness && cameraMouseControl) || Input.GetKey(settingsManager.settings.cameraLeft))
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

        if (Input.GetKey(settingsManager.settings.cameraRotRight))
        {
            rottarget.y += cameraRotationSpeed * Time.deltaTime * 5;
        }

        if (Input.GetKey(settingsManager.settings.cameraRotLeft))
        {
            rottarget.y -= cameraRotationSpeed * Time.deltaTime * 5;
        }

        if (Time.time - lastKeyPressTime >= cooldownTime)
        {
            if (Input.GetKeyDown(settingsManager.settings.cameraLock))
            {
                CenterCamera(true);
            }
        }
        if (isKeyPressed && !Input.GetKey(settingsManager.settings.cameraLock))
        {
            isKeyPressed = false;
        }

        if (isFollowing)
        {
            pos = new Vector3(localPlayerCharacter.transform.position.x, transform.position.y, localPlayerCharacter.transform.position.z);
        }

        if (zoomEnabled)
        {
            Vector3 zoomDir = followOffset.normalized;
            if (!localPlayerCharacter.GetComponent<PlayerController>().IsPointerOverGameObject())
            {
                if (Input.mouseScrollDelta.y < 0)
                    followOffset += zoomDir;
                if (Input.mouseScrollDelta.y > 0)
                    followOffset -= zoomDir;
            }
            if (followOffset.magnitude < cameraZoomMin)
                followOffset = zoomDir * cameraZoomMin;
            if (followOffset.magnitude > cameraZoomMax)
                followOffset = zoomDir * cameraZoomMax;
        }

        transform.rotation = Quaternion.Euler(rottarget);

        if (camBounds.bounds.Contains(pos + followOffset * 1.5f))
        {
            transform.position = pos;
            cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = Vector3.Lerp(cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset, followOffset, Time.deltaTime * cameraSpeed);
        }
        else
            followOffset = cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }
    public void CenterCamera(bool toggleLock)
    {
        if (toggleLock)
        {
            isKeyPressed = true;
            lastKeyPressTime = Time.time;

            isFollowing = !isFollowing;
        }
        else
            transform.position = new Vector3(localPlayerCharacter.transform.position.x, transform.position.y, localPlayerCharacter.transform.position.z);

        if (isFollowing)
        {
            cam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 0;
            cam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 0;
            cam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0;
            cam.LookAt = localPlayerCharacter.transform;
        }
        else
        {
            cam.LookAt = null;
            cam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 1;
            cam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 1;
            cam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 1;
        }
    }
    public void Teleport(Transform dest)
    {
        if (isFollowing)
            CenterCamera(true);
        transform.position = dest.position;
    }
    public void Teleport(Vector3 dest)
    {
        if (isFollowing)
            CenterCamera(true);
        transform.position = dest;
    }

    public void SetLocalPlayerCharacter(PlayerCharacter player)
    {
        localPlayerCharacter = player;
    }
}

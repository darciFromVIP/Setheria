using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
public class CanBuild : NetworkBehaviour
{
    private PlayerController playerController;
    private CanMove moveComp;
    public BuildStructure buildStructureAction;

    public StructureDatabase structureDatabase;

    public InputEnabledScriptable inputEnabled;

    [HideInInspector] public UnityEvent Structure_Built = new();

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        moveComp = GetComponent<CanMove>();
    }
    public void BeginBuildingStructure(BuildStructure structure)
    {
        if (isOwned)
        {
            playerController.CmdChangeState(PlayerState.Busy);
            StartCoroutine(ChoosingBuildLocation(structure));
        }
    }
    private IEnumerator ChoosingBuildLocation(BuildStructure structure)
    {
        var ghost = Instantiate(structure.structureGhostPrefab);
        RaycastHit hit;
        while (true)
        {
            if (inputEnabled.inputEnabled)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Default", "Water")))
                {
                    if (hit.collider is TerrainCollider || hit.collider.CompareTag("Water"))
                    {
                        ghost.transform.position = hit.point;
                        if (Input.GetKeyDown(KeyCode.Mouse0) && ghost.GetComponent<StructureGhost>().canBuild)
                        {
                            StartCoroutine(GoToBuildStructure(structure, hit.point, ghost.transform.rotation));
                            Destroy(ghost.gameObject);
                            playerController.CmdChangeState(PlayerState.None);
                            break;
                        }
                        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape))
                        {
                            if (buildStructureAction)
                                buildStructureAction.StopExecute(this);
                            Destroy(ghost.gameObject);
                            playerController.CmdChangeState(PlayerState.None);
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
    }
    private IEnumerator GoToBuildStructure(BuildStructure structure, Vector3 destination, Quaternion rotation)
    {
        moveComp.MoveTo(destination);
        var originDest = moveComp.agent.destination;
        while (true)
        {
            if (originDest != moveComp.agent.destination)
            {
                yield break;
            }
            if (moveComp.HasReachedDestination())
                break;
            yield return null;
        }
        SpawnBuilding(structure.structurePrefab.name, destination, rotation);
        Structure_Built.Invoke();
        moveComp.Stop();
    }
    [Command]
    private void SpawnBuilding(string structureName, Vector3 destination, Quaternion rotation)
    {
        var structure = structureDatabase.GetStructureByName(structureName);
        GameObject building = Instantiate(structure.gameObject, destination, rotation);
        NetworkServer.Spawn(building);
    }
}

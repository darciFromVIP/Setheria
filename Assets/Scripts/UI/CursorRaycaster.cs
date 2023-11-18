using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Mirror;
public class CursorRaycaster : NetworkBehaviour
{
    private CustomCursor cursor;
    private PlayerController player;
    public LayerMask layerMask;
    private void Start()
    {
        cursor = FindObjectOfType<CustomCursor>();
        player = GetComponent<PlayerController>();
    }
    private void FixedUpdate()
    {
        if (!isOwned)
            return;
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);
        if (results.Count > 0)
        {
            cursor.SetBaseCursor();
            return;
        }
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            if (player.castingState == CastingState.EnemyOnly || player.castingState == CastingState.Both || player.castingState == CastingState.BothExceptSelf && hit.collider.TryGetComponent(out EnemyCharacter enemyy))
                cursor.SetEnemyCastingCursor();
            else if (player.state != PlayerState.Casting && hit.collider.TryGetComponent(out EnemyCharacter enemy))
                cursor.SetEnemyCursor();
            else if (hit.collider.TryGetComponent(out PlayerCharacter playerCharacter) &&
                ((player.castingState == CastingState.AllyExceptSelf || player.castingState == CastingState.BothExceptSelf) && playerCharacter.gameObject != gameObject ||
                player.castingState == CastingState.AllyOnly || player.castingState == CastingState.Both))
            {
                cursor.SetAllyCursor();
            }
            else if (player.state == PlayerState.Casting)
                cursor.SetCastingCursor();
            else if (hit.collider.CompareTag("Water") && player.state != PlayerState.OutOfGame)       // Display Base Cursor on Shallow Water
            {
                RaycastHit hit1;
                Ray ray1 = new Ray(hit.point, Vector3.down);
                if (Physics.Raycast(ray1, out hit1, 1))
                    cursor.SetBaseCursor();
                else
                    cursor.SetInvalidCursor();
            }
            else if (player.state != PlayerState.OutOfGame && player.state != PlayerState.Casting
                && (hit.collider.TryGetComponent(out LootableObject lootable)
                || hit.collider.TryGetComponent(out Item item)
                || hit.collider.TryGetComponent(out Structure structure)
                || hit.collider.TryGetComponent(out TreasureChest treasureChest)))
                cursor.SetInteractionCursor();
            else if ((hit.collider.gameObject.layer == 0 && player.state != PlayerState.OutOfGame) || hit.collider.TryGetComponent(out PlayerCharacter playerChar) || !player.GetComponentInChildren<SkinnedMeshRenderer>().enabled)
                cursor.SetBaseCursor();
            else
                cursor.SetInvalidCursor();
        }
    }
}

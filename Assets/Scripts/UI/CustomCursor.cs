using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CursorType
{
    Base, Interaction, Invalid, Enemy, Ally, Casting, EnemyCasting
}
public class CustomCursor : MonoBehaviour
{
    public Texture2D baseCursor, interactionCursor, invalidCursor, enemyCursor, allyCursor, castingCursor, enemyCastingCursor;
    public CursorType cursorType;
    void Start()
    {
        SetBaseCursor();
    }
    public void SetBaseCursor()
    {
        Cursor.SetCursor(baseCursor, Vector2.zero, CursorMode.ForceSoftware);
        cursorType = CursorType.Base;
    }
    public void SetInteractionCursor()
    {
        Cursor.SetCursor(interactionCursor, Vector2.zero, CursorMode.ForceSoftware);
        cursorType = CursorType.Interaction;
    }
    public void SetInvalidCursor()
    {
        Cursor.SetCursor(invalidCursor, Vector2.zero, CursorMode.ForceSoftware);
        cursorType = CursorType.Invalid;
    }
    public void SetEnemyCursor()
    {
        Cursor.SetCursor(enemyCursor, Vector2.zero, CursorMode.ForceSoftware);
        cursorType = CursorType.Enemy;
    }
    public void SetAllyCursor()
    {
        Cursor.SetCursor(allyCursor, Vector2.zero, CursorMode.ForceSoftware);
        cursorType = CursorType.Ally;
    }
    public void SetCastingCursor()
    {
        Cursor.SetCursor(castingCursor, Vector2.zero, CursorMode.ForceSoftware);
        cursorType = CursorType.Casting;
    }
    public void SetEnemyCastingCursor()
    {
        Cursor.SetCursor(enemyCastingCursor, Vector2.zero, CursorMode.ForceSoftware);
        cursorType = CursorType.EnemyCasting;
    }
}

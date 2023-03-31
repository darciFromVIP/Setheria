using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D baseCursor, interactionCursor, invalidCursor, enemyCursor, allyCursor, castingCursor, enemyCastingCursor;
    void Start()
    {
        SetBaseCursor();
    }
    public void SetBaseCursor()
    {
        Cursor.SetCursor(baseCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void SetInteractionCursor()
    {
        Cursor.SetCursor(interactionCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void SetInvalidCursor()
    {
        Cursor.SetCursor(invalidCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void SetEnemyCursor()
    {
        Cursor.SetCursor(enemyCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void SetAllyCursor()
    {
        Cursor.SetCursor(allyCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void SetCastingCursor()
    {
        Cursor.SetCursor(castingCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void SetEnemyCastingCursor()
    {
        Cursor.SetCursor(enemyCastingCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
}

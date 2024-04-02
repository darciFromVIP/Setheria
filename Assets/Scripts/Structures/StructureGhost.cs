using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class StructureGhost : MonoBehaviour
{
    public bool canBuild = true;
    public List<Image> canBuildGraphic;
    public Image arrowGraphic;
    public LayerMask obstacleLayers;
    protected List<Collider> collisions = new();
    protected Collider col;

    protected UnityEvent<bool> Water_Collided = new();
    protected virtual void Awake()
    {
        col = GetComponent<Collider>();
        FindObjectOfType<CameraTarget>().zoomEnabled = false;
    }
    private void OnDestroy()
    {
        FindObjectOfType<CameraTarget>().zoomEnabled = true;
    }
    protected virtual void Update()
    {
        if (Input.mouseScrollDelta.y < 0)
            transform.Rotate(Vector3.up, -5);
        if (Input.mouseScrollDelta.y > 0)
            transform.Rotate(Vector3.up, 5);

        List<Vector3> points = new()
        {
            new Vector3(col.bounds.center.x, col.bounds.center.y, col.bounds.center.z),
            new Vector3(col.bounds.center.x + col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z),
            new Vector3(col.bounds.center.x - col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z),
            new Vector3(col.bounds.center.x, col.bounds.center.y, col.bounds.center.z + col.bounds.size.z / 2),
            new Vector3(col.bounds.center.x, col.bounds.center.y, col.bounds.center.z - col.bounds.size.z / 2),
            new Vector3(col.bounds.center.x + col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z + col.bounds.size.z / 2),
            new Vector3(col.bounds.center.x + col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z - col.bounds.size.z / 2),
            new Vector3(col.bounds.center.x - col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z + col.bounds.size.z / 2),
            new Vector3(col.bounds.center.x - col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z - col.bounds.size.z / 2)
        };

        for (int i = 0; i < points.Count; i++)
        {
            Ray ray = new Ray(points[i], Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, (col.bounds.size.y / 2) + 0.1f, obstacleLayers))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);

                if (angle > 2)
                {
                    canBuild = false;
                    canBuildGraphic[i].color = new Color(1, 0, 0, 0.1f);
                    arrowGraphic.color = new Color(1, 0, 0, 1);
                    break;
                }
                else
                {
                    if (collisions.Count == 0)
                    {
                        canBuild = true;
                        canBuildGraphic[i].color = new Color(0, 1, 0, 0.1f);
                        arrowGraphic.color = new Color(0, 1, 0, 1);
                    }
                }
            }
            else
            {
                canBuild = false;
                canBuildGraphic[i].color = new Color(1, 0, 0, 0.1f);
                arrowGraphic.color = new Color(1, 0, 0, 1);
                break;
            }
        }
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        collisions.Add(other);
        if (this is not ShipyardGhost)
        {
            canBuild = false;
            foreach (var item in canBuildGraphic)
            {
                item.color = new Color(1, 0, 0, 0.1f);
                arrowGraphic.color = new Color(1, 0, 0, 1);
            }
        }
        if (other.CompareTag("Water"))
            Water_Collided.Invoke(true);
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        collisions.Remove(other);
        if (collisions.Count == 0 && this is not ShipyardGhost)
        {
            canBuild = true;
            canBuild = false;
            foreach (var item in canBuildGraphic)
            {
                item.color = new Color(0, 1, 0, 0.1f);
                arrowGraphic.color = new Color(0, 1, 0, 1);
            }
        }
        if (other.CompareTag("Water"))
            Water_Collided.Invoke(false);
    }
}

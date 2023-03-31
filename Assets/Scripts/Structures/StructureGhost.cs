using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public class StructureGhost : MonoBehaviour
{
    public bool canBuild = true;
    public List<Image> canBuildGraphic;
    public LayerMask obstacleLayers;
    protected List<Collider> collisions = new();
    protected Collider col;

    protected UnityEvent<bool> Water_Collided = new();
    protected virtual void Awake()
    {
        col = GetComponent<Collider>();
    }
    protected virtual void Update()
    {
        List<Vector3> points = new();
        points.Add(new Vector3(col.bounds.center.x, col.bounds.center.y, col.bounds.center.z));
        points.Add(new Vector3(col.bounds.center.x + col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z));
        points.Add(new Vector3(col.bounds.center.x - col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z));
        points.Add(new Vector3(col.bounds.center.x, col.bounds.center.y, col.bounds.center.z + col.bounds.size.z / 2));
        points.Add(new Vector3(col.bounds.center.x, col.bounds.center.y, col.bounds.center.z - col.bounds.size.z / 2));
        points.Add(new Vector3(col.bounds.center.x + col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z + col.bounds.size.z / 2));
        points.Add(new Vector3(col.bounds.center.x + col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z - col.bounds.size.z / 2));
        points.Add(new Vector3(col.bounds.center.x - col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z + col.bounds.size.z / 2));
        points.Add(new Vector3(col.bounds.center.x - col.bounds.size.x / 2, col.bounds.center.y, col.bounds.center.z - col.bounds.size.z / 2));

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
                    canBuildGraphic[i].color = Color.red;
                    break;
                }
                else
                {
                    if (collisions.Count == 0)
                    {
                        canBuild = true;
                        canBuildGraphic[i].color = Color.green;
                    }
                }
            }
            else
            {
                canBuild = false;
                canBuildGraphic[i].color = Color.red;
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
                item.color = Color.red;
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
                item.color = Color.green;
            }
        }
        if (other.CompareTag("Water"))
            Water_Collided.Invoke(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipyardGhost : StructureGhost
{
    private bool isInWater = false;
    protected override void Awake()
    {
        base.Awake();
        Water_Collided.AddListener(WaterCollided);
    }
    protected override void Update()
    {
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5, obstacleLayers))
        {
            if (!hit.collider.CompareTag("Water"))
            {
                canBuild = false;
                foreach (var item in canBuildGraphic)
                {
                    item.color = new Color(1, 0, 0, 0.1f);
                }
                arrowGraphic.color = new Color(1, 0, 0, 1);
                return;
            }
            List<Vector3> circlePoints = new()
            {
                hit.point + Vector3.forward * 4,
                hit.point + Vector3.forward * 2 + Vector3.right * 2,
                hit.point + Vector3.right * 4,
                hit.point - Vector3.forward * 2 + Vector3.right * 2,
                hit.point - Vector3.forward * 4,
                hit.point - Vector3.forward * 2 - Vector3.right * 2,
                hit.point - Vector3.right * 4,
                hit.point + Vector3.forward * 2 - Vector3.right * 2
            };

            Vector3 deepestPoint = transform.position;
            foreach (var item in circlePoints)
            {
                Ray ray1 = new Ray(item, Vector3.down);
                RaycastHit hit1;
                Debug.DrawRay(item, Vector3.down, Color.red, 5);
                if (Physics.Raycast(ray1, out hit1, 100, LayerMask.GetMask("Default")))
                {
                    if (deepestPoint.y > hit1.point.y)
                        deepestPoint = hit1.point;
                }
            }
            deepestPoint.y = transform.position.y;
            var rot = Quaternion.LookRotation(deepestPoint - transform.position);
            rot.x = 0;
            rot.z = 0;
            transform.rotation = rot;
            if (isInWater)
            {
                canBuild = true;
                foreach (var item in canBuildGraphic)
                {
                    item.color = new Color(0, 1, 0, 0.1f);
                }
                arrowGraphic.color = new Color(0, 1, 0, 1);
            }
            else
            {
                canBuild = false;
                foreach (var item in canBuildGraphic)
                {
                    item.color = new Color(1, 0, 0, 0.1f);
                }
                arrowGraphic.color = new Color(1, 0, 0, 1);
            }
        }
    }
    private void WaterCollided(bool value)
    {
        isInWater = value;
    }
}

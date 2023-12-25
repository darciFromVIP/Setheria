using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.Animations;

namespace RPG_Indicator
{
    public class RpgIndicator : MonoBehaviour
    {
        public RpgIndicatorData[] data;
        public DecalProjector MainIndicator;
        public DecalProjector RangeIndicator;
        public float ProjectorHeight = 5;
        [Space(10)]
        public bool ShowMainIndicator;
        public bool ShowRangeIndicator;
        [Space(10)]
        public bool isPlayer;
        public enum IndicatorType
        {
            Range,
            Cone,
            Area,
            Radius,
            Line
        }
        public enum IndicatorColor
        {
            Ally,
            Neutral,
            Enemy,
            Custom
        }
        [Range(0, 1)]
        public float Opacity = 0.5f;
        [Range(0, 25)]
        public float EmissiveStrength = 1f;


        private bool isCasting;
        private float CastingTime = 5;
        private float Range = 15;
        private float FillPercentage;

        private float endValue = 1;
        private float startValue = 0;
        private float timeElapsed;
        private Color MainColor;
        private Color RangeColor;
        private LayerMask layermask;
        private Transform player;
        private new Camera camera;
        private GameObject PoleObject;
        private RotationConstraint MainRotator;
        private RotationConstraint RangeRotator;
        private bool followMouse;
        private bool lookAtMouse;
        private Vector3 initialPosition = new Vector3(0,0,0);

        void Start()
        {
            InitiateIndicator();
        }
        private void Update()
        {
            if (followMouse) FollowMouse();
            if(lookAtMouse) RotateIndicator();
            if (isCasting) Casting(CastingTime);
        }
        public void ShowCone(float angle, float range, bool showRangeIndicator, IndicatorColor color, int style)
        {
            MainIndicator.material = new Material(data[style].ConeIndicator);
            BasicSetup();
            MainIndicator.material.SetFloat("_Angle", angle);
            MainIndicator.transform.localScale = new Vector3(range * 2, range * 2, ProjectorHeight);
            Range = range;

            if (showRangeIndicator) ShowRange(range, color, style);
            ColorChange(color, style);
            if(isPlayer)
            {
                lookAtMouse = true;
                followMouse = false;
                MainRotator.constraintActive = false;
            }
        }
        public void ShowLine(float length, float range, bool showRangeIndicator, IndicatorColor color, int style)
        {
            MainIndicator.material = new Material(data[style].LineIndicator);
            BasicSetup();
            MainIndicator.transform.localScale = new Vector3(length, (range * 2), ProjectorHeight);
            Range = range;

            if (showRangeIndicator) ShowRange(range, color, style);
            ColorChange(color, style);
            if(isPlayer)
            {
                lookAtMouse = true;
                followMouse = false;
                MainRotator.constraintActive = false;
            }
        }
        public void ShowArea(float radius, float range, bool showRangeIndicator, IndicatorColor color, int style)
        {
            MainIndicator.material = new Material(data[style].AreaIndicator);
            BasicSetup();
            MainIndicator.transform.localScale = new Vector3(radius * 2, radius * 2, ProjectorHeight);
            Range = range;

            if (showRangeIndicator) ShowRange(range, color, style);
            ColorChange(color, style);
            if(isPlayer)
            {
                followMouse = true;
                lookAtMouse = false;
                MainRotator.constraintActive = true;
            }
        }
        public void ShowRadius(float radius, bool showRangeIndicator, IndicatorColor color, int style)
        {
            HideRange();
            MainIndicator.material = new Material(data[style].RadiusIndicator);
            BasicSetup();
            MainIndicator.transform.localScale = new Vector3(radius * 2, radius * 2, ProjectorHeight);
            Range = radius;
            if (showRangeIndicator) ShowRange(radius, color, style);
            ColorChange(color, style);
            if(isPlayer)
            {
                lookAtMouse = false;
                followMouse = false;
                MainRotator.constraintActive = true;
            }
        }
        public void ShowRange(float range, IndicatorColor color, int style)
        {
            RangeIndicator.material = new Material(RangeIndicator.material);
            ColorChange(color, style);
            RangeIndicator.scaleMode = DecalScaleMode.InheritFromHierarchy;
            RangeIndicator.enabled = true;
            RangeIndicator.transform.localScale = new Vector3(range * 2, range * 2, ProjectorHeight);
            Range = range;
            if (isPlayer)
            {
                RangeRotator.constraintActive = true;
            }
        }
        public void HideMain()
        {
            MainIndicator.enabled = false;
            MainIndicator.material.SetFloat("_Fill", 0);
            if(isPlayer)
            {
                if (MainRotator != null) MainRotator.constraintActive = false;
                followMouse = false;
                lookAtMouse = false;
            }
        }
        public void HideRange()
        {
            RangeIndicator.enabled = false;
            if(isPlayer)
            {
                if (RangeRotator != null) RangeRotator.constraintActive = false;
            }
        }
        public void Casting(float castingTime)
        {
            HideRange();

            isCasting = true;
            CastingTime = castingTime;
            if (timeElapsed < castingTime)
            {
                FillPercentage = Mathf.Lerp(startValue, endValue, timeElapsed / castingTime);
                timeElapsed += Time.deltaTime;
                MainIndicator.material.SetFloat("_Fill", FillPercentage);
            }
            if (timeElapsed >= castingTime)
            {
                HideMain();
                isCasting = false;
                timeElapsed = 0;
            }
        }
        public void InterruptCasting()
        {
            HideMain();
            HideRange();
            isCasting = false;
            timeElapsed = 0f;
            MainIndicator.material.SetFloat("_Fill", 0f);
        }
        public void CustomColor(string mainColor, string rangeColor)
        {
            Color color1;
            Color color2;

            ColorUtility.TryParseHtmlString(mainColor, out color1);
            ColorUtility.TryParseHtmlString(rangeColor, out color2);

            MainColor = color1;
            RangeColor = color2;
        }
        private void FollowMouse()
        {
            RangeIndicator.enabled = true;
            if (camera == null)
            {
                camera = Camera.main;
            }
            if (player == null)
            {
                player = GameObject.FindWithTag("Player").transform;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 2000, layermask))
            {
                Vector3 targetPos = (player.position + raycastHit.point) / 2;
                Vector3 Distance = targetPos - player.position;
                Distance = Vector3.ClampMagnitude((Distance * 2), Range);
                MainIndicator.transform.position = player.position + Distance;
            }
        }
        private void RotateIndicator()
        {
            RangeIndicator.enabled = true;
            if (camera == null)
            {
                camera = Camera.main;
            }
            if (player == null)
            {
                player = GameObject.FindWithTag("Player").transform;
            }

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 2000, layermask))
            {

                Vector3 targetPos = raycastHit.point - transform.position;
                Quaternion lookRot = Quaternion.LookRotation(targetPos, Vector3.up);
                float eulerY = lookRot.eulerAngles.y;
                Quaternion rotation = Quaternion.Euler(90, eulerY, 0);
                MainIndicator.transform.rotation = rotation;
            }
        }
        private void BasicSetup()
        {
            transform.localPosition = new Vector3(0, ProjectorHeight / 2, 0);
            MainIndicator.transform.localPosition = initialPosition;
            MainIndicator.scaleMode = DecalScaleMode.InheritFromHierarchy;
            MainIndicator.enabled = true;
            MainIndicator.material.SetFloat("_Fill", 0f);
        }
        private void ColorChange(IndicatorColor color, int style)
        {
            layermask = data[style].Layer;
            switch (color)
            {
                case IndicatorColor.Ally:
                    {
                        MainColor = data[style].MainAllyColor;
                        RangeColor = data[style].RangeAllyColor;
                    }
                    break;
                case IndicatorColor.Neutral:
                    {
                        MainColor = data[style].MainNeutralColor;
                        RangeColor = data[style].RangeNeutralColor;
                    }
                    break;
                case IndicatorColor.Enemy:
                    {
                        MainColor = data[style].MainEnemyColor;
                        RangeColor = data[style].RangeEnemyColor;
                    }
                    break;
                case IndicatorColor.Custom:
                    {
                        // You need to call CustomColor() before using an indicator
                    }
                    break;
                default:
                    break;
            }
            MainIndicator.material.SetColor("_Color", MainColor);
            RangeIndicator.material.SetColor("_Color", RangeColor);
            MainIndicator.material.SetFloat("_Opacity", Opacity);
            RangeIndicator.material.SetFloat("_Opacity", Opacity);
            MainIndicator.material.SetFloat("_Emissive_Strength", EmissiveStrength);
            RangeIndicator.material.SetFloat("_Emissive_Strength", EmissiveStrength);
        }
        private void SetupPole()
        {
            PoleObject = new GameObject("Indicator Pole - Do not delete");

            ConstraintSource MyConstraint = new ConstraintSource();
            MyConstraint.sourceTransform = PoleObject.transform;
            MyConstraint.weight = 1;

            MainRotator = MainIndicator.gameObject.AddComponent<RotationConstraint>();
            RangeRotator = RangeIndicator.gameObject.AddComponent<RotationConstraint>();

            Vector3 offset = new Vector3(90, 0, 0);
            MainRotator.rotationOffset = offset;
            RangeRotator.rotationOffset = offset;
            


            MainRotator.AddSource(MyConstraint);
            RangeRotator.AddSource(MyConstraint);
        }
        private void InitiateIndicator()
        {
            if (isPlayer) SetupPole();
            if (!ShowMainIndicator) HideMain();
            if (!ShowRangeIndicator) HideRange();
        }
    }
}

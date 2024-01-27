using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FoW
{
    [AddComponentMenu("FogOfWar/HideInFog")]
    public class HideInFog : MonoBehaviour
    {
        public int team = 0;
        bool visible = false;

        [Range(0.0f, 1.0f)]
        public float minFogStrength = 0.2f;
        
        Transform _transform;
        Renderer _renderer;
        Graphic _graphic;
        Canvas _canvas;

        public UnityEvent<bool> Visibility_Changed = new();

        void Start()
        {
            _transform = transform;
            _renderer = GetComponentInChildren<Renderer>();
            _graphic = GetComponentInChildren<Graphic>();
            _canvas = GetComponentInChildren<Canvas>();
        }

        void Update()
        {
            FogOfWarTeam fow = FogOfWarTeam.GetTeam(team);
            if (fow == null)
            {
                //Debug.LogWarning("There is no Fog Of War team for team #" + team.ToString());
                return;
            }
            var temp = fow.GetFogValue(_transform.position) < minFogStrength * 255;
            if (visible != temp)
                Visibility_Changed.Invoke(temp);
            visible = temp;
            if (_renderer != null)
                _renderer.enabled = visible;
            if (_graphic != null)
                _graphic.enabled = visible;
            if (_canvas != null)
                _canvas.enabled = visible;
        }
    }
}

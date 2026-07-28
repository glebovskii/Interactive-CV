using System.Collections.Generic;
using System.Linq;
using Dissolver.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Dissolver.Demo
{
    public class ExampleHolder : MonoBehaviour
    {
        private List<Material> materials = new();

        public List<GameObject> childMeshes;

        public NoiseType NoiseType;

        [Space(10)] public Slider dissolveSlider;
        public Slider delaySlider;
        public Slider timeScaleSlider;
        public Slider edgeWidthSlider;
        public ToggleGroup axisToggleGroup;
        public Toggle x_axis;
        public Toggle y_axis;
        public Toggle z_axis;

        private static readonly int Dissolve = Shader.PropertyToID("_Dissolve");
        private static readonly int Delay = Shader.PropertyToID("_Delay");
        private static readonly int TimeScale = Shader.PropertyToID("_TimeScale");
        private static readonly int EdgeWidth = Shader.PropertyToID("_EdgeWidth");

        public Vector3 Centre => transform.position;

        private bool _hasAnimator;
        private Animator _animator;
        private bool isPaused;

        private Dictionary<Axis, string> axisOptions = new()
        {
            { Axis.X, "_AXIS_X" },
            { Axis.Y, "_AXIS_Y" },
            { Axis.Z, "_AXIS_Z" }
        };

        private Dictionary<NoiseType, string> noiseOptions = new()
        {
            { NoiseType.Simple, "_NOISE_SIMPLE" },
            { NoiseType.Texture, "_NOISE_FROM_TEXTURE" },
            { NoiseType.Voronoi, "_NOISE_VORONOI" },
            { NoiseType.Gradient, "_NOISE_GRADIENT" }
        };

        private void Start()
        {
            foreach (var mesh in childMeshes)
            {
                if (mesh.TryGetComponent<MeshRenderer>(out var renderer))
                {
                    materials.Add(renderer.material);
                }
                else if (mesh.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
                {
                    materials.Add(skinnedMeshRenderer.material);
                }
                else
                {
                    Debug.LogError("Please provide a material with Mesh renderer in " + gameObject.name);
                }
            }

            foreach (var noise in noiseOptions)
            {
                materials.ForEach(x => x.DisableKeyword(noise.Value));
            }

            materials.ForEach(x => x.EnableKeyword(noiseOptions.FirstOrDefault(x => x.Key == NoiseType).Value));

            _animator = GetComponentInChildren<Animator>();
            _hasAnimator = _animator != null;

            dissolveSlider.value = materials[0].GetFloat(Dissolve);
            delaySlider.value = materials[0].GetFloat(Delay);
            timeScaleSlider.value = materials[0].GetFloat(TimeScale);
            edgeWidthSlider.value = materials[0].GetFloat(EdgeWidth);

            x_axis.onValueChanged.AddListener(_ => SetAxis(axisOptions[Axis.X]));
            y_axis.onValueChanged.AddListener(_ => SetAxis(axisOptions[Axis.Y]));
            z_axis.onValueChanged.AddListener(_ => SetAxis(axisOptions[Axis.Z]));

            x_axis.isOn = materials[0].IsKeywordEnabled("_AXIS_X");
            y_axis.isOn = materials[0].IsKeywordEnabled("_AXIS_Y");
            z_axis.isOn = materials[0].IsKeywordEnabled("_AXIS_Z");

            dissolveSlider.onValueChanged.AddListener(OnDissolveChanged);
            delaySlider.onValueChanged.AddListener(OnDelayChanged);
            timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
            edgeWidthSlider.onValueChanged.AddListener(OnEdgeWidthChanged);
        }

        private void SetAxis(string axis)
        {
            axisOptions.Where(x => x.Value != axis).ToList().ForEach(x => SetKeyword(x.Value, false));
            SetKeyword(axis, true);
        }

        private void SetKeyword(string keyword, bool state)
        {
            if (state)
            {
                materials.ForEach(x => x.EnableKeyword(keyword));
            }
            else
            {
                materials.ForEach(x => x.DisableKeyword(keyword));
            }
        }


        private void Update()
        {
            if (!_hasAnimator)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _animator.speed = isPaused ? 1 : 0;
                isPaused = !isPaused;
            }
        }

        private void OnEdgeWidthChanged(float edge)
        {
            materials.ForEach(x => x.SetFloat(EdgeWidth, edge));
        }

        private void OnTimeScaleChanged(float time)
        {
            materials.ForEach(x => x.SetFloat(TimeScale, time));
        }

        private void OnDelayChanged(float delay)
        {
            materials.ForEach(x => x.SetFloat(Delay, delay));
        }

        private void OnDissolveChanged(float dissolve)
        {
            materials.ForEach(x => x.SetFloat(Dissolve, dissolve));
        }
    }
}
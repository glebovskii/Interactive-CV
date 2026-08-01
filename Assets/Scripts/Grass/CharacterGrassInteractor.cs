using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

    public class CharacterGrassInteractor : MonoBehaviour
    {
        private const float ColorMax = 256f;
        private const float RotationMax = 360;

        [Tooltip(
            "Max distance to check snow material collision. Decrease if character leaves tracks on snow when jumping")]
        [SerializeField]
        private float maxHeight = 2f;

        [SerializeField] private LayerMask grassLayer;


        Texture2D input;
        private List<MaterialData> materialData;

        public event Action<Vector4> OnWalk;

        private RaycastHit[] hit = new RaycastHit[1];
        // private RaycastHit hit;

        private void Awake()
        {

        }

        public void Init(List<MaterialData> data)
        {
            materialData = data;
        }

        private void FixedUpdate()
        {
            Draw();
        }

        private void Draw()
        {
        //UpdateMaterials();


        if (Physics.RaycastNonAlloc(new Ray(transform.position, transform.up * -1), hit, float.MaxValue, grassLayer) > 0)
         //if(Physics.Raycast(new Ray(transform.position + transform.up * 2f, transform.up * -1),  out var hit, maxHeight, grassLayer))
        {
            Debug.LogError($"WALK {hit[0].collider.gameObject.name}");
            OnWalk?.Invoke(new Vector4(hit[0].textureCoord.x, hit[0].textureCoord.y, 0, 0));
            //Debug.LogError($"WALK {hit.collider.gameObject.name}");
            //OnWalk?.Invoke(new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0, 0));
            //UpdateMaterials();
        }
    }

        private void UpdateMaterials()
        {
            foreach (var material in materialData)
            {
                var playerPos = transform.position;
                material.Material.SetVector(Player,
                    new Vector4(playerPos.x, playerPos.z, transform.rotation.eulerAngles.y, 0));
                material.Material.SetFloat(Dirty, Random.Range(0, 100));
            }
        }

        private static readonly int Player = Shader.PropertyToID("_Player");
        private static readonly int Dirty = Shader.PropertyToID("_Dirty");

    }

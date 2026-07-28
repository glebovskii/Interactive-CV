using System.Collections.Generic;
using UnityEngine;

namespace Dissolver.Scripts
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshDissolveController : DissolveController
    {
        public List<MeshFilter> meshes;

        public bool asOneMesh = false;

        private MeshFilter _meshFilter;
        
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            mesh = _meshFilter.mesh;
            dissolveMat = GetComponent<MeshRenderer>().material;
            if (asOneMesh)
            {
                Vector3 transformOffset = transform.position;
                CombineInstance[] combine = new CombineInstance[meshes.Count];
                for (int i = 0; i < combine.Length; i++)
                {
                    Quaternion rotationOffset = Quaternion.FromToRotation(transform.eulerAngles, meshes[i].transform.rotation.eulerAngles);
                    meshes[i].transform.position -= transformOffset;
                    meshes[i].transform.rotation = Quaternion.Euler(meshes[i].transform.eulerAngles) * Quaternion.Inverse(rotationOffset);

                    combine[i].mesh = meshes[i].sharedMesh;
                    combine[i].transform = meshes[i].transform.localToWorldMatrix;
                    
                    meshes[i].transform.position += transformOffset;
                    meshes[i].transform.rotation *= rotationOffset;
                }

                Mesh combinedMesh = new Mesh();
                combinedMesh.CombineMeshes(combine);
                // mesh.mesh = combinedMesh;

                _meshFilter.mesh = combinedMesh;
                minBound = _meshFilter.mesh.bounds.min;
                maxBound = _meshFilter.mesh.bounds.max;
                foreach (var m in meshes)
                {
                    m.gameObject.SetActive(ReferenceEquals(_meshFilter.mesh, m));
                }
            }
            else
            {
                minBound = mesh.bounds.min;
                maxBound = mesh.bounds.max;
            }
            dissolveMat.DisableKeyword("_ISSKINNEDMESH");
            // minBound = new Vector3(minBound.x * transform.localScale.x, minBound.y * transform.localScale.y,
            //     minBound.z * transform.localScale.z);
            // maxBound = new Vector3(maxBound.x * transform.localScale.x, maxBound.y * transform.localScale.y,
            //     maxBound.z * transform.localScale.z);
            var index = GetAxis();
            lastKeyword = index.Value;
            RecalculateBounds(index.Key);
        }

    }
}
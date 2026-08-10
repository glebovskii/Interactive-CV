using System.Collections.Generic;
using UnityEngine;

namespace Dissolver.Scripts
{
    public enum Axis
    {
        X,Y,Z
    }
    
    public enum NoiseType
    {
        Simple,
        Texture,
        Voronoi,
        Gradient
    };
    
    
    public class DissolveController : MonoBehaviour
    {
        protected Material dissolveMat;

        protected Vector3 minBound, maxBound;
        protected Mesh mesh;

        protected static readonly int RangeID = Shader.PropertyToID("_Range");
        protected static readonly int MinBound = Shader.PropertyToID("_MinBound");


        protected string keyword;
        protected string lastKeyword;

        
        private Dictionary<Axis, string> axisOptions = new()
        {
            { Axis.X, "_AXIS_X" },
            { Axis.Y, "_AXIS_Y" },
            { Axis.Z, "_AXIS_Z" }
        };

        protected KeyValuePair<Axis, string> GetAxis()
        {
            foreach (var axi in axisOptions)
            {
                if (dissolveMat.IsKeywordEnabled(axi.Value))
                    return axi;
            }

            return new KeyValuePair<Axis, string>(Axis.X, "_AXIS_X");
        }
        
        protected void Update()
        {
            KeyValuePair<Axis, string> index = GetAxis();
            keyword = index.Value;
            
            if (keyword != lastKeyword)
            {
                lastKeyword = keyword;
                RecalculateBounds(index.Key);
            }
        }

        protected void RecalculateBounds(Axis index)
        {
            float range = 0;
            switch (index)
            {
                case Axis.X:
                    range = Mathf.Abs((maxBound.x) - (minBound.x));
                    dissolveMat.SetFloat(RangeID, range);
                    dissolveMat.SetFloat(MinBound, (minBound.x));
                    break;
                case Axis.Y:
                    range = Mathf.Abs((maxBound.y) - (minBound.y));
                    dissolveMat.SetFloat(RangeID, range);
                    dissolveMat.SetFloat(MinBound, (minBound.y));
                    break;
                case Axis.Z:
                    range = Mathf.Abs((maxBound.z) - (minBound.z));
                    dissolveMat.SetFloat(RangeID, range);
                    dissolveMat.SetFloat(MinBound, (minBound.z));
                    break;
            }
        }

        // private void Update()
        // {
        //     positionChange = normMax - Dissolve;
        //     // dissolveMat.SetFloat(HeightID, positionChange);
        //     // dissolveMat.SetFloat(DissolveID, Dissolve);
        // }

        // private void OnDrawGizmos()
        // {
        //     Gizmos.DrawLine(new Vector3(minBound.x, positionChange, minBound.z),
        //         new Vector3(maxBound.x, positionChange, maxBound.z));
        // }
    }
}
using UnityEngine;

// Based on 'Paint.cs' from https://assetstore.unity.com/packages/tools/paintz-free-145977
namespace Src.Scripts.Gameplay
{
    public class Paint
    {
        /// <summary>
        /// World-to-local transformation matrix
        /// </summary>
        public Matrix4x4 paintMatrix;
        public Vector4 channelMask;
        /// <summary>
        /// Identifies the location of the paint in the paint pattern grid
        /// </summary>
        public Vector4 scaleBias;
        public Brush brush;
        public int stepsRemaining;
    }

    [System.Serializable]
    public class Brush
    {
        public Texture2D paintPattern;
        public int splatsX = 1;
        public int splatsY = 1;
        public int splatIndex = -1;

        public float splatScale = 1.0f;
        public float splatRandomScaleMin = 1.0f;
        public float splatRandomScaleMax = 1.0f;

        public float splatRotation = 0f;
        public float splatRandomRotation = 180f;

        public int splatChannel = 0;
        [Tooltip("Number of steps over which the splat will be incrementally painted")]
        public int steps = 2; 
        
        public Vector4 GetMask()
        {
            return splatChannel switch
            {
                0 => new Vector4(1, 0, 0, 0),
                1 => new Vector4(0, 1, 0, 0),
                2 => new Vector4(0, 0, 1, 0),
                3 => new Vector4(0, 0, 0, 1),
                _ => new Vector4(0, 0, 0, 0)
            };
        }

        public Vector4 GetTile()
        {
            float splatscaleX = 1.0f / splatsX;
            float splatscaleY = 1.0f / splatsY;

            int index = splatIndex;
            if (index >= splatsX * splatsY)
            {
                splatIndex = 0;
                index = 0;
            }

            if (splatIndex == -1) index = Random.Range(0, splatsX * splatsY);

            float splatsBiasX = splatscaleX * (index % splatsX);
            float splatsBiasY = splatscaleY * (index / splatsX);

            return new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY);
        }
    }
}
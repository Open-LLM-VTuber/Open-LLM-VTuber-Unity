
using UnityEngine;
using Live2D.Cubism.Framework.MouthMovement;

namespace Live2D 
{
    public sealed class MouthInputController : MonoBehaviour
    {

        [SerializeField]
        public CubismAudioSamplingQuality SamplingQuality;

        [Range(1.0f, 10.0f)]
        public float Gain = 1.0f;

        [Range(0.0f, 1.0f)]
        public float Smoothing;
        
        private float LastRms { get; set; }
        private float VelocityBuffer;
        private CubismMouthController Target { get; set; }


        void Start()
        {
            Target = GetComponent<CubismMouthController>();
            AudioMessageHandler.Instance.OnSamplesPlayed += OnSamplesPlayed;
        }

        void OnDestroy()
        {
            AudioMessageHandler.Instance.OnSamplesPlayed -= OnSamplesPlayed;
        }

        private void OnSamplesPlayed(float[] samples)
        {
            float total = 0f;
            
            int i = 0;
            for (; i <= samples.Length - 4; i += 4)
            {
                float sample0 = samples[i];
                float sample1 = samples[i + 1];
                float sample2 = samples[i + 2];
                float sample3 = samples[i + 3];
                
                total += sample0 * sample0;
                total += sample1 * sample1;
                total += sample2 * sample2;
                total += sample3 * sample3;
            }
            
            for (; i < samples.Length; ++i)
            {
                float sample = samples[i];
                total += sample * sample;
            }

            var rms = Mathf.Sqrt(total / samples.Length) * Gain;

            rms = Mathf.Clamp(rms, 0.0f, 1.0f);

            rms = Mathf.SmoothDamp(LastRms, rms, ref VelocityBuffer, Smoothing * 0.1f);

            // Set rms as mouth opening and store it for next evaluation.
            Target.MouthOpening = rms;

            LastRms = rms;
        }



    }
}
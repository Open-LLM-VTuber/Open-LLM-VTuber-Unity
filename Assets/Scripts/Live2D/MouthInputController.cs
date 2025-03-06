
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
            for (var i = 0; i < samples.Length; ++i)
            {
                var sample = samples[i];
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
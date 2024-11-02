using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public class ParticleSimulate : MonoBehaviour
    {
        [SerializeField] private GameObject selected;
        [SerializeField] private ParticleSystem inParticle;
        [SerializeField] private bool isRunning = true;

        [Header("Particle Data")]
        [SerializeField] private float duration;
        [SerializeField] private float currentTime;

        //== Sub input ui
        [SerializeField] Slider gaugeBar;

        private void Synchronization()
        {
            if(gaugeBar != null)
            {
                currentTime = duration * gaugeBar.value;
            }

            inParticle.Simulate(currentTime, true, true);
            inParticle.Pause();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isRunning = !isRunning;
            }

            if (!isRunning) return;
#if UNITY_EDITOR
            if (Selection.activeGameObject != selected)
            {
                if (Selection.activeGameObject == this.gameObject) return;

                selected = Selection.activeGameObject;
                ParticleSystem newParticle = selected.GetComponent<ParticleSystem>();

                if (inParticle != null && newParticle != null && inParticle != newParticle)
                {
                    //== Deactivate previous particle
                    if(gaugeBar != null) gaugeBar.value = 0.0f;

                    inParticle.Simulate(inParticle.main.duration, true, true);
                    inParticle.Stop();

                    inParticle = newParticle;
                    duration = inParticle.main.duration;
                    currentTime = 0.0f;

                    inParticle.Play();
                }
            }
#endif
            Synchronization();
        }
    }
}

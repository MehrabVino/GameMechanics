using System.Collections;
using UnityEngine;

namespace MechanicGames.BlockBlast
{
    public class BlockBlastEffects : MonoBehaviour
    {
        [Header("Screen Shake")]
        [SerializeField] private float shakeIntensity = 3f;
        [SerializeField] private float shakeDuration = 0.2f;
        
        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem placeParticles;
        [SerializeField] private ParticleSystem clearParticles;
        [SerializeField] private ParticleSystem scoreParticles;
        
        [Header("Light Effects")]
        [SerializeField] private Light gameLight;
        [SerializeField] private float lightFlashIntensity = 2f;
        [SerializeField] private float lightFlashDuration = 0.1f;
        
        private Vector3 originalCameraPosition;
        private float originalLightIntensity;
        
        void Start()
        {
            if (Camera.main != null)
                originalCameraPosition = Camera.main.transform.position;
            
            if (gameLight != null)
                originalLightIntensity = gameLight.intensity;
        }
        
        public void TriggerPlaceEffect(Vector3 position)
        {
            if (placeParticles != null)
            {
                placeParticles.transform.position = position;
                placeParticles.Play();
            }
            
            StartCoroutine(LightFlash());
        }
        
        public void TriggerClearEffect(Vector3 position)
        {
            if (clearParticles != null)
            {
                clearParticles.transform.position = position;
                clearParticles.Play();
            }
            
            StartCoroutine(ScreenShake());
            StartCoroutine(LightFlash());
        }
        
        public void TriggerScoreEffect(Vector3 position)
        {
            if (scoreParticles != null)
            {
                scoreParticles.transform.position = position;
                scoreParticles.Play();
            }
        }
        
        IEnumerator ScreenShake()
        {
            if (Camera.main == null) yield break;
            
            float elapsed = 0f;
            Vector3 originalPos = Camera.main.transform.position;
            
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / shakeDuration;
                float intensity = shakeIntensity * (1f - progress);
                
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity),
                    0f
                );
                
                Camera.main.transform.position = originalPos + shakeOffset;
                yield return null;
            }
            
            Camera.main.transform.position = originalPos;
        }
        
        IEnumerator LightFlash()
        {
            if (gameLight == null) yield break;
            
            float elapsed = 0f;
            float originalIntensity = gameLight.intensity;
            
            while (elapsed < lightFlashDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / lightFlashDuration;
                float intensity = Mathf.Lerp(originalIntensity, lightFlashIntensity, Mathf.Sin(progress * Mathf.PI));
                gameLight.intensity = intensity;
                yield return null;
            }
            
            gameLight.intensity = originalIntensity;
        }
        
        public void SetEffectIntensity(float intensity)
        {
            shakeIntensity = intensity * 3f;
            lightFlashIntensity = intensity * 2f;
        }
        
        public void StopAllEffects()
        {
            StopAllCoroutines();
            
            if (Camera.main != null)
                Camera.main.transform.position = originalCameraPosition;
            
            if (gameLight != null)
                gameLight.intensity = originalLightIntensity;
        }
    }
}

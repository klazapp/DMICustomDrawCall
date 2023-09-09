using UnityEngine;

public class ParticleUIManager : MonoBehaviour
{
   public void OnGenerateParticlesButtonPressed()
   {
      EventManager.GenerateParticles();
   }
}

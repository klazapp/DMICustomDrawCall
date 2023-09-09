using UnityEngine;

public class ParticleUIManager : MonoBehaviour
{
   public void OnGenerateParticlesButtonPressed()
   {
      //Trigger particles generate event
      EventManager.GenerateParticles();
   }
}

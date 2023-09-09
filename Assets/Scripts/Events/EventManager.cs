public class EventManager : TMonoSingleton<EventManager>
{
    //Event to subscribe to when generating particles
    public delegate void GenerateParticlesAction();
    public static event GenerateParticlesAction GenerateParticlesEvent;

    public static void GenerateParticles()
    {
        GenerateParticlesEvent?.Invoke();
    }
}

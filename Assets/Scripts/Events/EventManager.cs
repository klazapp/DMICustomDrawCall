public class EventManager : TMonoSingleton<EventManager>
{
    public delegate void GenerateParticlesAction();
    public static event GenerateParticlesAction GenerateParticlesEvent;

    public static void GenerateParticles()
    {
        GenerateParticlesEvent?.Invoke();
    }
}

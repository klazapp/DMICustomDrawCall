using UnityEngine;

public class GlobalManager : TMonoSingleton<GlobalManager>
{
    public static float smoothDeltaTime;
    public static float deltaTime;
    public static float playerDeltaTime = 1f;
    public static float fixedDeltaTime;

    #region Shader Id
    public static readonly int _ColorsINT = Shader.PropertyToID("_Colors");
    #endregion

    private void FixedUpdate()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        deltaTime = Time.deltaTime;
        smoothDeltaTime = Time.smoothDeltaTime;
    }
}

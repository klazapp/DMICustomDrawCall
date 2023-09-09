public class TSingleton<T> where T : new()
{
	private static object _lock = new object();
	private static T _instance;
	public static T Instance
	{
		get
		{
			if (_instance != null) 
				return _instance;
			
			lock (_lock)
			{
				_instance ??= new T();
			}
			return _instance;
		}
	}
}

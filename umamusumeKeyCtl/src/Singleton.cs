namespace umamusumeKeyCtl
{
    public class Singleton<T> where T : class, new()
    {
        protected static readonly object instanceLock = new object();
        private static T instance = default(T);

        public static T Instance
        {
            get
            {
                if ((object) Singleton<T>.instance == null)
                {
                    lock (Singleton<T>.instanceLock)
                    {
                        if ((object) Singleton<T>.instance == null)
                            Singleton<T>.instance = new T();
                    }
                }

                return Singleton<T>.instance;
            }
        }
    }
}
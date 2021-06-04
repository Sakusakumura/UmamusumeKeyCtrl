using umamusumeKeyCtl.FeaturePointMethod;

namespace umamusumeKeyCtl.Factory
{
    public abstract class FeaturePointMethodFactoryBase
    {
        public MatchingMethodBase Create(MatchingFeaturePointMethod method)
        {
            return createMatchingMethodBase(method);
        }

        protected abstract MatchingMethodBase createMatchingMethodBase(MatchingFeaturePointMethod method);
    }

    public enum MatchingFeaturePointMethod
    {
        Akaze,
        ORB,
        SIFT
    }
}
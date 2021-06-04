using System;
using umamusumeKeyCtl.FeaturePointMethod;

namespace umamusumeKeyCtl.Factory
{
    public class FeaturePointMethodFactory : FeaturePointMethodFactoryBase
    {
        protected override MatchingMethodBase createMatchingMethodBase(MatchingFeaturePointMethod method)
        {
            if (method == MatchingFeaturePointMethod.Akaze)
            {
                return new Akaze();
            }

            if (method == MatchingFeaturePointMethod.ORB)
            {
                return new Orb();
            }

            if (method == MatchingFeaturePointMethod.SIFT)
            {
                return new Sift();
            }

            throw new ArgumentException($"Method type \"{method}\" is not implemented.");
        }
    }
}
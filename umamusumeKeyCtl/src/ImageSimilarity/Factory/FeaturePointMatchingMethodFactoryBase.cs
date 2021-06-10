using umamusumeKeyCtl.ImageSimilarity.Method;

namespace umamusumeKeyCtl.ImageSimilarity.Factory
{
    public abstract class FeaturePointMatchingMethodFactoryBase
    {
        public MatchingMethod Create(DetectorMethod detectorMethod, DescriptorMethod descriptorMethod)
        {
            return createMatchingMethodBase(detectorMethod, descriptorMethod);
        }

        protected abstract MatchingMethod createMatchingMethodBase(DetectorMethod detectorMethod, DescriptorMethod descriptorMethod);
    }
}
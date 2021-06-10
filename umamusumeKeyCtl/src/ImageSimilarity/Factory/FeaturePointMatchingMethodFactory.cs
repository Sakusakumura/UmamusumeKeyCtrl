using System;
using umamusumeKeyCtl.ImageSimilarity.Method;

namespace umamusumeKeyCtl.ImageSimilarity.Factory
{
    public class FeaturePointMatchingMethodFactory : FeaturePointMatchingMethodFactoryBase
    {
        protected override MatchingMethod createMatchingMethodBase(DetectorMethod detectorMethod, DescriptorMethod descriptorMethod)
        {
            return new MatchingMethod(GetDetector(detectorMethod), GetDescriptor(descriptorMethod));
        }

        private MatchingMethodBase GetDetector(DetectorMethod detectorMethod)
        {
            if (detectorMethod == DetectorMethod.FAST)
            {
                return new Fast();
            }

            if (detectorMethod == DetectorMethod.ORB)
            {
                return new Orb();
            }

            if (detectorMethod == DetectorMethod.SIFT)
            {
                return new Sift();
            }

            if (detectorMethod == DetectorMethod.Akaze)
            {
                return new Akaze();
            }

            throw new ArgumentException($"DetectorMethod type \"{detectorMethod}\" is not implemented.");
        }
        
        private MatchingMethodBase GetDescriptor(DescriptorMethod detectorMethod)
        {
            if (detectorMethod == DescriptorMethod.BRIEF)
            {
                return new Brief();
            }

            if (detectorMethod == DescriptorMethod.ORB)
            {
                return new Orb();
            }

            if (detectorMethod == DescriptorMethod.SIFT)
            {
                return new Sift();
            }

            throw new ArgumentException($"DetectorMethod type \"{detectorMethod}\" is not implemented.");
        }
    }
}
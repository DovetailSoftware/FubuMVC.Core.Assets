using System.Collections.Generic;
using System.Linq;

namespace FubuMVC.Core.Assets.Combination
{
    // TODO -- gotta hit this with integration tests.  Hard.
    public class CombinationDeterminationService : ICombinationDeterminationService
    {
        private readonly IAssetCombinationCache _cache;
        private readonly ICombinationPolicyCache _policies;

        public CombinationDeterminationService(IAssetCombinationCache cache, ICombinationPolicyCache policies)
        {
            _cache = cache;
            _policies = policies;
        }

        protected IAssetCombinationCache cache
        {
            get { return _cache; }
        }

        protected IEnumerable<ICombinationPolicy> policies
        {
            get { return _policies; }
        }

        public virtual void TryToReplaceWithCombinations(AssetTagPlan plan)
        {
            applyPoliciesToDiscoverPotentialCombinations(plan);

            TryAllExistingCombinations(plan);
        }

        private void applyPoliciesToDiscoverPotentialCombinations(AssetTagPlan plan)
        {
            IEnumerable<ICombinationPolicy> mimeTypePolicies = _policies.Where(x => x.MimeType == plan.MimeType);
            IEnumerable<ICombinationPolicy> combinationPolicies =
                _cache.OrderedCombinationCandidatesFor(plan.MimeType).Union(mimeTypePolicies);
            combinationPolicies.Each(policy => ExecutePolicy(plan, policy));
        }

        public virtual void ExecutePolicy(AssetTagPlan plan, ICombinationPolicy policy)
        {
            policy.DetermineCombinations(plan).Each(combo => _cache.StoreCombination(plan.MimeType, combo));
        }

        public void TryAllExistingCombinations(AssetTagPlan plan)
        {
            _cache.OrderedListOfCombinations(plan.MimeType).Each(combo => plan.TryCombination(combo));
        }
    }
}
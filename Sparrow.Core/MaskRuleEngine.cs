namespace Sparrow.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Engine for evaluating mask rules.
    /// </summary>
    /// <typeparam name="TMask">The type of the mask.</typeparam>
    /// <typeparam name="TKnow">The type of the knowledge base used by rules.</typeparam>
    /// <remarks>This class is thread-safe.</remarks>
    internal sealed class MaskRuleEngine<TMask, TKnow> where TKnow : IKnowledgeBase<TMask>
    {
        private readonly IMaskRuleSetFactory<TMask, TKnow> ruleFactory;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MaskRuleEngine{TMask, TKnow}"/> class.
        /// </summary>
        /// <param name="ruleFactory">The rule set factory.</param>
        /// <exception cref="System.ArgumentNullException">When ruleProvider null.</exception>
        /// <remarks>This class expects the factory to be thread-safe.</remarks>
        public MaskRuleEngine(IMaskRuleSetFactory<TMask, TKnow> ruleFactory)
        {
            if (ruleFactory == null)
            {
                throw new ArgumentNullException("ruleFactory");
            }

            this.ruleFactory = ruleFactory;
        }

        /// <summary>
        /// Evaluates a file name defined by the context against a set of rules defined when the engine was created.
        /// </summary>
        /// <param name="context">The context containing the file name to evaluate.</param>
        /// <returns>True the file name could be fully process; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">When context null.</exception>
        public async Task<bool> EvaluateFileNameAgainstRulesAsync(FileNameEnvironmentContext<TMask, TKnow> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.AllTokensHaveMask)
            {
                return true;
            }
                        
            IList<IMaskRule<TMask>> rules = this.ruleFactory.CreateRuleSet(context);
                        
            if (await TryEvaluateAgainstExistingKnowledgeBase(context, rules).ConfigureAwait(false))
            {
                context.MasksResolvedCount = context.MaskedFileName.Tokenizer.TokenCount;
            }
            else
            {
                await this.EvaluateWithRulesEvaluator(context, rules).ConfigureAwait(false);
            }
           

            return context.AllTokensHaveMask;
        }

        private async Task<bool> TryEvaluateAgainstExistingKnowledgeBase(FileNameEnvironmentContext<TMask, TKnow> context, IList<IMaskRule<TMask>> rules)
        {
            IList<MaskedFileName<TMask>> maskedFileNames = await context.KnowledgeBase.GetMaskFileNamesByTokenizerAsync(context.MaskedFileName.Tokenizer).ConfigureAwait(false);
            Dictionary<TMask, IMaskRule<TMask>> rulesByMask = rules.ToDictionary(rule => rule.Mask);
            int matches = 0;

            foreach (MaskedFileName<TMask> maskedFileName in maskedFileNames)
            {
                if (await TryMatchMaskedFileName(maskedFileName, rulesByMask).ConfigureAwait(false))
                {
                    matches++;
                }
            }

            if (matches == 1)
            {
                return true;
            }
            else if (matches > 1)
            {
                // TODO: come up with a possible method of choosing best match or maybe it not possible to match
            }

            return false;
        }

        private static async Task<bool> TryMatchMaskedFileName(MaskedFileName<TMask> maskedFileName, Dictionary<TMask, IMaskRule<TMask>> rules)
        {
            MaskedFileNameReader<TMask> reader = new MaskedFileNameReader<TMask>(maskedFileName);

            while (!reader.EndOfMaskedFileName)
            {
                TMask mask;
                string value = reader.ReadNextMaskValue(out mask);

                if (!await rules[mask].IsMatchAsync(value).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task EvaluateWithRulesEvaluator(FileNameEnvironmentContext<TMask, TKnow> context, IList<IMaskRule<TMask>> rules)
        {
            MaskRuleSetEvaluator<TMask, TKnow> evaluator = new MaskRuleSetEvaluator<TMask, TKnow>(context, rules);

            do
            {
                // keep evaluating individually until not successful
                while (await evaluator.EvaluateMaskedStringTokensIndividuallyAsync().ConfigureAwait(false)) { }
            }
            while (await evaluator.EvaluateMaskedStringTokenSequencesAsync().ConfigureAwait(false) && !context.AllTokensHaveMask);
        }
    }
}

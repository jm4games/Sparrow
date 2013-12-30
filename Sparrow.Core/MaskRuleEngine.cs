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
    internal sealed class MaskRuleEngine<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        private readonly IMaskedRuleFactory<TMask, TKnow> ruleFactory;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MaskRuleEngine{TMask, TKnow}"/> class.
        /// </summary>
        /// <param name="ruleFactory">The rule factory.</param>
        /// <exception cref="System.ArgumentNullException">When ruleProvider null.</exception>
        /// <remarks>This class expects the factory to be thread-safe.</remarks>
        public MaskRuleEngine(IMaskedRuleFactory<TMask, TKnow> ruleFactory)
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
        /// <returns>True if the</returns>
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
            
            MaskRulesEvaluator<TMask, TKnow> evaluator = new MaskRulesEvaluator<TMask, TKnow>(context, this.ruleFactory.CreateRules(context));

            do
            {
                // keep evaluating individually until not successful
                while (await evaluator.EvaluateMaskedStringTokensIndividuallyAsync().ConfigureAwait(false)) { }
            }
            while (await evaluator.EvaluateMaskedStringTokenSequencesAsync().ConfigureAwait(false) && !context.AllTokensHaveMask);

            return context.AllTokensHaveMask;
        }
    }
}

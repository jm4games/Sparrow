namespace Sparrow.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Engine for evaluating mask rules.
    /// </summary>
    /// <typeparam name="TMask">The type of mask the rules are for.</typeparam>
    /// <typeparam name="TKnow">The knowledge base type used by rules.</typeparam>
    public sealed class MaskRuleEngine<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        private readonly List<Type> rules;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MaskRuleEngine{TMask, TKnow}"/> class.
        /// </summary>
        /// <param name="ruleTypes">The rule types.</param>
        /// <exception cref="System.ArgumentNullException">When ruleTypes null.</exception>
        /// <exception cref="System.ArgumentException">
        /// A type supplied for a rule was not a subclass of MaskRule or Rule type does not have a default constructor.
        /// </exception>
        public MaskRuleEngine(IEnumerable<Type> ruleTypes)
        {
            if (ruleTypes == null)
            {
                throw new ArgumentNullException("ruleTypes");
            }

            foreach (Type ruleType in ruleTypes)
            {
                if (!ruleType.IsSubclassOf(typeof(MaskRule<TMask, TKnow>)))
                {
                    throw new ArgumentException("A type supplied for a rule was not a subclass of '" + typeof(MaskRule<TMask, TKnow>).FullName + "'.");
                }

                if (ruleType.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new ArgumentException("Rule type '" + ruleType.FullName + "' does not have a default constructor.");
                }
            }

            this.rules = new List<Type>(ruleTypes);
        }

        /// <summary>
        /// Evaluates a file name defined by the context against a set of rules defined when the engine was created.
        /// </summary>
        /// <param name="context">The context containing the file name to evaluate.</param>
        /// <returns>True if the</returns>
        /// <exception cref="System.ArgumentNullException">When context null.</exception>
        /// <remarks>Rules will be added to the context if no rules exists, otherwise only existing rules will be evaluated.</remarks>
        public async Task<bool> EvaluateFileNameAgainstRulesAsync(FileNameEvaluationContext<TMask, TKnow> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.AllTokensHaveMask)
            {
                return true;
            }

            if (context.Rules.Count == 0)
            {
                this.AddRules(context.Rules);
            }

            do
            {
                // keep evaluating individually until not successful
                while (await EvaluateMaskedStringTokensIndividuallyAsync(context).ConfigureAwait(false)) { } 
            }
            while (await EvaluateMaskedStringTokenSequencesAsync(context).ConfigureAwait(false) && !context.AllTokensHaveMask);

            return context.AllTokensHaveMask;
        }

        private void AddRules(IList<MaskRule<TMask, TKnow>> destination)
        {
            foreach (Type type in this.rules)
            {
                destination.Add(Activator.CreateInstance(type) as MaskRule<TMask, TKnow>);
            }
        }

        /// <summary>
        /// Evaluates the tokens of a masked string individually against all rules.
        /// </summary>
        /// <param name="context">The context containing the rules and tokens to evaluate.</param>
        /// <returns>True if at least one rule evaluated to true against a token; otherwise false.</returns>
        private static async Task<bool> EvaluateMaskedStringTokensIndividuallyAsync(FileNameEvaluationContext<TMask, TKnow> context)
        {
            bool maskFound = false;

            for (int i = 0; i < context.MaskedString.Tokenizer.TokenCount && !context.AllTokensHaveMask; i++)
            {
                MatchResult result = await TryMatchRuleAsync(context, context.MaskedString.Tokenizer[i]).ConfigureAwait(false);

                if (!context.MaskedString.IsMaskSetForToken(i) && result.WasMatch)
                {
                    context.MaskedString.SetTokenMask(i, result.Mask);
                    maskFound = true;
                }
            }

            return maskFound;
        }

        /// <summary>
        /// Evaluates all token (without mask) sequences that can be created from the masked string provided by the context,
        /// against all the rules defined in the context.
        /// </summary>
        /// <param name="context">The context containing the rules and tokens to evaluate.</param>
        /// <returns>True if at least one rule evaluated to true against a sequence; otherwise false.</returns>
        private static async Task<bool> EvaluateMaskedStringTokenSequencesAsync(FileNameEvaluationContext<TMask, TKnow> context)
        {            
            bool ruleFound = false;

            for (int i = 0; i < context.MaskedString.Tokenizer.TokenCount && !context.AllTokensHaveMask; i++)
            {
                if (context.MaskedString.IsMaskSetForToken(i))
                {
                    continue;
                }

                ruleFound = await EvaluateMaskedStringTokenSequenceAsync(context, i).ConfigureAwait(false) || ruleFound;
            }

            return ruleFound;
        }

        /// <summary>
        /// Evaluates a sequences of tokens with no mask starting at the provided index.
        /// </summary>
        /// <param name="context">The context containing the rules and tokens to evaluate.</param>
        /// <param name="startingTokenIndex">Index of the token to start generating a sequence from.</param>
        /// <returns>True if at least one rule evaluated to true against a sequence; otherwise false.</returns>
        /// <remarks>
        /// This method will combine a sequence of tokens that don't have a mask into a single string which will be then
        /// evaluated against the rules. For sequence containing more then 2 tokens, the sequence will be gradually reduced 
        /// so that all subsequences are evaluated.
        /// <para />
        /// For example: the tokens {"I", "am", "bob" } would first be evaluated as "I am bob" and then evaluated as "am bob". There
        /// is no point in evaluating sequences of one since all tokens have already been evaluated individually.
        /// </remarks>
        private static async Task<bool> EvaluateMaskedStringTokenSequenceAsync(FileNameEvaluationContext<TMask, TKnow> context, int startingTokenIndex)
        {
            const int IndexNotSetValue = -1;
            int sequenceLastIndex = IndexNotSetValue;

            // try to create sequence
            for (int j = startingTokenIndex + 1; j < context.MaskedString.Tokenizer.TokenCount; j++)
            {
                if (context.MaskedString.IsMaskSetForToken(j))
                {
                    sequenceLastIndex = j;
                }
            }

            if (sequenceLastIndex == IndexNotSetValue &&
                startingTokenIndex + 1 < context.MaskedString.Tokenizer.TokenCount &&
                !context.MaskedString.IsMaskSetForToken(context.MaskedString.Tokenizer.TokenCount - 1))
            {
                // if condition evaluated to true it means that all tokens after index 'i' had no mask set.
                sequenceLastIndex = context.MaskedString.Tokenizer.TokenCount - 1;
            }

            if (sequenceLastIndex != IndexNotSetValue)
            {
                MatchResult result = await TryMatchRuleAsync(context, context.MaskedString.Tokenizer.GetTokenSequence(startingTokenIndex, sequenceLastIndex, " ")).ConfigureAwait(false);

                // combine the strings for the sequence and evaluate the result
                if (result.WasMatch)
                {
                    context.MaskedString.SetTokenMask(
                                           startingTokenIndex,
                                           (sequenceLastIndex - startingTokenIndex) + 1 /* +1 accounts for sequenceLastIndex being inclusive */, 
                                           result.Mask);
                    return true;
                }
            }

            return false;
        }

        private static async Task<MatchResult> TryMatchRuleAsync(FileNameEvaluationContext<TMask, TKnow> context, string value)
        {
            foreach (MaskRule<TMask, TKnow> rule in context.Rules)
            {
                if (await rule.IsMatchAsync(context, value).ConfigureAwait(false))
                {
                    return new MatchResult() { WasMatch = true, Mask = rule.Mask };
                }
            }

            return new MatchResult();
        }

        private struct MatchResult
        {
            public bool WasMatch { get; set; }

            public TMask Mask { get; set; }
        }
    }
}

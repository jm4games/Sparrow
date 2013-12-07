namespace Sparrow.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Evaluates the rules for a specified context.
    /// </summary>
    /// <typeparam name="TMask">The type of the mask.</typeparam>
    /// <typeparam name="TKnow">The type of the knowledge base used by rules.</typeparam>
    internal sealed class MaskRulesEvaluator<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        private readonly IList<IMaskRule<TMask, TKnow>> rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaskRulesEvaluator{TMask, TKnow}"/> class.
        /// </summary>
        /// <param name="context">The context used by rules.</param>
        /// <param name="rules">The rules to evaluate.</param>
        /// <exception cref="System.ArgumentNullException">When context or rules null.</exception>
        /// <exception cref="System.ArgumentException">When no rules are defined.</exception>
        public MaskRulesEvaluator(FileNameEnvironmentContext<TMask, TKnow> context, IList<IMaskRule<TMask, TKnow>> rules)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            if (rules.Count == 0)
            {
                throw new ArgumentException("No rules defined.", "context");
            }
            
            this.Context = context;
            this.rules = rules;
        }

        /// <summary>
        /// Gets the context the rules are evaluated with.
        /// </summary>
        public FileNameEnvironmentContext<TMask, TKnow> Context { get; private set; }

        /// <summary>
        /// Evaluates the tokens of a masked string individually against all rules.
        /// </summary>
        /// <returns>True if at least one rule evaluated to true against a token; otherwise false.</returns>
        public async Task<bool> EvaluateMaskedStringTokensIndividuallyAsync()
        {
            bool maskFound = false;

            for (int i = 0; i < this.Context.MaskedString.Tokenizer.TokenCount && !this.Context.AllTokensHaveMask; i++)
            {
                MatchResult result = await this.TryMatchRuleAsync(this.Context.MaskedString.Tokenizer[i]).ConfigureAwait(false);

                if (!this.Context.MaskedString.IsMaskSetForToken(i) && result.WasMatch)
                {
                    this.Context.MaskedString.SetTokenMask(i, result.Mask);
                    maskFound = true;
                }
            }

            return maskFound;
        }

        /// <summary>
        /// Evaluates all token (without mask) sequences that can be created from the masked string provided by the context,
        /// against all the rules defined in the this.Context.
        /// </summary>
        /// <returns>True if at least one rule evaluated to true against a sequence; otherwise false.</returns>
        public async Task<bool> EvaluateMaskedStringTokenSequencesAsync()
        {
            bool ruleFound = false;

            for (int i = 0; i < this.Context.MaskedString.Tokenizer.TokenCount && !this.Context.AllTokensHaveMask; i++)
            {
                if (this.Context.MaskedString.IsMaskSetForToken(i))
                {
                    continue;
                }

                ruleFound = await this.EvaluateMaskedStringTokenSequenceAsync(i).ConfigureAwait(false) || ruleFound;
            }

            return ruleFound;
        }

        /// <summary>
        /// Evaluates a sequences of tokens with no mask starting at the provided index.
        /// </summary>
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
        private async Task<bool> EvaluateMaskedStringTokenSequenceAsync(int startingTokenIndex)
        {
            const int IndexNotSetValue = -1;
            int sequenceLastIndex = IndexNotSetValue;

            // try to create sequence
            for (int j = startingTokenIndex + 1; j < this.Context.MaskedString.Tokenizer.TokenCount; j++)
            {
                if (this.Context.MaskedString.IsMaskSetForToken(j))
                {
                    sequenceLastIndex = j;
                }
            }

            if (sequenceLastIndex == IndexNotSetValue &&
                startingTokenIndex + 1 < this.Context.MaskedString.Tokenizer.TokenCount &&
                !this.Context.MaskedString.IsMaskSetForToken(this.Context.MaskedString.Tokenizer.TokenCount - 1))
            {
                // if condition evaluated to true it means that all tokens after index 'i' had no mask set.
                sequenceLastIndex = this.Context.MaskedString.Tokenizer.TokenCount - 1;
            }

            if (sequenceLastIndex != IndexNotSetValue)
            {
                MatchResult result = await this.TryMatchRuleAsync(this.Context.MaskedString.Tokenizer.GetTokenSequence(startingTokenIndex, sequenceLastIndex, " ")).ConfigureAwait(false);

                // combine the strings for the sequence and evaluate the result
                if (result.WasMatch)
                {
                    this.Context.MaskedString.SetTokenMask(
                                           startingTokenIndex,
                                           (sequenceLastIndex - startingTokenIndex) + 1 /* +1 accounts for sequenceLastIndex being inclusive */,
                                           result.Mask);
                    return true;
                }
            }

            return false;
        }

        private async Task<MatchResult> TryMatchRuleAsync(string value)
        {
            foreach (IMaskRule<TMask, TKnow> rule in this.rules)
            {
                if (await rule.IsMatchAsync(value).ConfigureAwait(false))
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

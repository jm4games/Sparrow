using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class MaskRuleEngine<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        private readonly List<Type> rules;

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

            this.rules = new List<Type>(rules);
        }

        public Task<bool> EvaluateFileNameAgainstRulesAsync(FileNameEvaluationContext<TMask, TKnow> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.AllTokensHaveMask)
            {
                return Task.FromResult(true);
            }

            if (context.Rules.Count == 0)
            {
                this.AddRules(context.Rules);
            }

            return EvaluateMaskedStringTokensIndividuallyAsync(context)
                        .ContinueWith<Task<bool>>(CompleteTaskEvaluateTokensIndividually, context)
                            .Unwrap();
        }

        private void AddRules(IList<MaskRule<TMask, TKnow>> destination)
        {
            foreach (Type type in this.rules)
            {
                destination.Add(Activator.CreateInstance(type) as MaskRule<TMask, TKnow>);
            }
        }

        private async static Task<bool> EvaluateMaskedStringTokensIndividuallyAsync(FileNameEvaluationContext<TMask, TKnow> context)
        {
            for (int i = 0; i < context.MaskedString.Tokenizer.TokenCount && !context.AllTokensHaveMask; i++)
            {
                MatchResult result = await TryMatchRuleAsync(context, context.MaskedString.Tokenizer[i]).ConfigureAwait(false);

                if (!context.MaskedString.IsMaskSetForToken(i) && result.WasMatch)
                {
                    context.MaskedString.SetTokenMask(i, result.Mask);
                }
            }

            return context.AllTokensHaveMask;
        }

        private static Task<bool> CompleteTaskEvaluateTokensIndividually(Task<bool> completedTask, object state)
        {
            if (completedTask.IsFaulted)
            {
                return completedTask;
            }

            return EvaluateMaskedStringTokenSequencesAsync(state as FileNameEvaluationContext<TMask, TKnow>);
        }

        private async static Task<bool> EvaluateMaskedStringTokenSequencesAsync(FileNameEvaluationContext<TMask, TKnow> context)
        {
            const int IndexNotSetValue = -1;
            int seqStart = IndexNotSetValue;
            int seqEnd = IndexNotSetValue;
            bool creatingSeq = false;

            for (int i = 0; i < context.MaskedString.Tokenizer.TokenCount && !context.AllTokensHaveMask; i++)
            {
                if (!context.MaskedString.IsMaskSetForToken(i))
                {
                    if (seqStart == IndexNotSetValue) // first index of the sequence
                    {
                        seqStart = i;
                        creatingSeq = true;
                    }
                    else
                    {
                        seqEnd = i; // current end of sequence, this will change until a token with a mask is set.
                    }
                }
                else if (creatingSeq)
                {
                    if (seqEnd == IndexNotSetValue) 
                    {
                        // no sequence found, meaning we found 1 token without a mask, but the token after had a mask
                        // therefore we have a sequence of 1 word which isn't a sequence.
                        seqStart = IndexNotSetValue;
                    }
                    else
                    {
                        MatchResult result = await TryMatchRuleAsync(context, context.MaskedString.Tokenizer.GetTokenSequence(seqStart, seqEnd, " ")).ConfigureAwait(false);

                        // combine the strings for the sequence and evaluate the result
                        if (result.WasMatch)
                        {
                            context.MaskedString.SetTokenMask(seqStart, (seqEnd - seqStart) + 1 /* +1 accounts for seqEnd being inclusive */, result.Mask);
                        }

                        seqStart = seqEnd = IndexNotSetValue;
                    }
                }
            }

            return context.AllTokensHaveMask;
        }

        private async static Task<MatchResult> TryMatchRuleAsync(FileNameEvaluationContext<TMask, TKnow> context, string value)
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

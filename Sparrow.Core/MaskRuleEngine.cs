using System;
using System.Collections.Generic;
using System.Linq;

namespace Sparrow.Core
{
    public sealed class MaskRuleEngine<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        private readonly List<Type> rules;

        public MaskRuleEngine(IEnumerable<Type> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            if (rules.Any(type => !type.IsSubclassOf(typeof(MaskRule<TMask, TKnow>))))
            {
                throw new ArgumentException("A type supplied for a rule was not a subclass of '" + typeof(MaskRule<TMask, TKnow>) + "'.");
            }

            //TODO: check for default ctor on all types
            this.rules = new List<Type>(rules);
        }

        public void EvaluateFileNameAgainstRules(FileNameEvaluationContext<TMask, TKnow> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.AllTokensHaveMask)
            {
                return;
            }

            if (context.Rules.Count == 0)
            {
                this.AddRules(context.Rules);
            }

            //TODO: evaluate
        }

        private void AddRules(IList<MaskRule<TMask, TKnow>> destination)
        {
            foreach (Type type in this.rules)
            {
                destination.Add(Activator.CreateInstance(type) as MaskRule<TMask, TKnow>);
            }
        }

        private void EvaluateTokens(FileNameEvaluationContext<TMask, TKnow> context)
        {
            bool maskFound;

            do
            {
                maskFound = false;

                // evaluate each token by itself
                for (int i = 0; i < context.MaskedString.Tokenizer.TokenCount; i++)
                {
                    if (!context.MaskedString.IsMaskSetForToken(i))
                    {
                        foreach (MaskRule<TMask, TKnow> rule in context.Rules)
                        {
                            if (rule.IsMatch(context, context.MaskedString.Tokenizer[i]))
                            {
                                context.MaskedString.SetTokenMask(i, rule.Mask);
                                maskFound = true;
                                break;
                            }
                        }
                    }
                }

                // next evaluate each token sequence greater than 1
                //TODO: evaluate token sequences
            }
            while (maskFound && !context.AllTokensHaveMask);
        }
    }
}

namespace Sparrow.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines contract for creating a mask rule set.
    /// </summary>
    /// <typeparam name="TMask">The type of the mask rules are defined for.</typeparam>
    /// <typeparam name="TKnow">The type of the knowledge base used by rules.</typeparam>
    public interface IMaskRuleSetFactory<TMask, TKnow> where TKnow : IKnowledgeBase<TMask>
    {
        /// <summary>
        /// Creates the rule set.
        /// </summary>
        /// <param name="context">The context to create rules with.</param>
        /// <returns>The rule set.</returns>
        IList<IMaskRule<TMask>> CreateRuleSet(FileNameEnvironmentContext<TMask, TKnow> context);
    }
}

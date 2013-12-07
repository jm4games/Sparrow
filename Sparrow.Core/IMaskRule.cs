namespace Sparrow.Core
{
    using System;

    public interface IMaskRule<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        TMask Mask { get; }

         System.Threading.Tasks.Task<bool> IsMatchAsync(string value);
    }
}

namespace Sparrow.Core
{
    public abstract class MaskRule<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        public abstract TMask Mask { get; }

        public abstract System.Threading.Tasks.Task<bool> IsMatchAsync(FileNameEvaluationContext<TMask, TKnow> context, string value);
    }
}

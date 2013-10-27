namespace Sparrow.Core
{
    public abstract class MaskRule<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        public abstract TMask Mask { get; }

        public abstract bool IsMatch(FileNameEvaluationContext<TMask, TKnow> context, string value);
    }
}

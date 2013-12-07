using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public interface IMaskedRuleFactory<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        IList<IMaskRule<TMask, TKnow>> CreateRules(FileNameEnvironmentContext<TMask, TKnow> context);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public interface IMaskRuleFactory<TMask, TKnow> where TKnow : IKnowledgeBase<TMask>
    {
        IList<IMaskRule<TMask>> CreateRules(FileNameEnvironmentContext<TMask, TKnow> context);
    }
}

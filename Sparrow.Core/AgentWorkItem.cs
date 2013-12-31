// ----------------------------------------------------------------------------------------
// <copyright file="AgentWorkItem.cs" company="Microsoft">
//      Copyright (C) Microsoft. All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------------------

namespace Sparrow.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class AgentWorkItem<TMask, TKnow> where TKnow : IKnowledgeBase
    {
        public AgentWorkItem(FileNameEnvironmentContext<TMask, TKnow> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.Context = context;
        }

        public FileNameEnvironmentContext<TMask, TKnow> Context { get; private set; }

        public bool IsCancelled
        {
            get { return this.Context.FileNameProcessedCompletionSource.Task.IsCanceled; }
        }

        public int MasksNotResolved
        {
            get { return this.Context.MaskedFileName.Tokenizer.TokenCount - this.Context.MasksResolvedCount; }
        }

        public int Attempts { get; set; }
    }
}

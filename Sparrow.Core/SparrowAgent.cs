using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sparrow.Core
{
    public sealed class SparrowAgent<TMask, TKnow> where TKnow : IKnowledgeBase<TMask>
    {
        private readonly PriorityQueue<AgentWorkItem<TMask, TKnow>> queue = new PriorityQueue<AgentWorkItem<TMask, TKnow>>();

        private readonly IDictionary<TMask, MaskConfiguration> masks;

        private readonly MaskRuleEngine<TMask, TKnow> ruleEngine;

        private readonly IFileNameFactory<TMask> fileNameFactory;
        
        private readonly TKnow knowledgeBase;

        public SparrowAgent(IDictionary<TMask, MaskConfiguration> masks, IMaskRuleFactory<TMask, TKnow> ruleFactory, TKnow knowledgeBase, IFileNameFactory<TMask> fileNameFactory)
        {
            if (masks == null)
            {
                throw new ArgumentNullException("masks");
            }

            if (ruleFactory == null)
            {
                throw new ArgumentNullException("factory");
            }

            this.masks = masks;
            this.knowledgeBase = knowledgeBase;
            this.fileNameFactory = fileNameFactory;
            this.ruleEngine = new MaskRuleEngine<TMask, TKnow>(ruleFactory);   
        }

        public Task<string> GetProcessedFileNameAsync(string filePath)
        {
            FileNameEnvironmentContext<TMask, TKnow> context = new FileNameEnvironmentContext<TMask, TKnow>(filePath, this.masks, this.knowledgeBase);
            AgentWorkItem<TMask, TKnow> workItem = new AgentWorkItem<TMask, TKnow>(context);

             // do not await the following line, the invoker of current method doesn't care when it completes
            this.ruleEngine.EvaluateFileNameAgainstRulesAsync(context).ContinueWith(this.CompleteTaskEvaluateFileName, workItem);

            return context.FileNameProcessedCompletionSource.Task.ContinueWith<Task<string>>(this.CompleteTaskFileNameProcessedAsync, workItem).Unwrap();
        }

        private void CompleteTaskEvaluateFileName(Task<bool> task, object workItemState)
        {
            AgentWorkItem<TMask, TKnow> workItem = workItemState as AgentWorkItem<TMask, TKnow>;

            if (task.IsFaulted)
            {
                workItem.Context.FileNameProcessedCompletionSource.SetException(task.Exception);
            }
            else if (task.Result)
            {
                workItem.Context.FileNameProcessedCompletionSource.SetResult(this.fileNameFactory.CreateNewFileName(
                                                                                        workItem.Context.SourceDirectory, 
                                                                                        workItem.Context.MaskedFileName));
            }
            else if (!workItem.IsCancelled)
            {
                //TODO: check to see if we need to start teaching

                workItem.Attempts++;

                // do not await the following line, the invoker of current method doesn't care when it completes
                this.queue.EnqueueAsync(workItem).ContinueWith(this.CompleteTaskEnqueue);
            }
        }

        private void CompleteTaskEnqueue(Task completedTask)
        {
            //TODO: come up with algorithm/settings to throttle filename processing
            this.queue.DequeueAsync()
                        .ContinueWith((task) => this.ruleEngine.EvaluateFileNameAgainstRulesAsync(task.Result.Context)
                                                                    .ContinueWith(this.CompleteTaskEvaluateFileName, task.Result));
        }

        private Task<string> CompleteTaskFileNameProcessedAsync(Task<string> completedTask, object workItem)
        {
            if (completedTask.IsCanceled)
            {
                return this.queue.RemoveAsync(workItem as AgentWorkItem<TMask, TKnow>)
                                    .ContinueWith((task, fileNameProcessedTask) => fileNameProcessedTask as Task<string>, completedTask)
                                        .Unwrap();
            }

            return completedTask;
        }
    }
}

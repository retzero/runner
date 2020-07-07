using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.DistributedTask.ObjectTemplating.Tokens;
using GitHub.DistributedTask.Pipelines.ContextData;
using GitHub.DistributedTask.WebApi;
using GitHub.Runner.Common;
using GitHub.Runner.Sdk;
using Pipelines = GitHub.DistributedTask.Pipelines;

namespace GitHub.Runner.Worker.Handlers
{
    [ServiceLocator(Default = typeof(CompositeActionOutputHandler))]
    public interface ICompositeActionOutputHandler : IHandler
    {
        CompositeActionExecutionData Data { get; set; }
    }

    public sealed class CompositeActionOutputHandler : Handler, ICompositeActionOutputHandler
    {
        public CompositeActionExecutionData Data { get; set; }
        public Task RunAsync(ActionRunStage stage)
        {
            // Evaluate the mapped outputs value
            if (Data.Outputs != null)
            {
                // Evaluate in the steps context
                var evaluator = ExecutionContext.ToPipelineTemplateEvaluator();

                // TODO: Check if Errors thrown are outputted in a good user experience way
                // ex: fromJson('not json"'
                DictionaryContextData actionOutputs = evaluator.EvaluateStepScopeOutputs(Data.Outputs, ExecutionContext.ExpressionValues, ExecutionContext.ExpressionFunctions);
                foreach (var pair in actionOutputs)
                {
                    var outputsName = pair.Key;
                    var outputsValue = pair.Value as StringContextData;
                    // Set output in the whole composite scope. 
                    if (!String.IsNullOrEmpty(outputsName) && !String.IsNullOrEmpty(outputsValue))
                    {
                        ExecutionContext.FinalizeContext.SetOutput(outputsName, outputsValue, out string test);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
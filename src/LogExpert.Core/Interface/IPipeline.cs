using System.Collections.Concurrent;

namespace LogExpert.Core.Interface;

public interface IPipeline<TInput, TOutput>
{
    void Execute (TInput input);
    event Action<TOutput> Finished;
    void Complete();
}

public class TypedPipelineBuilder<TInput, TOutput>
{
    private readonly List<object> _steps = [];

    private TypedPipelineBuilder (List<object> existingSteps)
    {
        _steps = existingSteps;
    }

    public TypedPipelineBuilder () { }

    public TypedPipelineBuilder<TInput, TNext> AddStep<TNext> (Func<TOutput, TNext> step)
    {
        _steps.Add(step);
        return new TypedPipelineBuilder<TInput, TNext>(_steps);
    }

    public IPipeline<TInput, TOutput> Build ()
    {
        return new TypedPipeline<TInput, TOutput>(_steps);
    }
}

public class TypedPipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
{
    private readonly List<object> _steps;
    private readonly BlockingCollection<object>[] _buffers;
    private readonly Task[] _tasks;
    private bool _isStarted;

    public event Action<TOutput> Finished;

    public TypedPipeline(List<object> steps)
    {
        _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        _buffers = new BlockingCollection<object>[_steps.Count];
        _tasks = new Task[_steps.Count];
        
        for (int i = 0; i < _steps.Count; i++)
        {
            _buffers[i] = new BlockingCollection<object>(100); // Bounded capacity
        }
    }

    public void Execute(TInput input)
    {
        if (!_isStarted)
        {
            Start();
        }
        
        _buffers[0].Add(input);
    }

    public void Complete()
    {
        if (_buffers.Length > 0)
        {
            _buffers[0].CompleteAdding();
        }
        
        try
        {
            Task.WaitAll(_tasks);
        }
        catch (AggregateException ex)
        {
            // Log but don't throw - expected on pipeline errors
            Console.WriteLine($"Pipeline completion error: {ex.Message}");
        }
    }

    private void Start()
    {
        for (int i = 0; i < _steps.Count; i++)
        {
            var stepIndex = i;
            var step = _steps[stepIndex];
            
            _tasks[stepIndex] = Task.Run(() => ProcessStep(stepIndex, step));
        }
        
        _isStarted = true;
    }

    private void ProcessStep(int stepIndex, object step)
    {
        var inputBuffer = _buffers[stepIndex];
        var isLastStep = stepIndex == _steps.Count - 1;
        
        try
        {
            // Don't pass cancellation token - let the collection complete naturally
            foreach (var input in inputBuffer.GetConsumingEnumerable())
            {
                try
                {
                    // Invoke the step function via delegate
                    var stepFunc = (Delegate)step;
                    var output = stepFunc.DynamicInvoke(input);
                    
                    if (isLastStep)
                    {
                        Finished?.Invoke((TOutput)output);
                    }
                    else
                    {
                        _buffers[stepIndex + 1].Add(output);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle error - but continue processing
                    Console.WriteLine($"Pipeline step {stepIndex} processing error: {ex.Message}");
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Expected when collection is completed
        }
        finally
        {
            // Complete next buffer
            if (!isLastStep && stepIndex + 1 < _buffers.Length)
            {
                _buffers[stepIndex + 1].CompleteAdding();
            }
        }
    }
}
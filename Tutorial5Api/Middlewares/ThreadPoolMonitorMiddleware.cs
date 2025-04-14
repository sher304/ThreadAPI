namespace Lecture5.Middlewares;

public class ThreadPoolMonitorMiddleware
{
    private readonly RequestDelegate _next;
    
    public ThreadPoolMonitorMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        ThreadPool.GetAvailableThreads(out int workerThreadsBefore, out int completionPortThreadsBefore);
        ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
        
        int usedWorkerThreads = maxWorkerThreads - workerThreadsBefore;
        int usedCompletionPortThreads = maxCompletionPortThreads - completionPortThreadsBefore;
        
        context.Items["ThreadPoolBefore"] = new ThreadPoolInfo
        {
            UsedWorkerThreads = usedWorkerThreads,
            UsedCompletionPortThreads = usedCompletionPortThreads,
            MaxWorkerThreads = maxWorkerThreads,
            MaxCompletionPortThreads = maxCompletionPortThreads
        };
        
        await _next(context);
        
        ThreadPool.GetAvailableThreads(out int workerThreadsAfter, out int completionPortThreadsAfter);
        
        int usedWorkerThreadsAfter = maxWorkerThreads - workerThreadsAfter;
        int usedCompletionPortThreadsAfter = maxCompletionPortThreads - completionPortThreadsAfter;
        
        var before = (ThreadPoolInfo)context.Items["ThreadPoolBefore"];
        var threadDelta = usedWorkerThreadsAfter - before.UsedWorkerThreads;
        
        Console.WriteLine($"Request: {context.Request.Path} - ThreadPool change: {threadDelta}");
    }
}

public class ThreadPoolInfo
{
    public int UsedWorkerThreads { get; set; }
    public int UsedCompletionPortThreads { get; set; }
    public int MaxWorkerThreads { get; set; }
    public int MaxCompletionPortThreads { get; set; }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class StudentsLoadTests
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:5253/students";

    public StudentsLoadTests(ITestOutputHelper output)
    {
        _output = output;
        _httpClient = new HttpClient();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task CompareEndpointPerformance(int concurrentUsers)
    {
        _output.WriteLine($"Running load test with {concurrentUsers} concurrent users");
        
        // Number of requests each user will make
        int requestsPerUser = 5;
        
        // Test the synchronous endpoint
        var syncResults = await RunEndpointTest(
            $"{_baseUrl}", concurrentUsers, requestsPerUser);
        
        // Wait a moment between tests to let the server recover
        await Task.Delay(2000);
        
        // Test the asynchronous endpoint
        var asyncResults = await RunEndpointTest(
            $"{_baseUrl}/async", concurrentUsers, requestsPerUser);
        
        // Output the results
        _output.WriteLine("\nSynchronous Endpoint Results:");
        OutputResults(syncResults);
        
        _output.WriteLine("\nAsynchronous Endpoint Results:");
        OutputResults(asyncResults);
        
        // Compare the results
        _output.WriteLine($"\nComparison:");
        _output.WriteLine($"Total time ratio (sync/async): {syncResults.TotalTime / (double)asyncResults.TotalTime:F2}x");
        _output.WriteLine($"Average response time ratio (sync/async): {syncResults.AverageResponseTime / asyncResults.AverageResponseTime:F2}x");
        
        // Additional metrics
        _output.WriteLine($"\nSuccess rates:");
        _output.WriteLine($"Sync: {syncResults.SuccessRate:P2}");
        _output.WriteLine($"Async: {asyncResults.SuccessRate:P2}");
        
        // Assertions - verify that the async endpoint is more efficient under load
        if (concurrentUsers >= 10)
        {
            // For higher loads, async should be significantly faster
            Assert.True(asyncResults.AverageResponseTime < syncResults.AverageResponseTime, 
                "Async endpoint should have better average response time under load");
        }
    }
    
    [Fact]
    public async Task MeasureScalabilityAsync()
    {
        // This test measures how each endpoint scales with increasing load
        var userCounts = new[] { 1, 5, 10, 25, 50, 100 };
        var requestsPerUser = 3; // Fewer requests per user to keep test duration reasonable
        
        _output.WriteLine("Measuring scalability of both endpoints with increasing load");
        _output.WriteLine("User Count | Sync Avg Response (ms) | Async Avg Response (ms) | Ratio (Sync/Async)");
        _output.WriteLine("----------|------------------------|-------------------------|------------------");
        
        var syncResults = new List<TestResult>();
        var asyncResults = new List<TestResult>();
        
        foreach (var userCount in userCounts)
        {
            // Test sync endpoint
            var syncResult = await RunEndpointTest($"{_baseUrl}", userCount, requestsPerUser);
            syncResults.Add(syncResult);
            
            // Short delay between tests
            await Task.Delay(1000);
            
            // Test async endpoint
            var asyncResult = await RunEndpointTest($"{_baseUrl}/async", userCount, requestsPerUser);
            asyncResults.Add(asyncResult);
            
            // Output row for this user count
            _output.WriteLine($"{userCount,10} | {syncResult.AverageResponseTime,24:F2} | {asyncResult.AverageResponseTime,23:F2} | {syncResult.AverageResponseTime / asyncResult.AverageResponseTime,18:F2}x");
            
            // Longer delay between different user counts
            await Task.Delay(2000);
        }
        
        // Verify that as load increases, the performance gap also increases
        if (userCounts.Length >= 3)
        {
            double ratioAtLowestLoad = syncResults.First().AverageResponseTime / asyncResults.First().AverageResponseTime;
            double ratioAtHighestLoad = syncResults.Last().AverageResponseTime / asyncResults.Last().AverageResponseTime;
            
            _output.WriteLine($"\nPerformance gap analysis:");
            _output.WriteLine($"Sync/Async ratio at lowest load ({userCounts.First()} users): {ratioAtLowestLoad:F2}x");
            _output.WriteLine($"Sync/Async ratio at highest load ({userCounts.Last()} users): {ratioAtHighestLoad:F2}x");
            
            Assert.True(ratioAtHighestLoad > ratioAtLowestLoad, 
                "The performance gap between sync and async should widen as load increases");
        }
    }
    
    [Fact]
    public async Task TestEndpointUnderSustainedLoadAsync()
    {
        // This test runs a sustained load against both endpoints
        // to see how they perform over time
        
        int concurrentUsers = 20;
        int testDurationSeconds = 10;
        
        _output.WriteLine($"Testing both endpoints under sustained load of {concurrentUsers} users for {testDurationSeconds} seconds");
        
        // Run sustained load test on sync endpoint
        var syncResults = await RunSustainedLoadTest($"{_baseUrl}", concurrentUsers, testDurationSeconds);
        
        // Wait between tests
        await Task.Delay(5000);
        
        // Run sustained load test on async endpoint
        var asyncResults = await RunSustainedLoadTest($"{_baseUrl}/async", concurrentUsers, testDurationSeconds);
        
        // Output results
        _output.WriteLine("\nSynchronous Endpoint Sustained Load Results:");
        _output.WriteLine($"Total requests: {syncResults.TotalRequests}");
        _output.WriteLine($"Successful requests: {syncResults.SuccessfulRequests}");
        _output.WriteLine($"Failed requests: {syncResults.FailedRequests}");
        _output.WriteLine($"Requests per second: {syncResults.RequestsPerSecond:F2}");
        _output.WriteLine($"Average response time: {syncResults.AverageResponseTime:F2} ms");
        
        _output.WriteLine("\nAsynchronous Endpoint Sustained Load Results:");
        _output.WriteLine($"Total requests: {asyncResults.TotalRequests}");
        _output.WriteLine($"Successful requests: {asyncResults.SuccessfulRequests}");
        _output.WriteLine($"Failed requests: {asyncResults.FailedRequests}");
        _output.WriteLine($"Requests per second: {asyncResults.RequestsPerSecond:F2}");
        _output.WriteLine($"Average response time: {asyncResults.AverageResponseTime:F2} ms");
        
        // Compare throughput
        _output.WriteLine($"\nThroughput comparison:");
        _output.WriteLine($"Async/Sync requests per second ratio: {asyncResults.RequestsPerSecond / syncResults.RequestsPerSecond:F2}x");
        
        // Assert that async handles higher throughput
        Assert.True(asyncResults.RequestsPerSecond > syncResults.RequestsPerSecond,
            "Async endpoint should support higher throughput under sustained load");
    }
    
    private async Task<TestResult> RunEndpointTest(string endpoint, int concurrentUsers, int requestsPerUser)
    {
        var stopwatch = Stopwatch.StartNew();
        int successfulRequests = 0;
        int failedRequests = 0;
        var responseTimes = new List<long>();
        
        // Create tasks for each simulated user
        var tasks = new List<Task>();
        for (int i = 0; i < concurrentUsers; i++)
        {
            tasks.Add(SimulateUserAsync(
                endpoint, 
                requestsPerUser,
                time => { lock (responseTimes) responseTimes.Add(time); },
                () => Interlocked.Increment(ref successfulRequests),
                () => Interlocked.Increment(ref failedRequests)
            ));
        }
        
        // Wait for all users to complete their requests
        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        
        // Calculate statistics
        return new TestResult
        {
            Endpoint = endpoint,
            ConcurrentUsers = concurrentUsers,
            RequestsPerUser = requestsPerUser,
            TotalRequests = concurrentUsers * requestsPerUser,
            SuccessfulRequests = successfulRequests,
            FailedRequests = failedRequests,
            TotalTime = stopwatch.ElapsedMilliseconds,
            AverageResponseTime = responseTimes.Count > 0 ? responseTimes.Average() : 0,
            MedianResponseTime = responseTimes.Count > 0 ? Median(responseTimes) : 0,
            MinResponseTime = responseTimes.Count > 0 ? responseTimes.Min() : 0,
            MaxResponseTime = responseTimes.Count > 0 ? responseTimes.Max() : 0
        };
    }
    
    private async Task<TestResult> RunSustainedLoadTest(string endpoint, int concurrentUsers, int durationSeconds)
    {
        var stopwatch = Stopwatch.StartNew();
        int successfulRequests = 0;
        int failedRequests = 0;
        var responseTimes = new List<long>();
        var cts = new CancellationTokenSource();
        
        // Set timeout
        cts.CancelAfter(TimeSpan.FromSeconds(durationSeconds));
        
        // Create tasks for each simulated user that run continuously until cancelled
        var tasks = new List<Task>();
        for (int i = 0; i < concurrentUsers; i++)
        {
            tasks.Add(SimulateContinuousUserAsync(
                endpoint,
                cts.Token,
                time => { lock (responseTimes) responseTimes.Add(time); },
                () => Interlocked.Increment(ref successfulRequests),
                () => Interlocked.Increment(ref failedRequests)
            ));
        }
        
        // Wait for the test duration
        await Task.Delay(durationSeconds * 1000);
        
        // Signal cancellation to stop all tasks
        cts.Cancel();
        
        try
        {
            // Wait for all tasks to complete (they should exit quickly once cancelled)
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Expected exception when tasks are cancelled
        }
        
        stopwatch.Stop();
        
        // Calculate statistics
        int totalRequests = successfulRequests + failedRequests;
        double requestsPerSecond = totalRequests / (stopwatch.ElapsedMilliseconds / 1000.0);
        
        return new TestResult
        {
            Endpoint = endpoint,
            ConcurrentUsers = concurrentUsers,
            TotalRequests = totalRequests,
            SuccessfulRequests = successfulRequests,
            FailedRequests = failedRequests,
            TotalTime = stopwatch.ElapsedMilliseconds,
            AverageResponseTime = responseTimes.Count > 0 ? responseTimes.Average() : 0,
            MedianResponseTime = responseTimes.Count > 0 ? Median(responseTimes) : 0,
            RequestsPerSecond = requestsPerSecond
        };
    }
    
    private async Task SimulateUserAsync(
        string endpoint, 
        int requestCount,
        Action<long> recordResponseTime,
        Action onSuccess,
        Action onFailure)
    {
        for (int i = 0; i < requestCount; i++)
        {
            var requestStopwatch = Stopwatch.StartNew();
            
            try
            {
                // Send request to the endpoint
                var response = await _httpClient.GetAsync(endpoint);
                
                requestStopwatch.Stop();
                recordResponseTime(requestStopwatch.ElapsedMilliseconds);
                
                if (response.IsSuccessStatusCode)
                {
                    onSuccess();
                }
                else
                {
                    onFailure();
                }
            }
            catch
            {
                requestStopwatch.Stop();
                onFailure();
            }
        }
    }
    
    private async Task SimulateContinuousUserAsync(
        string endpoint,
        CancellationToken cancellationToken,
        Action<long> recordResponseTime,
        Action onSuccess,
        Action onFailure)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var requestStopwatch = Stopwatch.StartNew();
            
            try
            {
                // Send request to the endpoint
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                
                requestStopwatch.Stop();
                recordResponseTime(requestStopwatch.ElapsedMilliseconds);
                
                if (response.IsSuccessStatusCode)
                {
                    onSuccess();
                }
                else
                {
                    onFailure();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Test duration completed, exit gracefully
                break;
            }
            catch
            {
                requestStopwatch.Stop();
                onFailure();
            }
        }
    }
    
    private void OutputResults(TestResult result)
    {
        _output.WriteLine($"Endpoint: {result.Endpoint}");
        _output.WriteLine($"Concurrent users: {result.ConcurrentUsers}");
        _output.WriteLine($"Requests per user: {result.RequestsPerUser}");
        _output.WriteLine($"Total requests: {result.TotalRequests}");
        _output.WriteLine($"Successful requests: {result.SuccessfulRequests}");
        _output.WriteLine($"Failed requests: {result.FailedRequests}");
        _output.WriteLine($"Total time: {result.TotalTime} ms");
        _output.WriteLine($"Average response time: {result.AverageResponseTime:F2} ms");
        _output.WriteLine($"Median response time: {result.MedianResponseTime:F2} ms");
        _output.WriteLine($"Min response time: {result.MinResponseTime} ms");
        _output.WriteLine($"Max response time: {result.MaxResponseTime} ms");
    }
    
    private static double Median(List<long> values)
    {
        var sortedValues = values.OrderBy(v => v).ToList();
        int count = sortedValues.Count;
        
        if (count == 0)
            return 0;
            
        if (count % 2 == 0)
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        else
            return sortedValues[count / 2];
    }
    
    public class TestResult
    {
        public string Endpoint { get; set; }
        public int ConcurrentUsers { get; set; }
        public int RequestsPerUser { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public long TotalTime { get; set; }
        public double AverageResponseTime { get; set; }
        public double MedianResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public double RequestsPerSecond { get; set; }
        
        public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests : 0;
    }
}
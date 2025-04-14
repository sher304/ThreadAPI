namespace Tutorial5;

    /// <summary>
    /// Examples using Task and async/await for asynchronous programming
    /// </summary>
    public static class AsyncTaskExamples
    {
        /// <summary>
        /// Basic Task usage with continuation
        /// Question: What will be the order of the output?
        /// </summary>
        public static void Example1()
        {
            Console.WriteLine("Async/Task Example 1 started");
            
            Console.WriteLine("Creating and starting a task");
            
            // Create and start a task
            Task task = Task.Run(() => 
            {
                Console.WriteLine("Task: I'm running in the background");
                Thread.Sleep(500); // Simulate work
                Console.WriteLine("Task: I've completed my work");
            });
            
            // Add a continuation
            task.ContinueWith(_ => 
            {
                Console.WriteLine("Continuation: The original task is now complete");
            });
            
            Console.WriteLine("Main thread: Continuing execution without waiting");
            
            // Wait for all tasks to complete
            Task.WaitAll(task);
            
            Console.WriteLine("Async/Task Example 1 completed");
        }

        /// <summary>
        /// Task with return value
        /// Question: What will be the order of the output?
        /// </summary>
        public static void Example2()
        {
            Console.WriteLine("Async/Task Example 2 started");
            
            Console.WriteLine("Starting a task that returns a value");
            
            // Create a task that returns a value
            Task<int> calculationTask = Task.Run(() => 
            {
                Console.WriteLine("Task: Performing calculation");
                Thread.Sleep(1000); // Simulate complex calculation
                Console.WriteLine("Task: Calculation complete");
                return 42;
            });
            
            Console.WriteLine("Main thread: Doing other work while calculation runs");
            
            // Wait for the result
            Console.WriteLine("Main thread: Now waiting for the result");
            int result = calculationTask.Result; // This will block until the result is available
            
            Console.WriteLine($"Main thread: The answer is {result}");
            Console.WriteLine("Async/Task Example 2 completed");
        }

        /// <summary>
        /// async/await basic pattern
        /// Question: What will be the order of the output?
        /// </summary>
        public static void Example3()
        {
            Console.WriteLine("Async/Task Example 3 started");
            
            // Call an async method from a synchronous context
            Task task = RunAsyncExample();
            
            Console.WriteLine("Main thread: Continuing immediately after calling async method");
            
            // Wait for the async operation to complete
            task.Wait();
            
            Console.WriteLine("Async/Task Example 3 completed");
        }
        
        private static async Task RunAsyncExample()
        {
            Console.WriteLine("Inside async method: Before first await");
            
            // Simulate an asynchronous operation
            await Task.Delay(1000);
            
            Console.WriteLine("Inside async method: Between awaits");
            
            // Simulate another asynchronous operation
            await Task.Delay(500);
            
            Console.WriteLine("Inside async method: After all awaits");
        }

        /// <summary>
        /// Task composition with WhenAll
        /// Question: What will be the order of the output?
        /// </summary>
        public static void Example4()
        {
            Console.WriteLine("Async/Task Example 4 started");
            
            Task<string> task1 = Task.Run(() => 
            {
                Console.WriteLine("Task 1 started");
                Thread.Sleep(800);
                Console.WriteLine("Task 1 completed");
                return "Result from Task 1";
            });
            
            Task<string> task2 = Task.Run(() => 
            {
                Console.WriteLine("Task 2 started");
                Thread.Sleep(400);
                Console.WriteLine("Task 2 completed");
                return "Result from Task 2";
            });
            
            Task<string> task3 = Task.Run(() => 
            {
                Console.WriteLine("Task 3 started");
                Thread.Sleep(1200);
                Console.WriteLine("Task 3 completed");
                return "Result from Task 3";
            });
            
            Console.WriteLine("Main thread: Waiting for all tasks to complete");
            
            // Wait for all tasks to complete and get results
            Task.WhenAll(task1, task2, task3).ContinueWith(t => 
            {
                Console.WriteLine("All tasks completed!");
                Console.WriteLine($"Results: {task1.Result}, {task2.Result}, {task3.Result}");
            }).Wait();
            
            Console.WriteLine("Async/Task Example 4 completed");
        }

        /// <summary>
        /// Task composition with WhenAny
        /// Question: What will be the order of the output?
        /// </summary>
        public static void Example5()
        {
            Console.WriteLine("Async/Task Example 5 started");
            
            Console.WriteLine("Creating tasks with different completion times");
            
            Task<string> task1 = Task.Run(async () => 
            {
                await Task.Delay(2000);
                Console.WriteLine("Task 1 completed (slow)");
                return "Task 1 (2000ms)";
            });
            
            Task<string> task2 = Task.Run(async () => 
            {
                await Task.Delay(500);
                Console.WriteLine("Task 2 completed (fast)");
                return "Task 2 (500ms)";
            });
            
            Task<string> task3 = Task.Run(async () => 
            {
                await Task.Delay(1000);
                Console.WriteLine("Task 3 completed (medium)");
                return "Task 3 (1000ms)";
            });
            
            Console.WriteLine("Waiting for the first task to complete...");
            
            // Create a task that completes when any of the tasks complete
            Task<Task<string>> whenAnyTask = Task.WhenAny(task1, task2, task3);
            
            // Wait for the first task to complete
            whenAnyTask.Wait();
            
            // Get the completed task
            Task<string> completedTask = whenAnyTask.Result;
            
            Console.WriteLine($"First completed: {completedTask.Result}");
            
            // Wait for all tasks to ensure they all complete
            Task.WaitAll(task1, task2, task3);
            
            Console.WriteLine("Async/Task Example 5 completed");
        }
    }
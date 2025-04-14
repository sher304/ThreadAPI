namespace Tutorial5;

    /// <summary>
    /// Examples using the Thread class for concurrency
    /// </summary>
    public static class ThreadExamples
    {
        /// <summary>
        /// Basic thread creation with sequential execution
        /// Question: What will be the order of the output?
        /// First the main threads is going to run, after when initialized the thread worker, whe may run this thread
        /// and the thread worker, is doing its job.
        /// after the thread.join() has been called, so its waits when the main thread is going to complete
        /// </summary>
        public static void Example1()
        {
            Console.WriteLine("Thread Example 1 started");
            
            Console.WriteLine("Main thread: Hello from the main thread");
            
            Thread thread = new Thread(() => 
            {
                Console.WriteLine("Worker thread: Hello from the worker thread");
            });
            
            thread.Start();
            
            // Join waits for the thread to complete
            thread.Join();
            
            Console.WriteLine("Main thread: Worker thread has completed");
            Console.WriteLine("Thread Example 1 completed");
        }

        /// <summary>
        /// Multiple threads without Join
        /// Question: What will be the order of the output?
        /// So it's going to print the starting of the main thread
        /// When we created the thread1, whe sleep the current thread for a one second (with delay)
        /// during the delay, we run the thread2, and it doesn't with for a thread1 and gives us an output
        /// after the delay, we get an output of the running the thread1
        /// </summary>
        public static void Example2()
        {
            Console.WriteLine("Thread Example 2 started");
            
            Console.WriteLine("Main thread: Creating threads");
            
            Thread thread1 = new Thread(() => 
            {
                Thread.Sleep(100); // Small delay
                Console.WriteLine("Thread 1: I'm running");
            });
            
            Thread thread2 = new Thread(() => 
            {
                Console.WriteLine("Thread 2: I'm running");
            });
            
            thread1.Start();
            thread2.Start();
            
            // No Join calls, so the main thread continues immediately
            
            Console.WriteLine("Main thread: Created and started threads");
            Console.WriteLine("Thread Example 2 completed");
        }

        /// <summary>
        /// Thread priorities
        /// Question: What will be the order of the output?
        /// Thread priority is just a suggestion to the system.
        ///The system tries to follow it, but not always.
        /// That's why your output isn't always in the same order.
        /// </summary>
        public static void Example3()
        {
            Console.WriteLine("Thread Example 3 started");
            
            Thread highPriorityThread = new Thread(() => 
            {
                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"High priority thread: Message {i}");
                    Thread.Sleep(100);
                }
            });
            
            Thread normalPriorityThread = new Thread(() => 
            {
                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"Normal priority thread: Message {i}");
                    Thread.Sleep(100);
                }
            });
            
            Thread lowPriorityThread = new Thread(() => 
            {
                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"Low priority thread: Message {i}");
                    Thread.Sleep(100);
                }
            });
            
            // Set thread priorities
            highPriorityThread.Priority = ThreadPriority.Highest;
            lowPriorityThread.Priority = ThreadPriority.Lowest;
            
            // Start threads
            highPriorityThread.Start();
            normalPriorityThread.Start();
            lowPriorityThread.Start();
            
            // Wait for all threads to complete
            highPriorityThread.Join();
            normalPriorityThread.Join();
            lowPriorityThread.Join();
            
            Console.WriteLine("Thread Example 3 completed");
        }

        /// <summary>
        /// Thread with shared data and race condition
        /// Question: What will be the output and why might it vary?
        /// so there is a simple race condition problem, to avoid it
        /// we should use the sync to the counter.
        /// 🔐 Use lock or Interlocked.Increment to avoid it.
        /// </summary>
        public static void Example4()
        {
            Console.WriteLine("Thread Example 4 started");
            
            int counter = 0;
            
            Thread thread1 = new Thread(() => 
            {
                for (int i = 0; i < 100000; i++)
                {
                    counter++;
                }
                Console.WriteLine($"Thread 1 finished. Counter value: {counter}");
            });
            
            Thread thread2 = new Thread(() => 
            {
                for (int i = 0; i < 100000; i++)
                {
                    counter++;
                }
                Console.WriteLine($"Thread 2 finished. Counter value: {counter}");
            });
            
            thread1.Start();
            thread2.Start();
            
            thread1.Join();
            thread2.Join();
            
            Console.WriteLine($"Final counter value: {counter}");
            Console.WriteLine("Thread Example 4 completed");
        }

        /// <summary>
        /// Thread with data passing and foreground/background threads
        /// Question: Will the background thread always complete its work?
        /// foregroundThread is joined, so the main thread waits for it.
        ///backgroundThread is not joined, and it's marked as .IsBackground = true.
        ///If the main method (Example5) finishes before the background thread is done,
        /// the background thread is killed immediately — even if it's in the middle of work or sleeping!
        
        
        /// 🟢 Foreground thread = "Hey! I'm doing important stuff! Don't shut down the app until I'm done!"
        /// Background thread = "I'm just doing side work. If the app wants to close, it's fine to stop me."
        /// </summary>
        public static void Example5()
        {
            Console.WriteLine("Thread Example 5 started");
            
            Thread foregroundThread = new Thread(param => 
            {
                string message = param as string;
                Console.WriteLine($"Foreground thread received: {message}");
                Thread.Sleep(100);
                Console.WriteLine("Foreground thread completed");
            });
            
            Thread backgroundThread = new Thread(() => 
            {
                Console.WriteLine("Background thread started");
                Thread.Sleep(5000); // Long operation
                Console.WriteLine("Background thread completed"); // This might not execute
            });
            
            // Set thread types
            foregroundThread.IsBackground = false; // Default is foreground
            backgroundThread.IsBackground = true;
            
            // Start threads
            foregroundThread.Start("Hello from main thread");
            backgroundThread.Start();
            
            // Wait for foreground thread only
            foregroundThread.Join();
            
            Console.WriteLine("Thread Example 5 completed");
            // Program may exit before background thread completes
        }
    }

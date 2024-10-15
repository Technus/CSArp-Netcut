using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Thread = (System.Threading.CancellationTokenSource cts, System.Threading.Tasks.Task task, string name);
using System.Threading.Tasks;
using System;

namespace CSArp.Model;

public static class TaskBuffer
{
    public static int Count => buffer.Count;

    public static int AliveCount => buffer.Count(t => !t.task.IsCompleted);

    private static readonly List<Thread> buffer = [];

    public static Task Add(CancellationTokenSource cts, Func<Task> task, string name = default)
    {
        var t = task();
        buffer.Add((cts, t, name));
        return t;
    }

    private static async ValueTask Remove(Thread thread)
    {
        await thread.cts.CancelAsync();
        _ = buffer.Remove(thread);
        await thread.task;
        thread.cts.Dispose();
    }

    public static async ValueTask StopThreadByName(string threadName)
    {
        foreach (var t in buffer.Where(t => t.name.Equals(threadName)).ToArray())
            await Remove(t);
    }

    public static async ValueTask StopThreadByPrefix(string prefix)
    {
        foreach (var t in buffer.Where(t => t.name.StartsWith(prefix)).ToArray())
            await Remove(t);
    }

    public static async ValueTask Clear()
    {
        foreach (var t in buffer)
            if (!t.task.IsCompleted)
                await Remove(t);
        buffer.Clear();
    }
}

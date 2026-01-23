using System;
using System.Threading.Channels;
using System.Threading.Tasks;

var timeout = TimeSpan.FromSeconds(30);
var startTime = DateTime.UtcNow;
int iteration = 0;

while (DateTime.UtcNow - startTime < timeout)
{
    iteration++;
    var channel = Channel.CreateUnbounded<int>(new UnboundedChannelOptions
    {
        SingleWriter = false,
        SingleReader = false,
        AllowSynchronousContinuations = false
    });

    await channel.Writer.WriteAsync(1);
    await channel.Writer.WriteAsync(2);

    channel.Reader.TryRead(out _);

    var t1 = Task.Run(() => channel.Reader.TryRead(out _));
    var t2 = Task.Run(() => channel.Writer.TryComplete());

    await Task.WhenAll(t1, t2);

    channel.Reader.TryRead(out _);

    if (!channel.Reader.Completion.IsCompleted)
    {
        Console.WriteLine($"BUG REPRODUCED at iteration {iteration}!");
        Console.WriteLine($"Completion.IsCompleted = {channel.Reader.Completion.IsCompleted}");
        Environment.Exit(1);
    }
    
    if (iteration % 10000 == 0)
    {
        Console.WriteLine($"Iteration {iteration} - still working");
    }
}

Console.WriteLine($"Completed {iteration} iterations without reproducing the bug.");

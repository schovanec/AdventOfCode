using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace day23
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var program = IntcodeMachine.ParseCode(File.ReadLines("input.txt").First());

            var mode = int.Parse(args.First());
            if (mode == 1)
            {
                var network = new SimpleNetworkSwitch();
                var tasks = new List<Task>();
                for (var id = 0; id < 50; ++id)
                {
                    tasks.Add(StartProgram(program, id, network));
                }

                var result = await network.ResultTask;

                Console.WriteLine($"Part 1 Result = {result}");
            }
            else if (mode == 2)
            {
                var network = new NetworkSwitchMonitor(50);
                var tasks = new List<Task>();
                for (var id = 0; id < 50; ++id)
                {
                    tasks.Add(StartProgram(program, id, network));
                }

                var result = await network.StartMonitor();

                Console.WriteLine($"Part 2 Result = {result}");
            }
        }

        private static Task StartProgram(long[] program, long address, INetworkSwitch network)
        {
            return Task.Factory.StartNew(
                action: () =>
                {
                    var writeQueue = new Queue<long>();

                    var readQueue = new Queue<long>();
                    readQueue.Enqueue(address);

                    Console.WriteLine($"Starting: Address={address}");

                    IntcodeMachine.RunProgram(
                        program: program,
                        write: v =>
                        {
                            writeQueue.Enqueue(v);
                            if (writeQueue.Count >= 3)
                            {
                                var target = writeQueue.Dequeue();
                                var packet = (writeQueue.Dequeue(), writeQueue.Dequeue());
                                Console.WriteLine($"Sending {packet} to {target}");

                                network.Send(address, target, packet);
                            }
                        },
                        input: () =>
                        {
                            if (readQueue.Count == 0)
                            {
                                var packet = network.Receive(address);
                                if (packet.HasValue)
                                {
                                    Console.WriteLine($"Received {packet} for {address}");
                                    readQueue.Enqueue(packet.Value.x);
                                    readQueue.Enqueue(packet.Value.y);
                                }
                            }

                            if (readQueue.Count > 0)
                                return readQueue.Dequeue();
                            else
                                return -1;
                        });
                },
                creationOptions: TaskCreationOptions.LongRunning
            );
        }

        private interface INetworkSwitch
        {
            void Send(long sender, long target, (long x, long y) packet);
            (long x, long y)? Receive(long recipient);
        }

        private class SimpleNetworkSwitch : INetworkSwitch
        {
            private readonly ConcurrentDictionary<long, ConcurrentQueue<(long x, long y)>> queues
                = new ConcurrentDictionary<long, ConcurrentQueue<(long x, long y)>>();

            private readonly TaskCompletionSource<(long x, long y)> result = new TaskCompletionSource<(long x, long y)>();

            public Task<(long x, long y)> ResultTask => result.Task;

            public void Send(long sender, long target, (long x, long y) packet)
            {
                if (target == 255)
                {
                    result.TrySetResult(packet);
                }
                else
                {
                    var queue = queues.GetOrAdd(target, _ => new ConcurrentQueue<(long x, long y)>());
                    queue.Enqueue(packet);
                }
            }

            public (long x, long y)? Receive(long recipient)
            {
                if (queues.TryGetValue(recipient, out var queue))
                {
                    if (queue.TryDequeue(out var packet))
                        return packet;
                }

                return default;
            }
        }

        private enum NodeState
        {
            Idle,
            Sending,
            Receiving,
            Waiting
        }

        private class NetworkSwitchMonitor : INetworkSwitch
        {
            private const long MonitorAddress = 255;

            private readonly INetworkSwitch inner;
            private readonly object sync = new object();
            private readonly NodeState[] states;
            private readonly Queue<(long x, long y)> natBuffer = new Queue<(long x, long y)>();

            public NetworkSwitchMonitor(int count, INetworkSwitch inner = null)
            {
                this.inner = inner ?? new SimpleNetworkSwitch();
                states = new NodeState[count];
            }

            public void Send(long sender, long target, (long x, long y) packet)
            {
                lock (sync)
                {
                    if (target == MonitorAddress)
                    {
                        natBuffer.Enqueue(packet);
                        Monitor.Pulse(sync);
                    }
                    else
                    {
                        inner.Send(sender, target, packet);
                        SetNodeState(sender, NodeState.Sending);
                    }
                }
            }

            public (long x, long y)? Receive(long recipient)
            {
                lock (sync)
                {
                    var result = inner.Receive(recipient);
                    SetNodeState(recipient, result.HasValue ? NodeState.Receiving : NodeState.Waiting);

                    return result;
                }
            }

            private void SetNodeState(long node, NodeState state)
            {
                if (node != MonitorAddress)
                {
                    lock (sync)
                    {
                        var current = states[node];

                        if (state == NodeState.Waiting && (current == NodeState.Waiting || current == NodeState.Idle))
                            state = NodeState.Idle;

                        if (current != state)
                        {
                            states[node] = state;
                            Monitor.Pulse(sync);
                        }
                    }
                }
            }

            public Task<(long x, long y)> StartMonitor()
            {
                var source = new TaskCompletionSource<(long x, long y)>();
                Task.Factory.StartNew(
                    action: () =>
                    {
                        var lastReceived = default((long x, long y)?);
                        var lastDelevered = default((long x, long y)?);

                        lock (sync)
                        {
                            while (true)
                            {
                                if (natBuffer.Count == 0 && (!lastReceived.HasValue || states.Any(x => x != NodeState.Idle)))
                                {
                                    Monitor.Wait(sync);
                                }
                                else
                                {
                                    while (natBuffer.Count > 0)
                                    {
                                        var next = natBuffer.Dequeue();
                                        Console.WriteLine($"NAT received {next}");
                                        lastReceived = next;
                                    }

                                    if (lastReceived.HasValue && states.All(x => x == NodeState.Idle))
                                    {
                                        Console.WriteLine($"Waking computers - Sending {lastReceived} to 0");
                                        Send(MonitorAddress, 0, lastReceived.Value);

                                        if (lastDelevered.HasValue && lastDelevered.Value.y == lastReceived.Value.y)
                                        {
                                            Console.WriteLine("NAT sent same value twice!");
                                            source.SetResult(lastReceived.Value);
                                            return;
                                        }

                                        lastDelevered = lastReceived;
                                        lastReceived = default;
                                        Monitor.Wait(sync);
                                    }
                                }
                            }
                        }
                    },
                    creationOptions: TaskCreationOptions.LongRunning);

                return source.Task;
            }
        }
    }
}

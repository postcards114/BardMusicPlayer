﻿/*
 * Copyright(c) 2023 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System.Collections.Concurrent;
using System.Diagnostics;
using BardMusicPlayer.Seer.Events;
using Machina.FFXIV;
using Machina.FFXIV.Oodle;
using Machina.Infrastructure;
using GameRegion = BardMusicPlayer.Quotidian.Enums.GameRegion;

namespace BardMusicPlayer.Seer.Utilities;

internal class MachinaManager : IDisposable
{
    internal static MachinaManager Instance => LazyInstance.Value;

    private static readonly Lazy<MachinaManager> LazyInstance = new(() => new MachinaManager());

    private MachinaManager()
    {
        _lock = new object();

        Trace.UseGlobalLock = false;
        Trace.Listeners.Add(new MachinaLogger());

        while (BmpSeer.Instance.Games.Count < 1) Thread.Sleep(1);

        _monitor = new FFXIVNetworkMonitor();

        if (BmpSeer.Instance.Games.Values.First().GameRegion == GameRegion.Global)
        {
            _useDeucalion = true;
        } 
        else
        {
            _monitor = new FFXIVNetworkMonitor
            {
                MonitorType = Environment.GetEnvironmentVariable("WINEPREFIX") != null ? NetworkMonitorType.WinPCap : NetworkMonitorType.RawSocket,
                OodlePath = BmpSeer.Instance.Games.Values.First().GamePath + @"\game\ffxiv_dx11.exe",
                OodleImplementation = OodleImplementation.FfxivUdp
            };

            _monitor.MessageReceivedEventHandler += MessageReceivedEventHandler;
        }
    }

    private static readonly List<int> Lengths = new() {48, 56, 88, 664, 672, 688, 928, 3576, 3704 };
    private readonly FFXIVNetworkMonitor _monitor;
    private readonly object _lock;
    private bool _monitorRunning;
    private static bool _useDeucalion = false;
    private static ConcurrentDictionary<uint, FFXIVNetworkMonitor> _deucalionSessions = new();

    internal delegate void MessageReceivedHandler(int processId, byte[] message);

    internal event MessageReceivedHandler MessageReceived;

    internal void AddGame(int pid)
    {
        if (!_useDeucalion)
        {
            lock (_lock)
            {
                if (_monitorRunning)
                {
                    _monitor.Stop();
                    _monitorRunning = false;
                }

                _monitor.ProcessIDList.Add((uint)pid);
                _monitor.Start();
                _monitorRunning = true;
            }
        }
        else
        {
            try
            {
                var deucalionSession = new FFXIVNetworkMonitor
                {
                    ProcessID    = (uint)pid,
                    UseDeucalion = true
                };
                _deucalionSessions.TryAdd((uint)pid, deucalionSession);
                deucalionSession.MessageReceivedEventHandler += MessageReceivedEventHandler;
                Console.WriteLine("Calling Machina Deucalion monitor Start()");
                deucalionSession.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

    internal void RemoveGame(int pid)
    {
        if (!_useDeucalion)
        {
            lock (_lock)
            {
                if (_monitorRunning)
                {
                    _monitor.Stop();
                    _monitorRunning = false;
                }

                _monitor.ProcessIDList.Remove((uint)pid);
                if (_monitor.ProcessIDList.Count <= 0) return;

                _monitor.Start();
                _monitorRunning = true;
            }
        } 
        else
        {
            try
            {
                if (_deucalionSessions.TryRemove((uint)pid, out var deucalionSession)) {
                    deucalionSession.Stop();
                    deucalionSession.MessageReceivedEventHandler -= MessageReceivedEventHandler;
                    deucalionSession.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }

    private void MessageReceivedEventHandler(TCPConnection connection, long epoch, byte[] message)
    {
        /*if (BmpSeer.Instance.Games.TryGetValue((int)connection.ProcessId, out Game game))
        {
            string hexString = BitConverter.ToString(message);
            System.Diagnostics.Debug.WriteLine("DE:" + hexString + " " + message.Length.ToString());
        }
            //game.PublishEvent(new NetworkPacket(EventSource.Machina, message));
        */
        if (BmpSeer.Instance.Games.TryGetValue((int)connection.ProcessId, out Game game)) game.PublishEvent(new NetworkPacket(EventSource.Machina, message));

        if (Lengths.Contains(message.Length))
            MessageReceived?.Invoke((int) connection.ProcessId, message);
    }

    ~MachinaManager() { Dispose(); }

    public void Dispose()
    {
        if (!_useDeucalion)
        {
            lock (_lock)
            {
                if (_monitorRunning)
                {
                    _monitor.Stop();
                    _monitorRunning = false;
                }

                _monitor.ProcessIDList.Clear();
                _monitor.MessageReceivedEventHandler -= MessageReceivedEventHandler;
            }
        } 
        else
        {
            foreach (var deucalionSession in _deucalionSessions.Values)
            {
                try
                {
                    deucalionSession.Stop();
                    deucalionSession.MessageReceivedEventHandler -= MessageReceivedEventHandler;
                    deucalionSession.Dispose();
                }
                catch
                {
                    // ignored
                }
                _deucalionSessions.Clear();
            }
        }
    }
}

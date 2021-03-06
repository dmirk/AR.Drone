﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using AI.Core.System;
using AR.Drone.Client.Configuration;
using AR.Drone.Client.Packets;

namespace AR.Drone.Client.Workers.Acquisition
{
    public class ConfigurationAcquisitionWorker : WorkerBase
    {
        private const int ControlPort = 5559;
        private const int NetworkBufferSize = 0x10000;
        private const int ConfigTimeout = 1000;
        private readonly INetworkConfiguration _networkConfiguration;
        private readonly Action<ConfigurationPacket> _configurationAcquired;

        public ConfigurationAcquisitionWorker(INetworkConfiguration networkConfiguration, Action<ConfigurationPacket> configurationAcquired)
        {
            _networkConfiguration = networkConfiguration;
            _configurationAcquired = configurationAcquired;
        }

        protected override void Loop(CancellationToken token)
        {
            using (var tcpClient = new TcpClient(_networkConfiguration.DroneHostname, ControlPort))
            using (NetworkStream stream = tcpClient.GetStream())
            {
                var buffer = new byte[NetworkBufferSize];
                Stopwatch swConfigTimeout = Stopwatch.StartNew();
                while (token.IsCancellationRequested == false && swConfigTimeout.ElapsedMilliseconds < ConfigTimeout)
                {
                    int offset = 0;
                    if (tcpClient.Available > 0)
                    {
                        offset += stream.Read(buffer, offset, buffer.Length);

                        // config eof check
                        if (offset > 0 && buffer[offset - 1] == 0x00)
                        {
                            var data = new byte[offset];
                            Array.Copy(buffer, data, offset);
                            var packet = new ConfigurationPacket
                                {
                                    Timestamp = DateTime.UtcNow.Ticks,
                                    Data = data
                                };
                            _configurationAcquired(packet);

                            return;
                        }
                    }
                    Thread.Sleep(10);
                }
            }
        }
    }
}
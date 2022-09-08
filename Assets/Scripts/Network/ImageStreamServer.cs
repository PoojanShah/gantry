using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace Network
{
    public class ConnectedClient
    {
        public TcpClient client;
        public BinaryWriter writer;

        public ConnectedClient(TcpClient aClient)
        {
            client = aClient;
            writer = new BinaryWriter(client.GetStream());
        }

        public bool SendImageData(byte[] aData)
        {
            if (!client.Connected)
                return false;

            writer.Write(aData.Length);
            writer.Write(aData);
            return true;
        }
    }

    public class ImageStreamServer
    {
	    private TcpListener _server;
        private bool _isServerRunning;
        private Thread _listenerThread, _sendingThread;
        private bool _isSending;
        private readonly List<ConnectedClient> _clients = new();

        public ImageStreamServer()
        {
	        Run();
        }

        private void Run()
        {
	        _listenerThread = new Thread(ListenThread);
	        _listenerThread.Start();

	        _sendingThread = new Thread(SendThread);
	        _sendingThread.Start();

	        _isSending = false;
	        _isServerRunning = false;
	        _server.Stop();
	        _listenerThread.Join();
	        _sendingThread.Join();
        }

        private void ListenThread()
        {
            _server = new TcpListener(NetworkHelper.GetMyIp(), NetworkHelper.PORT);
            _server.Start();
            _isServerRunning = true;

            while (_isServerRunning)
            {
                try
                {
                    var newClient = _server.AcceptTcpClient();

                    lock (_clients)
                    {
                        _clients.Add(new ConnectedClient(newClient));
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }

            lock (_clients)
            {
                foreach (var c in _clients)
                {
                    try
                    {
                        c.client.Close();
                    }
                    catch
                    {
	                    // ignored
                    }
                }

                _clients.Clear();
            }
        }

        class FileItem
        {
            public string name;
            public byte[] data;
        }

        void SendThread()
        {
            var folder = new DirectoryInfo(Settings.ThumbnailsPath);
            var fileNames = folder.GetFiles("*.png");
            var files = fileNames
	            .Select(fn => new FileItem { data = File.ReadAllBytes(fn.FullName), name = fn.FullName }).ToList();

            _isSending = true;

            Random r = new Random();

            while (_isSending)
            {
                Thread.Sleep(500);

                var file = files[r.Next(files.Count)];

                Debug.Log("Sending File: " + file.name);

                ConnectedClient[] clients;

                lock (_clients)
	                clients = _clients.ToArray();

                foreach (var client in clients)
                {
                    var success = false;

                    try
                    {
                        success = client.SendImageData(file.data);
                    }
                    catch
                    {
                        success = false;

                        client.client.Close();
                    }
                    finally
                    {
                        if (!success)
                        {
                            lock (_clients)
                            {
                                _clients.Remove(client);
                            }
                        }
                    }
                }
            }
        }

        
    }
}
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace Rml.Communication.Tcp
{
    internal class Message
    {
        public byte[] Body;
    }

    internal static class MessageExtensions
    {
        private static async Task ReadBytesAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            var size = buffer.Length;
            var totalReadSize = 0;
            while (size != totalReadSize)
            {
                var readSize = await stream.ReadAsync(buffer, totalReadSize, size, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                totalReadSize += readSize;
            }
        }

        public static IObservable<Message> ToObservableMessage(this Stream sourceStream, IScheduler scheduler = null)
        {
            scheduler = scheduler ?? Scheduler.Default;

            var subscribed = 0;
            return Observable.Create<Message>(o =>
            {
                var previous = Interlocked.CompareExchange(ref subscribed, 1, 0);

                if (previous != 0)
                {
                    o.OnError(new Exception("subscribed"));
                }

                var cd = new CompositeDisposable {sourceStream};

                scheduler.ScheduleAsync(async (_, cancellationToken) =>
                {
                    var sizeBuffer = new byte[4];

                    while (cancellationToken.IsCancellationRequested == false)
                    {
                        try
                        {
                            await ReadBytesAsync(sourceStream, sizeBuffer, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            o.OnError(ex);
                            return;
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        var bodyLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sizeBuffer, 2));

                        var body = new byte[bodyLength];

                        try
                        {
                            await ReadBytesAsync(sourceStream, body, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            o.OnError(ex);
                            return;
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        var message = new Message {Body = body};
                        o.OnNext(message);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    o.OnCompleted();

                }).AddTo(cd);

                return cd;
            });
        }

        public static void WriteMessage(this Stream stream, Message message)
        {
            var body = message.Body;
            var bodySize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));
            stream.Write(bodySize, 0, bodySize.Length);
            stream.Write(body, 0, body.Length);
        }
    }

    public class TcpCommunicationService : ICommunicationService
    {
        private TcpListener _tcpListener;
        private ImmutableList<Stream> _stream = ImmutableList.Create<Stream>();

        private CompositeDisposable _cd;

        public event EventHandler<ReceiveEventArgs> Receive;

        public bool Send(byte[] buffer)
        {
            if (buffer.Length == 0)
            {
                return true;
            }

            var stream = _stream;

            if (stream.Any() == false)
            {
                return false;
            }

            var message = new Message
            {
                Body = buffer,
            };

            stream
                .ForEach(o => o.WriteMessage(message));
            
            return true;
        }

        public int GetConnectCount()
        {
            return _stream.Count;
        }

        public bool StartListener(string ipAddress, int port)
        {
            Stop();

            try
            {
                _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
                _tcpListener.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                return false;
            }

            _cd = new CompositeDisposable();

            Observable.Create<TcpClient>(async observer =>
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    observer.OnNext(tcpClient);
                    observer.OnCompleted();

                    return Disposable.Empty;
                })
                .Repeat()
                .Select(o => ReceiveMessage(o).Catch((IOException ex) =>
                {
                    Console.WriteLine(ex);
                    return Observable.Empty<Message>();
                }))
                .Merge()
                .Subscribe()
                .AddTo(_cd);

            return true;
        }

        public bool StartClient(string hostName, int port)
        {
            Stop();

            _cd = new CompositeDisposable();

            TcpClient tcpClient;
            try
            {
                tcpClient = new TcpClient(hostName, port);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                return false;
            }

            ReceiveMessage(tcpClient)
                .Catch((IOException ex) =>
                {
                    Console.WriteLine(ex);
                    return Observable.Empty<Message>();
                })
                .Subscribe()
                .AddTo(_cd);

            return true;
        }

        public bool Stop()
        {
            _cd?.Dispose();

            return true;
        }

        private IObservable<Message> ReceiveMessage(TcpClient tcpClient)
        {
            return Observable.Create<Message>(observer =>
            {
                var cd = new CompositeDisposable();
                cd.Add(tcpClient);
                var stream = tcpClient.GetStream().AddTo(cd);
                stream
                    .ToObservableMessage()
                    .Do(oo => Receive?.Invoke(this, new ReceiveEventArgs(oo.Body)))
                    .Subscribe(observer)
                    .AddTo(cd);

                _stream = _stream.Add(stream);
                Disposable.Create(() => _stream = _stream.Remove(stream)).AddTo(cd);

                return cd;
            });
        }
    }
}
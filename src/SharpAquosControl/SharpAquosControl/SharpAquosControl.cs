using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SharpAquosControl
{
    public class SharpAquosControl : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        public SharpAquosControl(string address, int port, string login, string password)
        {
            MacroOptions = new MacroOptions();
            try
            {
                _tcpClient = new TcpClient(address, port) { NoDelay = true, SendTimeout = 5000, ReceiveTimeout = 5000,  };
                _stream = _tcpClient.GetStream();
                Authenticate(login, password);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public event EventHandler<DataTransferredEventArgs> DataTransferred;

        public MacroOptions MacroOptions { get; set; }

        public void SendCommand(string command)
        {
            command = command.PadRight(8);
            Send(command);
            OnDataTransferred(new DataTransferredEventArgs(true, command));
        }

        public string GetResponse()
        {
            var response = ReadUntil("\r", 1000);
            OnDataTransferred(new DataTransferredEventArgs(false, response));
            return response;
        }

        public void PlayMacro(params string[] commands)
        {
            for (var commandIndex = 0; commandIndex < commands.Length; commandIndex++)
            {
                var command = commands[commandIndex];
                var attempt = 0;
                while (attempt < MacroOptions.CommandRetryCount)
                {
                    SendCommand(command);
                    var response = GetResponse();
                    if (response == "OK")
                        break;

                    attempt++;
                    if (attempt == MacroOptions.CommandRetryCount)
                        break;

                    Thread.Sleep(MacroOptions.CommandRetryTime);
                }
            }
        }

        private void OnDataTransferred(DataTransferredEventArgs e)
        {
            var handler = DataTransferred;
            if (handler != null) handler(this, e);
        }

        private void Authenticate(string login, string password)
        {
            try
            {
                if (ReadUntil(":", 250) != "Login")
                    throw new InvalidOperationException("Invalid response");

                Send(login);

                if (ReadUntil(":", 250).TrimStart('\r', '\n') != "Password")
                    throw new InvalidOperationException("Invalid response");

                Send(password);

                // in case of wrong password
                // the connection could be dropped anytime soon from now
                // even before we could receive corresponding message

                var eventualException = new AuthenticationException("Invalid login or password");
                try
                {
                    ReadUntil("\r\n", 250);

                    SendCommand(""); // an empty command; 
                    // if authentication was successfull, we will get the "ERR" response to empty command above
                    // otherwise we will get "User Name or Password mismatch. Connection Closed."
                    // then drop connection
                    // we may or may not get the connection drop
                    // but if we get ERR this definitely was a successful authentication
                    var response = GetResponse();
                    if (response != "ERR")
                        throw eventualException;
                }
                catch (System.IO.IOException) // and this is if connection was dropped before we even read something
                {
                    throw eventualException;
                }
            }
            catch (AuthenticationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AuthenticationException(ex.Message, ex);
            }
        }

        private string ReadUntil(string text, int timeout)
        {
            var sb = new StringBuilder(50);
            var buffer = new byte[1];
            var t = Environment.TickCount;
            while (Environment.TickCount < t + timeout)
            {
                if (!_stream.DataAvailable)
                {
                    Thread.Sleep(10);
                    continue;
                }
                var i = _stream.ReadByte();
                if (i < 0)
                {
                    Thread.Sleep(10);
                    continue;
                }
                buffer[0] = (byte)i;
                sb.Append(Encoding.ASCII.GetString(buffer));
                var result = sb.ToString();
                var textIndex = result.IndexOf(text, StringComparison.Ordinal);
                if (textIndex < 0) continue;
                result = result.Remove(textIndex);
                return result;
            }
            return sb.ToString();
        }

        private void Send(string message)
        {
            var buffer = Encoding.ASCII.GetBytes(message + "\r");
            _stream.Write(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient = null;
                }
                if (_stream != null)
                {
                    _stream.Close();
                    _stream = null;
                }
            }
        }
    }
}


using HomeControl.Integrations.TPLink.JSON;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace HomeControl.Integrations.TPLink
{
    public class ProtocolMessage
    {
        public int MessageHash
        {
            get
            {
                byte[] bytes = Encoding.ASCII.GetBytes(Message ?? JSON);
                int num = -2128831035;
                for (int i = 0; i < bytes.Length; i++)
                {
                    num = (num ^ bytes[i]) * 16777619;
                }

                num += num << 13;
                num ^= num >> 7;
                num += num << 3;
                num ^= num >> 17;
                return num + (num << 5);
            }
        }

        public string JSON { get => BuildCommandJson(System, Command, Argument, Value); }

        public string Message { get; }

        public string System { get; }

        public string Command { get; }

        public object Argument { get; }

        public object Value { get; }

        public ProtocolMessage(string system, string command, object argument = null, object value = null)
        {
            System = system;
            Command = command;
            Argument = argument;
            Value = value;
        }

        public ProtocolMessage(string system, string command, string json)
        {
            Message = json;
            System = system;
            Command = command;
        }

        public T Execute<T>(string hostname, int port) where T : Response
        {
            return (T)Execute(hostname, port, typeof(T));
        }

        public Response Execute(string hostname, int port)
        {
            return Execute(hostname, port, typeof(Response));
        }

        public Response Execute(string hostname, int port, Type responseType)
        {
            byte[] messageToSend = ProtocolEncoder.Encrypt(Message ?? JSON);

            using (var client = new TcpClient())
            {
                client.Connect(hostname, port);

                using NetworkStream stream = client.GetStream();

                stream.Write(messageToSend, 0, messageToSend.Length);

                int targetSize = 0;

                List<byte> buffer = new List<byte>();

                do
                {
                    byte[] chunk = new byte[1024];

                    int count = stream.Read(chunk, 0, chunk.Length);

                    if (!buffer.Any())
                    {
                        byte[] array = chunk.Take(4).ToArray();

                        if (BitConverter.IsLittleEndian)
                        {
                            array = array.Reverse().ToArray();
                        }

                        targetSize = (int)BitConverter.ToUInt32(array, 0);
                    }

                    buffer.AddRange(chunk.Take(count));
                }
                while (buffer.Count != targetSize + 4);

                var responseData = buffer.Skip(4).Take(targetSize).ToArray();

                return ParseResponseData(responseData, System, Command, responseType);
            }
        }

        public static string BuildCommandJson(string system, string command, object argument = null, object value = null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("{ ");

            stringBuilder.Append($"\"{system}\"")
                .Append(": ")
                .Append("{ ")
                .Append($"\"{command}\"")
                .Append(": ");

            if (value == null) stringBuilder.Append(JsonSerializer.Serialize(argument));
            else
            {
                stringBuilder.Append("{ ")
                    .Append($"\"{argument}\"")
                    .Append(": ")
                    .Append(JsonSerializer.Serialize(value))
                    .Append(" }");
            }

            return stringBuilder.Append(" }").Append(" }").ToString();
        }

        public static T ParseResponseData<T>(byte[] responseData, string system, string command) where T : Response
        {
            return (T)ParseResponseData(responseData, system, command, typeof(T));
        }

        public static Response ParseResponseData(byte[] responseData, string system, string command, Type responseType)
        {
            var decryptedResponseData = ProtocolEncoder.Decrypt(responseData);

            var responseJson = Encoding.ASCII.GetString(decryptedResponseData);

            var jsonDocument = JsonDocument.Parse(responseJson.Trim(new char[1]));

            var response = (Response)jsonDocument.RootElement.GetProperty(system).GetProperty(command).Deserialize(responseType);

            if (response.ErrorCode != 0)
            {
                throw new ProtocolErrorException(response.ErrorCode, $"Protocol error {response.ErrorCode} ({response.ErrorMessage})");
            }

            return response;
        }
    }
}
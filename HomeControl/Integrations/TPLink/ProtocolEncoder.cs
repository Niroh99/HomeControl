using System.Text;

namespace HomeControl.Integrations.TPLink
{
    public static class ProtocolEncoder
    {
        private const byte INITIALIZATION_VECTOR = 171;

        public static byte[] Encrypt(string data)
        {
            byte[] dataBytes = Encrypt(Encoding.ASCII.GetBytes(data));

            byte[] headerBytes = BitConverter.GetBytes((uint)dataBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                headerBytes = headerBytes.Reverse().ToArray();
            }

            return headerBytes.Concat(dataBytes).ToArray();
        }

        public static byte[] Encrypt(byte[] data)
        {
            byte[] array = new byte[data.Length];

            byte b = INITIALIZATION_VECTOR;

            for (int i = 0; i < data.Length; i++)
            {
                array[i] = (byte)(b ^ data[i]);

                b = array[i];
            }

            return array;
        }

        public static byte[] Decrypt(byte[] data)
        {
            byte[] array = (byte[])data.Clone();

            byte b = INITIALIZATION_VECTOR;

            for (int i = 0; i < data.Length; i++)
            {
                byte num = array[i];

                array[i] = (byte)(b ^ array[i]);

                b = num;
            }

            return array;
        }
    }
}
namespace HomeControl.Helpers
{
    public static class StringHelper
    {
        public static byte[] HexToByteArray(this string hexString)
        {
            int byteArraylength = hexString.Length / 2;

            byte[] byteArray = new byte[byteArraylength];

            for (int i = 0; i < byteArraylength; i++)
            {
                var charIndex = i * 2;

                byteArray[i] = Convert.ToByte(new string([hexString[charIndex], hexString[charIndex + 1]]), 16);
            }
            return byteArray;
        }
    }
}
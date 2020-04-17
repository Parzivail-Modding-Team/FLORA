namespace FLORA
{
    /// <summary>
    /// Provides functions to translate an arbitrary string to a unique, reversible sequence
    /// of alpha-only characters, because LiteDB v5 doesn't accept numbers or special characters
    /// in table names.
    /// </summary>
    public static class AlphaOnlyStringEncoder
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Encode(string text)
        {
            var pos = 0;
            var buf = new char[text.Length << 1];

            int i;
            while ((i = pos) < text.Length)
            {
                buf[i << 1] = Alphabet[text[pos] >> 4];
                buf[(i << 1) + 1] = Alphabet[(Alphabet.Length - 1) - (text[pos] & 0x0F)];
                pos++;
            }

            return new string(buf);
        }

        public static string Decode(string text)
        {
            if (text.Length % 2 != 0)
                return null;

            var nPos = new int[2];
            var buf = new char[text.Length >> 1];

            for (var i = 0; i < text.Length >> 1; i++)
            {
                nPos[0] = Alphabet.IndexOf(text[i << 1]);
                nPos[1] = (Alphabet.Length - 1) - Alphabet.IndexOf(text[(i << 1) + 1]);
                if (nPos[0] < 0 || nPos[1] < 0)
                    return null;

                buf[i] = (char)((nPos[0] << 4) | nPos[1]);
            }
            return new string(buf);
        }
    }
}

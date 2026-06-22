
namespace mygame.sdk
{
    public class AdsFbEcpmDF
    {
        private static string[] ecpmCountry = { "in", "id", "mx", "tr", "br", "iq", "us", "ar", "th", "ph", "dz", "bd", "pk", "co", "pe", "cl", "eg", "kz", "ps", "my", "sa", "ve", "uz", "gt", "ro", "ua", "pl", "vn", "ec", "az", "za", "it", "sy", "la", "jo", "lb", "tn", "kh", "es", "do", "ma", "ge", "hn", "il", "fr", "gr", "de", "by", "bo", "cr", "mm", "rs", "hu", "sk", "ir", "cz", "py", "ni", "sv", "pt", "pa", "bg", "om", "uy", "ae", "ly", "am", "ke", "be", "ba", "jp", "gb", "mn", "kr", "hr", "md", "tw", "nl", "kg", "al", "ca", "lt", "lk", "hk", "at", "kw", "au", "nz", "jm", "xk", "lv", "mk", "qa", "sr", "si", "et", "cy", "pf", "ru", "se", "tt", "re", "bh", "ie", "me", "fi", "ch", "pr", "ng", "ga", "no", "dk", "af", "mu", "ci", "gf", "cn", "bz", "bf", "sg" };
        private static float[] ecmpValue = { 0.81f, 1.28f, 5.04f, 3.81f, 4.53f, 0.82f, 29.91f, 3.73f, 4.21f, 2.19f, 0.75f, 0.63f, 0.83f, 2.71f, 3.14f, 5.97f, 0.76f, 4.12f, 3.19f, 4.11f, 5.48f, 1.06f, 2.55f, 2.07f, 15.81f, 4.20f, 11.65f, 2.26f, 2.12f, 3.22f, 6.01f, 11.92f, 0.00f, 0.71f, 1.59f, 1.35f, 1.00f, 1.29f, 12.92f, 2.11f, 0.88f, 7.27f, 1.77f, 11.87f, 18.72f, 18.10f, 45.24f, 4.51f, 1.34f, 5.17f, 2.27f, 5.32f, 19.63f, 24.41f, 0.00f, 28.72f, 2.44f, 1.49f, 2.49f, 14.79f, 5.08f, 13.83f, 6.35f, 5.66f, 8.52f, 0.71f, 9.21f, 1.30f, 39.29f, 5.62f, 9.70f, 32.96f, 1.07f, 24.32f, 23.42f, 6.77f, 11.22f, 28.75f, 3.10f, 8.87f, 31.94f, 23.81f, 1.41f, 21.39f, 28.03f, 6.74f, 35.52f, 24.27f, 4.26f, 4.79f, 32.32f, 5.81f, 5.42f, 2.56f, 30.39f, 0.93f, 27.26f, 6.93f, 0.00f, 35.73f, 4.37f, 8.43f, 9.45f, 28.88f, 8.05f, 54.34f, 52.45f, 13.03f, 2.16f, 1.18f, 61.94f, 54.08f, 0.65f, 5.01f, 0.95f, 0.81f, 0.81f, 2.60f, 0.75f, 16.22f };

        public static float getDefault(string countryCode)
        {
            float re = 0.8f;
            string cl = countryCode.ToLower();
            for (int i = 0; i < ecpmCountry.Length; i++)
            {
                if (cl.CompareTo(ecpmCountry[i]) == 0)
                {
                    re = ecmpValue[i];
                    break;
                }
            }
            return re;
        }
    }
}

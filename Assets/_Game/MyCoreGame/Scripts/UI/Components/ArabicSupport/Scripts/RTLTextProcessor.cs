using UnityEngine;
using System.Globalization;
using ArabicSupport;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class RTLTextProcessor
{
    public static bool IsRTL(string text)
    {
        foreach (char c in text)
        {
            var dir = CharUnicodeInfo.GetUnicodeCategory(c);
            if (dir == UnicodeCategory.OtherLetter)
            {
                if ((c >= 0x0600 && c <= 0x06FF) || // Arabic
                    (c >= 0x0750 && c <= 0x077F) || // Arabic Supplement
                    (c >= 0x08A0 && c <= 0x08FF) || // Arabic Extended
                    (c >= 0x0590 && c <= 0x05FF))   // Hebrew
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Hàm xử lý văn bản RTL và giữ tag như <color>
    public static string FixRTLText(string input)
    {
        if (!IsRTL(input)) return input;

        string pattern = @"(<.*?>)";
        string[] parts = Regex.Split(input, pattern);
        for (int i = 0; i < parts.Length; i++)
        {
            if (!Regex.IsMatch(parts[i], pattern)) // nếu không phải tag
            {
                parts[i] = ArabicFixer.Fix(parts[i], true, false);
            }
        }

        return string.Join("", parts); // KHÔNG đảo mảng!
    }
}

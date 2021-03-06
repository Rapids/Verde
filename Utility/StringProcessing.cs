﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace Verde.Utility
{
    class StringProcessing
    {
        public static bool IsNumeric(string strChecking)
        {
            Double nChecking;
            return Double.TryParse(strChecking, System.Globalization.NumberStyles.Any, null, out nChecking);
        }

        public static bool IsNumeric(object oChecking)
        {
            return StringProcessing.IsNumeric(oChecking.ToString());
        }

        public static bool IsHexadecimal(string strChecking)
        {
            Int32 nChecking;
            return Int32.TryParse(strChecking, System.Globalization.NumberStyles.HexNumber, null, out nChecking);
        }

        public static bool IsHexadecimal(object oChecking)
        {
            return StringProcessing.IsHexadecimal(oChecking.ToString());
        }

        public static Color ConvertFromHexStringToColor(string strChecking)
        {
            if ((strChecking.Length == 6 || strChecking.Length == 8) && StringProcessing.IsHexadecimal(strChecking)) {
                if (strChecking.Length == 6) {
                    return Color.FromRgb(Convert.ToByte(strChecking.Substring(0, 2), 16), Convert.ToByte(strChecking.Substring(2, 2), 16), Convert.ToByte(strChecking.Substring(4, 2), 16));
                } else if (strChecking.Length == 8) {
                    return Color.FromArgb(Convert.ToByte(strChecking.Substring(0, 2), 16), Convert.ToByte(strChecking.Substring(2, 2), 16), Convert.ToByte(strChecking.Substring(4, 2), 16), Convert.ToByte(strChecking.Substring(6, 2), 16));
                }
            }
            return Color.FromRgb(255, 255, 255);
        }

        public static string ToHexString(byte[] byteArray)
        {
            string strHex = String.Empty;
            for (int i = 0; i < byteArray.Length; i++) {
                strHex += String.Format("{0:X2}", byteArray[i]);
            }
            return strHex;
        }

        public static string GetInnerWord(string strWord, char cBegin, char cEnd)
        {
            int nBegin = strWord.IndexOf(cBegin) + 1;
            int nEnd = strWord.IndexOf(cEnd);
            return strWord.Substring(nBegin, nEnd - nBegin);
        }

        public static string GetLastWord(string strWords)
        {
            for (var i = 1; i < strWords.Length; i++) {
                string strRet = strWords.Substring(strWords.Length - i);
                if (Regex.IsMatch(strRet, @"^[a-zA-Z0-9]+$") == false) {
                    return strRet.Substring(1);
                }
            }
            return String.Empty;
        }

        public static string GetDelimitedWord(string strWords, char cDelimiter)
        {
            return strWords.Substring(strWords.IndexOf(cDelimiter) + 1);
        }

        public static void GetNumbers(string strWords, char cDelimiter, List<int> listNumbers)
        {
            string strTmp = strWords;
            while (true) {
                var nPos = strTmp.IndexOf(cDelimiter);
                if (nPos < 0) break;
                string strNumber = strTmp.Substring(nPos + 1);
                for (var i = 0; strNumber.Length > 0; i++) {
                    if (Regex.IsMatch(strNumber, @"^[0-9]+$") == true) {
                        listNumbers.Add(int.Parse(strNumber));
                        strTmp = strTmp.Substring(strTmp.Length - i);
                        break;
                    }
                    strNumber = strNumber.Substring(0, strNumber.Length - 1);
                }
            }
        }
    }
}

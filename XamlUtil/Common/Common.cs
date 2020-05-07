using System;
using System.Text;
using System.Text.RegularExpressions;

namespace XamlUtil.Common
{
    public static class Common
    {
        public static string GetCopyName(string originalName, string copyFlag, Predicate<string> isExists)
        {
            if (string.IsNullOrWhiteSpace(originalName))
                throw new ArgumentNullException("originalName");

            if (isExists == null)
                return originalName.Trim();

            var copyName = originalName;
            int i = 0;
            while (isExists(copyName.Trim()))
            {
                i++;

                if (i == 1)
                    copyName += copyFlag;
                else
                {
                    if (Regex.IsMatch(copyName, copyFlag + @"\(\d+\)$"))
                        copyName = Regex.Replace(copyName, copyFlag + @"\(\d+\)$", copyFlag + "(" + i + ")");
                    else
                        copyName += ("(" + i + ")");
                }
            }

            return copyName.Trim();
        }

        public static string RemoveLineBreak(string originString)
        {
            return Regex.Replace(originString, @"[\n\r]", "");
        }

        public static string GetExceptionStringFormat(Exception ex)
        {
            if (ex == null)
                return string.Empty;

            int HResult = (int)(ex.GetType().GetProperty("HResult", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(ex, null));
            return string.Format("ErrorCode = {0} [ 0x{1:X} ]\nMessage = {2}\nStackTrace = {3}\n\r", HResult, HResult, RemoveLineBreak(ex.Message), ex.StackTrace) + GetAllInnerException(ex);
        }

        public static Exception GetLastInnerException(Exception ex)
        {
            if (ex == null)
                return null;

            if (ex.InnerException == null)
                return ex;

            var tempEx = ex.InnerException;

            while (tempEx != null)
            {
                if (tempEx.InnerException == null)
                    break;

                tempEx = tempEx.InnerException;
            }

            return tempEx;
        }

        public static string GetAllInnerException(Exception ex)
        {
            if (ex.InnerException == null)
                return string.Empty;

            var tempEx = ex.InnerException;

            var stringBuilder = new StringBuilder();
            int i = 1;
            while (tempEx != null)
            {
                stringBuilder.AppendLine(string.Format("=================== InnerException[{0}] ===================", i++));
                stringBuilder.AppendLine(string.Format("Message = {0}\nStackTrace = {1}\n", RemoveLineBreak(tempEx.Message), tempEx.StackTrace));

                tempEx = tempEx.InnerException;
            }

            return stringBuilder.ToString();
        }
    }
}

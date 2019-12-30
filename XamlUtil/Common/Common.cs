using System;
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
    }
}

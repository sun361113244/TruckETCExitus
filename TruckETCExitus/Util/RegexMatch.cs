using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Util
{
    class RegexMatch
    {
        public static bool CheckUserInput(String input)
        {
            Regex reg = new Regex(@"^\w+$");//只有数字，字母，下划线

            if (reg.IsMatch(input))
                return true;
            else
                return false;
        }

        public static bool CheckNum(String input)
        {
            Regex reg = new Regex(@"^[0-9]+(\.\d+)?$");
            if (reg.IsMatch(input))
                return true;
            else
                return false;
        }

        public static bool CheckTextInput(string input)
        {
            Regex reg = new Regex(@"^[^']*$");//过滤掉单引号
            if (reg.IsMatch(input))
                return true;
            else
                return false;
        }
    }
}

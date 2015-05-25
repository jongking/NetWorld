using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JHelper
{
    /// <summary>
    /// 反射方法类
    /// </summary>
    public static class ReflectionHelper
    {
        public static void RunMethod(object classobj, string methodName)
        {
            var t = classobj.GetType();
            var mi = t.GetMethod(methodName);
            mi.Invoke(classobj, new object[] { });
        }
    }
}

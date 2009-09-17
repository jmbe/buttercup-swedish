using System;
using System.Windows.Browser;
using System.Collections.Generic;
using System.Globalization;

namespace Wilco.Windows.Browser.Extensions {

    public static class ScriptObjectUtility {

        private const string HelperScript =
@"({
    createDelegate: function(f, t) {
        return function() { f.apply(null, t ? t(arguments) : arguments); };
    },
    enumerate: function(o) {
        var members = [];
        for (var m in o) {
            members.push(m);
        }
        return members;
    }
})";

        private static ScriptObject s_helperObject;

        static ScriptObjectUtility() {
            s_helperObject = (ScriptObject)HtmlPage.Window.Eval(HelperScript);
        }

        public static object GetItem(this ScriptObject obj, int index) {
            return obj.GetProperty(index.ToString());
        }

        public static T GetItem<T>(this ScriptObject obj, int index) {
            return obj.GetProperty<T>(index.ToString());
        }

        public static T GetProperty<T>(this ScriptObject obj, string member) {
            object value = obj.GetProperty(member);
            if (value == null) {
                return default(T);
            }

            Type desiredPropertyType = typeof(T);
            ScriptObject objectValue = value as ScriptObject;
            if (objectValue != null) {
                return objectValue.ConvertTo<T>();
            }
            else {
                return (T)Convert.ChangeType(value, desiredPropertyType, CultureInfo.InvariantCulture);
            }
        }

        public static ScriptObject ToScriptFunction(this Delegate d) {
            return (ScriptObject)s_helperObject.Invoke("createDelegate", d);
        }

        public static ScriptObject ToScriptFunction(this Delegate d, Func<ScriptObject, ScriptObject> argumentsTranslator) {
            return (ScriptObject)s_helperObject.Invoke("createDelegate", d, argumentsTranslator);
        }

        public static IEnumerable<string> GetMembers(this ScriptObject obj) {
            ScriptObject members = (ScriptObject)s_helperObject.Invoke("enumerate", obj);
            int length = members.GetProperty<int>("length");
            for (int i = 0; i < length; i++) {
                yield return members.GetItem<string>(i);
            }
        }
    }
}
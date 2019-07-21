using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SocketIO {
    static class ExtensionMethods {
        public static IJsonValue ToJsonValue(this object o) {
            if(o.GetType().IsArray) {
                var outArray = new JsonArray();
                foreach(var x in (IEnumerable)o) {
                    outArray.Add(x.ToJsonValue());
                }
                return outArray;
            }

            switch(o) {
                case int i:
                    return JsonValue.CreateNumberValue(i);
                case uint i:
                    return JsonValue.CreateNumberValue(i);
                case float f:
                    return JsonValue.CreateNumberValue(f);
                case double d:
                    return JsonValue.CreateNumberValue(d);
                case string s:
                    return JsonValue.CreateStringValue(s);
                case bool b:
                    return JsonValue.CreateBooleanValue(b);                    
                case null:
                    return JsonValue.CreateNullValue();
                default:
                    throw new ArgumentException("Oops, can't handle this argument yet");
            }
        }

        public static object[] ToObjectArray(this IJsonValue v) {
            List<object> result = new List<object>();

            switch(v.ValueType) {
                case JsonValueType.Boolean:
                    result.Add(v.GetBoolean());
                    break;
                case JsonValueType.Null:
                    result.Add(null);
                    break;
                case JsonValueType.Number:
                    result.Add(v.GetNumber());
                    break;
                case JsonValueType.String:
                    result.Add(v.GetString());
                    break;
                case JsonValueType.Array:
                    result.Add(v.GetArray().ToObjectArray());
                    break;
            }

            return result.ToArray();
        }
    }
}

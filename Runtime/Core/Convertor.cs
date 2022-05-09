using System;
using System.Collections.Generic;

namespace Edger.Unity.Glue {
    public static class Convertor {
        public const string Null = "null";

        public readonly static BoolConvertor BoolConvertor = new BoolConvertor();
        public readonly static IntConvertor IntConvertor = new IntConvertor();
        public readonly static LongConvertor LongConvertor = new LongConvertor();
        public readonly static FloatConvertor FloatConvertor = new FloatConvertor();
        public readonly static DoubleConvertor DoubleConvertor = new DoubleConvertor();
        public readonly static StringConvertor StringConvertor = new StringConvertor();

        private static Dictionary<Type, InternalConvertor> _Convertors = new Dictionary<Type, InternalConvertor>();

        static Convertor() {
            RegisterConvertor<bool>(BoolConvertor);
            RegisterConvertor<int>(IntConvertor);
            RegisterConvertor<long>(LongConvertor);
            RegisterConvertor<float>(FloatConvertor);
            RegisterConvertor<double>(DoubleConvertor);
            RegisterConvertor<string>(StringConvertor);
        }

        public static bool RegisterConvertor<T>(Convertor<T> convertor) {
            if (_Convertors.ContainsKey(typeof(T))) {
                return false;
            }
            _Convertors[typeof(T)] = convertor;
            return true;
        }

        private static InternalConvertor GetInternalConvertor(Type type) {
            if (type == null) return null;
            InternalConvertor result;
            if (_Convertors.TryGetValue(type, out result)) {
                return result;
            }
            return null;
        }

        private static string GetTypeStr(Type type) {
            return type == null ? Null : type.FullName;
        }

        public static Convertor<T> GetConvertor<T>(bool isDebug = false) {
            InternalConvertor convertor = GetInternalConvertor(typeof(T));
            if (convertor != null) {
                return convertor.As<Convertor<T>>();
            }
            Log.ErrorOrDebug(isDebug, "Convertor Not Found: {0}", GetTypeStr(typeof(T)));
            return null;
        }

        public static bool TryParse<T>(string str, out T val, bool isDebug = false) {
            Convertor<T> convertor = GetConvertor<T>();
            if (convertor != null) {
                return convertor.TryParse(str, out val, isDebug);
            } else {
                val = default(T);
                Log.ErrorOrDebug(isDebug, "Parse Failed, Unknown Type: <{0}> {1}",
                                            GetTypeStr(typeof(T)), str);
            }
            return false;
        }

        public static T Parse<T>(string str) {
            Convertor<T> convertor = GetConvertor<T>();
            if (convertor != null) {
                return convertor.Parse(str);
            } else {
                Log.Critical("Parse Failed, Unknown Type: <{0}> {1}",
                                        GetTypeStr(typeof(T)), str);
                return default(T);
            }
        }

        public static string Convert<T>(T val) {
            Convertor<T> convertor = GetConvertor<T>();
            if (convertor != null) {
                return convertor.Convert(val);
            } else {
                Log.Error("Convert Failed, Unknown Type: <{0}> {1}",
                                            GetTypeStr(typeof(T)), val);
            }
            return Null;
        }

        public static bool TryParse(Type type, string str, out object val, bool isDebug = false) {
            InternalConvertor convertor = GetInternalConvertor(type);
            if (convertor != null) {
                return convertor._TryParseInternal(str, out val, isDebug);
            } else {
                val = null;
                Log.ErrorOrDebug(isDebug, "Parse Failed, Unknown Type: <{0}> {1}",
                                            GetTypeStr(type), str);
            }
            return false;
        }

        public static object Parse(Type type, string str) {
            InternalConvertor convertor = GetInternalConvertor(type);
            if (convertor != null) {
                return convertor._ParseInternal(str);
            } else {
                Log.Critical("Parse Failed, Unknown Type: <{0}> {1}",
                                        GetTypeStr(type), str);
                return null;
            }
        }

        //Not giving isDebug default value, othewise with get conflict with the generic version
        public static string Convert(object val, bool isDebug) {
            if (val == null) return Null;

            InternalConvertor convertor = GetInternalConvertor(val.GetType());
            if (convertor != null) {
                return convertor._ConvertInternal(val);
            } else {
                Log.ErrorOrDebug(isDebug, "Convert Failed, Unknown Type: <{0}> {1}",
                                            GetTypeStr(val.GetType()), val);
                return val.ToString();
            }
        }
    }

    public interface IConvertor<T> {
        bool TryParse(string str, out T val, bool isDebug = false);
        T Parse(string str);
        string Convert(T val);
    }

    public abstract class InternalConvertor {
        internal abstract bool _TryParseInternal(string str, out object val, bool isDebug = false);
        internal abstract object _ParseInternal(string str);
        internal abstract string _ConvertInternal(object val);
    }

    public abstract class Convertor<T> : InternalConvertor, IConvertor<T> {
        internal override bool _TryParseInternal(string str, out object val, bool isDebug = false) {
            try {
                val = _ParseInternal(str);
                return true;
            } catch (Exception e) {
                Log.ErrorOrDebug(isDebug, "Parse Failed: <{0}> {1} -> \n{2}", typeof(T).FullName, str, e);
            }
            val = default(T);
            return false;
        }

        internal override object _ParseInternal(string str) {
            return Parse(str);
        }

        internal override string _ConvertInternal(object val) {
            T _val = val.As<T>();
            return Convert(_val);
        }

        public bool TryParse(string str, out T val, bool isDebug = false) {
            try {
                val = Parse(str);
                return true;
            } catch (Exception e) {
                Log.ErrorOrDebug(isDebug, "Parse Failed: <{0}> {1} -> \n{2}", typeof(T).FullName, str, e);
            }
            val = default(T);
            return false;
        }

        public abstract T Parse(string str);
        public abstract string Convert(T val);
    }

    public class BoolConvertor : Convertor<bool> {
        public override string Convert(bool val) {
            return val ? "true" : "false";
        }

        public override bool Parse(string str) {
            return str != null && str.ToLower() == "true";
        }
    }

    public class IntConvertor : Convertor<int> {
        public override string Convert(int val) {
            return val.ToString();
        }

        public override int Parse(string str) {
            return System.Convert.ToInt32(str);
        }
    }

    public class LongConvertor : Convertor<long> {
        public override string Convert(long val) {
            return val.ToString();
        }

        public override long Parse(string str) {
            return System.Convert.ToInt64(str);
        }
    }

    public class FloatConvertor : Convertor<float> {
        public override string Convert(float val) {
            return val.ToString();
        }

        public override float Parse(string str) {
            return System.Convert.ToSingle(str);
        }
    }

    public class DoubleConvertor : Convertor<double> {
        public override string Convert(double val) {
            return val.ToString();
        }

        public override double Parse(string str) {
            return System.Convert.ToDouble(str);
        }
    }

    public class StringConvertor : Convertor<string> {
        public override string Convert(string val) {
            return val == null ? Convertor.Null : val.ToString();
        }

        public override string Parse(string str) {
            return str;
        }
    }
}

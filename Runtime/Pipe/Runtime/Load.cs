using System;

using Edger.Unity.Glue;

namespace Edger.Unity.Glue.Pipe.Runtime_ {
    public class Load : Pipe<Runtime, object, string> {
        public Load(string param1) : base(param1) {
            CheckStateKey(param1);
        }

        protected override object Do(Runtime runtime, Caret caret, Runtime input, string stateKey) {
            return input.Load(stateKey);
        }
    }
}

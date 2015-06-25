using System.Collections.Generic;

namespace FftWrap.Codegen
{
    public class Method
    {
        private readonly string _returnType;
        private readonly string _name;
        private readonly bool _returnTypeIsPointer;
        private readonly bool _returnTypeIsConst;
        private readonly IReadOnlyCollection<Parameter> _parameters;

        public Method(string returnType, string name, bool returnTypeIsPointer, bool returnTypeIsConst, IReadOnlyCollection<Parameter> parameters)
        {
            _returnType = returnType;
            _name = name;
            _returnTypeIsPointer = returnTypeIsPointer;
            _returnTypeIsConst = returnTypeIsConst;
            _parameters = parameters;
        }
        
        public string ReturnType
        {
            get { return _returnType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool ReturnTypeIsPointer
        {
            get { return _returnTypeIsPointer; }
        }

        public IReadOnlyCollection<Parameter> Parameters
        {
            get { return _parameters; }
        }

        public bool ReturnTypeIsConst
        {
            get { return _returnTypeIsConst; }
        }
    }
}

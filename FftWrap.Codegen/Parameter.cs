namespace FftWrap.Codegen
{
    public class Parameter
    {
        private readonly string _type;
        private readonly string _name;
        private readonly bool _isPointer;
        private readonly bool _isConst;
        
        public Parameter(string type, string name, bool isPointer, bool isConst)
        {
            _type = type;
            _name = name;
            _isPointer = isPointer;
            _isConst = isConst;
        }

        public string Type
        {
            get { return _type; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool IsPointer
        {
            get { return _isPointer; }
        }

        public bool IsConst
        {
            get { return _isConst; }
        }
    }
}

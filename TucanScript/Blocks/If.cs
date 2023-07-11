namespace TucanScript.Blocks
{
    public class If : Invokable
    {
        private readonly FunctionTypeDef condition;

        public If(Invokable parent, Engine engine, FunctionTypeDef condition, string source) :
            base(parent, engine, source)
        {
            this.condition = condition;
        }
        
        public override void Invoke()
        {
            if (!(bool) condition.Invoke(Engine.Variables))
            {
                return;
            }
            
            base.Invoke();
        }
    }
}
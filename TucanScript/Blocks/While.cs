namespace TucanScript.Blocks
{
    public class While : Invokable
    {
        private readonly FunctionTypeDef condition;

        public While(Invokable parent, Engine engine, FunctionTypeDef condition, string source) :
            base(parent, engine, source)
        {
            this.condition = condition;
        }
        
        public override void Invoke()
        {
            while ((bool) condition.Invoke(Engine.Variables))
            {
                base.Invoke();
            }
        }
    }
}
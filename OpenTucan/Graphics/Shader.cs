using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTucan.Graphics
{
    public abstract class Shader
    {
        private readonly int programId;
        private readonly int vertexShaderId, fragmentShaderId;
        private readonly Dictionary<string, int> uniforms;

        protected Shader(string vertexShader, string fragmentShader)
        {
            uniforms = new Dictionary<string, int>();
                
            vertexShaderId = LoadShaderFromSource(vertexShader, ShaderType.VertexShader);
            fragmentShaderId = LoadShaderFromSource(fragmentShader, ShaderType.FragmentShader);
            programId = GL.CreateProgram();
            
            GL.AttachShader(programId, vertexShaderId);
            GL.AttachShader(programId, fragmentShaderId);
            
            BindAttributes();
            
            GL.LinkProgram(programId);
            GL.ValidateProgram(programId);
        }

        public void Start() 
        {
            GL.UseProgram(programId);
        }
        
        public void Stop() 
        {
            GL.UseProgram(0);
        }

        public void Clear() 
        {
            GL.UseProgram(0);
            GL.DetachShader(programId, vertexShaderId);
            GL.DetachShader(programId, fragmentShaderId);
            GL.DeleteShader(vertexShaderId);
            GL.DeleteShader(fragmentShaderId);
            GL.DeleteProgram(programId);
        }

        protected abstract void BindAttributes();

        protected void BindAttribute(int attribute, string variableName)
        {
            GL.BindAttribLocation(programId, attribute, variableName);
        }

        public void SetUniform(string uniformName, float value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform1(uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, int value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform1(uniforms[uniformName], value);
        }

        public void SetUniform(string uniformName, Vector2 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform2(uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, float x, float y) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform2(uniforms[uniformName], x, y);
        }
        
        public void SetUniform(string uniformName, Vector3 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform3(uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, float x, float y, float z) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform3(uniforms[uniformName], x, y, z);
        }
        
        public void SetUniform(string uniformName, Vector4 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform4(uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, float x, float y, float z, float w) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform4(uniforms[uniformName], x, y, z, w);
        }

        public void SetUniform(string uniformName, Color4 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform4(uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, Matrix4 value)
        {
            CheckUniformLocation(uniformName);
            GL.UniformMatrix4(uniforms[uniformName], false, ref value);
        }

        public void SetUniform(string uniformName, bool value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform1(uniforms[uniformName], Convert.ToInt32(value));
        }
        
        private int GetUniformLocation(string uniformName)
        {
            return GL.GetUniformLocation(programId, uniformName);
        }

        private void CheckUniformLocation(string uniformName)
        {
            if (!uniforms.ContainsKey(uniformName))
            {
                uniforms.Add(uniformName, GetUniformLocation(uniformName));
            }
        }

        private static int LoadShaderFromSource(string source, ShaderType type)
        {
            var shaderId = GL.CreateShader(type);
            
            GL.ShaderSource(shaderId, source);
            GL.CompileShader(shaderId);
            
            var log = GL.GetShaderInfoLog(shaderId);
            if (!string.IsNullOrEmpty(log)) 
            {
                throw new Exception(log);
            }
            return shaderId;
        }
    }
}
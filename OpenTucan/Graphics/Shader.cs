using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenTucan.Graphics
{
    public abstract class Shader
    {
        private readonly int _programId;
        private readonly int _vertexShaderId, _fragmentShaderId;
        private readonly Dictionary<string, int> _uniforms;

        protected Shader(string vertexShader, string fragmentShader)
        {
            _uniforms = new Dictionary<string, int>();
                
            _vertexShaderId = LoadShaderFromSource(vertexShader, ShaderType.VertexShader);
            _fragmentShaderId = LoadShaderFromSource(fragmentShader, ShaderType.FragmentShader);
            _programId = GL.CreateProgram();

            AttachAndValidate();
        }

        private void AttachAndValidate()
        {
            GL.AttachShader(_programId, _vertexShaderId);
            GL.AttachShader(_programId, _fragmentShaderId);
            
            BindAttributes();
            
            GL.LinkProgram(_programId);
            GL.ValidateProgram(_programId);
        }

        public void Start() 
        {
            GL.UseProgram(_programId);
        }
        
        public void Stop() 
        {
            GL.UseProgram(0);
        }

        public void Clear() 
        {
            GL.UseProgram(0);
            GL.DetachShader(_programId, _vertexShaderId);
            GL.DetachShader(_programId, _fragmentShaderId);
            GL.DeleteShader(_vertexShaderId);
            GL.DeleteShader(_fragmentShaderId);
            GL.DeleteProgram(_programId);
        }

        protected abstract void BindAttributes();

        protected void BindAttribute(int attribute, string variableName)
        {
            GL.BindAttribLocation(_programId, attribute, variableName);
        }

        public void SetUniform(string uniformName, float value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform1(_uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, int value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform1(_uniforms[uniformName], value);
        }

        public void SetUniform(string uniformName, Vector2 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform2(_uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, float x, float y) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform2(_uniforms[uniformName], x, y);
        }
        
        public void SetUniform(string uniformName, Vector3 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform3(_uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, float x, float y, float z) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform3(_uniforms[uniformName], x, y, z);
        }
        
        public void SetUniform(string uniformName, Vector4 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform4(_uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, float x, float y, float z, float w) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform4(_uniforms[uniformName], x, y, z, w);
        }

        public void SetUniform(string uniformName, Color4 value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform4(_uniforms[uniformName], value);
        }
        
        public void SetUniform(string uniformName, Matrix4 value)
        {
            CheckUniformLocation(uniformName);
            GL.UniformMatrix4(_uniforms[uniformName], false, ref value);
        }

        public void SetUniform(string uniformName, bool value) 
        {
            CheckUniformLocation(uniformName);
            GL.Uniform1(_uniforms[uniformName], Convert.ToInt32(value));
        }
        
        private int GetUniformLocation(string uniformName)
        {
            return GL.GetUniformLocation(_programId, uniformName);
        }

        private void CheckUniformLocation(string uniformName)
        {
            if (!_uniforms.ContainsKey(uniformName))
            {
                _uniforms.Add(uniformName, GetUniformLocation(uniformName));
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
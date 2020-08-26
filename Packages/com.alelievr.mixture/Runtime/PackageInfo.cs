using System.Runtime.CompilerServices;

// Note: This is not the ShaderGraph asmdef but the editor asmdef of mixture
// We need this hack because all the ShaderGraph API is internal ¯\_(ツ)_/¯
[assembly: InternalsVisibleTo("Unity.ShaderGraph.GraphicsTests")]
[assembly: InternalsVisibleTo("com.alelievr.mixture-documentation")]

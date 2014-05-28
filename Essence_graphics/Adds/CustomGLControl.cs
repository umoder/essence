using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace Essence_graphics
{
    #region UserGLControl
    /// <summary>
    /// Изменный GLControl для использования 2хАА
    /// </summary>
    public class CustomGLControl : GLControl
    {
        // 32bpp color, 24bpp z-depth, 0bpp stencil and 2x antialiasing
        // OpenGL version is major=0, minor=0
        public CustomGLControl()
            : base(new GraphicsMode(32, 24, 0, 4), 0, 0, GraphicsContextFlags.Default)
        { }
    }
    #endregion
}

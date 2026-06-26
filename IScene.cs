using System.Windows.Media;

namespace idotmatrix_gui
{
    public interface IScene
    {
        string Name { get; }
        
        /// <summary>
        /// Draws a 32x32 frame buffer representing the display.
        /// </summary>
        /// <param name="frameCount">Total frame count of the main loop (for animations).</param>
        /// <returns>A 32x32 color array representing pixels.</returns>
        Color[,] DrawFrame(int frameCount);

        /// <summary>
        /// Optional clean up when the scene is stopped or switched.
        /// </summary>
        void Stop();

        /// <summary>
        /// Indicates if this scene manages its own completion timing (e.g. waiting for scrolls).
        /// </summary>
        bool CustomCompletion => false;

        /// <summary>
        /// True if the scene is finished and ready to transition to the next scene.
        /// Only used if CustomCompletion is true.
        /// </summary>
        bool IsDone => false;
    }
}

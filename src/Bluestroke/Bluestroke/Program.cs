using Bluestroke.Forms;

namespace Bluestroke;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Ensure single instance
        using var mutex = new Mutex(true, "BluestrokeKeyboardSoundApp", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Bluestroke is already running.", "Bluestroke", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}
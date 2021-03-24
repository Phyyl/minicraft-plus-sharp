using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Core
{
    public class Initializer : Game
    {
        private Initializer()
            : base()
        {
        }

        public static int fra, tik; //these store the number of frames and ticks in the previous second; used for fps, at least.

        public static int GetCurFps()
        {
            return fra;
        }

        public static void ParseArgs(string[] args)
        {
            bool debug = false;
            bool packetdebug = false;
            bool autoclient = false;
            bool autoserver = false;

            // parses command line arguments
            string saveDir = FileHandler.systemGameDir;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--debug"))
                {
                    debug = true;
                }
                else if (args[i].Equals("--packetdebug"))
                {
                    packetdebug = true;
                }
                else if (args[i].Equals("--savedir") && i + 1 < args.Length)
                {
                    i++;
                    saveDir = args[i];
                }
                else if (args[i].Equals("--localclient"))
                {
                    autoclient = true;
                }
                else if (args[i].Equals("--server"))
                {
                    autoserver = true;
                    if (i + 1 < args.Length)
                    {
                        i++;
                        WorldSelectDisplay.setWorldName(args[i], true);
                    }
                    else
                    {
                        Console.Error.WriteLine("A world name is required.");
                        Environment.Exit(1);
                    }
                }
                else if (args[i].Equals("--port"))
                {
                    int customPort = MinicraftProtocol.PORT;

                    if (i + 1 < args.Length)
                    {
                        string portString = args[++i];
                        try
                        {
                            customPort = int.Parse(portString);
                        }
                        catch (FormatException)
                        {
                            Console.Error.WriteLine("Port wasn't a number! Using the default port: " + portString);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Missing new port! Using the default port " + MinicraftProtocol.PORT);
                    }

                    Game.CUSTOM_PORT = customPort;
                }
            }
            Game.debug = debug;
            Game.packet_debug = packetdebug;
            HAS_GUI = !autoserver;

            FileHandler.DetermineGameDir(saveDir);

            Network.autoclient = autoclient; // this will make the game automatically jump to the MultiplayerMenu, and attempt to connect to localhost.
        }



        /** This is the main loop that runs the game. It:
         *	-keeps track of the amount of time that has passed
         *	-fires the ticks needed to run the game
         *	-fires the command to render out the screen.
         */
        public static void Run()
        {
            long lastTime = JavaSystem.NanoTime();
            long lastRender = JavaSystem.NanoTime();
            double unprocessed = 0;
            int frames = 0;
            int ticks = 0;
            long lastTimer1 = JavaSystem.CurrentTimeMillis();

            //main game loop? calls tick() and render().
            if (!HAS_GUI)
            {
                new ConsoleReader().Start();
            }

            while (running)
            {
                long now = JavaSystem.NanoTime();
                double nsPerTick = 1E9D / Updater.normSpeed; // nanosecs per sec divided by ticks per sec = nanosecs per tick

                if (menu == null)
                {
                    nsPerTick /= Updater.gamespeed;
                }

                unprocessed += (now - lastTime) / nsPerTick; //figures out the unprocessed time between now and lastTime.
                lastTime = now;

                while (unprocessed >= 1)
                { // If there is unprocessed time, then tick.
                    ticks++;
                    Updater.Tick(); // calls the tick method (in which it calls the other tick methods throughout the code.
                    unprocessed--;
                }

                try
                {
                    Thread.Sleep(2); // makes a small pause for 2 milliseconds
                }
                catch (ThreadInterruptedException e)
                {
                    e.PrintStackTrace();
                }

                if ((now - lastRender) / 1.0E9 > 1.0 / MAX_FPS)
                {
                    frames++;
                    lastRender = JavaSystem.NanoTime();
                    Renderer.Render();
                }

                if (JavaSystem.CurrentTimeMillis() - lastTimer1 > 1000)
                { //updates every 1 second
                    lastTimer1 += 1000; // adds a second to the timer

                    fra = frames; //saves total frames in last second
                    tik = ticks; //saves total ticks in last second
                    frames = 0; //resets frames
                    ticks = 0; //resets ticks; ie, frames and ticks only are per second
                }
            }
        }


        // Creates and displays the JFrame window that the game appears in.
        static void CreateAndDisplayFrame()
        {
            if (!HAS_GUI)
            {
                return;
            }

            Renderer.canvas.SetMinimumSize(new Dimension(1, 1));
            Renderer.canvas.SetPreferredSize(Renderer.GetWindowSize());

            JFrame frame = new JFrame(NAME);

            frame.setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);
            frame.setLayout(new BorderLayout()); // sets the layout of the window
            frame.add(Renderer.canvas, BorderLayout.CENTER); // Adds the game (which is a canvas) to the center of the screen.
            frame.pack(); //squishes everything into the preferredSize.

            try
            {
                BufferedImage logo = ImageIO.read(Game.GetType().getResourceAsStream("/resources/logo.png"));
                frame.setIconImage(logo);
            }
            catch (IOException e)
            {
                e.printStackTrace();
            }

            frame.setLocationRelativeTo(null); // the window will pop up in the middle of the screen when launched.

            frame.addComponentListener(new ComponentAdapter()
            {

            public void componentResized(ComponentEvent e)
            {
                float w = frame.getWidth() - frame.getInsets().left - frame.getInsets().right;
                float h = frame.getHeight() - frame.getInsets().top - frame.getInsets().bottom;
                Renderer.SCALE = Math.min(w / Renderer.WIDTH, h / Renderer.HEIGHT);
            }
        });

        frame.addWindowListener(new WindowListener()
        {
            void windowActivated(WindowEvent e) { }
            void windowDeactivated(WindowEvent e) { }
            void windowIconified(WindowEvent e) { }
            void windowDeiconified(WindowEvent e) { }
            void windowOpened(WindowEvent e) { }
            void windowClosed(WindowEvent e) { Console.WriteLine("Window closed"); }
            void windowClosing(WindowEvent e)
            {
                Console.WriteLine("Window closing");
                quit();
            }
        });

frame.setVisible(true);
	}
}
}

using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace GadgeteerRoborace
{
    public partial class Program
    {
        InfraredSensor sensorRight10;
        InfraredSensor sensorFront9;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/

            sensorRight10 = new InfraredSensor(10);
            sensorFront9 = new InfraredSensor(9);

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            GT.Timer timer = new GT.Timer(50); // every second (50ms)
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
        }

        const int GoLeft = 70;
        const int GoRight = -70;

        const int GoForward = 100;
        const int GoSlow = 60;
        const int GoBack = -80;

        void SetSpeed(int power)
        {
            motorControllerL298.MoveMotor(MotorControllerL298.Motor.Motor2, power);
        }

        void Turn(int power)
        {
            motorControllerL298.MoveMotor(MotorControllerL298.Motor.Motor1, power);
        }

        enum State
        {
            None,
            Left,
            Right,
            Forward,
            Back,
        }
        State state = State.None;

        static string[] States = new string[] { "None", "Left", "Right", "Forward", "Back" };

        static string S(State s) { return States[(int)s]; }
        static int CM(double d) { return (int)(d * 100); }

        int restore = 0;

        void timer_Tick(GT.Timer timer)
        {
            var distF = sensorFront9.ReadDistance();
            var distR = sensorRight10.ReadDistance();
            Debug.Print(string.Concat("F:",CM(distF), "  R:", CM(distR), "  S:", S(state)));

            double frontTreshold = state == State.Back ? 0.25 : 0.15;
            double rightTreshold = 0.2;

            if (distF > frontTreshold)
            {
                if (distR > rightTreshold + 0.2 || restore-- > 0)
                {
                    if (SetState(State.Right))      // towards the wall
                    {
                        Turn(GoRight);
                        SetSpeed(GoSlow);
                    }
                }
                else if (distR < rightTreshold)
                {
                    if (SetState(State.Left))       // away from the wall
                    {
                        Turn(GoLeft);
                        SetSpeed(GoSlow);
                    }
                }
                else 
                {
                    if (SetState(State.Forward))
                    {
                        Turn(0);
                        SetSpeed(GoForward);
                    }
                }
            }
            else
            {
                restore = 30;
                if (SetState(State.Back))
                {
                    Turn(GoRight);
                    SetSpeed(GoBack);
                }
            }
        }

        bool SetState(State state)
        {
            if (this.state != state)
            {
                this.state = state;
                Debug.Print(S(state));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

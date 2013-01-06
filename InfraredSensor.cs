using Gadgeteer.Modules;
using Gadgeteer.Interfaces;

public class InfraredSensor : Module
{
    private readonly AnalogInput analogIn;

    public bool Active
    {
        get { return analogIn.Active; }
        set { analogIn.Active = value; }
    }

    public InfraredSensor(int socketno)
    {
        var socket = Gadgeteer.Socket.GetSocket(socketno, true, null, "A");
        analogIn = new AnalogInput(socket, Gadgeteer.Socket.Pin.Three, null);
    }

    private const double w0 = -1.092719553;
    private const double w1 = 4.69784695;

    private double lastVoltage;
    private double lastDistance;

    public double ReadDistance()
    {
        var prev = lastVoltage;
        lastVoltage = analogIn.ReadVoltage();
        var voltage = lastVoltage * 0.75 + prev * 0.25;   // poor man's filter

        var dist =  1.0 / (voltage * w1 + w0);
        if (dist < 0.0)
            dist = double.PositiveInfinity;
        return lastDistance = dist;
    }
}

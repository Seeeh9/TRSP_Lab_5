

using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

abstract class AbstractFuelStation
{
    public int Reserve { get; protected set; }
    public int Capacity { get; } = 1000;
    public int ColumnCount { get; }

    public AbstractFuelStation(int columnCount, int reserve = 1000)
    {
        if (columnCount <= 0) throw new Exception("Invalid count");
        if (reserve > Capacity || reserve < 0) throw new Exception("Invalid reserve");
        Reserve = reserve;
    }

    public abstract void Fill(int amount);
    public abstract bool TryRefuel(Car car, int volume);
}

class Car
{
    private static Random random = new Random();
    public string Name { get; set; }
    public int TankVolume { get; }
    public AbstractFuelStation Station { get; }
    public int RefuelCount { get; }

    public Car (string name, AbstractFuelStation station, int tankVolume, int refuelCount)
    {
        Name = name;
        TankVolume = tankVolume;
        Station = station;
        RefuelCount = refuelCount;
    }

    public void Run()
    {
        Console.WriteLine($"{Name} STARTED; refuel count is {RefuelCount}");
        for (int i = 0; i < RefuelCount; i++)
        {
            var volume = random.Next(TankVolume) + 1;
            Console.WriteLine($"{Name} tries to get {volume} lit. for {i + 1} times");
            while (!Station.TryRefuel (this, volume))
            {
                Console.WriteLine($"{Name} is waiting");
                Thread.Sleep(random.Next(1, 5) + 1000);
            }

            Thread.Sleep(random.Next(1, 3) + 1000);
            Console.WriteLine($"{Name} left the station for {i + 1} times");
        }
        Console.WriteLine($"{Name} SAID GOOD-BYE");
    }
}

//class FuelStationSemaphore : AbstractFuelStation
//{
//    private Mutex mutex = new Mutex ();
//    private SemaphoreSlim semaphore;

//    public FuelStationSemaphore(int columnCount, int reserve = 1000) : base(columnCount, reserve)
//    {
//        semaphore = new SemaphoreSlim(ColumnCount, ColumnCount);
//    }

//    public override void Fill (int amount)
//    {
//        mutex.WaitOne();
//        if (amount + Reserve > Capacity)
//            Reserve = Capacity;
//        else
//            Reserve += Capacity;
//        Console.WriteLine("STATION TANK REFULED");
//        mutex.ReleaseMutex ();
//    }

//    public override bool TryRefuel(Car car, int volume)
//    {
//        semaphore.Wait();
//        mutex.WaitOne();
//        bool result = false;
//        if (volume <= Reserve)
//        {
//            Console.WriteLine($"Fuels reserve is {Reserve}. {volume} liters fueling began for {car.Name}");
//            Reserve -= volume;
//            mutex.ReleaseMutex ();
//            Thread.Sleep (100 * volume);
//            Console.WriteLine($"{car.Name} fueling finished");
//            result = true;
//        }
//        else
//        {
//            Console.WriteLine($"Fuel reserve {Reserve} is insufficient for {car.Name}");
//            mutex.ReleaseMutex ();
//        }
//        semaphore.Release();
//        return result;
//    }
//}


class FuelStationMonitor : AbstractFuelStation
{
    private object lockObject;
    private int freeColumnCount;

    public FuelStationMonitor (int columnCount, int reserve) : base (columnCount, reserve)
    {
        lockObject = new object();
        freeColumnCount = ColumnCount;
    }

    public override void Fill (int amount)
    {
        lock (lockObject)
        {
            if (amount + Reserve > Capacity)
                Reserve = Capacity;
            else
                Reserve += Capacity;
            Console.WriteLine("Station tank REFUELED");
            Monitor.PulseAll(lockObject);
        }
    }

    public override bool TryRefuel(Car car, int volume)
    {
        bool result = false;
        lock (lockObject)
        {
            while (freeColumnCount <= 0)
                Monitor.Wait(lockObject);
            if (volume <= Reserve)
            {
                freeColumnCount--;
                Console.WriteLine($"Fuels reserve is {Reserve}. {volume} liters fueling began for {car.Name}");
                Reserve -= volume;
                result = true;
            }
            else
            {
                Console.WriteLine($"Fuel reserve {Reserve} is insufficient for {car.Name}");
            }
        }
        if (result)
        {
            Thread.Sleep(100 * volume);
            Console.WriteLine($"{car.Name} fueling finished");
            lock (lockObject)
            {
                freeColumnCount++; ;
                Monitor.PulseAll(lockObject);
            }
        }
        return result;
    }
}

class Program
{
    static void Main(string[] args)
    {
        int carCount = 5;

        Random random = new Random();
        Console.WriteLine("\tGO!!!");
        var fuelStation = new FuelStationMonitor(2, 300);
        var tasks = new Task[carCount];
        for (int i = 0; i < carCount; i++)
        {
            var car = new Car("car" + (i+1), fuelStation, 100, random.Next(100));
            tasks[i] = new Task(car.Run);
        }
        tasks.ToList().ForEach(x => x.Start());
        Thread.Sleep(30000);
        fuelStation.Fill(2000);
        Task.WaitAll(tasks);
        Console.WriteLine("\tFinish");

    }
}

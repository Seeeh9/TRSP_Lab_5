using System;
using System.Threading;

class Wagon
{
    private readonly int capacity;
    private readonly int[] passengers;
    private int passengerCount;
    private Semaphore semaphore;

    public Wagon(int capacity, Semaphore semaphore)
    {
        this.capacity = capacity;
        this.passengers = new int[capacity];
        this.passengerCount = 0;
        this.semaphore = semaphore;
    }

    public void Load(int passengerId)
    {
        semaphore.WaitOne();

        lock (this)
        {
            Console.WriteLine($"Passenger {passengerId} boarded wagon");
            passengers[passengerCount] = passengerId;
            passengerCount++;

            if (passengerCount == capacity)
            {
                Monitor.PulseAll(this);
            }
        }
    }

    public void TakeRide()
    {
        Console.WriteLine("Wagon is taking a ride");
        Thread.Sleep(2000);
    }

    public void Unload()
    {
        lock (this)
        {
            Console.Write("Passengers ");
            for (int i = 0; i < passengerCount; i++)
            {
                Console.Write(passengers[i] + " ");
            }
            Console.WriteLine("left wagon");
            passengerCount = 0;

            semaphore.Release(capacity);

            Monitor.PulseAll(this);
        }
    }
}

class Passenger
{
    private readonly int id;
    private readonly Action<int> action;

    public Passenger(int id, Action<int> action)
    {
        this.id = id;
        this.action = action;
    }

    public void Board()
    {
        action(id);
    }
}

class Program
{
    static void Main(string[] args)
    {
        int n = 12; // Кількість пасажирів
        int c = 4; // Максимальна ємність вагончика

        for (int i = 1; i <= n; i += c)
        {
            Semaphore semaphore = new Semaphore(c, c);
            Wagon wagon = new Wagon(c, semaphore);

            int passengersToBoard = Math.Min(c, n - i + 1);

            for (int j = i; j < i + passengersToBoard; j++)
            {
                Passenger passenger = new Passenger(j, passengerId =>
                {
                    lock (wagon)
                    {
                        wagon.Load(passengerId);
                    }
                });

                Thread thread = new Thread(new ThreadStart(passenger.Board));
                thread.Start();
            }

            Thread.Sleep(1000);

            lock (wagon)
            {
                wagon.TakeRide();
                wagon.Unload();
            }
        }
    }
}

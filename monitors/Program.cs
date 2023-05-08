using System;
using System.Threading;

class Wagon
{
    public int capacity;
    public int[] passengers;
    public int passengerCount;
    public bool isFull;

    public Wagon(int capacity)
    {
        this.capacity = capacity;
        this.passengers = new int[capacity];
        this.passengerCount = 0;
        this.isFull = false;
    }

    public void Load(int passengerId)
    {
        lock (this)
        {
            while (passengerCount >= capacity)
            {
                Monitor.Wait(this);
            }

            Console.WriteLine($"Passenger {passengerId} boarded wagon");
            passengers[passengerCount] = passengerId;
            passengerCount++;

            if ((passengerCount == capacity) && (passengers.Count() <= capacity))
            {
                isFull = true;
            }

            Monitor.PulseAll(this);
        }
    }

    public void TakeRide()
    {
        lock (this)
        {
            while (!isFull)
            {
                Monitor.Wait(this);
            }

            Console.WriteLine("Wagon is taking a ride");
            Thread.Sleep(2000);

            Monitor.PulseAll(this);
        }
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
            isFull = false;

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
        int n = 10; // Кількість пасажирів
        int c = 2; // Максимальна ємність вагончика

        Wagon wagon = new Wagon(c);

        Thread[] threads = new Thread[n];

        for (int i = 1; i <= n; i++)
        {
            Passenger passenger = new Passenger(i, passengerId =>
            {
                wagon.Load(passengerId);
            });

            threads[i - 1] = new Thread(new ThreadStart(passenger.Board));
            threads[i - 1].Start();

            if (i % c == 0)
            {
                Thread.Sleep(1000); // Затримка, щоб пасажири не вибігали одночасно

                wagon.TakeRide();
                wagon.Unload();
            }
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }
}

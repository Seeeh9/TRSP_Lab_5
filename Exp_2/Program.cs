int x = 0;
object locker = new();     // об'єкт-заглушка
// запускаємо п'ять потоків
for (int i = 1; i < 6; i++)
{
    Thread myThread = new(Print);
    myThread.Name = $"Потiк {i}";
    myThread.Start();
}

void Print()
{
    bool acquiredLock = false;
    try
    {
        Monitor.Enter(locker, ref acquiredLock);
        x = 1;
        for (int i = 1; i < 6; i++)
        {
            Console.WriteLine($"{Thread.CurrentThread.Name}: {x}");
            x++;
            Thread.Sleep(100);
        }
    }
    finally
    {
        if (acquiredLock) Monitor.Exit(locker);
    }
}
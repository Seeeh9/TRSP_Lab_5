// запускаем пять потоков
using System.Reflection.PortableExecutable;

for (int i = 1; i < 6; i++)
{
    Reader reader = new Reader(i);
}
class Reader
{
    // создаем семафор
    static Semaphore sem = new Semaphore(3, 3);
    Thread myThread;
    int count = 3;// лічильник читання


    public Reader(int i)
     {
        myThread = new Thread(Read);
        myThread.Name = $"Читач {i}";
        myThread.Start();
    }

    public void Read()
     {
        while (count > 0)
        {
            sem.WaitOne();     // очікуємо, коли звільнитися місце

            Console.WriteLine($"{Thread.CurrentThread.Name} входить до бiблiотеки");

            Console.WriteLine($"{Thread.CurrentThread.Name} читач");
            Thread.Sleep(1000);

            Console.WriteLine($"{Thread.CurrentThread.Name} залишає бiблiотеку");

            sem.Release();     // звільняємо місце

            count--;
            Thread.Sleep(1000);
        }
    }
}
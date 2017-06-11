using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tutorial1
{
    class Program
    {
        /// <summary>
        /// Переделать код так, чтобы потоки выполнялись один за другим и на момент вывода на экран состояние было "Finished"
        /// Как можно улучшить этот код? Какие проблемы вы заметили? Улучшайте!
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Worker worker = new Worker();

            while (true)
            {
                Console.ReadLine();
                Task worker1 = new Task(worker.Start);
                Task worker2 = new Task(worker.Finish);

                // блокируем поток main  пока не будут выполнены задачи в потоках
                worker1.Start();
                worker1.Wait();
                //к этому моменту первый поток завершится
                worker2.Start();
                // далее будет соревнование между main и worker2, поэтому ждем завершения worker2
                worker2.Wait();

                Console.WriteLine(worker.Condition);
            }
        }

        public class Worker
        {
            public string Condition = "none";

            public void Start()
            {
                Condition = "Just started";
            }

            public void Finish()
            {
                Condition = "Finished";
            }
        }
    }
}
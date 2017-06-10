using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SharedList
{
    /// <summary>
    /// В сборочном цеху завода, вокруг одного стола работает несколько роботов
    /// Один робот раскладывает по столу заготовки, еще несколько роботов собирают их.
    /// Роботы-сборщики берут деталь со стола и собирают ее в два этапа, сборка деталей моментальная, но после сборки роботу необходимо немного остыть.
    /// За отведенный период времени заготовки должны быть собраны. 
    /// 
    /// Доработать код так, чтобы бракованных изделий не было, а количество недоделанных свести к минимуму
    /// Как можно улучшить этот код? Какие проблемы вы заметили? Улучшайте!
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            FrameProvider frameProvider = new FrameProvider();

            Assembler assembler = new Assembler();

            Thread provider = new Thread(frameProvider.Start);
            provider.Start();

            for (int i = 0; i < 5; i++)
            {
                new Thread(assembler.Start).Start();
            }

            Thread.Sleep(10000);

            ListManager.StopTrigger = true;
            Console.WriteLine(ListManager.GetInfo());
            Console.ReadLine();
            Console.ReadLine();
        }

        //Рама = 0,
        //Полусобрано = 1,
        //Собрано = 2,
        //Испорчено >= 3

        public static class ListManager
        {
            public static bool StopTrigger = false;
            public static List<Detail> SharedList;
            static ListManager()
            {
                SharedList = new List<Detail>();
            }

            public static string GetInfo()
            {
                string result = "";
                int frames = 0;
                int halfs = 0;
                int assembled = 0;
                int broken = 0;

                foreach (var element in SharedList)
                {
                    switch (element.Status)
                    {
                        case 0:
                            {
                                frames++;
                                break;
                            }

                        case 1:
                            {
                                halfs++;
                                break;
                            }

                        case 2:
                            {
                                assembled++;
                                break;
                            }
                        default:
                            {
                                broken++;
                                break;
                            }
                    }
                    result = String.Format("Итого:\r\n" +
                                           "Рамы:{0}\r\n" +
                                           "Полусобрано:{1}\r\n" +
                                           "Собрано:{2}\r\n" +
                                           "Испорчено:{3}\r\n",
                        frames, halfs, assembled, broken
                    );
                }

                return result;
            }

            public static int UpdateDetail(int[] requiredConditions)
            {

                for (int i = 0; i < SharedList.Count; i++)
                {
                    while (true)
                    {
                        int current = SharedList[i].Status;
                        if (requiredConditions.Contains(current))
                        {
                            int nextVal = current + 1;
                            // пытаемся изменить статус детальки
                            // если удачно, то вернем индекс измененной детали
                            // если неудачно, то пробуем еще раз
                            if (SharedList[i].CompareAndSet(nextVal, current))
                            {
                                return i;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return -1;
            }
        }

        public class FrameProvider : Robot
        {
            //Добавляет продукты в общий список
            public override void DoWork()
            {
                if (ListManager.SharedList.Count < 150)
                    ListManager.SharedList.Add(new Detail {Status=0 });
            }
        }

        //Сборщики должны брать в работу только пустые рамы или недоделанные изделия
        public class Assembler : Robot
        {
            public override void DoWork()
            {
                int index = ListManager.UpdateDetail(new[] { 0, 1 });

                if (index >= 0)
                {
                   // ListManager.SharedList[index]++;
                    Thread.Sleep(250);
                }
            }
        }

        public abstract class Robot
        {
            public void Start()
            {
                while (!ListManager.StopTrigger)
                {
                    DoWork();
                }
            }

            public abstract void DoWork();
        }

      public  class Detail
        {
            private int status;
            public int Status { get => status; set => status = value; }

            public bool CompareAndSet(int nextStatus, int comparand)
            {
              return Interlocked.CompareExchange(ref this.status, nextStatus, comparand)== comparand;
            }
        }
    }
}

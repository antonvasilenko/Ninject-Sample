using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DIContainerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            iocInit();

            var calc = Ioc.Get<ICalculator>();

            var summ = calc.Add(111, 222);
            Console.WriteLine("Summ is {0}", summ);
        }

        private static void iocInit()
        {
            var simple = false;

            Ioc.Init((kernel) =>
            {

                if (simple)
                {
                    kernel.Bind<ICalculator>().To<SimpleCalculator>().InTransientScope(); // default
                    // kernel.Bind<ICalculator>().To<SimpleCalculator>().InSingletonScope();
                    // kernel.Bind<ICalculator>().To<SimpleCalculator>().InThreadScope(); // 1 instance pre thread
                }
                else
                {
                    kernel.Bind<ICalculator>().To<ServerCalculator>().InTransientScope();

                }
                kernel.Bind<ServerProxy>().ToSelf();

            });
        }
    }

    public interface ICalculator
    {
        int? Add(int a, int b);
        int? Substract(int a, int b);
    }

    public class SimpleCalculator: ICalculator
    {
        public int? Add(int a, int b)
        {
            return a + b;
        }

        public int? Substract(int a, int b)
        {
            return a - b;
        }
    }

    public class ServerCalculator : ICalculator
    {
        private readonly ServerProxy _proxy;

        public ServerCalculator(ServerProxy proxy)
        {
            _proxy = proxy;
        }

        public int? Add(int a, int b)
        {
            var correlationKey = Guid.NewGuid();
            try
            {

                Console.WriteLine("{0}: Calculation started", correlationKey);
                var res = _proxy.SendAdd(a, b);
                Console.WriteLine("{0}: Calculation ended", correlationKey);
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: Calculation failed with error '{1}'", correlationKey, ex.Message);
                return null;
            }
        }

        public int? Substract(int a, int b)
        {
            var correlationKey = Guid.NewGuid();
            try
            {

                Console.WriteLine("{0}: Calculation started", correlationKey);
                var res = _proxy.SendSubstract(a, b);
                Console.WriteLine("{0}: Calculation ended", correlationKey);
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: Calculation failed with error '{1}'", correlationKey, ex.Message);
                return null;
            }
        }
    }

    public class ServerProxy
    {
        // own dependencies, like HttpClient, TokenService, Serializer, ...

        public int SendAdd(int a, int b)
        {
            // POST http://some.external.server/add
            Thread.Sleep(1000);
            return a + b;
        }

        public int SendSubstract(int a, int b)
        {
            // POST http://some.external.server/add
            Thread.Sleep(1000);
            return a - b;
        }
    }
}

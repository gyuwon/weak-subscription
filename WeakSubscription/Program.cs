using System;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace WeakSubscription
{
    public class Subscriber
    {
        private string _name;

        public Subscriber(string name)
        {
            _name = name;
            Messenger.Default.Register<string>(this, m => Console.WriteLine("{0} received \"{1}\"", _name, m));
        }

        public string Name { get { return _name; } }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            var s1 = new Subscriber("s1");
            new Subscriber("s2");

            Messenger.Default.Send("Hello");

            GC.Collect();
            GC.WaitForFullGCComplete();

            Messenger.Default.Send("World");
        }
    }
}
using System;
using System.Linq;
using Xamarin.Forms;

namespace WeakSubscription
{
    public class Subscriber
    {
        private string _name;

        public Subscriber(string name)
        {
            _name = name;
            MessagingCenter.Subscribe<object>(this, "Hello", _ => Console.WriteLine("{0} received", _name));
        }

        public string Name { get { return _name; } }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            object sender = new object();

            var s1 = new Subscriber("s1");
            new Subscriber("s2");

            MessagingCenter.Send(sender, "Hello");

            GC.Collect();
            GC.WaitForFullGCComplete();

            MessagingCenter.Send(sender, "Hello");
        }
    }
}
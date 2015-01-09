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
            MessagingCenter.Subscribe<object, string>(
                subscriber: this,
                   message: "Greeting",
                  callback: (s, m) => Console.WriteLine("{0} received \"{1}\"", _name, m));
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

            MessagingCenter.Send(sender, "Greeting", "Hello");

            GC.Collect();
            GC.WaitForFullGCComplete();

            MessagingCenter.Send(sender, "Greeting", "World");
        }
    }
}
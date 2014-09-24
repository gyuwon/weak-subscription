using System;
using System.Collections.Generic;
using System.Linq;

namespace WeakSubscription
{
    public class Messenger
    {
        public static readonly Messenger Instance = new Messenger();

        private List<Action<string>> _callbacks = new List<Action<string>>();

        public void Subscribe(Action<string> callback)
        {
            _callbacks.Add(callback);
        }

        public void Unsubscribe(Action<string> callback)
        {
            _callbacks.Remove(callback);
        }

        public void Publish(string message)
        {
            foreach (var callback in _callbacks)
            {
                callback.Invoke(message);
            }
        }
    }

    public class Subscriber
    {
        private string _name;

        public Subscriber(string name)
        {
            _name = name;
            Messenger.Instance.Subscribe(m => Console.WriteLine("{0} received \"{1}\"", _name, m));
        }

        public string Name { get { return _name; } }
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            var s1 = new Subscriber("s1");
            new Subscriber("s2");

            Messenger.Instance.Publish("Hello");

            GC.Collect();
            GC.WaitForFullGCComplete();

            Messenger.Instance.Publish("World");
        }
    }
}
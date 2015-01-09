using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WeakSubscription
{
    public class Messenger
    {
        public static readonly Messenger Instance = new Messenger();

        private class Subscription
        {
            public WeakReference<Subscriber> Subscriber { get; set; }

            public Action<Subscriber, string> Callback { get; set; }
        }

        private List<Subscription> _subscriptions = new List<Subscription>();

        public void Subscribe(Action<string> callback)
        {
            var target = callback.Target;
            var method = callback.Method;
            var delegateType = typeof(Action<Subscriber, string>);
            _subscriptions.Add(new Subscription
            {
                Subscriber = new WeakReference<Subscriber>((Subscriber)target),
                Callback = (Action<Subscriber, string>)method.CreateDelegate(delegateType)
            });
        }

        public void Publish(string message)
        {
            var collected = new List<int>();
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                var subscription = _subscriptions[i];
                Subscriber subscriber;
                if (subscription.Subscriber.TryGetTarget(out subscriber))
                {
                    subscription.Callback.Invoke(subscriber, message);
                }
                else
                {
                    collected.Add(i);
                }
            }
            collected.Reverse();
            foreach (var index in collected)
            {
                _subscriptions.RemoveAt(index);
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
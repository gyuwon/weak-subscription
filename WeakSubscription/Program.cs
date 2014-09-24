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
            public WeakReference<object> Subscriber { get; set; }

            public Action<object, string> Callback { get; set; }
        }

        private List<Subscription> _subscriptions = new List<Subscription>();
        private MethodInfo _createCallbackTemplate;

        public Messenger()
        {
            _createCallbackTemplate = typeof(Messenger).GetTypeInfo().GetMethod("CreateCallback");
        }

        public void Subscribe(Action<string> callback)
        {
            var subscription = new Subscription
            {
                Subscriber = new WeakReference<object>(callback.Target),
                Callback = (Action<object, string>)_createCallbackTemplate
                                                   .MakeGenericMethod(callback.Target.GetType())
                                                   .Invoke(this, new object[] { callback.Method })
            };
            _subscriptions.Add(subscription);
        }

        public Action<object, string> CreateCallback<T>(MethodInfo method)
        {
            var callback = (Action<T, string>)method.CreateDelegate(typeof(Action<T, string>));
            return (target, message) => callback.Invoke((T)target, message);
        }

        public void Publish(string message)
        {
            var collected = new List<int>();
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                var subscription = _subscriptions[i];
                object subscriber;
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
            Messenger.Instance.Subscribe(m => Console.WriteLine("{0} received \"{1}\"", name, m));
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
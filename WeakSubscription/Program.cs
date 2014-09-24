using System;
using System.Collections.Generic;
using System.Linq;

namespace WeakSubscription
{
    public class Messenger
    {
        public static readonly Messenger Instance = new Messenger();

        private List<WeakReference<Action<string>>> _callbacks = new List<WeakReference<Action<string>>>();

        public void Subscribe(Action<string> callback)
        {
            _callbacks.Add(new WeakReference<Action<string>>(callback));
        }

        public void Unsubscribe(Action<string> callback)
        {
            int? index = null;
            for (int i = 0; i < _callbacks.Count; i++)
            {
                var reference = _callbacks[i];
                Action<string> target;
                if (reference.TryGetTarget(out target) && target == callback)
                {
                    index = i;
                    break;
                }
            }
            if (null != index)
            {
                _callbacks.RemoveAt(index.Value);
            }
        }

        public void Publish(string message)
        {
            var collected = new List<int>();
            for (int i = 0; i < _callbacks.Count; i++)
            {
                var reference = _callbacks[i];
                Action<string> callback;
                if (reference.TryGetTarget(out callback))
                {
                    callback.Invoke(message);
                }
                else
                {
                    collected.Add(i);
                }
            }
            collected.Reverse();
            foreach (var index in collected)
            {
                _callbacks.RemoveAt(index);
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
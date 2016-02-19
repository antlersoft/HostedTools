using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Publisher;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Utility.Testing
{
    /// <summary>
    /// An implementation of IHtPublisher that works only within a single process, and only when all objects are obtained from the
    /// same PrivateHtPublisher instance
    /// </summary>
    public class PrivateHtPublisher : IHtPublisher
    {
        private readonly object _topicLock = new object();
        private Dictionary<string, PrivateTopic> _topics = new Dictionary<string, PrivateTopic>();
        public IPublisherSubscription CreateSubscription(string subscriptionId, IPublisherTopic topic)
        {
            PrivateTopic pti;
            lock (_topicLock)
            {
                pti = _topics[topic.TopicId];
            }
            return pti.GetSubscription(subscriptionId, true);
        }

        public IPublisherTopic CreateTopic(string topicId)
        {
            lock (_topicLock)
            {
                PrivateTopic result;
                if (! _topics.TryGetValue(topicId, out result))
                {
                    result = new PrivateTopic(topicId);
                    _topics.Add(topicId, result);
                }
                return result;
            }
        }

        public void DeleteSubscription(string subscriptionId)
        {
            lock (_topicLock)
            {
                foreach (var t in _topics.Values)
                {
                    t.DeleteSubscription(subscriptionId);
                }
            }
        }

        public void DeleteTopic(string topicId)
        {
            lock (_topicLock)
            {
                _topics.Remove(topicId);
            }
        }

        public IList<IPublisherTopicInfo> GetAvailableTopics(string topicId = null)
        {
            lock (_topicLock)
            {
                return _topics.Values.Select(t => t as IPublisherTopicInfo).ToList();
            }
        }

        public IPublisherSubscription GetSubscription(string subscriptionId)
        {
            List<PrivateTopic> pts;
            lock (_topicLock)
            {
                pts = _topics.Values.ToList();
            }
            return pts.Select(pt => GetSubscription(subscriptionId)).First(s => s != null);
        }

        public IPublisherTopic GetTopic(string topicId)
        {
            lock (_topicLock)
            {
                return _topics[topicId];
            }
        }

        public void Dispose()
        {
            lock (_topicLock)
            {
                _topics = null;
            }
        }
    }

    internal class PrivateSubscription : IPublisherSubscription, IPublisherSubscriptionInfo
    {
        internal PrivateSubscription(string topicId, string subscriptionId)
        {
            TopicId = topicId;
            SubscriptionId = subscriptionId;
            CreatedDate = DateTime.UtcNow;
        }
        public Func<IPublisherMessage, Task> Listener { get; set; }

        public string SubscriptionId { get; private set; }

        public int TaskLimit { get; set; }

        public void Dispose()
        {
        }

        public DateTime CreatedDate { get; private set; }

        public string TopicId { get; private set; }
    }

    internal class PrivateTopic : IPublisherTopic, IPublisherTopicInfo
    {
        private readonly Dictionary<string, PrivateSubscription> _subscriptions = new Dictionary<string, PrivateSubscription>();
        private readonly string _topicId;

        internal PrivateTopic(string topicId)
        {
            _topicId = topicId;
        }

        internal PrivateSubscription GetSubscription(string id, bool createIfNotExist = false)
        {
            lock (_subscriptions)
            {
                PrivateSubscription result;
                if (_subscriptions.TryGetValue(id, out result))
                {
                    return result;
                }
                if (! createIfNotExist)
                {
                    return null;
                }
                result = new PrivateSubscription(_topicId, id);
                _subscriptions.Add(id, result);
                return result;
            }
        }

        internal void DeleteSubscription(string subId)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(subId);
            }
        }

        public async Task<IPublisherMessage> PublishAsync(IHtValue messageBody)
        {
            var pm = new PrivateMessage(messageBody, _topicId);
            List<PrivateSubscription> ps;
            lock (_subscriptions)
            {
                ps = _subscriptions.Values.ToList();
            }
            if (ps.Count > 0)
            {
                await Task.WhenAll(ps.Select(s => s.Listener).Where(f => f!=null).Select(f => f(pm))).ConfigureAwait(false);
            }
            return pm;
        }

        public IList<IPublisherSubscriptionInfo> Subscriptions
        {
            get { return _subscriptions.Values.Select(v => v as IPublisherSubscriptionInfo).ToList(); }
        }

        public string TopicId
        {
            get { return _topicId; }
        }
        public void Dispose()
        {
        }
    }

    internal class PrivateMessage : IPublisherMessage
    {
        internal PrivateMessage(IHtValue body, string topicId)
        {
            Body = new JsonHtValue(body);
            TopicId = topicId;
            MessageId = Guid.NewGuid().ToString();
            PublishTimeUtc = DateTime.UtcNow;
        }

        public IHtValue Body { get; private set; }

        public string MessageId { get; private set; }

        public DateTime PublishTimeUtc { get; private set; }

        public string TopicId { get; private set; }
    }
}

﻿using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;

namespace Tests.Mocks
{
    public class MockMessageChannel : IMessageChannel
    {
        public MockMessageChannel()
        {
            var random = new Random();
            Id = (ulong)random.Next(0, int.MaxValue);
        }

        public string Name => throw new NotImplementedException();

        public DateTimeOffset CreatedAt => throw new NotImplementedException();

        public ulong Id { get; init; }

        public List<MockMessage> messages = new(8);

        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
        {
            int index = messages.FindIndex(message => message.Id == messageId);
            messages.RemoveAt(index);
            return Task.CompletedTask;
        }

        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
        {
            int index = messages.FindIndex(msg => message.Id == msg.Id);
            messages.RemoveAt(index);
            return Task.CompletedTask;
        }

        public IDisposable EnterTypingState(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            return Task.FromResult(messages.FirstOrDefault(message => message.Id == id) as IMessage);
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            if (limit > messages.Count) limit = messages.Count;
            var range = messages.GetRange(0, limit).Select(message => (IMessage)message).ToList();
            IReadOnlyCollection<IMessage>[] collections = { new ReadOnlyCollection<IMessage>(range) };
            return collections.ToAsyncEnumerable();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            if (dir is Direction.Around or Direction.Before) throw new NotImplementedException();
            if (!messages.Any(message => message.Id == fromMessageId)) return null;
            var index = messages.FindIndex(message => message.Id == fromMessageId);
            if (limit > messages.Count) limit = messages.Count - index;

            var range = messages.GetRange(index, limit).Select(message => (IMessage)message).ToList();
            IReadOnlyCollection<IMessage>[] collections = { new ReadOnlyCollection<IMessage>(range) };
            return collections.ToAsyncEnumerable();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
            => GetMessagesAsync(fromMessage.Id, dir, limit, mode, options);

        public Task<IReadOnlyCollection<IMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null)
        {
            throw new NotImplementedException();
        }

        public Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null)
        {
            var message = new UserMockMessage(text, this, null);
            messages.Insert(0, message);
            return Task.FromResult(message as IUserMessage);
        }

        public Task TriggerTypingAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
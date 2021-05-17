using System;

namespace GhostNetwork.Content.Comments
{
    public class Comment
    {
        public Comment(string id, string content, DateTimeOffset createdOn, string reactionKey, string replyCommentId, UserInfo author)
        {
            Id = id;
            Content = content;
            CreatedOn = createdOn;
            EntityKey = reactionKey;
            ReplyCommentId = replyCommentId;
            Author = author;
        }

        public string Id { get; }

        public string Content { get; }

        public string EntityKey { get; }

        public UserInfo Author { get; }

        public DateTimeOffset CreatedOn { get; }

        public string ReplyCommentId { get; }

        public static Comment New(string text, string entityKey, string replyId, UserInfo author)
        {
            return new Comment(default, text, DateTimeOffset.UtcNow, entityKey, replyId, author);
        }
    }
}
